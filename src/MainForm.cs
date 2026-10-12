using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gibbed.IO;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Nixson.Common;

namespace PrototypeUltimateTool
{
    public class MainForm : Form
    {
        private Pure3DFile _file;
        private string _currentFilePath;
        private Bitmap _currentSourceBmp;
        private bool _flipY = true;
        private bool _isDarkTheme = true;
        private bool _onlyTextures = false;

        private List<NodeCacheItem> _cachedNodes = new List<NodeCacheItem>();

        private MenuStrip menuStrip;
        private ToolStripMenuItem themeMenuItem;
        private ToolStrip mainToolStrip;
        private ToolStrip filterToolStrip;
        private SplitContainer mainSplit;
        private SplitContainer rightSplit;

        private TreeView nodeTreeView;
        private ContextMenuStrip treeContextMenu;
        private ToolStripTextBox searchTextBox;
        private ToolStripComboBox sortComboBox;
        private ToolStripButton onlyTexturesButton;

        private ToolStrip viewModeToolStrip;
        private ToolStripButton mode2DButton;
        private ToolStripButton mode3DButton;
        private Panel viewContainerPanel;

        private Panel panel2D;
        private ToolStrip channelToolStrip;
        private ToolStripButton rButton, gButton, bButton, aButton;
        private ToolStripLabel resolutionLabel;
        private PictureBox previewBox;

        private Model3DViewer viewer3D;
        private PropertyGrid propGrid;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private const int WM_SETREDRAW = 0x000B;

        public MainForm(string startupFile = null)
        {
            this.Text = "Prototype Ultimate Tool";
            this.Size = new Size(1366, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AllowDrop = true;

            this.LoadSettings();
            this.BuildUI();

            this.DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            this.DragDrop += (s, e) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0) OpenFile(files[0]);
            };

            this.Load += (s, e) =>
            {
                this.ResetSplitterProportions();
                this.ApplyTheme(this._isDarkTheme);

                // Автоматическое открытие файла при запуске через "Открыть с помощью"
                if (!string.IsNullOrEmpty(startupFile) && File.Exists(startupFile))
                {
                    this.OpenFile(startupFile);
                }
            };

            this.FormClosing += (s, e) => this.SaveSettings();
        }

        private static string GetConfigPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");

        private void SaveSettings()
        {
            try
            {
                string path = GetConfigPath();
                string content = string.Format("DarkTheme={0}\nFlipY={1}\nOnlyTextures={2}\n", this._isDarkTheme, this._flipY, this._onlyTextures);
                File.WriteAllText(path, content);
            }
            catch { }
        }

        private void LoadSettings()
        {
            try
            {
                string path = GetConfigPath();
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string val = parts[1].Trim();
                            if (key == "DarkTheme" && bool.TryParse(val, out bool dark)) this._isDarkTheme = dark;
                            if (key == "FlipY" && bool.TryParse(val, out bool flip)) this._flipY = flip;
                            if (key == "OnlyTextures" && bool.TryParse(val, out bool ot)) this._onlyTextures = ot;
                        }
                    }
                }
            }
            catch { }
        }

        private void ResetSplitterProportions()
        {
            if (this.ClientSize.Width > 200) this.mainSplit.SplitterDistance = Math.Max(320, this.ClientSize.Width / 3);
            if (this.rightSplit.ClientSize.Height > 200) this.rightSplit.SplitterDistance = (int)(this.rightSplit.ClientSize.Height * 0.65);
        }

        private void BuildUI()
        {
            // Меню
            this.menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Открыть (.p3d, .rz, .rcf)...", null, (s, e) => this.OnOpenDialog());
            fileMenu.DropDownItems.Add("Сохранить P3D", null, (s, e) => this.OnSaveFile(false));
            fileMenu.DropDownItems.Add("Сохранить P3D как...", null, (s, e) => this.OnSaveFile(true));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("📁 Экспорт всех текстур...", null, (s, e) => this.OnExportAllTextures());
            fileMenu.DropDownItems.Add("📦 Распаковать весь P3D целиком (Dump)...", null, (s, e) => this.OnDumpWholeP3D());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => Application.Exit());

            var rcfMenu = new ToolStripMenuItem("RCF Архивы");
            rcfMenu.DropDownItems.Add("Распаковать .rcf архив в папку...", null, (s, e) => this.OnUnpackRcf());
            rcfMenu.DropDownItems.Add("Собрать папку в .rcf архив...", null, (s, e) => this.OnPackRcf());

            var viewMenu = new ToolStripMenuItem("Вид");
            this.themeMenuItem = new ToolStripMenuItem(this._isDarkTheme ? "🌙 Тёмная тема" : "☀️ Светлая тема", null, (s, e) =>
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    this.ApplyTheme(!this._isDarkTheme);
                    this.SaveSettings();
                });
            })
            { CheckOnClick = true, Checked = this._isDarkTheme };

            viewMenu.DropDownItems.Add(this.themeMenuItem);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add("Сбросить пропорции (1/3 к 2/3)", null, (s, e) => this.ResetSplitterProportions());
            viewMenu.DropDownItems.Add("Развернуть дерево", null, (s, e) => this.ExpandTreeSafely(true));
            viewMenu.DropDownItems.Add("Свернуть дерево", null, (s, e) => this.ExpandTreeSafely(false));

            this.menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, rcfMenu, viewMenu });
            this.MainMenuStrip = this.menuStrip;

            var sharedRenderer = new ThemedToolStripRenderer(() => this._isDarkTheme);
            this.menuStrip.Renderer = sharedRenderer;

            // Верхний тулбар
            this.mainToolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Renderer = sharedRenderer };
            var openBtn = new ToolStripButton("📂 Открыть", null, (s, e) => this.OnOpenDialog());
            var saveBtn = new ToolStripButton("💾 Сохранить", null, (s, e) => this.OnSaveFile(false));
            var exportAllTexBtn = new ToolStripButton("📁 Экспорт текстур", null, (s, e) => this.OnExportAllTextures());
            exportAllTexBtn.Font = new Font(this.Font, FontStyle.Bold);
            var dumpAllBtn = new ToolStripButton("📦 Распаковать весь P3D", null, (s, e) => this.OnDumpWholeP3D());
            var exportSelBtn = new ToolStripButton("📤 Экспорт выделенного", null, (s, e) => this.OnExportSelected());
            var importBtn = new ToolStripButton("📥 Заменить...", null, (s, e) => this.OnReplaceTexture());
            var flipYBtn = new ToolStripButton("🔄 Отразить Y", null, (s, e) =>
            {
                this._flipY = !this._flipY;
                ((ToolStripButton)s).Checked = this._flipY;
                this.SaveSettings();
                this.ReloadPreview();
            })
            { CheckOnClick = true, Checked = this._flipY };

            this.mainToolStrip.Items.AddRange(new ToolStripItem[]
            {
                openBtn, saveBtn, new ToolStripSeparator(),
                exportAllTexBtn, dumpAllBtn, new ToolStripSeparator(),
                exportSelBtn, importBtn, new ToolStripSeparator(),
                flipYBtn
            });

            // Тулбар поиска слева
            this.filterToolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Renderer = sharedRenderer };
            this.searchTextBox = new ToolStripTextBox { Size = new Size(110, 25), ToolTipText = "Поиск по названию..." };
            this.searchTextBox.TextChanged += (s, e) => this.RefreshTree();
            var clearSearchBtn = new ToolStripButton("✕", null, (s, e) => this.searchTextBox.Text = "") { ToolTipText = "Очистить поиск" };

            this.sortComboBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(130, 25) };
            this.sortComboBox.Items.AddRange(new object[]
            {
                "По умолчанию",
                "По имени (А → Я)",
                "По имени (Я → А)",
                "По размеру (убыв.)",
                "По размеру (возр.)",
                "По разрешению (убыв.)",
                "По разрешению (возр.)",
                "По типу"
            });
            this.sortComboBox.SelectedIndex = 0;
            this.sortComboBox.SelectedIndexChanged += (s, e) => this.RefreshTree();

            this.onlyTexturesButton = new ToolStripButton("🖼️ Только текстуры", null, (s, e) =>
            {
                this._onlyTextures = !this._onlyTextures;
                this.onlyTexturesButton.Checked = this._onlyTextures;
                this.SaveSettings();
                this.RefreshTree();
            })
            { CheckOnClick = true, Checked = this._onlyTextures };

            this.filterToolStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripLabel("🔍"), this.searchTextBox, clearSearchBtn,
                new ToolStripSeparator(),
                this.sortComboBox,
                new ToolStripSeparator(),
                this.onlyTexturesButton
            });

            // Контекстное меню дерева
            this.treeContextMenu = new ContextMenuStrip();
            this.treeContextMenu.Items.Add("📤 Экспортировать этот элемент...", null, (s, e) => this.OnExportSelected());
            this.treeContextMenu.Items.Add("📥 Заменить данные элемента...", null, (s, e) => this.OnReplaceTexture());

            // Дерево нод
            this.nodeTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                HideSelection = false,
                ContextMenuStrip = this.treeContextMenu
            };
            typeof(TreeView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, this.nodeTreeView, new object[] { true });

            this.nodeTreeView.AfterSelect += (s, e) => this.OnNodeSelected(e.Node != null ? e.Node.Tag as BaseNode : null);
            this.nodeTreeView.BeforeExpand += (s, e) => this.OnNodeBeforeExpand(e.Node);

            // Переключатель 2D / 3D
            this.viewModeToolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Renderer = sharedRenderer, Dock = DockStyle.Top };
            this.mode2DButton = new ToolStripButton("🖼️ 2D Текстура", null, (s, e) => this.SwitchViewMode(false)) { Font = new Font(this.Font, FontStyle.Bold), Checked = true };
            this.mode3DButton = new ToolStripButton("🧊 3D Модель", null, (s, e) => this.SwitchViewMode(true)) { Font = new Font(this.Font, FontStyle.Bold) };

            this.viewModeToolStrip.Items.AddRange(new ToolStripItem[] { this.mode2DButton, this.mode3DButton });

            // 2D Viewer
            this.panel2D = new Panel { Dock = DockStyle.Fill };
            this.channelToolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Renderer = sharedRenderer, Dock = DockStyle.Top };
            var rgbaPreset = new ToolStripButton("RGBA", null, (s, e) => this.SetChannels(true, true, true, true)) { Font = new Font(this.Font, FontStyle.Bold) };
            var rgbPreset = new ToolStripButton("RGB", null, (s, e) => this.SetChannels(true, true, true, false));

            this.rButton = new ToolStripButton("R", null, (s, e) => this.ApplyChannels()) { CheckOnClick = true, Checked = true, ForeColor = Color.IndianRed, Font = new Font(this.Font, FontStyle.Bold) };
            this.gButton = new ToolStripButton("G", null, (s, e) => this.ApplyChannels()) { CheckOnClick = true, Checked = true, ForeColor = Color.LightGreen, Font = new Font(this.Font, FontStyle.Bold) };
            this.bButton = new ToolStripButton("B", null, (s, e) => this.ApplyChannels()) { CheckOnClick = true, Checked = true, ForeColor = Color.DodgerBlue, Font = new Font(this.Font, FontStyle.Bold) };
            this.aButton = new ToolStripButton("A", null, (s, e) => this.ApplyChannels()) { CheckOnClick = true, Checked = true, ForeColor = Color.LightGray, Font = new Font(this.Font, FontStyle.Bold) };
            this.resolutionLabel = new ToolStripLabel { Alignment = ToolStripItemAlignment.Right };

            this.channelToolStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripLabel("Пресет:"), rgbaPreset, rgbPreset,
                new ToolStripSeparator(),
                new ToolStripLabel("Каналы:"), this.rButton, this.gButton, this.bButton, this.aButton,
                new ToolStripSeparator(), this.resolutionLabel
            });

            this.previewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.Fixed3D
            };

            this.panel2D.Controls.Add(this.previewBox);
            this.panel2D.Controls.Add(this.channelToolStrip);

            // 3D Viewer
            this.viewer3D = new Model3DViewer { Dock = DockStyle.Fill, Visible = false };

            this.viewContainerPanel = new Panel { Dock = DockStyle.Fill };
            this.viewContainerPanel.Controls.Add(this.panel2D);
            this.viewContainerPanel.Controls.Add(this.viewer3D);

            this.propGrid = new PropertyGrid { Dock = DockStyle.Fill, HelpVisible = false, ToolbarVisible = false };

            var leftPanel = new Panel { Dock = DockStyle.Fill };
            leftPanel.Controls.Add(this.nodeTreeView);
            leftPanel.Controls.Add(this.filterToolStrip);

            var topViewPanel = new Panel { Dock = DockStyle.Fill };
            topViewPanel.Controls.Add(this.viewContainerPanel);
            topViewPanel.Controls.Add(this.viewModeToolStrip);

            this.rightSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 480,
                SplitterWidth = 5
            };
            this.rightSplit.Panel1.Controls.Add(topViewPanel);
            this.rightSplit.Panel2.Controls.Add(this.propGrid);

            this.mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 380,
                SplitterWidth = 5,
                FixedPanel = FixedPanel.Panel1
            };
            this.mainSplit.Panel1.Controls.Add(leftPanel);
            this.mainSplit.Panel2.Controls.Add(this.rightSplit);

            this.mainSplit.Paint += (s, e) => this.PaintSplitter(s as SplitContainer, e.Graphics);
            this.rightSplit.Paint += (s, e) => this.PaintSplitter(s as SplitContainer, e.Graphics);

            this.Controls.Add(this.mainSplit);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.menuStrip);
        }

        private void SwitchViewMode(bool is3D)
        {
            this.mode2DButton.Checked = !is3D;
            this.mode3DButton.Checked = is3D;
            this.panel2D.Visible = !is3D;
            this.viewer3D.Visible = is3D;
        }

        private void PaintSplitter(SplitContainer split, Graphics g)
        {
            if (split != null)
            {
                Color color = this._isDarkTheme ? Color.FromArgb(40, 40, 42) : SystemColors.Control;
                using (SolidBrush b = new SolidBrush(color)) g.FillRectangle(b, split.SplitterRectangle);
            }
        }

        private void ApplyTheme(bool isDark)
        {
            this._isDarkTheme = isDark;
            this.themeMenuItem.Checked = isDark;
            this.themeMenuItem.Text = isDark ? "🌙 Тёмная тема" : "☀️ Светлая тема";

            try
            {
                int darkMode = isDark ? 1 : 0;
                if (DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));
                }
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            }
            catch { }

            Color bgMain = isDark ? Color.FromArgb(28, 28, 28) : SystemColors.Control;
            Color bgPanel = isDark ? Color.FromArgb(37, 37, 38) : SystemColors.Window;
            Color bgInput = isDark ? Color.FromArgb(50, 50, 50) : SystemColors.Window;
            Color fgText = isDark ? Color.FromArgb(220, 220, 220) : SystemColors.ControlText;
            Color border = isDark ? Color.FromArgb(50, 50, 52) : SystemColors.ControlDark;

            this.BackColor = bgMain;
            this.ForeColor = fgText;

            this.menuStrip.BackColor = isDark ? Color.FromArgb(40, 40, 42) : SystemColors.Control;
            this.mainToolStrip.BackColor = isDark ? Color.FromArgb(40, 40, 42) : SystemColors.Control;
            this.filterToolStrip.BackColor = isDark ? Color.FromArgb(32, 32, 34) : SystemColors.Control;
            this.viewModeToolStrip.BackColor = isDark ? Color.FromArgb(32, 32, 34) : SystemColors.Control;
            this.channelToolStrip.BackColor = isDark ? Color.FromArgb(32, 32, 34) : SystemColors.Control;
            this.treeContextMenu.BackColor = bgPanel;
            this.treeContextMenu.ForeColor = fgText;

            this.searchTextBox.BackColor = bgInput;
            this.searchTextBox.ForeColor = fgText;
            this.sortComboBox.BackColor = bgInput;
            this.sortComboBox.ForeColor = fgText;

            this.nodeTreeView.BackColor = isDark ? Color.FromArgb(24, 24, 24) : SystemColors.Window;
            this.nodeTreeView.ForeColor = fgText;
            this.nodeTreeView.LineColor = isDark ? Color.FromArgb(70, 70, 70) : SystemColors.ControlDark;

            this.mainSplit.BackColor = border;
            this.rightSplit.BackColor = border;

            this.propGrid.ViewBackColor = bgPanel;
            this.propGrid.ViewForeColor = fgText;
            this.propGrid.LineColor = border;
            this.propGrid.CategoryForeColor = isDark ? Color.FromArgb(86, 156, 214) : SystemColors.HotTrack;

            this.previewBox.BackColor = isDark ? Color.FromArgb(16, 16, 16) : Color.FromArgb(235, 235, 235);
            this.resolutionLabel.ForeColor = isDark ? Color.FromArgb(160, 160, 160) : SystemColors.GrayText;

            this.viewer3D.ApplyTheme(isDark);
            this.Invalidate(true);
        }

        public void OpenFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            string ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext == ".rcf")
            {
                if (MessageBox.Show("Вы открыли архив RCF игры. Распаковать его содержимое в папку?", "RCF Архив", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (ModernFolderBrowserDialog mfbd = new ModernFolderBrowserDialog())
                    {
                        mfbd.Title = "Куда распаковать RCF архив?";
                        if (mfbd.ShowDialog(this) == DialogResult.OK)
                        {
                            using (FileStream fs = File.OpenRead(path)) new CementFile(fs).Unpack(mfbd.SelectedPath);
                            MessageBox.Show("Архив успешно распакован!", "RCF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                return;
            }

            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    this._file = new Pure3DFile { GamePicked = PrototypeGame.Any };
                    Stream stream = fs;

                    if (Utils.IsRZFile(path))
                    {
                        stream = new MemoryStream();
                        Utils.DecompressRZ(fs, stream);
                        stream.Position = 0;
                    }

                    this._file.Deserialize(stream, Endian.Little);
                    if (stream != fs) stream.Dispose();
                }

                this._currentFilePath = path;
                this.Text = "Prototype Ultimate Tool - " + Path.GetFileName(path);

                this.CacheNodesMetadata();
                this.RefreshTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия файла: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CacheNodesMetadata()
        {
            this._cachedNodes.Clear();
            if (this._file == null || this._file.Nodes == null) return;

            foreach (BaseNode node in this._file.Nodes)
            {
                this._cachedNodes.Add(new NodeCacheItem(node));
            }
        }

        private void RefreshTree()
        {
            SendMessage(this.nodeTreeView.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            this.nodeTreeView.BeginUpdate();
            this.nodeTreeView.Nodes.Clear();

            if (this._cachedNodes != null && this._cachedNodes.Count > 0)
            {
                IEnumerable<NodeCacheItem> list = this._cachedNodes;

                if (this._onlyTextures) list = list.Where(item => item.IsTexture);

                string query = this.searchTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(query)) list = list.Where(item => item.Matches(query));

                switch (this.sortComboBox.SelectedIndex)
                {
                    case 1: list = list.OrderBy(n => n.SortName, StringComparer.OrdinalIgnoreCase); break;
                    case 2: list = list.OrderByDescending(n => n.SortName, StringComparer.OrdinalIgnoreCase); break;
                    case 3: list = list.OrderByDescending(n => n.Size); break;
                    case 4: list = list.OrderBy(n => n.Size); break;
                    case 5: list = list.OrderByDescending(n => n.PixelCount); break;
                    case 6: list = list.OrderBy(n => n.PixelCount); break;
                    case 7: list = list.OrderBy(n => n.Extension, StringComparer.OrdinalIgnoreCase); break;
                }

                List<TreeNode> treeNodes = new List<TreeNode>();
                foreach (NodeCacheItem item in list)
                {
                    TreeNode tn = new TreeNode(item.DisplayText) { Tag = item.Node };
                    if (item.Node.Children != null && item.Node.Children.Count > 0)
                    {
                        tn.Nodes.Add(new TreeNode { Tag = null, Text = "Загрузка..." });
                    }
                    treeNodes.Add(tn);
                }

                this.nodeTreeView.Nodes.AddRange(treeNodes.ToArray());
            }

            this.nodeTreeView.EndUpdate();
            SendMessage(this.nodeTreeView.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            this.nodeTreeView.Invalidate();
        }

        private void OnNodeBeforeExpand(TreeNode node)
        {
            if (node == null || node.Nodes.Count == 0) return;

            if (node.Nodes[0].Tag == null)
            {
                node.Nodes.Clear();
                BaseNode baseNode = node.Tag as BaseNode;
                if (baseNode != null && baseNode.Children != null)
                {
                    List<TreeNode> children = new List<TreeNode>();
                    foreach (BaseNode child in baseNode.Children)
                    {
                        TreeNode childTn = new TreeNode(child.ToString()) { Tag = child };
                        if (child.Children != null && child.Children.Count > 0)
                        {
                            childTn.Nodes.Add(new TreeNode { Tag = null, Text = "Загрузка..." });
                        }
                        children.Add(childTn);
                    }
                    node.Nodes.AddRange(children.ToArray());
                }
            }
        }

        private void ExpandTreeSafely(bool expand)
        {
            SendMessage(this.nodeTreeView.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            this.nodeTreeView.BeginUpdate();

            if (expand)
            {
                foreach (TreeNode tn in this.nodeTreeView.Nodes) this.LoadAllChildNodesRecursively(tn);
                this.nodeTreeView.ExpandAll();
            }
            else
            {
                this.nodeTreeView.CollapseAll();
            }

            this.nodeTreeView.EndUpdate();
            SendMessage(this.nodeTreeView.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            this.nodeTreeView.Invalidate();
        }

        private void LoadAllChildNodesRecursively(TreeNode node)
        {
            if (node.Nodes.Count > 0 && node.Nodes[0].Tag == null) this.OnNodeBeforeExpand(node);
            foreach (TreeNode child in node.Nodes) this.LoadAllChildNodesRecursively(child);
        }

        private void OnNodeSelected(BaseNode node)
        {
            this.propGrid.SelectedObject = node;

            if (this._currentSourceBmp != null)
            {
                this._currentSourceBmp.Dispose();
                this._currentSourceBmp = null;
            }

            if (node == null)
            {
                this.ApplyChannels();
                return;
            }

            // 1. Проверяем 3D модель или скелет
            bool is3D = this.viewer3D.LoadNode(node, this._file);
            if (is3D)
            {
                this.SwitchViewMode(true);
            }
            else
            {
                // 2. Иначе пробуем 2D текстуру
                this._currentSourceBmp = DdsDecoder.ExtractBitmap(node, this._flipY);
                this.ApplyChannels();
                if (this._currentSourceBmp != null)
                {
                    this.SwitchViewMode(false);
                }
            }
        }

        private void ReloadPreview()
        {
            var node = this.nodeTreeView.SelectedNode != null ? this.nodeTreeView.SelectedNode.Tag as BaseNode : null;
            this.OnNodeSelected(node);
        }

        private void SetChannels(bool r, bool g, bool b, bool a)
        {
            this.rButton.Checked = r;
            this.gButton.Checked = g;
            this.bButton.Checked = b;
            this.aButton.Checked = a;
            this.ApplyChannels();
        }

        private void ApplyChannels()
        {
            if (this.previewBox.Image != null)
            {
                this.previewBox.Image.Dispose();
                this.previewBox.Image = null;
            }

            if (this._currentSourceBmp == null)
            {
                this.resolutionLabel.Text = "";
                return;
            }

            this.previewBox.Image = DdsDecoder.ProcessChannels(
                this._currentSourceBmp,
                this.rButton.Checked,
                this.gButton.Checked,
                this.bButton.Checked,
                this.aButton.Checked
            );

            this.resolutionLabel.Text = $"{this._currentSourceBmp.Width} × {this._currentSourceBmp.Height}";
        }

        private void OnOpenDialog()
        {
            using (OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Все форматы Prototype (*.p3d;*.rz;*.rcf)|*.p3d;*.rz;*.rcf|Pure3D (*.p3d)|*.p3d|Архивы RCF (*.rcf)|*.rcf|Все файлы (*.*)|*.*"
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK) OpenFile(ofd.FileName);
            }
        }

        private void OnSaveFile(bool saveAs)
        {
            if (this._file == null) return;
            string targetPath = this._currentFilePath;

            if (saveAs || string.IsNullOrEmpty(targetPath))
            {
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "Pure3D Files (*.p3d;*.rz)|*.p3d;*.rz|All Files (*.*)|*.*" })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    targetPath = sfd.FileName;
                }
            }

            try
            {
                using (FileStream fs = File.Create(targetPath))
                {
                    Stream outStream = new MemoryStream();
                    this._file.Serialize(outStream, Endian.Little);
                    outStream.Position = 0;

                    if (Utils.IsRZFile(targetPath))
                    {
                        using (Stream compressed = Utils.CompressRZ(outStream))
                        {
                            compressed.Position = 0;
                            compressed.CopyTo(fs);
                        }
                    }
                    else
                    {
                        outStream.CopyTo(fs);
                    }
                    outStream.Dispose();
                }

                this._currentFilePath = targetPath;
                this.Text = "Prototype Ultimate Tool - " + Path.GetFileName(targetPath);
                MessageBox.Show("Файл успешно сохранен!", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 1. Экспорт всех текстур
        private void OnExportAllTextures()
        {
            if (this._file == null || this._file.Nodes == null) return;

            using (ModernFolderBrowserDialog mfbd = new ModernFolderBrowserDialog { Title = "Выберите папку для экспорта текстур" })
            {
                if (mfbd.ShowDialog(this) != DialogResult.OK) return;

                List<BaseNode> exportable = new List<BaseNode>();
                CollectExportable(this._file.Nodes, exportable);

                int count = 0;
                HashSet<string> used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (BaseNode node in exportable)
                {
                    try
                    {
                        string cleanName = GetSafeFileName(node);
                        string fullPath = Path.Combine(mfbd.SelectedPath, cleanName);

                        int c = 1;
                        int dot = cleanName.LastIndexOf('.');
                        string baseName = (dot >= 0) ? cleanName.Substring(0, dot) : cleanName;
                        string ext = (dot >= 0) ? cleanName.Substring(dot) : "";

                        while (used.Contains(fullPath) || File.Exists(fullPath))
                        {
                            fullPath = Path.Combine(mfbd.SelectedPath, $"{baseName}_{c}{ext}");
                            c++;
                        }
                        used.Add(fullPath);

                        using (FileStream fs = File.Create(fullPath)) node.Export(fs);
                        count++;
                    }
                    catch { }
                }

                MessageBox.Show($"Экспорт завершен!\nУспешно сохранено текстур: {count}", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 2. Экспорт выделенного узла (включая экспорт моделей в .OBJ для Blender!)
        private void OnExportSelected()
        {
            var node = this.nodeTreeView.SelectedNode != null ? this.nodeTreeView.SelectedNode.Tag as BaseNode : null;
            if (node == null)
            {
                MessageBox.Show("Сначала выделите элемент в дереве слева.", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nodeStr = node.ToString() ?? "asset";
            string name = GetCleanName(node);

            // Если выделена 3D модель (PolySkin / CompositeDrawable / PrimitiveGroup) -> Экспорт в .OBJ для Blender!
            bool isMesh = (node is PolySkin || node is CompositeDrawable || node is PrimitiveGroup);
            if (isMesh)
            {
                using (SaveFileDialog sfd = new SaveFileDialog
                {
                    FileName = name + ".obj",
                    Filter = "Wavefront 3D (.obj для Blender)|*.obj|Pure3D (.p3d)|*.p3d|Все файлы (*.*)|*.*"
                })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;

                    if (sfd.FileName.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
                    {
                        bool ok = ExportNodeToObj(node, this._file, sfd.FileName);
                        if (ok) MessageBox.Show("3D-модель успешно экспортирована в .OBJ!\nТеперь её можно открыть в Blender.", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else MessageBox.Show("Не удалось извлечь полигоны этой модели.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            // Если выделена текстура
            if (node.Exportable)
            {
                using (SaveFileDialog sfd = new SaveFileDialog
                {
                    FileName = GetSafeFileName(node),
                    Filter = "DDS Textures (*.dds)|*.dds|Все файлы (*.*)|*.*"
                })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    using (FileStream fs = File.Create(sfd.FileName)) node.Export(fs);
                    MessageBox.Show("Текстура успешно сохранена!", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            // Резервный экспорт ноды в отдельный P3D
            using (SaveFileDialog sfd = new SaveFileDialog { FileName = name + ".p3d", Filter = "Pure3D Node (*.p3d)|*.p3d" })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                var singleP3d = new Pure3DFile();
                singleP3d.Nodes.Add(node);
                using (FileStream fs = File.Create(sfd.FileName)) singleP3d.Serialize(fs, Endian.Little);
                MessageBox.Show("Узел сохранен как P3D!", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 3. Полная распаковка P3D целиком по папкам (Dump)
        private void OnDumpWholeP3D()
        {
            if (this._file == null || this._file.Nodes == null) return;

            using (ModernFolderBrowserDialog mfbd = new ModernFolderBrowserDialog { Title = "Куда распаковать всё содержимое P3D?" })
            {
                if (mfbd.ShowDialog(this) != DialogResult.OK) return;

                string outDir = mfbd.SelectedPath;
                string texDir = Path.Combine(outDir, "Textures");
                string objDir = Path.Combine(outDir, "Models_OBJ");
                string otherDir = Path.Combine(outDir, "Other_P3D");

                Directory.CreateDirectory(texDir);
                Directory.CreateDirectory(objDir);
                Directory.CreateDirectory(otherDir);

                int texCount = 0, modelCount = 0, otherCount = 0;

                // 1. Извлекаем текстуры
                List<BaseNode> exportable = new List<BaseNode>();
                CollectExportable(this._file.Nodes, exportable);
                foreach (var texNode in exportable)
                {
                    try
                    {
                        string safeName = GetSafeFileName(texNode);
                        using (FileStream fs = File.Create(Path.Combine(texDir, safeName))) texNode.Export(fs);
                        texCount++;
                    }
                    catch { }
                }

                // 2. Извлекаем 3D-модели в OBJ для Blender
                foreach (var node in this._file.Nodes)
                {
                    if (node is PolySkin || node is CompositeDrawable)
                    {
                        string cleanName = GetCleanName(node);
                        string objPath = Path.Combine(objDir, cleanName + ".obj");
                        if (ExportNodeToObj(node, this._file, objPath)) modelCount++;
                    }
                    else if (!exportable.Contains(node))
                    {
                        try
                        {
                            string safeName = GetCleanName(node) + ".p3d";
                            var singleP3d = new Pure3DFile();
                            singleP3d.Nodes.Add(node);
                            using (FileStream fs = File.Create(Path.Combine(otherDir, safeName))) singleP3d.Serialize(fs, Endian.Little);
                            otherCount++;
                        }
                        catch { }
                    }
                }

                MessageBox.Show(string.Format("Полный дамп P3D завершён!\n\n📁 Текстур (DDS): {0}\n🧊 3D-моделей (OBJ для Blender): {1}\n📦 Прочих узлов: {2}", texCount, modelCount, otherCount), "Распаковка P3D", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Конвертер 3D-нод в формат Wavefront OBJ (сразу открывается в Blender)
        private static bool ExportNodeToObj(BaseNode node, Pure3DFile p3d, string filePath)
        {
            try
            {
                List<PrimitiveGroup> groups = new List<PrimitiveGroup>();
                if (node is PrimitiveGroup pg) groups.Add(pg);
                else if (node is PolySkin ps) CollectPrimitiveGroupsRecursively(ps, groups);
                else if (node is CompositeDrawable cd)
                {
                    CollectPrimitiveGroupsRecursively(cd, groups);
                    if (groups.Count == 0 && p3d != null)
                    {
                        string cdName = GetCleanName(cd);
                        foreach (var n in p3d.Nodes)
                        {
                            if (n is PolySkin skin && GetCleanName(skin).IndexOf(cdName, StringComparison.OrdinalIgnoreCase) >= 0)
                                CollectPrimitiveGroupsRecursively(skin, groups);
                        }
                    }
                }

                if (groups.Count == 0) return false;

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine("# Prototype Ultimate Tool - 3D OBJ Export");
                    sw.WriteLine("# Model: " + GetCleanName(node));

                    int vertOffset = 1;
                    int groupIndex = 1;

                    foreach (var g in groups)
                    {
                        var rawVerts = Nixson.Prototype1.ImportP3D.GetPositionVertices(g);
                        var rawFaces = Nixson.Prototype1.ImportP3D.GetFaces(g);
                        if (rawVerts == null || rawFaces == null || rawVerts.Count == 0 || rawFaces.Count == 0) continue;

                        sw.WriteLine("o Part_" + groupIndex);

                        foreach (var v in rawVerts)
                        {
                            sw.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "v {0:F4} {1:F4} {2:F4}", v.X * 1000f, v.Z * -1000f, v.Y * 1000f));
                        }

                        foreach (var f in rawFaces)
                        {
                            sw.WriteLine(string.Format("f {0} {1} {2}", f.Point1 + vertOffset, f.Point3 + vertOffset, f.Point2 + vertOffset));
                        }

                        vertOffset += rawVerts.Count;
                        groupIndex++;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnReplaceTexture()
        {
            var node = this.nodeTreeView.SelectedNode != null ? this.nodeTreeView.SelectedNode.Tag as BaseNode : null;
            if (node == null || !node.Importable)
            {
                MessageBox.Show("Выберите элемент, поддерживающий замену данных (например, текстуру).", "Импорт", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "DDS Textures (*.dds)|*.dds|Все файлы (*.*)|*.*" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (FileStream fs = File.OpenRead(ofd.FileName)) node.Import(fs);
                    this.ReloadPreview();
                    MessageBox.Show("Данные успешно заменены! Не забудьте сохранить P3D файл.", "Импорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка замены: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnUnpackRcf()
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "RCF Files (*.rcf)|*.rcf" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                using (ModernFolderBrowserDialog mfbd = new ModernFolderBrowserDialog { Title = "Куда распаковать архив?" })
                {
                    if (mfbd.ShowDialog(this) != DialogResult.OK) return;
                    using (FileStream fs = File.OpenRead(ofd.FileName)) new CementFile(fs).Unpack(mfbd.SelectedPath);
                    MessageBox.Show("Архив RCF успешно распакован!", "RCF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnPackRcf()
        {
            using (ModernFolderBrowserDialog mfbd = new ModernFolderBrowserDialog { Title = "Выберите папку для сборки в RCF" })
            {
                if (mfbd.ShowDialog(this) != DialogResult.OK) return;
                using (SaveFileDialog sfd = new SaveFileDialog { Filter = "RCF Files (*.rcf)|*.rcf" })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    CementFile cement = new CementFile { Endian = Endian.Little, MajorVersion = 2, MinorVersion = 1 };
                    cement.Pack(mfbd.SelectedPath);
                    using (FileStream fs = File.Create(sfd.FileName)) cement.Serialize(fs);
                    MessageBox.Show("Архив RCF успешно собран!", "RCF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private static void CollectExportable(IEnumerable<BaseNode> nodes, List<BaseNode> result)
        {
            if (nodes == null) return;
            foreach (var n in nodes)
            {
                if (n.Exportable) result.Add(n);
                else if (n.Children != null) CollectExportable(n.Children, result);
            }
        }

        private static void CollectPrimitiveGroupsRecursively(BaseNode node, List<PrimitiveGroup> list)
        {
            if (node == null) return;
            if (node is PrimitiveGroup pg) { list.Add(pg); return; }
            if (node.Children != null)
            {
                foreach (var c in node.Children) CollectPrimitiveGroupsRecursively(c, list);
            }
        }

        private static string GetSafeFileName(BaseNode node)
        {
            string name = GetSortNameStatic(node);
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            string ext = node.ExportExtension ?? "dds";
            if (!ext.StartsWith(".")) ext = "." + ext;
            if (!name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) name += ext;
            return name;
        }

        private static string GetSortNameStatic(BaseNode node)
        {
            if (node == null) return "";
            var p = node.GetType().GetProperty("Name");
            if (p != null && p.GetValue(node, null) is string n && !string.IsNullOrWhiteSpace(n)) return n;
            string s = node.ToString() ?? "";
            int o = s.IndexOf('('), c = s.LastIndexOf(')');
            return (o >= 0 && c > o) ? s.Substring(o + 1, c - o - 1) : s;
        }

        private static string GetCleanName(BaseNode node) => GetSortNameStatic(node);

        private class NodeCacheItem
        {
            public BaseNode Node { get; }
            public string DisplayText { get; }
            public string SortName { get; }
            public string Extension { get; }
            public long Size { get; }
            public long PixelCount { get; }
            public bool IsTexture { get; }

            public NodeCacheItem(BaseNode node)
            {
                this.Node = node;
                this.DisplayText = node.ToString();
                this.SortName = GetSortNameStatic(node);

                int d = this.SortName.LastIndexOf('.');
                this.Extension = (d >= 0) ? this.SortName.Substring(d).ToLowerInvariant() : "";

                long size = 0;
                var pSize = node.GetType().GetProperty("TotalSize");
                if (pSize != null)
                {
                    try { size = Convert.ToInt64(pSize.GetValue(node, null)); } catch { }
                }
                this.Size = size;

                int w = 0, h = 0;
                var pw = node.GetType().GetProperty("Width");
                var ph = node.GetType().GetProperty("Height");
                if (pw != null && ph != null)
                {
                    try
                    {
                        w = Convert.ToInt32(pw.GetValue(node, null));
                        h = Convert.ToInt32(ph.GetValue(node, null));
                    }
                    catch { }
                }
                this.PixelCount = (long)w * h;

                string text = this.DisplayText.ToLowerInvariant();
                this.IsTexture = node.Exportable || text.Contains("texture") || text.Contains(".dds") || this.PixelCount > 0;
            }

            public bool Matches(string query)
            {
                return this.DisplayText.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                       this.SortName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private class ThemedColorTable : ProfessionalColorTable
        {
            private readonly Func<bool> _isDark;
            public ThemedColorTable(Func<bool> isDark) => _isDark = isDark;

            public override Color ToolStripDropDownBackground => _isDark() ? Color.FromArgb(37, 37, 38) : base.ToolStripDropDownBackground;
            public override Color ImageMarginGradientBegin => _isDark() ? Color.FromArgb(37, 37, 38) : base.ImageMarginGradientBegin;
            public override Color ImageMarginGradientMiddle => _isDark() ? Color.FromArgb(37, 37, 38) : base.ImageMarginGradientMiddle;
            public override Color ImageMarginGradientEnd => _isDark() ? Color.FromArgb(37, 37, 38) : base.ImageMarginGradientEnd;
            public override Color MenuBorder => _isDark() ? Color.FromArgb(63, 63, 70) : base.MenuBorder;
            public override Color MenuItemBorder => _isDark() ? Color.FromArgb(63, 63, 70) : base.MenuItemBorder;
            public override Color MenuItemSelected => _isDark() ? Color.FromArgb(62, 62, 64) : base.MenuItemSelected;
            public override Color MenuStripGradientBegin => _isDark() ? Color.FromArgb(40, 40, 42) : base.MenuStripGradientBegin;
            public override Color MenuStripGradientEnd => _isDark() ? Color.FromArgb(40, 40, 42) : base.MenuStripGradientEnd;
            public override Color MenuItemSelectedGradientBegin => _isDark() ? Color.FromArgb(62, 62, 64) : base.MenuItemSelectedGradientBegin;
            public override Color MenuItemSelectedGradientEnd => _isDark() ? Color.FromArgb(62, 62, 64) : base.MenuItemSelectedGradientEnd;
            public override Color MenuItemPressedGradientBegin => _isDark() ? Color.FromArgb(28, 28, 30) : base.MenuItemPressedGradientBegin;
            public override Color MenuItemPressedGradientEnd => _isDark() ? Color.FromArgb(28, 28, 30) : base.MenuItemPressedGradientEnd;
            public override Color ToolStripBorder => _isDark() ? Color.FromArgb(40, 40, 42) : base.ToolStripBorder;
            public override Color ToolStripGradientBegin => _isDark() ? Color.FromArgb(40, 40, 42) : base.ToolStripGradientBegin;
            public override Color ToolStripGradientMiddle => _isDark() ? Color.FromArgb(40, 40, 42) : base.ToolStripGradientMiddle;
            public override Color ToolStripGradientEnd => _isDark() ? Color.FromArgb(40, 40, 42) : base.ToolStripGradientEnd;
            public override Color SeparatorDark => _isDark() ? Color.FromArgb(60, 60, 65) : base.SeparatorDark;
            public override Color SeparatorLight => _isDark() ? Color.FromArgb(40, 40, 42) : base.SeparatorLight;
            public override Color ButtonSelectedHighlight => _isDark() ? Color.FromArgb(62, 62, 64) : base.ButtonSelectedHighlight;
            public override Color ButtonSelectedGradientBegin => _isDark() ? Color.FromArgb(62, 62, 64) : base.ButtonSelectedGradientBegin;
            public override Color ButtonSelectedGradientMiddle => _isDark() ? Color.FromArgb(62, 62, 64) : base.ButtonSelectedGradientMiddle;
            public override Color ButtonSelectedGradientEnd => _isDark() ? Color.FromArgb(62, 62, 64) : base.ButtonSelectedGradientEnd;
            public override Color ButtonPressedHighlight => _isDark() ? Color.FromArgb(28, 28, 30) : base.ButtonPressedHighlight;
            public override Color ButtonPressedGradientBegin => _isDark() ? Color.FromArgb(28, 28, 30) : base.ButtonPressedGradientBegin;
            public override Color ButtonPressedGradientMiddle => _isDark() ? Color.FromArgb(28, 28, 30) : base.ButtonPressedGradientMiddle;
            public override Color ButtonPressedGradientEnd => _isDark() ? Color.FromArgb(28, 28, 30) : base.ButtonPressedGradientEnd;
            public override Color CheckBackground => _isDark() ? Color.FromArgb(0, 122, 204) : base.CheckBackground;
            public override Color CheckSelectedBackground => _isDark() ? Color.FromArgb(0, 122, 204) : base.CheckSelectedBackground;
            public override Color ButtonCheckedGradientBegin => _isDark() ? Color.FromArgb(0, 122, 204) : base.ButtonCheckedGradientBegin;
            public override Color ButtonCheckedGradientMiddle => _isDark() ? Color.FromArgb(0, 122, 204) : base.ButtonCheckedGradientMiddle;
            public override Color ButtonCheckedGradientEnd => _isDark() ? Color.FromArgb(0, 122, 204) : base.ButtonCheckedGradientEnd;
        }

        private class ThemedToolStripRenderer : ToolStripProfessionalRenderer
        {
            private readonly Func<bool> _isDark;
            public ThemedToolStripRenderer(Func<bool> isDark) : base(new ThemedColorTable(isDark)) => _isDark = isDark;

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (_isDark())
                {
                    if (e.Item.ForeColor == Color.Firebrick || e.Item.ForeColor == Color.IndianRed)
                        e.TextColor = Color.IndianRed;
                    else if (e.Item.ForeColor == Color.ForestGreen || e.Item.ForeColor == Color.LightGreen)
                        e.TextColor = Color.LightGreen;
                    else if (e.Item.ForeColor == Color.RoyalBlue || e.Item.ForeColor == Color.DodgerBlue)
                        e.TextColor = Color.DodgerBlue;
                    else
                        e.TextColor = Color.FromArgb(220, 220, 220);
                }
                else
                {
                    if (e.Item.ForeColor == Color.Firebrick || e.Item.ForeColor == Color.IndianRed)
                        e.TextColor = Color.Firebrick;
                    else if (e.Item.ForeColor == Color.ForestGreen || e.Item.ForeColor == Color.LightGreen)
                        e.TextColor = Color.ForestGreen;
                    else if (e.Item.ForeColor == Color.RoyalBlue || e.Item.ForeColor == Color.DodgerBlue)
                        e.TextColor = Color.RoyalBlue;
                    else
                        e.TextColor = SystemColors.ControlText;
                }

                base.OnRenderItemText(e);
            }
        }
    }
}