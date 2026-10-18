using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PrototypeUltimateTool
{
    internal sealed class ApplicationLogFile
    {
        public string Path { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsCurrent { get; private set; }

        public ApplicationLogFile(string path, string displayName, bool isCurrent)
        {
            Path = path;
            DisplayName = displayName;
            IsCurrent = isCurrent;
        }

        public override string ToString() { return DisplayName; }
    }

    /// <summary>
    /// Per-process log session. The file is opened once and flushed after every entry so
    /// frequent diagnostics do not repeatedly open the file, and the viewer can tail it safely.
    /// </summary>
    internal static class ApplicationLog
    {
        private static readonly object Sync = new object();
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private static string _logDirectory;
        private static string _currentPath;
        private static DateTime _sessionStartedLocal;
        private static StreamWriter _writer;

        public static string CurrentLogPath
        {
            get
            {
                lock (Sync)
                {
                    try { EnsureSessionNoLock(); } catch { }
                    return _currentPath ?? Path.Combine(GetLogDirectory(), "session-log-unavailable.log");
                }
            }
        }

        public static void BeginSession()
        {
            // Logging is best-effort: a permissions/disk error must never prevent startup.
            lock (Sync) { try { EnsureSessionNoLock(); } catch { } }
        }

        public static void Write(string category, string message, Exception exception = null)
        {
            try
            {
                lock (Sync)
                {
                    EnsureSessionNoLock();
                    WriteLineNoLock("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "] [" +
                        (string.IsNullOrWhiteSpace(category) ? "APP" : category.Trim()) + "] " + (message ?? string.Empty));

                    if (exception != null)
                    {
                        int depth = 0;
                        for (Exception current = exception; current != null && depth < 16; current = current.InnerException, depth++)
                        {
                            WriteLineNoLock(string.Format(CultureInfo.InvariantCulture,
                                "Exception[{0}] Type={1}; HRESULT=0x{2:X8}; Message={3}", depth,
                                current.GetType().FullName ?? current.GetType().Name,
                                unchecked((uint)current.HResult), current.Message ?? string.Empty));
                        }
                        WriteLineNoLock(exception.ToString());
                    }
                }
            }
            catch
            {
                // Diagnostics must never crash or block the operation being diagnosed.
            }
        }

        public static List<ApplicationLogFile> GetAvailableLogFiles()
        {
            BeginSession();
            var paths = new List<string>();
            string directory = GetLogDirectory();
            try
            {
                if (Directory.Exists(directory))
                    paths.AddRange(Directory.GetFiles(directory, "session-*.log", SearchOption.TopDirectoryOnly));
            }
            catch { }

            string current = CurrentLogPath;
            if (!paths.Any(path => PathEquals(path, current))) paths.Add(current);

            // Preserve access to logs generated before per-session logging was introduced.
            string oldApplicationLog = Path.Combine(directory, "application.log");
            if (File.Exists(oldApplicationLog)) paths.Add(oldApplicationLog);
            string oldStartupLog = Path.Combine(Path.GetDirectoryName(directory) ?? directory, "startup.log");
            if (File.Exists(oldStartupLog)) paths.Add(oldStartupLog);

            var unique = paths.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            unique.Sort((left, right) =>
            {
                bool leftCurrent = PathEquals(left, current);
                bool rightCurrent = PathEquals(right, current);
                if (leftCurrent != rightCurrent) return leftCurrent ? -1 : 1;
                DateTime leftTime = SafeLastWriteTime(left);
                DateTime rightTime = SafeLastWriteTime(right);
                return rightTime.CompareTo(leftTime);
            });

            var result = new List<ApplicationLogFile>(unique.Count);
            foreach (string path in unique)
                result.Add(new ApplicationLogFile(path, BuildDisplayName(path, current), PathEquals(path, current)));
            return result;
        }

        /// <summary>Reads only the tail of a log so opening the viewer remains fast for old large logs.</summary>
        public static string ReadTail(string path, int maxBytes, int maxLines)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return "Журнал пока пуст.";
            maxBytes = Math.Max(4096, maxBytes);
            maxLines = Math.Max(100, maxLines);

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete, 32 * 1024, FileOptions.SequentialScan))
                {
                    long length = stream.Length;
                    long start = Math.Max(0, length - maxBytes);
                    bool byteTruncated = start > 0;
                    stream.Position = start;
                    var lines = new Queue<string>();
                    int droppedLines = 0;
                    using (var reader = new StreamReader(stream, Encoding.UTF8, true, 32 * 1024, false))
                    {
                        // If seeking into a UTF-8 file, the first line may start in the middle of a character/entry.
                        if (byteTruncated) reader.ReadLine();
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines.Enqueue(line);
                            if (lines.Count > maxLines)
                            {
                                lines.Dequeue();
                                droppedLines++;
                            }
                        }
                    }

                    var text = new StringBuilder(Math.Min((int)Math.Min(length, maxBytes), maxBytes) + 160);
                    if (byteTruncated || droppedLines > 0)
                    {
                        text.Append("[Показан хвост журнала: ограничение ")
                            .Append((maxBytes / 1024).ToString(CultureInfo.InvariantCulture)).Append(" KiB и ")
                            .Append(maxLines.ToString(CultureInfo.InvariantCulture)).Append(" строк. Для полного журнала используйте «Открыть файл журнала».")
                            .AppendLine("]");
                    }
                    foreach (string entry in lines) text.AppendLine(entry);
                    return text.Length == 0 ? "Журнал пока пуст." : text.ToString();
                }
            }
            catch (Exception ex)
            {
                return "Не удалось прочитать журнал: " + ex.Message;
            }
        }

        private static void EnsureSessionNoLock()
        {
            if (_writer != null) return;
            _logDirectory = GetLogDirectory();
            Directory.CreateDirectory(_logDirectory);
            _sessionStartedLocal = DateTime.Now;
            string fileName = "session-" + _sessionStartedLocal.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + "-" +
                Environment.ProcessId.ToString(CultureInfo.InvariantCulture) + ".log";
            _currentPath = Path.Combine(_logDirectory, fileName);
            var stream = new FileStream(_currentPath, FileMode.CreateNew, FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete, 16 * 1024, FileOptions.SequentialScan);
            _writer = new StreamWriter(stream, Utf8NoBom, 16 * 1024, false) { AutoFlush = true };
            WriteLineNoLock("[" + _sessionStartedLocal.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                "] [SESSION_START] PID=" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture) +
                "; Exe=" + Environment.ProcessPath + "; CWD=" + Environment.CurrentDirectory);
        }

        private static void WriteLineNoLock(string line)
        {
            if (_writer != null) _writer.WriteLine(line ?? string.Empty);
        }

        private static string GetLogDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PrototypeUltimateTool", "Logs");
        }

        private static string BuildDisplayName(string path, string currentPath)
        {
            string fileName = System.IO.Path.GetFileName(path);
            string name;
            if (PathEquals(path, currentPath))
            {
                name = "Текущий запуск — " + _sessionStartedLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (fileName.StartsWith("session-", StringComparison.OrdinalIgnoreCase))
            {
                name = "Запуск — " + TryGetSessionTimestamp(fileName);
            }
            else if (string.Equals(fileName, "application.log", StringComparison.OrdinalIgnoreCase))
            {
                name = "Старый объединённый журнал (application.log)";
            }
            else
            {
                name = "Старый журнал запуска (startup.log)";
            }

            try
            {
                long bytes = new FileInfo(path).Length;
                name += "  ·  " + (bytes < 1024 * 1024
                    ? (bytes / 1024.0).ToString("0.#", CultureInfo.InvariantCulture) + " KiB"
                    : (bytes / (1024.0 * 1024.0)).ToString("0.##", CultureInfo.InvariantCulture) + " MiB");
            }
            catch { }
            return name;
        }

        private static string TryGetSessionTimestamp(string fileName)
        {
            string stem = System.IO.Path.GetFileNameWithoutExtension(fileName);
            const string prefix = "session-";
            if (stem.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string timestamp = stem.Substring(prefix.Length);
                if (timestamp.Length >= 19) timestamp = timestamp.Substring(0, 19);
                DateTime parsed;
                if (DateTime.TryParseExact(timestamp, "yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out parsed))
                    return parsed.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            }
            return stem;
        }

        private static DateTime SafeLastWriteTime(string path)
        {
            try { return File.GetLastWriteTime(path); }
            catch { return DateTime.MinValue; }
        }

        private static bool PathEquals(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }
}
