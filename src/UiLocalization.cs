using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace PrototypeUltimateTool
{
    /// <summary>
    /// Metadata for an installed runtime language. Add a new XML file to the Languages folder
    /// to add a language; no code changes or rebuild are required.
    /// </summary>
    internal sealed class UiLanguageOption
    {
        public string Code { get; private set; }
        public string DisplayName { get; private set; }
        public string NativeName { get; private set; }
        public string CultureName { get; private set; }

        public UiLanguageOption(string code, string displayName, string nativeName, string cultureName)
        {
            Code = code ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? (string.IsNullOrWhiteSpace(Code) ? "Language" : Code) : displayName;
            NativeName = string.IsNullOrWhiteSpace(nativeName) ? DisplayName : nativeName;
            CultureName = string.IsNullOrWhiteSpace(cultureName) ? Code : cultureName;
        }
    }

    /// <summary>
    /// Data-driven runtime localization.
    /// Static controls use stable technical IDs in Control.Name / ToolStripItem.Name.
    /// All interface text is resolved through stable technical IDs from external XML catalogs.
    /// </summary>
    internal static class UiLocalization
    {
        private sealed class LanguageCatalog
        {
            public UiLanguageOption Option;
            public readonly Dictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        private static readonly object Sync = new object();
        private static readonly Dictionary<string, LanguageCatalog> Catalogs = new Dictionary<string, LanguageCatalog>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<UiLanguageOption> Options = new List<UiLanguageOption>();
        private static string _languageDirectory;
        private static bool _initialized;

        public static string DefaultLanguageCode { get { return IsLanguageAvailable("ru") ? "ru" : Options.FirstOrDefault()?.Code ?? "ru"; } }

        public static IList<UiLanguageOption> AvailableLanguages
        {
            get { lock (Sync) return Options.ToList().AsReadOnly(); }
        }

        public static void Initialize(string languageDirectory)
        {
            lock (Sync)
            {
                if (_initialized && string.Equals(_languageDirectory, languageDirectory, StringComparison.OrdinalIgnoreCase)) return;
                _languageDirectory = languageDirectory ?? string.Empty;
                Catalogs.Clear();
                Options.Clear();

                // Per-user dictionaries are read first, so custom languages and user overrides
                // do not require write access to the application installation directory.
                var searchDirectories = new List<string>();
                string userLanguageDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PrototypeUltimateTool", "Languages");
                if (Directory.Exists(userLanguageDirectory)) searchDirectories.Add(userLanguageDirectory);
                if (!string.IsNullOrWhiteSpace(_languageDirectory) && Directory.Exists(_languageDirectory) &&
                    !searchDirectories.Any(d => string.Equals(d, _languageDirectory, StringComparison.OrdinalIgnoreCase)))
                    searchDirectories.Add(_languageDirectory);

                foreach (string directory in searchDirectories)
                {
                    string[] dictionaryFiles;
                    try { dictionaryFiles = Directory.GetFiles(directory, "*.xml", SearchOption.TopDirectoryOnly); }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Localization directory could not be scanned: " + directory + " — " + ex.Message);
                        continue;
                    }
                    foreach (string path in dictionaryFiles.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                    {
                        try
                        {
                            LanguageCatalog catalog = LoadCatalog(path);
                            if (catalog == null || string.IsNullOrWhiteSpace(catalog.Option.Code)) continue;
                            LanguageCatalog existing;
                            if (Catalogs.TryGetValue(catalog.Option.Code, out existing))
                            {
                                bool isUserDictionary = string.Equals(directory, userLanguageDirectory, StringComparison.OrdinalIgnoreCase);
                                // User dictionary wins for matching keys; bundled values only fill
                                // gaps so a short override remains useful and maintainable.
                                foreach (KeyValuePair<string, string> pair in catalog.Strings)
                                {
                                    if (isUserDictionary || !existing.Strings.ContainsKey(pair.Key))
                                        existing.Strings[pair.Key] = pair.Value;
                                }
                                continue;
                            }
                            Catalogs.Add(catalog.Option.Code, catalog);
                            Options.Add(catalog.Option);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Localization file could not be loaded: " + path + " — " + ex.Message);
                        }
                    }
                }

                Options.Sort((a, b) =>
                {
                    int rankA = string.Equals(a.Code, "ru", StringComparison.OrdinalIgnoreCase) ? 0 :
                        string.Equals(a.Code, "en", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
                    int rankB = string.Equals(b.Code, "ru", StringComparison.OrdinalIgnoreCase) ? 0 :
                        string.Equals(b.Code, "en", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
                    int rank = rankA.CompareTo(rankB);
                    return rank != 0 ? rank : string.Compare(a.NativeName, b.NativeName, StringComparison.CurrentCultureIgnoreCase);
                });

                _initialized = true;
            }
        }

        private static LanguageCatalog LoadCatalog(string path)
        {
            var doc = new XmlDocument { XmlResolver = null };
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                doc.Load(stream);
            XmlElement root = doc.DocumentElement;
            if (root == null || !string.Equals(root.Name, "language", StringComparison.OrdinalIgnoreCase)) return null;

            string code = (root.GetAttribute("code") ?? string.Empty).Trim();
            if (code.Length == 0) return null;
            string displayName = root.GetAttribute("displayName");
            string nativeName = root.GetAttribute("nativeName");
            string cultureName = root.GetAttribute("culture");
            var result = new LanguageCatalog
            {
                Option = new UiLanguageOption(code, displayName, nativeName, cultureName)
            };

            XmlNode strings = root.SelectSingleNode("strings");
            if (strings != null)
            {
                foreach (XmlNode node in strings.SelectNodes("string"))
                {
                    XmlElement element = node as XmlElement;
                    if (element == null) continue;
                    string key = element.GetAttribute("key").Trim();
                    if (key.Length == 0) continue;
                    result.Strings[key] = element.InnerText ?? string.Empty;
                }
            }

            return result;
        }

        public static bool IsLanguageAvailable(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) return false;
            lock (Sync) return Catalogs.ContainsKey(languageCode.Trim());
        }

        /// <summary>
        /// Selects the best installed UI language for a Windows culture. Exact culture matches
        /// take priority, then the neutral language (for example en-CA -> en), and finally the
        /// configured default language. This is used only when the user has no saved preference.
        /// </summary>
        public static string FindLanguageForCulture(string cultureName)
        {
            string requested = (cultureName ?? string.Empty).Trim().Replace('_', '-');
            if (requested.Length == 0) return DefaultLanguageCode;

            lock (Sync)
            {
                UiLanguageOption exactCulture = Options.FirstOrDefault(option =>
                    string.Equals(option.CultureName, requested, StringComparison.OrdinalIgnoreCase));
                if (exactCulture != null) return exactCulture.Code;

                LanguageCatalog exactCode;
                if (Catalogs.TryGetValue(requested, out exactCode)) return exactCode.Option.Code;

                int separator = requested.IndexOf('-');
                string neutral = separator > 0 ? requested.Substring(0, separator) : requested;
                LanguageCatalog neutralCode;
                if (Catalogs.TryGetValue(neutral, out neutralCode)) return neutralCode.Option.Code;

                UiLanguageOption cultureFamily = Options.FirstOrDefault(option =>
                    string.Equals((option.CultureName ?? string.Empty).Split('-')[0], neutral, StringComparison.OrdinalIgnoreCase));
                if (cultureFamily != null) return cultureFamily.Code;
            }

            return DefaultLanguageCode;
        }

        public static string NormalizeLanguageCode(string languageCode)
        {
            if (!string.IsNullOrWhiteSpace(languageCode))
            {
                string code = languageCode.Trim();
                lock (Sync)
                {
                    if (Catalogs.ContainsKey(code)) return Catalogs.Keys.First(k => string.Equals(k, code, StringComparison.OrdinalIgnoreCase));
                }
            }
            return DefaultLanguageCode;
        }

        public static string TranslateId(string key, string fallback, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(key)) return fallback ?? string.Empty;
            lock (Sync)
            {
                string value;
                LanguageCatalog active;
                if (!string.IsNullOrWhiteSpace(languageCode) && Catalogs.TryGetValue(languageCode, out active) && active.Strings.TryGetValue(key, out value))
                    return value;
                LanguageCatalog primary;
                if (Catalogs.TryGetValue(DefaultLanguageCode, out primary) && primary.Strings.TryGetValue(key, out value))
                    return value;
                return fallback ?? string.Empty;
            }
        }

        public static string TranslateIdPrefixInText(string text, string key, string fallback, string languageCode)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(key)) return text;
            string translated = TranslateId(key, fallback, languageCode);
            lock (Sync)
            {
                var variants = new HashSet<string>(StringComparer.Ordinal);
                if (!string.IsNullOrEmpty(fallback)) variants.Add(fallback);
                foreach (LanguageCatalog catalog in Catalogs.Values)
                {
                    string value;
                    if (catalog.Strings.TryGetValue(key, out value) && !string.IsNullOrEmpty(value)) variants.Add(value);
                }
                foreach (string variant in variants.OrderByDescending(value => value.Length))
                {
                    if (string.IsNullOrEmpty(variant) || string.Equals(variant, translated, StringComparison.Ordinal)) continue;
                    if (text.StartsWith(variant, StringComparison.Ordinal))
                        return translated + text.Substring(variant.Length);
                }
            }
            return text;
        }

        public static string TranslateIdSuffixInText(string text, string key, string fallback, string languageCode)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(key)) return text;
            string translated = TranslateId(key, fallback, languageCode);
            lock (Sync)
            {
                var variants = new HashSet<string>(StringComparer.Ordinal);
                if (!string.IsNullOrEmpty(fallback)) variants.Add(fallback);
                foreach (LanguageCatalog catalog in Catalogs.Values)
                {
                    string value;
                    if (catalog.Strings.TryGetValue(key, out value) && !string.IsNullOrEmpty(value)) variants.Add(value);
                }
                foreach (string variant in variants.OrderByDescending(value => value.Length))
                {
                    if (string.IsNullOrEmpty(variant) || string.Equals(variant, translated, StringComparison.Ordinal)) continue;
                    if (text.EndsWith(variant, StringComparison.Ordinal))
                        return text.Substring(0, text.Length - variant.Length) + translated;
                }
            }
            return text;
        }

        private static bool TryTranslateId(string key, string fallback, string languageCode, out string translated)
        {
            translated = null;
            if (string.IsNullOrWhiteSpace(key)) return false;
            lock (Sync)
            {
                LanguageCatalog active;
                if (!string.IsNullOrWhiteSpace(languageCode) && Catalogs.TryGetValue(languageCode, out active) && active.Strings.TryGetValue(key, out translated)) return true;
                LanguageCatalog primary;
                if (Catalogs.TryGetValue(DefaultLanguageCode, out primary) && primary.Strings.TryGetValue(key, out translated)) return true;
                translated = fallback;
                return false;
            }
        }

        private static string TranslateUiText(string text, string technicalId, string languageCode)
        {
            string localized;
            if (TryTranslateId(technicalId, text, languageCode, out localized)) return localized ?? string.Empty;
            return text ?? string.Empty;
        }

        private static string TranslateOptionalId(string key, string currentValue, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrEmpty(currentValue)) return currentValue;
            string localized;
            return TryTranslateId(key, currentValue, languageCode, out localized)
                ? (localized ?? string.Empty)
                : currentValue;
        }

        public static void ApplyControlTree(Control root, string languageCode)
        {
            if (root == null) return;
            root.AccessibleName = TranslateOptionalId((root.Name ?? string.Empty) + ".accessibleName", root.AccessibleName, languageCode);
            root.AccessibleDescription = TranslateOptionalId((root.Name ?? string.Empty) + ".accessibleDescription", root.AccessibleDescription, languageCode);

            // Input contents and asset lists contain user/resource data, not UI labels.
            if (root is TextBoxBase || root is System.Windows.Forms.ComboBox || root is ListControl) return;

            ToolStrip strip = root as ToolStrip;
            if (strip != null)
            {
                ApplyToolStripItems(strip.Items, languageCode);
                return;
            }

            root.Text = TranslateUiText(root.Text, root.Name, languageCode);
            foreach (Control child in root.Controls) ApplyControlTree(child, languageCode);
        }


        public static void ApplyToolStripItems(ToolStripItemCollection items, string languageCode)
        {
            if (items == null) return;
            foreach (ToolStripItem item in items)
            {
                if (item == null) continue;
                if (item.Name != null && (item.Name.StartsWith("language.option.", StringComparison.Ordinal) || item.Name.StartsWith("recent.file.", StringComparison.Ordinal))) continue;

                ToolStripComboBox combo = item as ToolStripComboBox;
                if (combo != null)
                {
                    combo.ToolTipText = TranslateOptionalId((combo.Name ?? string.Empty) + ".tooltip", combo.ToolTipText, languageCode);
                    continue; // ComboBox items can be filenames, extensions, or invariant filter values.
                }
                ToolStripTextBox textBox = item as ToolStripTextBox;
                if (textBox != null)
                {
                    textBox.ToolTipText = TranslateOptionalId((textBox.Name ?? string.Empty) + ".tooltip", textBox.ToolTipText, languageCode);
                    continue; // Preserve user search/input text.
                }

                item.Text = TranslateUiText(item.Text, item.Name, languageCode);
                item.ToolTipText = TranslateOptionalId((item.Name ?? string.Empty) + ".tooltip", item.ToolTipText, languageCode);
                item.AccessibleName = TranslateOptionalId((item.Name ?? string.Empty) + ".accessibleName", item.AccessibleName, languageCode);
                item.AccessibleDescription = TranslateOptionalId((item.Name ?? string.Empty) + ".accessibleDescription", item.AccessibleDescription, languageCode);

                ToolStripDropDownItem dropDown = item as ToolStripDropDownItem;
                if (dropDown != null) ApplyToolStripItems(dropDown.DropDownItems, languageCode);
                ToolStripControlHost host = item as ToolStripControlHost;
                if (host != null && host.Control != null) ApplyControlTree(host.Control, languageCode);
            }
        }

    }
}
