using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Gibbed.Prototype.FileFormats;
using Gibbed.Prototype.FileFormats.Pure3D;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype1;
using Gibbed.Prototype.FileFormats.Pure3D.Prototype2;
using PrototypeUltimateTool.Modules.ImportExport.Prototype1;
using PrototypeUltimateTool.Modules.ImportExport.Animation;
using Nixson.Common;
using PrototypeUltimateTool.Modules.Rendering;

namespace PrototypeUltimateTool
{
    public class Model3DViewer : System.Windows.Forms.UserControl
    {
        private ElementHost host;
        private System.Windows.Controls.Grid wpfGrid;
        private Viewport3D viewport;
        private ModelVisual3D rootVisual;
        private Model3DGroup rootGroup;
        private PerspectiveCamera camera;

        // Гизмо осей
        private Border gizmoContainer;
        private Viewport3D gizmoViewport;
        private PerspectiveCamera gizmoCamera;
        private System.Windows.Controls.Label xLabel;
        private System.Windows.Controls.Label yLabel;
        private System.Windows.Controls.Label zLabel;

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStrip animationToolStrip;
        private ViewerToolStripRenderer _toolStripRenderer;
        private bool _isDarkTheme = true;
        private string _uiLanguage = "ru";
        private bool _applyingUiLanguage;
        private ToolStripButton resetCamButton;
        private ToolStripLabel shadingModeLabel;
        private ToolStripButton flipUvButton;
        private readonly Dictionary<ViewerShadingMode, ToolStripButton> _shadingModeButtons = new Dictionary<ViewerShadingMode, ToolStripButton>();
        private ToolStripComboBox textureOverrideCombo;
        private ToolStripComboBox animationCombo;
        private System.Windows.Forms.CheckBox showIncompatibleAnimationsCheckBox;
        private ToolStripControlHost showIncompatibleAnimationsHost;
        private ToolStripButton animationDisplayModeButton;
        private bool _animationMeshMode;
        private bool _animationMeshModeAvailable;
        private ToolStripButton playAnimationButton;
        private ToolStripButton loopAnimationButton;
        private ToolStripControlHost animationTrackHost;
        private ToolStripLabel animationFrameLabel;
        private System.Windows.Forms.TrackBar animationTrackBar;
        private System.Windows.Forms.Label statsLabel;
        private System.Windows.Forms.ToolTip statsToolTip;
        private System.Windows.Forms.Panel _dx12Panel;
        private readonly List<Dx12SceneMesh> _dx12MeshScratch = new List<Dx12SceneMesh>();
        private readonly Dictionary<GeometryModel3D, MeshRenderInfo> _dx12RenderInfoLookup = new Dictionary<GeometryModel3D, MeshRenderInfo>();
        private GeometryModel3D[] _dx12RenderInfoLookupModels = new GeometryModel3D[0];
        private readonly Dictionary<GeometryModel3D, Dx12GeometrySnapshotCache> _dx12GeometrySnapshotCache = new Dictionary<GeometryModel3D, Dx12GeometrySnapshotCache>();
        private readonly Dictionary<BitmapSource, BitmapSource> _dx12FrozenTextureCache = new Dictionary<BitmapSource, BitmapSource>();
        private readonly object _dx12FrameRequestSync = new object();
        private readonly ManualResetEvent _dx12RenderStopEvent = new ManualResetEvent(false);
        private Dx12RenderFrameRequest _dx12LatestFrameRequest;
        private Thread _dx12RenderThread;
        private IntPtr _dx12RenderHwnd;
        private int _dx12InitialRenderWidth;
        private int _dx12InitialRenderHeight;
        private System.Windows.Forms.Timer _dx12RenderTimer;
        private volatile Dx12MeshRenderer _dx12Renderer;
        private ToolStripLabel _dx12FpsLabel;
        private bool _dx12RendererActive;
        private LayeredStatsOverlay _sceneStatsOverlay;
        private System.Windows.Forms.Panel _dx12GizmoOverlay;
        private double _dx12SceneCollectionMilliseconds;
        private bool _dx12MouseOrbiting;
        private bool _dx12MousePanning;
        private System.Drawing.Point _dx12LastMousePosition;
        private DateTime _dx12LastFpsLabelUpdateUtc;
        private double _dx12RenderLoopIntervalMilliseconds;
        private double _dx12RenderWaitMilliseconds;
        private double _dx12RenderWaitOvershootMilliseconds;
        private DateTime _lastDx12StatsOverlayUpdateUtc = DateTime.MinValue;

        private System.Windows.Forms.Timer _animationTimer;
        private bool _suppressAnimationSelection;
        private bool _suppressAnimationFrameChange;
        private bool _animationPlaying;
        private DateTime _lastAnimationTickUtc;
        private float _animationFrame;
        private float _activeAnimationFrameCount = 1.0f;
        private Animation _activeAnimation;
        private SkeletonPreviewState _activeSkeleton;
        private readonly List<AnimationBoneTrack> _activeAnimationTracks = new List<AnimationBoneTrack>();

        // Камера
        private double _yaw = -45.0;
        private double _pitch = 20.0;
        private double _distance = 2500.0;
        // Camera fit distance for the current model at the current viewport aspect ratio.
        // Keeping this separately lets us preserve the user's zoom relative to the model
        // while switching between models whose world-space dimensions differ.
        private double _fitDistance;
        private double _targetX = 0.0;
        private double _targetY = 0.0;
        private double _targetZ = 0.0;
        private System.Windows.Point _lastMousePos;
        private bool _isOrbiting = false;
        private bool _isPanning = false;
        private bool _hasCameraView;
        private bool _preserveCameraOnReload;
        // The initial camera fit is deferred until WPF has measured the viewport.
        // Before that, ActualWidth/ActualHeight can be zero and the camera fit uses
        // an incorrect 1:1 aspect ratio, which makes the first model appear cropped.
        private bool _pendingInitialCameraFit;
        private bool _cameraTransitionPending;
        private double _pendingZoomFactor = 1.0;
        private double _pendingPanX;
        private double _pendingPanY;
        private double _pendingPanZ;
        private double _pendingReferenceRadius;

        private double _boundingRadius = 1000.0;
        private double _sceneCenterX;
        private double _sceneCenterY;
        private double _sceneCenterZ;
        private bool _flipUv = true;
        private ViewerShadingMode _shadingMode = ViewerShadingMode.Textured;
        private readonly List<Model3D> _loadedModels = new List<Model3D>();
        private readonly List<MeshRenderInfo> _meshRenderInfos = new List<MeshRenderInfo>();
        // Per-source-rig poses preserve each skinned mesh's original joint pivots and hierarchy.
        private readonly Dictionary<Skeleton, SkeletonPreviewState> _sourceSkeletonPoseStates = new Dictionary<Skeleton, SkeletonPreviewState>();

        private enum ViewerShadingMode
        {
            Wireframe,
            Solid,
            Textured,
            Normals,
            Skinning,
            Deformation
        }

        private sealed class BufferedOverlayPanel : System.Windows.Forms.Panel
        {
            public BufferedOverlayPanel()
            {
                this.DoubleBuffered = true;
                this.ResizeRedraw = true;
                this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
                this.UpdateStyles();
            }
        }

        // A real per-pixel-alpha child window: WinForms' simulated transparent BackColor
        // would blend against the Panel's GDI background, not the live DXGI swap-chain image.
        private sealed class LayeredStatsOverlay : System.Windows.Forms.Form
        {
            private const int WsExLayered = 0x00080000;
            private const int WsExTransparent = 0x00000020;
            private const int WsExToolWindow = 0x00000080;
            private const int WsExNoActivate = 0x08000000;
            private const int WmNcHitTest = 0x0084;
            private const int HtTransparent = -1;
            private const int UlwAlpha = 0x00000002;
            private const int DibRgbColors = 0;
            private const uint BiRgb = 0;
            private bool _updatingSurface;
            private bool _updatingAnchor;
            private bool _displayEnabled = true;
            private System.Windows.Forms.Control _anchor;
            private System.Windows.Forms.Form _ownerForm;
            private System.Drawing.Point _anchorOffset = new System.Drawing.Point(8, 8);

            [StructLayout(LayoutKind.Sequential)]
            private struct NativePoint
            {
                public int X;
                public int Y;
                public NativePoint(int x, int y) { X = x; Y = y; }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct NativeSize
            {
                public int Width;
                public int Height;
                public NativeSize(int width, int height) { Width = width; Height = height; }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct BlendFunction
            {
                public byte BlendOp;
                public byte BlendFlags;
                public byte SourceConstantAlpha;
                public byte AlphaFormat;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct BitmapInfoHeader
            {
                public uint Size;
                public int Width;
                public int Height;
                public ushort Planes;
                public ushort BitCount;
                public uint Compression;
                public uint SizeImage;
                public int XPelsPerMeter;
                public int YPelsPerMeter;
                public uint ClrUsed;
                public uint ClrImportant;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct BitmapInfo
            {
                public BitmapInfoHeader Header;
                public uint Colors;
            }

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern IntPtr CreateCompatibleDC(IntPtr hDc);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern bool DeleteDC(IntPtr hDc);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern IntPtr CreateDIBSection(IntPtr hDc, ref BitmapInfo bitmapInfo,
                uint usage, out IntPtr bits, IntPtr section, uint offset);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern bool DeleteObject(IntPtr hObject);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst,
                ref NativePoint destination, ref NativeSize size, IntPtr hdcSrc,
                ref NativePoint source, int colorKey, ref BlendFunction blend, int flags);

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams parameters = base.CreateParams;
                    parameters.ExStyle |= WsExLayered | WsExTransparent | WsExToolWindow | WsExNoActivate;
                    return parameters;
                }
            }

            public LayeredStatsOverlay()
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.ShowIcon = false;
                this.ControlBox = false;
                this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                this.TopMost = false;
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
                this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
                this.BackColor = System.Drawing.Color.FromArgb(158, 23, 27, 34);
                this.ForeColor = System.Drawing.Color.FromArgb(246, 236, 241, 248);
                this.Font = new System.Drawing.Font(new System.Drawing.FontFamily("Consolas"), 7.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
                this.Padding = new Padding(6, 4, 6, 4);
                this.MaximumSize = new System.Drawing.Size(600, 0);
                this.TabStop = false;
            }

            protected override bool ShowWithoutActivation { get { return true; } }

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public bool DisplayEnabled
            {
                get { return _displayEnabled; }
                set
                {
                    if (_displayEnabled == value)
                    {
                        if (value) UpdateAnchorPosition();
                        return;
                    }
                    _displayEnabled = value;
                    if (!_displayEnabled)
                    {
                        if (Visible) Hide();
                    }
                    else
                    {
                        UpdateAnchorPosition();
                    }
                }
            }

            protected override void WndProc(ref Message m)
            {
                // Never intercept mouse input meant for the DX12 viewport underneath.
                if (m.Msg == WmNcHitTest)
                {
                    m.Result = new IntPtr(HtTransparent);
                    return;
                }
                base.WndProc(ref m);
            }

            public void AttachTo(System.Windows.Forms.Control anchor)
            {
                if (ReferenceEquals(_anchor, anchor))
                {
                    UpdateAnchorPosition();
                    return;
                }

                if (_anchor != null)
                {
                    _anchor.LocationChanged -= OnAnchorGeometryChanged;
                    _anchor.SizeChanged -= OnAnchorGeometryChanged;
                    _anchor.VisibleChanged -= OnAnchorGeometryChanged;
                    _anchor.ParentChanged -= OnAnchorGeometryChanged;
                    _anchor.HandleCreated -= OnAnchorGeometryChanged;
                    _anchor.HandleDestroyed -= OnAnchorHandleDestroyed;
                }

                _anchor = anchor;
                if (_anchor != null)
                {
                    _anchor.LocationChanged += OnAnchorGeometryChanged;
                    _anchor.SizeChanged += OnAnchorGeometryChanged;
                    _anchor.VisibleChanged += OnAnchorGeometryChanged;
                    _anchor.ParentChanged += OnAnchorGeometryChanged;
                    _anchor.HandleCreated += OnAnchorGeometryChanged;
                    _anchor.HandleDestroyed += OnAnchorHandleDestroyed;
                }
                UpdateAnchorPosition();
            }

            public void UpdateAnchorPosition()
            {
                if (_updatingAnchor || IsDisposed) return;
                _updatingAnchor = true;
                try
                {
                    if (_anchor == null || _anchor.IsDisposed || !_anchor.IsHandleCreated)
                    {
                        if (Visible) Hide();
                        return;
                    }

                    System.Windows.Forms.Form owner = _anchor.FindForm();
                    if (owner == null || owner.IsDisposed)
                    {
                        if (Visible) Hide();
                        SetOwnerForm(null);
                        return;
                    }
                    SetOwnerForm(owner);

                    bool shouldShow = _displayEnabled && _anchor.Visible && owner.Visible &&
                        owner.WindowState != System.Windows.Forms.FormWindowState.Minimized;
                    if (!shouldShow)
                    {
                        if (Visible) Hide();
                        return;
                    }

                    System.Drawing.Point screenLocation = _anchor.PointToScreen(_anchorOffset);
                    if (this.Location != screenLocation)
                        this.Location = screenLocation;
                    else if (IsHandleCreated)
                        UpdateLayeredSurface();

                    if (!Visible)
                        Show(owner);
                }
                catch
                {
                    // Overlay placement must never interfere with switching viewport modes.
                    if (Visible) Hide();
                }
                finally
                {
                    _updatingAnchor = false;
                }
            }

            private void OnAnchorGeometryChanged(object sender, EventArgs e)
            {
                UpdateAnchorPosition();
            }

            private void OnAnchorHandleDestroyed(object sender, EventArgs e)
            {
                if (Visible) Hide();
            }

            private void SetOwnerForm(System.Windows.Forms.Form owner)
            {
                if (ReferenceEquals(_ownerForm, owner) && ReferenceEquals(this.Owner, owner))
                    return;

                if (Visible) Hide();
                if (_ownerForm != null)
                {
                    _ownerForm.LocationChanged -= OnOwnerGeometryChanged;
                    _ownerForm.SizeChanged -= OnOwnerGeometryChanged;
                    _ownerForm.Resize -= OnOwnerGeometryChanged;
                    _ownerForm.VisibleChanged -= OnOwnerGeometryChanged;
                    _ownerForm.FormClosed -= OnOwnerClosed;
                }

                _ownerForm = owner;
                this.Owner = owner;
                if (_ownerForm != null)
                {
                    _ownerForm.LocationChanged += OnOwnerGeometryChanged;
                    _ownerForm.SizeChanged += OnOwnerGeometryChanged;
                    _ownerForm.Resize += OnOwnerGeometryChanged;
                    _ownerForm.VisibleChanged += OnOwnerGeometryChanged;
                    _ownerForm.FormClosed += OnOwnerClosed;
                }
            }

            private void OnOwnerGeometryChanged(object sender, EventArgs e)
            {
                UpdateAnchorPosition();
            }

            private void OnOwnerClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
            {
                if (Visible) Hide();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_anchor != null)
                    {
                        _anchor.LocationChanged -= OnAnchorGeometryChanged;
                        _anchor.SizeChanged -= OnAnchorGeometryChanged;
                        _anchor.VisibleChanged -= OnAnchorGeometryChanged;
                        _anchor.ParentChanged -= OnAnchorGeometryChanged;
                        _anchor.HandleCreated -= OnAnchorGeometryChanged;
                        _anchor.HandleDestroyed -= OnAnchorHandleDestroyed;
                        _anchor = null;
                    }
                    SetOwnerForm(null);
                }
                base.Dispose(disposing);
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                UpdateLayeredSurface();
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnForeColorChanged(EventArgs e)
            {
                base.OnForeColorChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                base.OnBackColorChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnPaddingChanged(EventArgs e)
            {
                base.OnPaddingChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnLocationChanged(EventArgs e)
            {
                base.OnLocationChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnParentChanged(EventArgs e)
            {
                base.OnParentChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnSizeChanged(EventArgs e)
            {
                base.OnSizeChanged(e);
                UpdateLayeredSurface();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Pixels are supplied via UpdateLayeredWindow; do not erase the alpha surface.
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                // Pixels are supplied via UpdateLayeredWindow; do not paint an opaque GDI child surface.
            }

            private void UpdateLayeredSurface()
            {
                if (_updatingSurface || !this.IsHandleCreated || this.IsDisposed)
                    return;

                _updatingSurface = true;
                try
                {
                    string[] lines = (this.Text ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    int lineHeight = Math.Max(1, this.Font.Height + 1);
                    int textWidth = 1;
                    System.Drawing.Size measureBounds = new System.Drawing.Size(10000, 1000);
                    TextFormatFlags measureFlags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        System.Drawing.Size measured = TextRenderer.MeasureText(lines[i], this.Font, measureBounds, measureFlags);
                        if (measured.Width > textWidth) textWidth = measured.Width;
                    }

                    int maxWidth = this.MaximumSize.Width > 0 ? this.MaximumSize.Width : 600;
                    int width = Math.Max(80, Math.Min(maxWidth, textWidth + this.Padding.Horizontal));
                    int height = Math.Max(24, lines.Length * lineHeight + this.Padding.Vertical);
                    if (this.Size.Width != width || this.Size.Height != height)
                        this.Size = new System.Drawing.Size(width, height);

                    using (Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.Clear(System.Drawing.Color.Transparent);
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                            {
                                float radius = 5.0f;
                                float diameter = radius * 2.0f;
                                path.AddArc(0, 0, diameter, diameter, 180, 90);
                                path.AddArc(width - diameter - 1, 0, diameter, diameter, 270, 90);
                                path.AddArc(width - diameter - 1, height - diameter - 1, diameter, diameter, 0, 90);
                                path.AddArc(0, height - diameter - 1, diameter, diameter, 90, 90);
                                path.CloseFigure();
                                // ClearType needs an opaque destination under each glyph. The complete
                                // surface is faded later by UpdateLayeredWindow's constant alpha, so the
                                // final panel remains translucent while text stays sharply rasterized.
                                using (SolidBrush background = new SolidBrush(System.Drawing.Color.FromArgb(255,
                                    this.BackColor.R, this.BackColor.G, this.BackColor.B)))
                                    graphics.FillPath(background, path);
                            }

                            System.Drawing.Color opaqueText = System.Drawing.Color.FromArgb(255,
                                this.ForeColor.R, this.ForeColor.G, this.ForeColor.B);
                            TextFormatFlags drawFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix |
                                TextFormatFlags.SingleLine | TextFormatFlags.Left;
                            for (int i = 0; i < lines.Length; i++)
                            {
                                var textBounds = new System.Drawing.Rectangle(
                                    this.Padding.Left, this.Padding.Top + i * lineHeight,
                                    Math.Max(1, width - this.Padding.Horizontal), lineHeight + 2);
                                TextRenderer.DrawText(graphics, lines[i], this.Font, textBounds, opaqueText, drawFlags);
                            }
                        }

                        IntPtr screenDc = IntPtr.Zero;
                        IntPtr memoryDc = IntPtr.Zero;
                        IntPtr dib = IntPtr.Zero;
                        IntPtr previousObject = IntPtr.Zero;
                        System.Drawing.Imaging.BitmapData bitmapData = null;
                        try
                        {
                            screenDc = GetDC(IntPtr.Zero);
                            if (screenDc == IntPtr.Zero) return;
                            memoryDc = CreateCompatibleDC(screenDc);
                            if (memoryDc == IntPtr.Zero) return;

                            BitmapInfo info = new BitmapInfo
                            {
                                Header = new BitmapInfoHeader
                                {
                                    Size = (uint)Marshal.SizeOf(typeof(BitmapInfoHeader)),
                                    Width = width,
                                    Height = -height, // Top-down DIB matches Bitmap's scan-line order.
                                    Planes = 1,
                                    BitCount = 32,
                                    Compression = BiRgb,
                                    SizeImage = (uint)(width * height * 4)
                                }
                            };
                            IntPtr dibBits;
                            dib = CreateDIBSection(screenDc, ref info, DibRgbColors, out dibBits, IntPtr.Zero, 0);
                            if (dib == IntPtr.Zero || dibBits == IntPtr.Zero) return;
                            previousObject = SelectObject(memoryDc, dib);

                            bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            int rowBytes = width * 4;
                            byte[] row = new byte[rowBytes];
                            for (int y = 0; y < height; y++)
                            {
                                Marshal.Copy(IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride), row, 0, rowBytes);
                                Marshal.Copy(row, 0, IntPtr.Add(dibBits, y * rowBytes), rowBytes);
                            }
                            bitmap.UnlockBits(bitmapData);
                            bitmapData = null;

                            // This overlay is an owned top-level layered window, so Location is already screen-relative.
                            NativePoint destination = new NativePoint(this.Location.X, this.Location.Y);
                            NativeSize nativeSize = new NativeSize(width, height);
                            NativePoint source = new NativePoint(0, 0);
                            BlendFunction blend = new BlendFunction
                            {
                                BlendOp = 0, // AC_SRC_OVER
                                BlendFlags = 0,
                                SourceConstantAlpha = 190,
                                AlphaFormat = 1 // AC_SRC_ALPHA; whole surface fades uniformly after ClearType rasterization.
                            };
                            UpdateLayeredWindow(this.Handle, screenDc, ref destination, ref nativeSize,
                                memoryDc, ref source, 0, ref blend, UlwAlpha);
                        }
                        finally
                        {
                            if (bitmapData != null) bitmap.UnlockBits(bitmapData);
                            if (previousObject != IntPtr.Zero && memoryDc != IntPtr.Zero)
                                SelectObject(memoryDc, previousObject);
                            if (dib != IntPtr.Zero) DeleteObject(dib);
                            if (memoryDc != IntPtr.Zero) DeleteDC(memoryDc);
                            if (screenDc != IntPtr.Zero) ReleaseDC(IntPtr.Zero, screenDc);
                        }
                    }
                }
                catch
                {
                    // Overlay rendering must never interrupt viewport rendering.
                }
                finally
                {
                    _updatingSurface = false;
                }
            }
        }

        private sealed class Dx12GeometrySnapshotCache
        {
            public MeshGeometry3D SourceMesh;
            public Point3DCollection SourcePositions;
            public Vector3DCollection SourceNormals;
            public PointCollection SourceTextureCoordinates;
            public Int32Collection SourceTriangleIndices;
            public MeshGeometry3D FrozenMesh;
        }

        private sealed class Dx12RenderFrameRequest
        {
            public Dx12SceneMesh[] Meshes;
            public Dx12ShadingMode ShadingMode;
            public double Yaw;
            public double Pitch;
            public double Distance;
            public double TargetX;
            public double TargetY;
            public double TargetZ;
            public double BoundingRadius;
            public int Width;
            public int Height;
        }

        [DllImport("winmm.dll", ExactSpelling = true)]
        private static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", ExactSpelling = true)]
        private static extern uint timeEndPeriod(uint uMilliseconds);

        // High-resolution waitable timer support (Windows 10 1803+). If unavailable,
        // the render loop falls back to a timed wait with a short final spin interval.
        private const uint CreateWaitableTimerHighResolution = 0x00000002;
        private const uint TimerAllAccess = 0x001F0003;
        private const uint WaitObject0 = 0x00000000;
        private const uint WaitFailed = 0xFFFFFFFF;
        private const uint Infinite = 0xFFFFFFFF;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateWaitableTimerExW(IntPtr lpTimerAttributes, string lpTimerName, uint dwFlags, uint dwDesiredAccess);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool SetWaitableTimer(IntPtr hTimer, ref long lpDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool bWaitAll, uint dwMilliseconds);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private sealed class MeshRenderInfo
        {
            public GeometryModel3D Model;
            public MeshGeometry3D Mesh;
            public Material SolidMaterial;
            public Material TexturedMaterial;
            public List<Nixson.Common.Vector3> Weights = new List<Nixson.Common.Vector3>();
            public List<byte[]> Groups = new List<byte[]>();
            public uint[] MatrixIndices = new uint[0];
            // Bind-pose data stays in game coordinates; the WPF display conversion is applied only after skinning.
            public Vector3D[] BindPositions = new Vector3D[0];
            public Vector3D[] BindNormals = new Vector3D[0];
            public int[][] SkinBoneIndices = new int[0][];
            public double[][] SkinWeights = new double[0][];
            public bool[] SkinUsesRigidBoneOrigin = new bool[0];
            public int RigidBoneIndex = -1;
            // Skin palette indices belong to the skeleton named by the source PolySkin, not
            // necessarily to the skeleton selected for the animation preview. Cutscene rigs can
            // insert facial/collar joints and shift later indices while retaining many bone names.
            // When available, evaluate skinning on this source rig so auxiliary joints keep their own pivots.
            // Name-based mapping is only a fallback when the source Skeleton node cannot be resolved.
            public string SourceSkeletonName;
            public Skeleton SourceSkeletonNode;
            public List<JointData> SourceSkeletonJoints = new List<JointData>();
            public Skeleton BoneMapSkeletonNode;
            public int[] SourceToActiveBoneMap = new int[0];
            // Reused GPU skinning data; these arrays change only when the source rig mapping/size changes.
            public System.Numerics.Matrix4x4[] Dx12BoneMatricesCache;
            public long Dx12BoneMatrixCachedPoseVersion = -1;
            public long Dx12BoneMatrixRevision;
            public Skeleton Dx12BoneMatrixCachedSkeletonNode;
            public Matrix3D Dx12BoneMatrixCachedObjectTransform;
            public int[] Dx12MappedSkinBoneMap;
            public int[][] Dx12MappedSkinBoneIndices;
            public bool HasSkinning;
            public bool LegacyRawSkinLayout;
            public System.Numerics.Vector4[] Dx12SkinColorCache;
            public int Dx12SkinColorVertexCount;
        }

        private BaseNode _lastNode;
        private Pure3DFile _lastP3D;
        private string _preferredSkeletonName;
        // Keep the source CompositeDrawable identity, not just a stale skeleton-name hint.
        // Animation mesh preservation is valid only while the currently displayed mesh came from this composite.
        private CompositeDrawable _animationMeshSourceComposite;
        private Pure3DFile _animationMeshSourceP3D;
        private bool _loadingAnimationFromCombo;
        private Animation _comboRequestedAnimation;
        private Skeleton _comboRequestedSkeleton;
        private Pure3DFile _comboRequestedP3D;
        private bool _loopAnimationPreference = true;
        private bool _suppressLoopPreferenceUpdate;
        private CompositeDrawable _animationDropdownContextComposite;
        private bool _animationDropdownMeshUnavailable;
        private List<KeyValuePair<Pure3DFile, string>> _animationSourceFiles = new List<KeyValuePair<Pure3DFile, string>>();
        private Dictionary<Animation, Func<Animation>> _animationNodeLoaders = new Dictionary<Animation, Func<Animation>>();
        private AnimationOption _materializedExternalAnimationOption;
        private readonly Dictionary<Pure3DFile, AnimationSourceIndex> _animationSourceIndexCache = new Dictionary<Pure3DFile, AnimationSourceIndex>();
        private readonly Dictionary<Skeleton, AnimationSourceIndex> _skeletonSourceIndexes = new Dictionary<Skeleton, AnimationSourceIndex>();
        private readonly HashSet<Pure3DFile> _animationSourceIndexBuildPending = new HashSet<Pure3DFile>();
        private readonly HashSet<Pure3DFile> _animationSourceIndexBuildFailed = new HashSet<Pure3DFile>();
        private readonly HashSet<Pure3DFile> _preparedRcfAnimationSourceIndexSources = new HashSet<Pure3DFile>();
        private int _animationSourceSetGeneration;
        private bool _animationDropdownCatalogValid;
        private Pure3DFile _animationDropdownCatalogP3D;
        private string _animationDropdownCatalogRigKey;
        private int _animationDropdownCatalogSourceGeneration = -1;
        private bool _animationDropdownCatalogCompositeContext;
        private bool _animationDropdownCatalogShowIncompatible;
        private bool _animationDropdownCatalogSameRig = true;
        private List<CompositePreviewSource> _compositePreviewBatchForLoad;
        private List<CompositePreviewSource> _animationMeshSourceComposites = new List<CompositePreviewSource>();
        private Func<string> _refreshLocalizedStats;

        private sealed class AnimationOption
        {
            public Animation Node;
            // For RCF catalog nodes Node is replaced with the full clip only while selected;
            // CatalogNode remains the stripped metadata object held by the source index.
            public Animation CatalogNode;
            public Skeleton CompatibleSkeleton;
            public Pure3DFile SourceP3D;
            public string SourceLabel;
            public bool IsCompatible;
            public bool IsPlaceholder;
            public bool IsMeshUnavailablePlaceholder;
            public bool IsMultiCompositeRigMismatchPlaceholder;
            public bool IsSearchingPlaceholder;
            public string LanguageCode = "ru";
            public override string ToString()
            {
                if (IsPlaceholder && IsMeshUnavailablePlaceholder)
                    return UiLocalization.TranslateId("viewer.animation.noSkinnedMeshPlaceholder", "Нет деформируемого меша", LanguageCode);
                if (IsPlaceholder && IsMultiCompositeRigMismatchPlaceholder)
                    return UiLocalization.TranslateId("viewer.animation.multiCompositeRigMismatch", "Выбранные модели используют разные скелеты", LanguageCode);
                if (IsPlaceholder && IsSearchingPlaceholder)
                    return UiLocalization.TranslateId("viewer.animation.searching", "Поиск анимаций…", LanguageCode);
                if (IsPlaceholder)
                    return UiLocalization.TranslateId("viewer.animation.noCompatiblePlaceholder", "Нет совместимых анимаций", LanguageCode);
                if (Node == null) return UiLocalization.TranslateId("viewer.animation.none", "(нет анимации)", LanguageCode);
                string name = (Node.Name ?? string.Empty).TrimEnd('\0', ' ');
                string type = (Node.AnimationType ?? string.Empty).TrimEnd('\0', ' ');
                string frames = UiLocalization.TranslateId("viewer.animation.framesSuffix", " кадров", LanguageCode);
                string label = string.IsNullOrWhiteSpace(name) ? type + " — " + Node.NumFrames.ToString("0") + frames : name + " [" + type + "]";
                if (!IsCompatible)
                    label += UiLocalization.TranslateId("viewer.animation.incompatibleMarker", " [несовместима]", LanguageCode);
                if (!string.IsNullOrWhiteSpace(SourceLabel))
                    label += UiLocalization.TranslateId("viewer.animation.sourceSeparator", " — источник: ", LanguageCode) + SourceLabel;
                return label;
            }
        }

        private sealed class CompositePreviewSource
        {
            public BaseNode Node;
            public CompositeDrawable Composite;
            public Pure3DFile P3D;
        }

        private sealed class AnimationSourceIndex
        {
            public Pure3DFile Source;
            public List<Animation> Animations = new List<Animation>();
            public List<Skeleton> Skeletons = new List<Skeleton>();
            public Dictionary<Animation, List<AnimationBone>> TrackBones = new Dictionary<Animation, List<AnimationBone>>();
            public Dictionary<Skeleton, List<JointData>> Joints = new Dictionary<Skeleton, List<JointData>>();
            public Dictionary<string, Dictionary<Animation, Skeleton>> MatchingResults = new Dictionary<string, Dictionary<Animation, Skeleton>>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Opaque index prepared during the RCF-open scan. The index can be built on a worker
        /// thread and registered on the UI thread without re-walking every animation when a
        /// CompositeDrawable is selected for the first time.
        /// </summary>
        public sealed class PreparedAnimationSourceIndex
        {
            // Keep the implementation type private to Model3DViewer while allowing MainForm to
            // carry the prepared token from its worker scan to the UI-thread registration call.
            internal object IndexObject { get; private set; }

            internal PreparedAnimationSourceIndex(object index)
            {
                IndexObject = index;
            }
        }

        private sealed class SkeletonPreviewState
        {
            public Skeleton SkeletonNode;
            public List<JointData> Joints = new List<JointData>();
            public GeometryModel3D[] JointMarkers;
            public GeometryModel3D[] BoneModels;
            public Point3D[] WorldPositions;
            public Vector3D[] WorldGamePositions;
            public Vector3D[] WorldAxisX;
            public Vector3D[] WorldAxisY;
            public Vector3D[] WorldAxisZ;
            public Vector3D[] BindGamePositions;
            public Vector3D[] BindAxisX;
            public Vector3D[] BindAxisY;
            public Vector3D[] BindAxisZ;
            public long PoseVersion;
        }

        private sealed class AnimationBoneTrack
        {
            public int JointIndex;
            public string BoneName;
            public AnimationChannel RotationChannel;
            public AnimationChannel TranslationChannel;
            public BoneRotationData LegacyRotationData;
            public BoneTranslationData LegacyTranslationData;
            public Vector4 FirstTranslation;
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public Model3DViewer()
        {
            this.BuildUI();
        }

        /// <summary>
        /// Enables or disables the floating Direct3D 12 scene-statistics overlay.
        /// </summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool SceneStatsOverlayEnabled
        {
            get { return this._sceneStatsOverlay == null || this._sceneStatsOverlay.DisplayEnabled; }
            set
            {
                if (this._sceneStatsOverlay != null && !this._sceneStatsOverlay.IsDisposed)
                    this._sceneStatsOverlay.DisplayEnabled = value;
            }
        }

        /// <summary>
        /// Supplies P3D containers that can provide animation clips and skeletons for a selected
        /// CompositeDrawable. Source files are searched only while a composite animation context is active.
        /// </summary>
        public void SetAnimationSourceFiles(IEnumerable<KeyValuePair<Pure3DFile, string>> sources, bool refreshCurrentComposite)
        {
            var updated = new List<KeyValuePair<Pure3DFile, string>>();
            var seen = new HashSet<Pure3DFile>();
            if (sources != null)
            {
                foreach (var source in sources)
                {
                    if (source.Key == null || !seen.Add(source.Key)) continue;
                    updated.Add(new KeyValuePair<Pure3DFile, string>(source.Key, source.Value ?? string.Empty));
                }
            }
            var oldSources = new HashSet<Pure3DFile>(this._animationSourceFiles.Select(item => item.Key));
            var retainedSources = new HashSet<Pure3DFile>(updated.Select(item => item.Key));
            bool sourceSetChanged = oldSources.Count != retainedSources.Count || oldSources.Any(source => !retainedSources.Contains(source));
            bool incomingSetIsPreparedCatalog =
                (updated.Count == this._preparedRcfAnimationSourceIndexSources.Count &&
                 updated.All(source => this._preparedRcfAnimationSourceIndexSources.Contains(source.Key)) &&
                 this._preparedRcfAnimationSourceIndexSources.All(retainedSources.Contains)) ||
                (updated.Count == 0 && this._preparedRcfAnimationSourceIndexSources.Count > 0);
            this._animationSourceFiles = updated;
            if (sourceSetChanged)
            {
                // Swapping between empty preview context and the same prepared RCF catalogue is
                // not a change of the catalogue universe. Keep the generation and match cache so
                // choosing a folder/another CompositeDrawable does not trigger a full rematch.
                if (!incomingSetIsPreparedCatalog)
                {
                    this._animationSourceSetGeneration++;
                    foreach (AnimationSourceIndex cachedIndex in this._animationSourceIndexCache.Values)
                        cachedIndex.MatchingResults.Clear();
                }
                this._animationDropdownCatalogValid = false;
                if (!incomingSetIsPreparedCatalog)
                    this._animationSourceIndexBuildFailed.RemoveWhere(source => !retainedSources.Contains(source));
            }
            foreach (Pure3DFile cachedSource in this._animationSourceIndexCache.Keys
                .Where(source => !retainedSources.Contains(source) && !this._preparedRcfAnimationSourceIndexSources.Contains(source)).ToList())
            {
                AnimationSourceIndex removedIndex = this._animationSourceIndexCache[cachedSource];
                this._animationSourceIndexCache.Remove(cachedSource);
                foreach (Skeleton skeleton in removedIndex.Skeletons)
                {
                    AnimationSourceIndex mappedIndex;
                    if (this._skeletonSourceIndexes.TryGetValue(skeleton, out mappedIndex) && ReferenceEquals(mappedIndex, removedIndex))
                        this._skeletonSourceIndexes.Remove(skeleton);
                }
            }
            if (!refreshCurrentComposite || this._animationMeshSourceComposite == null || this._animationMeshSourceP3D == null)
                return;

            AnimationOption selected = this.animationCombo == null ? null : this.animationCombo.SelectedItem as AnimationOption;
            Animation selectedAnimation = selected != null ? selected.Node : null;
            this.PopulateAnimationDropdown(this._animationMeshSourceP3D, this._animationMeshSourceComposite, true, selectedAnimation);
            if (selectedAnimation != null) this.SyncAnimationSelection(selectedAnimation);
        }

        /// <summary>
        /// Maps lightweight RCF animation metadata nodes to a loader that materializes a single
        /// full Animation only when the user selects it. This avoids retaining frame samples for
        /// every clip in an entire archive simultaneously.
        /// </summary>
        public void SetAnimationNodeLoaders(IDictionary<Animation, Func<Animation>> loaders, bool refreshCurrentComposite)
        {
            var updated = loaders == null
                ? new Dictionary<Animation, Func<Animation>>()
                : loaders.Where(pair => pair.Key != null && pair.Value != null)
                    .GroupBy(pair => pair.Key).ToDictionary(group => group.Key, group => group.First().Value);
            bool changed = this._animationNodeLoaders.Count != updated.Count ||
                this._animationNodeLoaders.Any(pair => !updated.ContainsKey(pair.Key) || !Equals(updated[pair.Key], pair.Value));
            if (!changed) return;

            if (this._materializedExternalAnimationOption != null)
            {
                this._materializedExternalAnimationOption.Node = this._materializedExternalAnimationOption.CatalogNode;
                this._materializedExternalAnimationOption = null;
            }
            this._animationNodeLoaders = updated;
            // Loader delegates change how a selected catalogue clip is materialized, not the
            // animation tracks or skeleton universe. Preserve the precomputed compatibility map.
            this._animationDropdownCatalogValid = false;

            if (refreshCurrentComposite && this._animationMeshSourceComposite != null && this._animationMeshSourceP3D != null)
            {
                AnimationOption selected = this.animationCombo == null ? null : this.animationCombo.SelectedItem as AnimationOption;
                Animation selectedAnimation = selected == null ? null : selected.Node;
                this.PopulateAnimationDropdown(this._animationMeshSourceP3D, this._animationMeshSourceComposite, true, selectedAnimation);
                if (selectedAnimation != null) this.SyncAnimationSelection(selectedAnimation);
            }
        }

        /// <summary>Clears the current model/animation preview when a non-P3D archive item is selected.</summary>
        public void ClearScene()
        {
            this.ClearScene(false);
        }

        public void ClearScene(bool preservePreparedAnimationCatalog)
        {
            this.StopAnimationPlayback();
            this._lastNode = null;
            this._lastP3D = null;
            this._preferredSkeletonName = null;
            this._animationMeshSourceComposite = null;
            this._animationMeshSourceP3D = null;
            this._animationSourceFiles.Clear();
            this._animationNodeLoaders.Clear();
            this._materializedExternalAnimationOption = null;
            if (!preservePreparedAnimationCatalog)
            {
                this._animationSourceIndexCache.Clear();
                this._skeletonSourceIndexes.Clear();
                this._animationSourceIndexBuildFailed.Clear();
                this._preparedRcfAnimationSourceIndexSources.Clear();
            }
            else
            {
                foreach (Pure3DFile source in this._animationSourceIndexCache.Keys
                    .Where(source => !this._preparedRcfAnimationSourceIndexSources.Contains(source)).ToList())
                {
                    AnimationSourceIndex removedIndex = this._animationSourceIndexCache[source];
                    this._animationSourceIndexCache.Remove(source);
                    foreach (Skeleton skeleton in removedIndex.Skeletons)
                    {
                        AnimationSourceIndex mappedIndex;
                        if (this._skeletonSourceIndexes.TryGetValue(skeleton, out mappedIndex) && ReferenceEquals(mappedIndex, removedIndex))
                            this._skeletonSourceIndexes.Remove(skeleton);
                    }
                }
                this._animationSourceIndexBuildFailed.Clear();
                this._animationSourceIndexBuildPending.RemoveWhere(source => !this._preparedRcfAnimationSourceIndexSources.Contains(source));
            }
            if (!preservePreparedAnimationCatalog)
                this._animationSourceSetGeneration++;
            this._animationDropdownCatalogValid = false;
            this._animationMeshSourceComposites.Clear();
            this._compositePreviewBatchForLoad = null;
            this._activeAnimation = null;
            this._activeSkeleton = null;
            this._activeAnimationTracks.Clear();
            this._animationMeshMode = false;
            this._animationMeshModeAvailable = false;
            this._activeAnimationFrameCount = 1.0f;
            if (this._loadedModels != null)
            {
                foreach (var model in this._loadedModels) this.rootGroup.Children.Remove(model);
                this._loadedModels.Clear();
            }
            this._meshRenderInfos.Clear();
            this._sourceSkeletonPoseStates.Clear();
            this._suppressAnimationSelection = true;
            try
            {
                if (this.animationCombo != null) this.animationCombo.Items.Clear();
                if (this.textureOverrideCombo != null) this.textureOverrideCombo.Items.Clear();
            }
            finally { this._suppressAnimationSelection = false; }
            this.SetAnimationControlsEnabled(false);
            this.UpdateAnimationDisplayModeButton();
            this._refreshLocalizedStats = null;
            if (this.statsLabel != null)
            {
                this.statsLabel.Text = string.Empty;
                if (this.statsToolTip != null) this.statsToolTip.SetToolTip(this.statsLabel, string.Empty);
                this.UpdateStatsLabelLayout();
            }
            this.UpdateAnimationComboToolTip();
        }

        private void BuildUI()
        {
            this.toolStrip = new System.Windows.Forms.ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Dock = DockStyle.Top,
                Padding = Padding.Empty,
                BackColor = System.Drawing.Color.FromArgb(32, 32, 34),
                ForeColor = System.Drawing.Color.FromArgb(220, 220, 220)
            };
            this._toolStripRenderer = new ViewerToolStripRenderer(() => this._isDarkTheme);
            this.toolStrip.Renderer = this._toolStripRenderer;

            // Keep animation controls on their own row. The view/shading toolbar can
            // otherwise consume the horizontal space and truncate both the clip name
            // and the diagnostic text on narrower windows.
            this.animationToolStrip = new System.Windows.Forms.ToolStrip
            {
                Name = "viewer.animation.toolbar",
                GripStyle = ToolStripGripStyle.Hidden,
                Dock = DockStyle.Top,
                Padding = Padding.Empty,
                BackColor = System.Drawing.Color.FromArgb(32, 32, 34),
                ForeColor = System.Drawing.Color.FromArgb(220, 220, 220),
                Stretch = true
            };
            this.animationToolStrip.Renderer = this._toolStripRenderer;

            this.resetCamButton = new ToolStripButton("🏠 Сброс вида", null, (s, e) => this.ResetCamera()) { Name = "viewer.resetCamera" };
            this.shadingModeLabel = new ToolStripLabel("Вид:")
            {
                Name = "viewer.shading.label",
                ToolTipText = "Режим отображения модели"
            };
            this.BuildShadingModeButtons();

            this.flipUvButton = new ToolStripButton("🔄 Инверт UV", null, (s, e) =>
            {
                // CheckOnClick updates Checked before Click is raised. Keep the option state
                // in sync with the actual UI state instead of toggling two independent values.
                this._flipUv = this.flipUvButton.Checked;
                this.ReloadCurrentNodePreservingCamera();
            })
            {
                Name = "viewer.flipUv",
                CheckOnClick = true,
                Checked = true,
                ToolTipText = "По умолчанию включено: UVCoordinate уже инвертирует V при чтении P3D. Повторная инверсия здесь восстанавливает исходную ориентацию UV для WPF. Снимите отметку, чтобы посмотреть альтернативную ориентацию."
            };

            this.textureOverrideCombo = new ToolStripComboBox
            {
                Name = "viewer.textureOverride",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new System.Drawing.Size(180, 25),
                ToolTipText = "Выбрать текстуру вручную для этой модели"
            };
            this.textureOverrideCombo.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.textureOverrideCombo.ComboBox.DrawItem += this.OnViewerComboDrawItem;
            this.textureOverrideCombo.SelectedIndexChanged += (s, e) => { if (!this._applyingUiLanguage) this.ReloadCurrentNodePreservingCamera(); };

            this.animationCombo = new ToolStripComboBox
            {
                Name = "viewer.animation.combo",
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 280,
                DropDownWidth = 320,
                ToolTipText = "Выбрать анимацию из текущего P3D"
            };
            this.animationCombo.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.animationCombo.ComboBox.DrawItem += this.OnViewerComboDrawItem;

            this.showIncompatibleAnimationsCheckBox = new System.Windows.Forms.CheckBox
            {
                Name = "viewer.animation.showIncompatible",
                AutoSize = true,
                Checked = false,
                UseVisualStyleBackColor = false,
                BackColor = System.Drawing.Color.FromArgb(32, 32, 34),
                ForeColor = System.Drawing.Color.FromArgb(220, 220, 220),
                Margin = new Padding(3, 3, 3, 3),
                Text = "Показывать несовместимые"
            };
            this.showIncompatibleAnimationsHost = new ToolStripControlHost(this.showIncompatibleAnimationsCheckBox)
            {
                Name = "viewer.animation.showIncompatible.host",
                AutoSize = true,
                Margin = new Padding(2, 0, 2, 0),
                ToolTipText = "Показывать также анимации, которые не удалось однозначно сопоставить со скелетом"
            };
            this.showIncompatibleAnimationsCheckBox.CheckedChanged += (s, e) => this.RefreshAnimationDropdownForCurrentContext();

            this.animationCombo.SelectedIndexChanged += (s, e) =>
            {
                if (this._suppressAnimationSelection) return;
                var option = this.animationCombo.SelectedItem as AnimationOption;
                if (this._materializedExternalAnimationOption != null &&
                    !ReferenceEquals(this._materializedExternalAnimationOption, option))
                {
                    this._materializedExternalAnimationOption.Node = this._materializedExternalAnimationOption.CatalogNode;
                    this._materializedExternalAnimationOption = null;
                }
                this.UpdateAnimationComboToolTip();
                if (option == null || option.Node == null || this._lastP3D == null) return;

                Animation selectedAnimation = option.Node;
                Func<Animation> loader = null;
                if (option.CatalogNode != null && this._animationNodeLoaders.TryGetValue(option.CatalogNode, out loader))
                {
                    if (ReferenceEquals(option.Node, option.CatalogNode))
                    {
                        try { selectedAnimation = loader(); }
                        catch (Exception ex)
                        {
                            this.SetStatsText(() => this.LF("viewer.stats.animationSourceLoadFailed",
                                "Не удалось загрузить выбранную анимацию из RCF: {0}", ex.Message));
                            return;
                        }
                        if (selectedAnimation == null)
                        {
                            this.SetStatsText(() => this.L("viewer.stats.animationSourceLoadEmpty",
                                "Не удалось прочитать чанк выбранной анимации. Повторите сканирование RCF."));
                            return;
                        }
                        // Keep the full node only for the currently selected clip. All other
                        // combo rows point back to stripped metadata and release their frame buffers.
                        option.Node = selectedAnimation;
                        this._materializedExternalAnimationOption = option;
                    }
                    else selectedAnimation = option.Node;
                }

                Pure3DFile sourceP3D = this._lastP3D;
                bool keepLoopPreference = this._loopAnimationPreference;
                this._comboRequestedAnimation = selectedAnimation;
                this._comboRequestedSkeleton = option.CompatibleSkeleton;
                this._comboRequestedP3D = sourceP3D;
                this._loadingAnimationFromCombo = true;
                try { this.LoadNode(selectedAnimation, sourceP3D); }
                finally
                {
                    // Changing clips must not override the user's repeat preference with
                    // the selected clip's Cyclic metadata or incidental UI state changes.
                    this._loopAnimationPreference = keepLoopPreference;
                    this._suppressLoopPreferenceUpdate = true;
                    try { this.loopAnimationButton.Checked = keepLoopPreference; }
                    finally { this._suppressLoopPreferenceUpdate = false; }
                    this._comboRequestedAnimation = null;
                    this._comboRequestedSkeleton = null;
                    this._comboRequestedP3D = null;
                    this._loadingAnimationFromCombo = false;
                }
            };

            this.animationDisplayModeButton = new ToolStripButton
            {
                Name = "viewer.animation.displayMode",
                Text = "Меш",
                CheckOnClick = false,
                Enabled = false,
                ToolTipText = "Переключить между деформируемым мешем и скелетом"
            };
            this.animationDisplayModeButton.Click += (sender, args) =>
            {
                if (!this._animationMeshModeAvailable || this._activeAnimation == null || this._activeSkeleton == null) return;
                this._animationMeshMode = !this._animationMeshMode;
                this.ApplyAnimationDisplayMode();
                if (this._animationMeshMode) this.FitCameraToMeshBounds(false);
            };

            this.playAnimationButton = new ToolStripButton("▶", null, (s, e) => this.ToggleAnimationPlayback())
            {
                Name = "viewer.animation.play",
                Enabled = false,
                ToolTipText = "Воспроизвести / приостановить анимацию"
            };
            this.loopAnimationButton = new ToolStripButton("↻", null, (s, e) => { })
            {
                Name = "viewer.animation.loop",
                CheckOnClick = true,
                Checked = this._loopAnimationPreference,
                ToolTipText = "Зациклить анимацию"
            };
            this.loopAnimationButton.CheckedChanged += (s, e) =>
            {
                if (!this._suppressLoopPreferenceUpdate && this.loopAnimationButton != null)
                    this._loopAnimationPreference = this.loopAnimationButton.Checked;
            };
            this.animationTrackBar = new System.Windows.Forms.TrackBar
            {
                Name = "viewer.animation.track",
                Minimum = 0,
                Maximum = 1,
                Value = 0,
                TickStyle = TickStyle.None,
                AutoSize = false,
                Width = 132,
                Height = 24,
                Enabled = false,
                Margin = new Padding(0, 0, 0, 0)
            };
            this.animationTrackBar.ValueChanged += (s, e) =>
            {
                if (this._suppressAnimationFrameChange || this._activeAnimation == null) return;
                this._animationFrame = this.animationTrackBar.Value;
                this.UpdateSkeletonPose(this._animationFrame);
                this.UpdateAnimationFrameLabel();
            };
            this.animationTrackHost = new ToolStripControlHost(this.animationTrackBar)
            {
                Name = "viewer.animation.trackHost",
                AutoSize = false,
                Width = 138,
                Height = 25,
                Margin = new Padding(1, 0, 1, 0)
            };
            this.animationFrameLabel = new ToolStripLabel("кадр —") { Name = "viewer.animation.frameLabel", ToolTipText = "Текущий кадр анимации" };

            this._animationTimer = new System.Windows.Forms.Timer { Interval = 33 };
            this._animationTimer.Tick += this.OnAnimationTimerTick;

            this.statsLabel = new System.Windows.Forms.Label
            {
                Name = "viewer.animation.stats",
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 36,
                Padding = new Padding(6, 2, 6, 2),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                UseMnemonic = false,
                AutoEllipsis = false,
                BackColor = System.Drawing.Color.FromArgb(32, 32, 34),
                ForeColor = System.Drawing.SystemColors.GrayText
            };
            this.statsToolTip = new System.Windows.Forms.ToolTip
            {
                ShowAlways = true,
                InitialDelay = 450,
                ReshowDelay = 100,
                AutoPopDelay = 15000
            };
            this.statsToolTip.SetToolTip(this.statsLabel, string.Empty);
            this.statsLabel.SizeChanged += (s, e) => this.UpdateStatsLabelLayout();
            this.SizeChanged += (s, e) => this.UpdateStatsLabelLayout();
            this.Disposed += (s, e) =>
            {
                if (this._dx12RenderTimer != null)
                {
                    this._dx12RenderTimer.Stop();
                    this._dx12RenderTimer.Dispose();
                    this._dx12RenderTimer = null;
                }
                this._dx12RenderStopEvent.Set();
                Thread renderThread = this._dx12RenderThread;
                if (renderThread != null && renderThread.IsAlive && renderThread != Thread.CurrentThread)
                {
                    // The render thread owns all post-initialization DX12 calls and disposes the renderer on exit.
                    // Do not dispose resources concurrently with its command submission.
                    if (!renderThread.Join(1500))
                        ApplicationLog.Write("DX12_RENDER_THREAD_STOP_TIMEOUT", "Render thread did not stop within 1500 ms; it will finish disposal in the background.");
                }
                else if (this._dx12Renderer != null)
                {
                    try { this._dx12Renderer.Dispose(); }
                    catch (Exception ex) { ApplicationLog.Write("DX12_RENDER_DISPOSE_FAILED", "Renderer disposal failed.", ex); }
                    this._dx12Renderer = null;
                }
                if (this.statsToolTip != null)
                {
                    this.statsToolTip.Dispose();
                    this.statsToolTip = null;
                }
            };

            this._dx12FpsLabel = new ToolStripLabel("D3D12: запуск…") { Name = "viewer.dx12Fps" };

            this.toolStrip.Items.AddRange(new ToolStripItem[]
            {
                this.resetCamButton,
                new ToolStripSeparator(),
                this.shadingModeLabel,
                this._shadingModeButtons[ViewerShadingMode.Wireframe],
                this._shadingModeButtons[ViewerShadingMode.Solid],
                this._shadingModeButtons[ViewerShadingMode.Textured],
                this._shadingModeButtons[ViewerShadingMode.Normals],
                this._shadingModeButtons[ViewerShadingMode.Skinning],
                this._shadingModeButtons[ViewerShadingMode.Deformation],
                new ToolStripSeparator(),
                this._dx12FpsLabel,
                this.flipUvButton,
                this.textureOverrideCombo
            });

            this.animationToolStrip.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripLabel("Анимация:") { Name = "viewer.animation.label" },
                this.animationCombo,
                this.animationDisplayModeButton,
                this.playAnimationButton,
                this.loopAnimationButton,
                this.animationTrackHost,
                this.animationFrameLabel,
                this.showIncompatibleAnimationsHost
            });

            this.host = new ElementHost { Dock = DockStyle.Fill };
            this._dx12Panel = new System.Windows.Forms.Panel
            {
                Name = "viewer.dx12Surface",
                Dock = DockStyle.Fill,
                Visible = true,
                BackColor = System.Drawing.Color.FromArgb(14, 17, 23),
                TabStop = true
            };
            this._dx12Panel.SizeChanged += (sender, args) =>
            {
                this.PositionDx12Overlays();
                if (this._pendingInitialCameraFit && !this._hasCameraView &&
                    this._dx12Panel.ClientSize.Width > 1 && this._dx12Panel.ClientSize.Height > 1)
                    this.ScheduleInitialCameraFit();
                // Resize is applied by the DX12 render thread from the next immutable frame request.
                // DXGI swap-chain resources must not be resized concurrently with rendering.
            };
            this._dx12Panel.MouseDown += this.OnDx12PanelMouseDown;
            this._dx12Panel.MouseMove += this.OnDx12PanelMouseMove;
            this._dx12Panel.MouseUp += this.OnDx12PanelMouseUp;
            this._dx12Panel.MouseWheel += this.OnDx12PanelMouseWheel;
            this._dx12Panel.HandleCreated += (sender, args) => this.InitializeDx12Renderer();
            // This timer snapshots WPF-owned scene state only. Actual Direct3D 12 command submission,
            // Present and swap-chain resize run on a dedicated background thread at a paced frame rate.
            this._dx12RenderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            this._dx12RenderTimer.Tick += (sender, args) => this.RenderDx12Frame();

            this._sceneStatsOverlay = new LayeredStatsOverlay
            {
                Name = "viewer.dx12.sceneStats",
                MaximumSize = new System.Drawing.Size(600, 0),
                Padding = new Padding(6, 4, 6, 4),
                Font = new System.Drawing.Font("Consolas", 8.0f, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.FromArgb(255, 236, 241, 248),
                BackColor = System.Drawing.Color.FromArgb(255, 23, 27, 34),
                Text = "PrototypeUltimateTool  |  Direct3D 12\r\nWaiting for scene…"
            };
            this._dx12GizmoOverlay = new BufferedOverlayPanel
            {
                Name = "viewer.dx12.orientationGizmo",
                Size = new System.Drawing.Size(88, 88),
                BackColor = System.Drawing.Color.FromArgb(34, 37, 43),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            this._dx12GizmoOverlay.Paint += this.PaintDx12Gizmo;
            // Per-pixel alpha requires a top-level layered window on some WinForms/Windows configurations;
            // do not create WS_EX_LAYERED as a child HWND (that can fail while the 3D view is made visible).
            this._sceneStatsOverlay.AttachTo(this._dx12Panel);
            this._dx12Panel.VisibleChanged += (sender, args) => this.PositionDx12Overlays();
            this.VisibleChanged += (sender, args) => this.PositionDx12Overlays();
            this.ParentChanged += (sender, args) => this.PositionDx12Overlays();
            this._dx12Panel.Controls.Add(this._dx12GizmoOverlay);
            this._dx12GizmoOverlay.BringToFront();

            this.viewport = new Viewport3D();
            // Keep WPF's default 3D multisampling enabled. EdgeMode.Aliased disables
            // it; stock WPF Viewport3D uses 4 samples per pixel when hardware rendering
            // is available. WPF's public Viewport3D API does not expose an 8x sample count.
            System.Windows.Media.RenderOptions.SetEdgeMode(this.viewport, System.Windows.Media.EdgeMode.Unspecified);
            this.camera = new PerspectiveCamera
            {
                FieldOfView = 45,
                NearPlaneDistance = 1.0,
                FarPlaneDistance = 500000.0,
                // The display-space geometry is converted to Z-up by ToViewerPoint.
                UpDirection = new Vector3D(0, 0, 1)
            };
            this.viewport.Camera = this.camera;
            this.viewport.SizeChanged += this.OnViewportSizeChanged;

            this.rootGroup = new Model3DGroup();
            this.rootVisual = new ModelVisual3D { Content = this.rootGroup };
            this.viewport.Children.Add(this.rootVisual);

            this.UpdateLights();
            this.BuildGizmoUI();

            this.wpfGrid = new System.Windows.Controls.Grid
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 26))
            };
            this.wpfGrid.Children.Add(this.viewport);
            this.wpfGrid.Children.Add(this.gizmoContainer);

            this.wpfGrid.MouseDown += this.OnWpfMouseDown;
            this.wpfGrid.MouseMove += this.OnWpfMouseMove;
            this.wpfGrid.MouseUp += this.OnWpfMouseUp;
            this.wpfGrid.MouseWheel += this.OnWpfMouseWheel;

            this.host.Child = this.wpfGrid;

            this.Controls.Add(this.host);
            this.host.Visible = false;
            this.Controls.Add(this._dx12Panel);
            this.Controls.Add(this.statsLabel);
            this.Controls.Add(this.animationToolStrip);
            this.Controls.Add(this.toolStrip);
            this.UpdateStatsLabelLayout();
            this.PositionDx12Overlays();
            if (this._dx12Panel.IsHandleCreated) this.InitializeDx12Renderer();
        }

        private string GetShadingModeLabel(ViewerShadingMode mode)
        {
            switch (mode)
            {
                case ViewerShadingMode.Wireframe: return this.L("viewer.shading.wireframe", "Каркас: вершины и рёбра");
                case ViewerShadingMode.Solid: return this.L("viewer.shading.solid", "Сплошная модель");
                case ViewerShadingMode.Textured: return this.L("viewer.shading.textured", "Текстуры");
                case ViewerShadingMode.Normals: return this.L("viewer.shading.normals", "Визуализация нормалей");
                case ViewerShadingMode.Skinning: return this.L("viewer.shading.skinning", "Скиннинг / веса костей");
                case ViewerShadingMode.Deformation: return this.L("viewer.shading.deformation", "Деформация: сжатие / растяжение");
                default: return mode.ToString();
            }
        }

        private void BuildShadingModeButtons()
        {
            ViewerShadingMode[] modes = new ViewerShadingMode[]
            {
                ViewerShadingMode.Wireframe,
                ViewerShadingMode.Solid,
                ViewerShadingMode.Textured,
                ViewerShadingMode.Normals,
                ViewerShadingMode.Skinning,
                ViewerShadingMode.Deformation
            };

            foreach (ViewerShadingMode mode in modes)
            {
                ToolStripButton button = new ToolStripButton
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                    Image = CreateShadingThumbnail(mode, this._isDarkTheme, mode == this._shadingMode),
                    ImageScaling = ToolStripItemImageScaling.None,
                    AutoSize = false,
                    Size = new System.Drawing.Size(36, 24),
                    Padding = Padding.Empty,
                    Margin = Padding.Empty,
                    CheckOnClick = false,
                    ToolTipText = GetShadingModeLabel(mode),
                    AccessibleName = GetShadingModeLabel(mode),
                    Name = "viewer.shading." + mode.ToString().ToLowerInvariant(),
                    Tag = mode
                };
                button.Click += (sender, args) => this.SetShadingMode(mode);
                this._shadingModeButtons[mode] = button;
            }
            this.UpdateShadingModeButtonsState();
        }

        private static Bitmap CreateShadingThumbnail(ViewerShadingMode mode, bool isDark, bool isSelected)
        {
            Bitmap bitmap = new Bitmap(36, 24);
            System.Drawing.Color outlineColor = isDark
                ? System.Drawing.Color.FromArgb(220, 225, 230)
                : System.Drawing.Color.FromArgb(55, 60, 65);
            System.Drawing.Color darkColor = isDark
                ? System.Drawing.Color.FromArgb(70, 75, 82)
                : System.Drawing.Color.FromArgb(175, 180, 186);
            using (Graphics g = Graphics.FromImage(bitmap))
            using (System.Drawing.Pen outline = new System.Drawing.Pen(outlineColor, 1.3f))
            using (System.Drawing.Pen dark = new System.Drawing.Pen(darkColor, 1.0f))
            using (SolidBrush gray = new SolidBrush(System.Drawing.Color.FromArgb(155, 160, 168)))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(isDark ? System.Drawing.Color.FromArgb(38, 40, 44) : System.Drawing.Color.FromArgb(232, 234, 236));
                // Draw with the original 48x32 design coordinates onto the smaller 36x24 thumbnail.
                g.ScaleTransform(0.75f, 0.75f);
                if (mode == ViewerShadingMode.Wireframe)
                {
                    System.Drawing.Point[] a = { new System.Drawing.Point(8, 24), new System.Drawing.Point(23, 5), new System.Drawing.Point(40, 24), new System.Drawing.Point(8, 24) };
                    g.DrawLines(outline, a);
                    g.DrawLine(outline, 23, 5, 23, 24);
                    g.DrawLine(outline, 8, 24, 31, 15);
                    g.DrawLine(outline, 40, 24, 15, 15);
                    using (SolidBrush vertexBrush = new SolidBrush(System.Drawing.Color.FromArgb(70, 200, 255)))
                    {
                        foreach (System.Drawing.Point pt in new[] { new System.Drawing.Point(8, 24), new System.Drawing.Point(23, 5), new System.Drawing.Point(40, 24), new System.Drawing.Point(23, 24), new System.Drawing.Point(31, 15), new System.Drawing.Point(15, 15) })
                            g.FillEllipse(vertexBrush, pt.X - 1.8f, pt.Y - 1.8f, 3.6f, 3.6f);
                    }
                }
                else if (mode == ViewerShadingMode.Solid)
                {
                    g.FillEllipse(gray, 10, 3, 28, 26);
                    g.DrawEllipse(outline, 10, 3, 28, 26);
                    g.DrawArc(dark, 13, 6, 22, 20, 250, 150);
                }
                else if (mode == ViewerShadingMode.Textured)
                {
                    using (SolidBrush blue = new SolidBrush(System.Drawing.Color.FromArgb(55, 120, 180)))
                    using (SolidBrush tan = new SolidBrush(System.Drawing.Color.FromArgb(180, 145, 90)))
                    using (SolidBrush green = new SolidBrush(System.Drawing.Color.FromArgb(75, 150, 95)))
                    {
                        g.FillEllipse(blue, 10, 3, 28, 26);
                        g.FillPie(green, 10, 3, 28, 26, 35, 125);
                        g.FillPie(tan, 10, 3, 28, 26, 200, 85);
                        g.DrawEllipse(outline, 10, 3, 28, 26);
                    }
                }
                else if (mode == ViewerShadingMode.Normals)
                {
                    g.FillEllipse(gray, 10, 3, 28, 26);
                    g.DrawEllipse(outline, 10, 3, 28, 26);
                    using (System.Drawing.Pen red = new System.Drawing.Pen(System.Drawing.Color.IndianRed, 2f))
                    using (System.Drawing.Pen green = new System.Drawing.Pen(System.Drawing.Color.LightGreen, 2f))
                    using (System.Drawing.Pen blue = new System.Drawing.Pen(System.Drawing.Color.DeepSkyBlue, 2f))
                    {
                        g.DrawLine(red, 23, 16, 37, 12);
                        g.DrawLine(green, 23, 16, 14, 7);
                        g.DrawLine(blue, 23, 16, 23, 3);
                    }
                }
                else if (mode == ViewerShadingMode.Skinning)
                {
                    using (SolidBrush red = new SolidBrush(System.Drawing.Color.IndianRed))
                    using (SolidBrush green = new SolidBrush(System.Drawing.Color.LightGreen))
                    using (SolidBrush blue = new SolidBrush(System.Drawing.Color.DeepSkyBlue))
                    using (SolidBrush gold = new SolidBrush(System.Drawing.Color.Goldenrod))
                    {
                        g.FillPolygon(red, new[] { new System.Drawing.Point(7, 24), new System.Drawing.Point(17, 5), new System.Drawing.Point(23, 17) });
                        g.FillPolygon(green, new[] { new System.Drawing.Point(17, 5), new System.Drawing.Point(34, 7), new System.Drawing.Point(23, 17) });
                        g.FillPolygon(blue, new[] { new System.Drawing.Point(34, 7), new System.Drawing.Point(41, 25), new System.Drawing.Point(23, 17) });
                        g.FillPolygon(gold, new[] { new System.Drawing.Point(7, 24), new System.Drawing.Point(23, 17), new System.Drawing.Point(41, 25) });
                        g.DrawPolygon(outline, new[] { new System.Drawing.Point(7, 24), new System.Drawing.Point(17, 5), new System.Drawing.Point(34, 7), new System.Drawing.Point(41, 25) });
                    }
                }
                else
                {
                    using (SolidBrush compress = new SolidBrush(System.Drawing.Color.DeepSkyBlue))
                    using (SolidBrush stable = new SolidBrush(System.Drawing.Color.LightGreen))
                    using (SolidBrush stretch = new SolidBrush(System.Drawing.Color.IndianRed))
                    {
                        g.FillPolygon(compress, new[] { new System.Drawing.Point(6, 24), new System.Drawing.Point(17, 6), new System.Drawing.Point(20, 19) });
                        g.FillPolygon(stable, new[] { new System.Drawing.Point(17, 6), new System.Drawing.Point(34, 7), new System.Drawing.Point(22, 18) });
                        g.FillPolygon(stretch, new[] { new System.Drawing.Point(34, 7), new System.Drawing.Point(41, 24), new System.Drawing.Point(22, 18) });
                        g.DrawPolygon(outline, new[] { new System.Drawing.Point(6, 24), new System.Drawing.Point(17, 6), new System.Drawing.Point(34, 7), new System.Drawing.Point(41, 24) });
                    }
                }

                // Put the active-state frame inside the bitmap itself so the buttons can
                // exactly match the thumbnail dimensions with no padding/gaps around them.
                if (isSelected)
                {
                    g.ResetTransform();
                    using (var activePen = new System.Drawing.Pen(
                        isDark ? System.Drawing.Color.FromArgb(0, 160, 255) : System.Drawing.SystemColors.Highlight, 2.0f))
                    {
                        g.DrawRectangle(activePen, 1, 1, bitmap.Width - 3, bitmap.Height - 3);
                    }
                }
            }
            return bitmap;
        }

        private void UpdateShadingModeButtonsState()
        {
            foreach (KeyValuePair<ViewerShadingMode, ToolStripButton> pair in this._shadingModeButtons)
            {
                bool isSelected = pair.Key == this._shadingMode;
                pair.Value.Checked = isSelected;

                // Selected outline is baked into a tiny regenerated thumbnail. The button
                // itself is exactly 36x24 with zero margin and padding, eliminating the
                // light/dark gutters that ToolStrip otherwise paints around each image.
                System.Drawing.Image oldImage = pair.Value.Image;
                pair.Value.Image = CreateShadingThumbnail(pair.Key, this._isDarkTheme, isSelected);
                if (oldImage != null && !ReferenceEquals(oldImage, pair.Value.Image)) oldImage.Dispose();

                pair.Value.BackColor = this.toolStrip != null
                    ? this.toolStrip.BackColor
                    : (this._isDarkTheme ? System.Drawing.Color.FromArgb(32, 32, 34) : System.Drawing.SystemColors.Control);
                pair.Value.ForeColor = this.toolStrip != null
                    ? this.toolStrip.ForeColor
                    : (this._isDarkTheme ? System.Drawing.Color.FromArgb(220, 220, 220) : System.Drawing.SystemColors.ControlText);
                string prefix = this.L(isSelected ? "viewer.shading.tooltip.currentPrefix" : "viewer.shading.tooltip.switchPrefix",
                    isSelected ? "Текущий режим — " : "Переключить на режим — ");
                string toolTip = prefix + GetShadingModeLabel(pair.Key);
                if (pair.Key == ViewerShadingMode.Skinning)
                    toolTip += this.L("viewer.shading.skinningTooltipSuffix", ". Цвет показывает преобладающее влияние костей.");
                else if (pair.Key == ViewerShadingMode.Deformation)
                    toolTip += this.L("viewer.shading.deformationTooltipSuffix", ". Синий — сжатие площади треугольника, зелёный — без заметного изменения, красный — растяжение, пурпурный — возможный переворот грани.");
                pair.Value.ToolTipText = toolTip;
                pair.Value.AccessibleName = GetShadingModeLabel(pair.Key);
                pair.Value.AccessibleDescription = this.L(isSelected ? "viewer.shading.active" : "viewer.shading.inactive",
                    isSelected ? "Активный режим отображения" : "Неактивный режим отображения");
            }
        }

        private void SetShadingMode(ViewerShadingMode mode)
        {
            if (this._shadingMode == mode) return;

            this._shadingMode = mode;
            this.UpdateShadingModeButtonsState();

            // Shading is a render-state change, not a document/preview reload. Reloading
            // _lastNode here is unsafe while an Animation is selected: it tears down the
            // active timer and can replace the skinned mesh with a skeleton-only preview.
            // Re-apply materials and rebuild only transient shading overlays in-place.
            this.RefreshShadingModeInPlace();
        }

        private void RefreshShadingModeInPlace()
        {
            if (this.rootGroup == null) return;

            var meshModels = new HashSet<Model3D>(this._meshRenderInfos
                .Where(info => info != null && info.Model != null)
                .Select(info => (Model3D)info.Model));
            var skeletonModels = new HashSet<Model3D>();
            if (this._activeSkeleton != null && this._activeSkeleton.JointMarkers != null)
                foreach (Model3D model in this._activeSkeleton.JointMarkers)
                    if (model != null) skeletonModels.Add(model);
            if (this._activeSkeleton != null && this._activeSkeleton.BoneModels != null)
                foreach (Model3D model in this._activeSkeleton.BoneModels)
                    if (model != null) skeletonModels.Add(model);

            // Remove overlays produced by the previous shading mode before creating the
            // new set. Keep the actual mesh models and the currently loaded skeleton intact.
            foreach (Model3D model in this._loadedModels.ToList())
            {
                if (meshModels.Contains(model) || skeletonModels.Contains(model)) continue;
                this.rootGroup.Children.Remove(model);
                this._loadedModels.Remove(model);
            }

            // When the animation toolbar is displaying the skeleton, meshes are deliberately
            // hidden. Defer applying render materials/overlays until the user switches back
            // to mesh mode; ApplyAnimationDisplayMode will apply the selected mode then.
            bool skeletonOnlyPreview = this._activeAnimation != null &&
                this._activeSkeleton != null && this._animationMeshModeAvailable &&
                !this._animationMeshMode;
            if (!skeletonOnlyPreview)
                this.ApplyShadingModeToMeshes();

            if (this.host != null) this.host.Invalidate(true);
        }

        private void ReloadCurrentNodePreservingCamera()
        {
            if (this._lastNode == null || this._lastP3D == null) return;

            double yaw = this._yaw, pitch = this._pitch, distance = this._distance;
            double targetX = this._targetX, targetY = this._targetY, targetZ = this._targetZ;
            bool hasCameraView = this._hasCameraView;
            bool wasPreserving = this._preserveCameraOnReload;
            this._preserveCameraOnReload = true;
            try
            {
                this.LoadNode(this._lastNode, this._lastP3D);
            }
            finally
            {
                this._yaw = yaw;
                this._pitch = pitch;
                this._distance = distance;
                this._targetX = targetX;
                this._targetY = targetY;
                this._targetZ = targetZ;
                this._hasCameraView = hasCameraView;
                this._preserveCameraOnReload = wasPreserving;
                this.UpdateCamera();
            }
        }

        private void BuildGizmoUI()
        {
            this.gizmoViewport = new Viewport3D();
            // Use the same non-aliased WPF 3D rendering path for the orientation gizmo.
            System.Windows.Media.RenderOptions.SetEdgeMode(this.gizmoViewport, System.Windows.Media.EdgeMode.Unspecified);
            this.gizmoCamera = new PerspectiveCamera
            {
                FieldOfView = 45,
                NearPlaneDistance = 0.1,
                FarPlaneDistance = 100.0,
                // Keep the orientation gizmo in the same Z-up coordinate system as the scene.
                UpDirection = new Vector3D(0, 0, 1)
            };
            this.gizmoViewport.Camera = this.gizmoCamera;

            var gizmoGroup = new Model3DGroup();
            gizmoGroup.Children.Add(new AmbientLight(System.Windows.Media.Color.FromRgb(120, 120, 120)));
            gizmoGroup.Children.Add(new DirectionalLight(System.Windows.Media.Color.FromRgb(200, 200, 200), new Vector3D(-1, -1, -2)));

            double r = 0.06;
            gizmoGroup.Children.Add(CreateBox(new Point3D(0, 0, 0), new Point3D(1.1, 0, 0), r, System.Windows.Media.Color.FromRgb(229, 57, 53)));
            gizmoGroup.Children.Add(CreateBox(new Point3D(0, 0, 0), new Point3D(0, 1.1, 0), r, System.Windows.Media.Color.FromRgb(67, 160, 71)));
            gizmoGroup.Children.Add(CreateBox(new Point3D(0, 0, 0), new Point3D(0, 0, 1.1), r, System.Windows.Media.Color.FromRgb(30, 136, 229)));
            gizmoGroup.Children.Add(CreateBox(new Point3D(-0.08, -0.08, -0.08), new Point3D(0.08, 0.08, 0.08), 0.08, System.Windows.Media.Color.FromRgb(176, 190, 197)));

            this.gizmoViewport.Children.Add(new ModelVisual3D { Content = gizmoGroup });

            this.xLabel = new System.Windows.Controls.Label { Content = "X", Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 82, 82)), FontWeight = FontWeights.Bold, FontSize = 11, Padding = new Thickness(0) };
            this.yLabel = new System.Windows.Controls.Label { Content = "Y", Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(105, 240, 174)), FontWeight = FontWeights.Bold, FontSize = 11, Padding = new Thickness(0) };
            this.zLabel = new System.Windows.Controls.Label { Content = "Z", Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 196, 255)), FontWeight = FontWeights.Bold, FontSize = 11, Padding = new Thickness(0) };

            var gizmoCanvas = new Canvas { IsHitTestVisible = false };
            gizmoCanvas.Children.Add(this.xLabel);
            gizmoCanvas.Children.Add(this.yLabel);
            gizmoCanvas.Children.Add(this.zLabel);

            var gizmoInnerGrid = new System.Windows.Controls.Grid();
            gizmoInnerGrid.Children.Add(this.gizmoViewport);
            gizmoInnerGrid.Children.Add(gizmoCanvas);

            this.gizmoContainer = new Border
            {
                Width = 100,
                Height = 100,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                VerticalAlignment = System.Windows.VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 15, 0),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(130, 20, 20, 22)),
                CornerRadius = new CornerRadius(50),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(70, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Child = gizmoInnerGrid,
                IsHitTestVisible = false
            };
        }

        private static GeometryModel3D CreateBox(Point3D p1, Point3D p2, double radius, System.Windows.Media.Color color)
        {
            var mesh = new MeshGeometry3D();
            double minX = Math.Min(p1.X, p2.X) - radius, maxX = Math.Max(p1.X, p2.X) + radius;
            double minY = Math.Min(p1.Y, p2.Y) - radius, maxY = Math.Max(p1.Y, p2.Y) + radius;
            double minZ = Math.Min(p1.Z, p2.Z) - radius, maxZ = Math.Max(p1.Z, p2.Z) + radius;

            Point3D[] pts = new Point3D[]
            {
                new Point3D(minX, minY, minZ), new Point3D(maxX, minY, minZ),
                new Point3D(maxX, maxY, minZ), new Point3D(minX, maxY, minZ),
                new Point3D(minX, minY, maxZ), new Point3D(maxX, minY, maxZ),
                new Point3D(maxX, maxY, maxZ), new Point3D(minX, maxY, maxZ)
            };
            foreach (var pt in pts) mesh.Positions.Add(pt);

            int[] idxs = new int[]
            {
                0,2,1, 0,3,2,  1,6,5, 1,2,6,  5,7,4, 5,6,7,
                4,3,0, 4,7,3,  3,6,2, 3,7,6,  4,1,5, 4,0,1
            };
            foreach (var i in idxs) mesh.TriangleIndices.Add(i);

            var mat = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(mesh, mat);
        }

        private static GeometryModel3D CreateBlenderBone(Point3D head, Point3D tail, double boneWidth, System.Windows.Media.Color color)
        {
            var mesh = new MeshGeometry3D();
            Vector3D dir = tail - head;
            double len = dir.Length;
            if (len < 0.001) return CreateBox(head, tail, boneWidth, color);

            dir.Normalize();
            // Bones are already expressed in viewer space, where Z is up.
            Vector3D up = Math.Abs(dir.Z) < 0.9 ? new Vector3D(0, 0, 1) : new Vector3D(0, 1, 0);
            Vector3D side = Vector3D.CrossProduct(dir, up);
            side.Normalize();
            up = Vector3D.CrossProduct(side, dir);
            up.Normalize();

            Point3D mid = head + dir * (len * 0.2);

            mesh.Positions.Add(head);
            mesh.Positions.Add(tail);
            mesh.Positions.Add(mid + side * boneWidth);
            mesh.Positions.Add(mid + up * boneWidth);
            mesh.Positions.Add(mid - side * boneWidth);
            mesh.Positions.Add(mid - up * boneWidth);

            int[] idxs = new int[]
            {
                0, 2, 3,  0, 3, 4,  0, 4, 5,  0, 5, 2,
                1, 3, 2,  1, 4, 3,  1, 5, 4,  1, 2, 5
            };
            foreach (var i in idxs) mesh.TriangleIndices.Add(i);

            var mat = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(mesh, mat) { BackMaterial = mat };
        }

        private void OnViewerComboDrawItem(object sender, DrawItemEventArgs e)
        {
            System.Windows.Forms.ComboBox combo = sender as System.Windows.Forms.ComboBox;
            if (combo == null) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            System.Drawing.Color background = selected
                ? (this._isDarkTheme ? System.Drawing.Color.FromArgb(62, 62, 64) : System.Drawing.SystemColors.Highlight)
                : (this._isDarkTheme ? System.Drawing.Color.FromArgb(50, 50, 50) : System.Drawing.SystemColors.Window);
            System.Drawing.Color foreground = selected && !this._isDarkTheme
                ? System.Drawing.SystemColors.HighlightText
                : (this._isDarkTheme ? System.Drawing.Color.FromArgb(225, 225, 225) : System.Drawing.SystemColors.ControlText);

            using (var brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, e.Bounds);
            string text = e.Index >= 0 && e.Index < combo.Items.Count
                ? combo.GetItemText(combo.Items[e.Index])
                : combo.Text;
            TextRenderer.DrawText(e.Graphics, text, combo.Font, e.Bounds, foreground,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, foreground, background);
        }

        private string L(string key, string fallback)
        {
            return UiLocalization.TranslateId(key, fallback, this._uiLanguage);
        }

        private string LF(string key, string fallback, params object[] args)
        {
            return string.Format(UiLocalization.TranslateId(key, fallback, this._uiLanguage), args ?? new object[0]);
        }

        private void SetStatsText(Func<string> textFactory)
        {
            this._refreshLocalizedStats = textFactory;
            this.RefreshStatsText();
        }

        private void RefreshStatsText()
        {
            if (this.statsLabel == null) return;
            string text = this._refreshLocalizedStats == null ? string.Empty : (this._refreshLocalizedStats() ?? string.Empty);
            this.statsLabel.Text = text;
            if (this.statsToolTip != null) this.statsToolTip.SetToolTip(this.statsLabel, text);
            this.UpdateStatsLabelLayout();
        }

        private void UpdateStatsLabelLayout()
        {
            if (this.statsLabel == null || this.statsLabel.IsDisposed) return;
            int availableWidth = this.statsLabel.ClientSize.Width;
            if (availableWidth <= 1) availableWidth = Math.Max(120, this.ClientSize.Width - this.statsLabel.Padding.Horizontal - 8);
            availableWidth = Math.Max(120, availableWidth - this.statsLabel.Padding.Horizontal - 4);
            string text = this.statsLabel.Text ?? string.Empty;
            int contentHeight = string.IsNullOrEmpty(text) ? 0 : TextRenderer.MeasureText(
                text,
                this.statsLabel.Font,
                new System.Drawing.Size(availableWidth, 1000),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix).Height;
            int desiredHeight = string.IsNullOrEmpty(text) ? 26 : contentHeight + this.statsLabel.Padding.Vertical + 2;
            // Keep a readable multi-line status row without letting verbose parser warnings
            // consume the viewport; the tooltip retains the complete text when capped.
            this.statsLabel.Height = Math.Max(26, Math.Min(68, desiredHeight));
        }

        private void UpdateAnimationComboToolTip()
        {
            if (this.animationCombo == null) return;
            AnimationOption option = this.animationCombo.SelectedItem as AnimationOption;
            if (option != null && option.IsPlaceholder)
            {
                // Rebuild the explanation in the active language after language/theme changes.
                if (option.IsMultiCompositeRigMismatchPlaceholder)
                    this.animationCombo.ToolTipText = this.L("viewer.animation.multiCompositeRigMismatch.tooltip", "Для совместной анимации выберите CompositeDrawable с одинаковым SkeletonName. Статическое отображение всех выбранных моделей доступно.");
                else if (option.IsSearchingPlaceholder)
                    this.animationCombo.ToolTipText = this.L("viewer.animation.searching.tooltip", "Выполняется предварительная индексация P3D-контейнеров. Найденные анимации появятся автоматически после завершения сканирования RCF.");
                else if (this.showIncompatibleAnimationsCheckBox != null && this.showIncompatibleAnimationsCheckBox.Checked)
                    this.animationCombo.ToolTipText = this.L("viewer.animation.noAnimationsPlaceholder", "Анимации не найдены в доступных P3D-контейнерах");
                else if (this._animationDropdownMeshUnavailable && this._animationDropdownContextComposite != null)
                    this.animationCombo.ToolTipText = this.LF("viewer.animation.noSkinnedMeshForComposite",
                        "Модель '{0}' не содержит пригодных весов или привязок к костям. Просмотр анимированного меша недоступен.",
                        CleanNodeName(this._animationDropdownContextComposite.Name) ?? this.L("viewer.animation.unnamed", "без имени"));
                else
                    this.animationCombo.ToolTipText = this._animationDropdownContextComposite != null
                        ? this.LF("viewer.animation.noCompatibleForComposite",
                            "Нет совместимых скелетных анимаций для модели '{0}' (скелет: '{1}').",
                            CleanNodeName(this._animationDropdownContextComposite.Name) ?? this.L("viewer.animation.unnamed", "без имени"),
                            CleanNodeName(this._animationDropdownContextComposite.SkeletonName) ?? "—")
                        : this.L("viewer.animation.noCompatibleInP3D",
                            "В текущем P3D не найдено анимаций с каналами, совместимыми с доступными скелетами.");
                return;
            }
            this.animationCombo.ToolTipText = option != null && option.Node != null
                ? option.ToString() + (option.IsCompatible ? string.Empty : Environment.NewLine + this.L("viewer.animation.incompatible.tooltip", "Совместимость не подтверждена. При выборе программа попробует применить доступные костные каналы."))
                : this.L("viewer.tooltip.animation", "Выбрать совместимую анимацию");
        }

        private void UpdateAnimationComboLayout()
        {
            if (this.animationCombo == null || this.animationCombo.ComboBox == null) return;
            int maxAvailableWidth = this.ClientSize.Width;
            try { maxAvailableWidth = Screen.FromControl(this).WorkingArea.Width - 48; }
            catch { }
            maxAvailableWidth = Math.Max(this.animationCombo.Width, maxAvailableWidth);
            int desiredWidth = this.animationCombo.Width;
            foreach (object item in this.animationCombo.Items)
            {
                string text = item == null ? string.Empty : item.ToString();
                if (string.IsNullOrEmpty(text)) continue;
                int itemWidth = TextRenderer.MeasureText(text, this.animationCombo.ComboBox.Font, System.Drawing.Size.Empty, TextFormatFlags.NoPadding).Width;
                desiredWidth = Math.Max(desiredWidth, itemWidth + SystemInformation.VerticalScrollBarWidth + 14);
            }
            this.animationCombo.DropDownWidth = Math.Min(maxAvailableWidth, desiredWidth);
            this.UpdateAnimationComboToolTip();
        }

        private void ApplyViewerToolTips()
        {
            if (this.shadingModeLabel != null)
                this.shadingModeLabel.ToolTipText = this.L("viewer.tooltip.shading", "Режим отображения модели");
            if (this.flipUvButton != null)
                this.flipUvButton.ToolTipText = this.L("viewer.tooltip.flipUv", "По умолчанию включено: UVCoordinate уже инвертирует V при чтении P3D. Повторная инверсия здесь восстанавливает исходную ориентацию UV для WPF. Снимите отметку, чтобы посмотреть альтернативную ориентацию.");
            if (this.textureOverrideCombo != null)
                this.textureOverrideCombo.ToolTipText = this.L("viewer.tooltip.textureOverride", "Выбрать текстуру вручную для этой модели");
            if (this.animationCombo != null)
                this.animationCombo.ToolTipText = this.L("viewer.tooltip.animation", "Выбрать анимацию из текущего P3D");
            if (this.animationDisplayModeButton != null)
                this.animationDisplayModeButton.ToolTipText = this.L("viewer.animation.displayMode.tooltip", "Переключить между деформируемым мешем и скелетом");
            if (this.showIncompatibleAnimationsCheckBox != null)
                this.showIncompatibleAnimationsCheckBox.Text = this.L("viewer.animation.showIncompatible", "Показывать несовместимые");
            if (this.showIncompatibleAnimationsHost != null)
                this.showIncompatibleAnimationsHost.ToolTipText = this.L("viewer.animation.showIncompatible.tooltip", "Показывать также анимации, которые не удалось однозначно сопоставить со скелетом");
            if (this.playAnimationButton != null)
                this.playAnimationButton.ToolTipText = this.L("viewer.tooltip.play", "Воспроизвести / приостановить анимацию");
            if (this.loopAnimationButton != null)
                this.loopAnimationButton.ToolTipText = this.L("viewer.tooltip.loop", "Зациклить анимацию");
            if (this.animationFrameLabel != null)
                this.animationFrameLabel.ToolTipText = this.L("viewer.tooltip.frame", "Текущий кадр анимации");
        }

        public void ApplyUiLanguage(string languageCode)
        {
            this._uiLanguage = UiLocalization.NormalizeLanguageCode(languageCode);
            UiLocalization.ApplyControlTree(this, this._uiLanguage);

            // The texture names are resource data and must remain untouched; only the first
            // UI-owned entry in the dropdown is localized. Suppress SelectionChanged while
            // the displayed label is updated so a language switch never reloads the model.
            if (this.textureOverrideCombo != null && this.textureOverrideCombo.Items.Count > 0)
            {
                int selectedIndex = this.textureOverrideCombo.SelectedIndex;
                this._applyingUiLanguage = true;
                try
                {
                    this.textureOverrideCombo.Items[0] = this.L("viewer.texture.auto", "Автовыбор текстуры");
                    if (selectedIndex >= 0 && selectedIndex < this.textureOverrideCombo.Items.Count)
                        this.textureOverrideCombo.SelectedIndex = selectedIndex;
                }
                finally { this._applyingUiLanguage = false; }
            }
            if (this.animationCombo != null)
            {
                foreach (object item in this.animationCombo.Items)
                {
                    AnimationOption option = item as AnimationOption;
                    if (option != null) option.LanguageCode = this._uiLanguage;
                }
                this.animationCombo.Invalidate();
                this.UpdateAnimationComboToolTip();
            }

            this.ApplyViewerToolTips();
            this.UpdateShadingModeButtonsState();
            this.UpdateAnimationDisplayModeButton();
            this.UpdateAnimationFrameLabel();
            this.RefreshStatsText();
            this.UpdateAnimationComboLayout();
        }

        public void ApplyTheme(bool isDark)
        {
            if (this.IsDisposed) return;
            this._isDarkTheme = isDark;

            System.Drawing.Color bg = isDark
                ? System.Drawing.Color.FromArgb(32, 32, 34)
                : System.Drawing.SystemColors.Control;
            System.Drawing.Color input = isDark
                ? System.Drawing.Color.FromArgb(50, 50, 50)
                : System.Drawing.SystemColors.Window;
            System.Drawing.Color fg = isDark
                ? System.Drawing.Color.FromArgb(220, 220, 220)
                : System.Drawing.SystemColors.ControlText;

            // WinForms ToolStrip uses its renderer for most item backgrounds; style both
            // the strip and each item so labels, checked buttons, separators and hosted
            // controls do not fall back to the system's light palette.
            System.Windows.Forms.ToolStrip[] strips = new System.Windows.Forms.ToolStrip[] { this.toolStrip, this.animationToolStrip };
            foreach (System.Windows.Forms.ToolStrip strip in strips)
            {
                if (strip == null) continue;
                strip.BackColor = bg;
                strip.ForeColor = fg;
                foreach (ToolStripItem item in strip.Items)
                {
                    if (item == null) continue;
                    item.BackColor = bg;
                    item.ForeColor = fg;
                }
            }

            this.textureOverrideCombo.BackColor = input;
            this.textureOverrideCombo.ForeColor = fg;
            if (this.textureOverrideCombo.ComboBox != null)
            {
                this.textureOverrideCombo.ComboBox.BackColor = input;
                this.textureOverrideCombo.ComboBox.ForeColor = fg;
                this.textureOverrideCombo.ComboBox.FlatStyle = FlatStyle.Flat;
            }
            this.animationCombo.BackColor = input;
            this.animationCombo.ForeColor = fg;
            if (this.animationCombo.ComboBox != null)
            {
                this.animationCombo.ComboBox.BackColor = input;
                this.animationCombo.ComboBox.ForeColor = fg;
                this.animationCombo.ComboBox.FlatStyle = FlatStyle.Flat;
            }

            this.animationTrackHost.BackColor = bg;
            this.animationTrackHost.ForeColor = fg;
            if (this.showIncompatibleAnimationsCheckBox != null)
            {
                this.showIncompatibleAnimationsCheckBox.BackColor = bg;
                this.showIncompatibleAnimationsCheckBox.ForeColor = fg;
            }
            if (this.showIncompatibleAnimationsHost != null)
            {
                this.showIncompatibleAnimationsHost.BackColor = bg;
                this.showIncompatibleAnimationsHost.ForeColor = fg;
            }
            this.animationTrackBar.BackColor = bg;
            this.animationTrackBar.ForeColor = fg;
            this.animationFrameLabel.ForeColor = fg;
            this.statsLabel.BackColor = bg;
            this.statsLabel.ForeColor = isDark
                ? System.Drawing.Color.FromArgb(190, 190, 190)
                : System.Drawing.SystemColors.GrayText;
            this.UpdateShadingModeButtonsState();

            this.host.BackColor = bg;
            this.host.ForeColor = fg;
            byte c = (byte)(isDark ? 26 : 240);
            var background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(c, c, c));
            background.Freeze();
            this.wpfGrid.Background = background;

            // Theme switches must not recreate the ElementHost or the WPF scene. Refresh
            // the existing visual tree asynchronously at render priority so the viewport,
            // overlays and axis gizmo continue to receive input and redraw normally.
            this.toolStrip.Invalidate(true);
            this.animationToolStrip.Invalidate(true);
            this.Invalidate(true);
            try
            {
                this.wpfGrid.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new Action(() =>
                    {
                        if (this.IsDisposed || this.host == null || this.host.IsDisposed) return;
                        this.wpfGrid.UpdateLayout();
                        this.wpfGrid.InvalidateVisual();
                        this.viewport.InvalidateVisual();
                        if (this.gizmoViewport != null) this.gizmoViewport.InvalidateVisual();
                        this.host.Invalidate(true);
                    }));
            }
            catch (InvalidOperationException) { /* The WPF dispatcher may be closing during disposal. */ }
        }

        private void UpdateLights()
        {
            for (int i = this.rootGroup.Children.Count - 1; i >= 0; i--)
            {
                if (this.rootGroup.Children[i] is Light) this.rootGroup.Children.RemoveAt(i);
            }

            this.rootGroup.Children.Add(new AmbientLight(System.Windows.Media.Color.FromRgb(95, 95, 100)));
            this.rootGroup.Children.Add(new DirectionalLight(System.Windows.Media.Color.FromRgb(225, 225, 230), new Vector3D(-1, -2, -1.5)));
            this.rootGroup.Children.Add(new DirectionalLight(System.Windows.Media.Color.FromRgb(130, 130, 140), new Vector3D(1, 1.5, 2)));
        }

        private void PopulateTextureDropdown(Pure3DFile p3d)
        {
            this.textureOverrideCombo.Items.Clear();
            this.textureOverrideCombo.Items.Add(this.L("viewer.texture.auto", "Автовыбор текстуры"));

            if (p3d != null && p3d.Nodes != null)
            {
                foreach (var n in p3d.Nodes)
                {
                    if (n.Exportable || n.ToString().Contains("Texture"))
                    {
                        string name = GetCleanName(n);
                        this.textureOverrideCombo.Items.Add(name);
                    }
                }
            }
            this.textureOverrideCombo.SelectedIndex = 0;
        }

        public bool LoadCompositeDrawables(IEnumerable<KeyValuePair<CompositeDrawable, Pure3DFile>> composites)
        {
            var unique = new List<CompositePreviewSource>();
            var seen = new HashSet<CompositeDrawable>();
            if (composites != null)
            {
                foreach (var pair in composites)
                {
                    if (pair.Key == null || pair.Value == null || !seen.Add(pair.Key)) continue;
                    unique.Add(new CompositePreviewSource { Node = pair.Key, Composite = pair.Key, P3D = pair.Value });
                }
            }
            if (unique.Count == 0) return false;
            CompositePreviewSource primary = unique[unique.Count - 1];
            if (unique.Count == 1) return this.LoadNode(primary.Composite, primary.P3D);
            this._compositePreviewBatchForLoad = unique;
            try { return this.LoadNode(primary.Composite, primary.P3D); }
            finally { this._compositePreviewBatchForLoad = null; }
        }

        public bool LoadNode(BaseNode baseNode, Pure3DFile p3d)
        {
            // Keep the most recently selected CompositeDrawable's SkeletonName as context for
            // animation-combo changes. Validate the target before clearing the current viewport;
            // failed preview attempts must not leave a blank scene.
            if (!object.ReferenceEquals(this._lastP3D, p3d))
            {
                this._preferredSkeletonName = null;
                this._animationMeshSourceComposite = null;
                this._animationMeshSourceP3D = null;
                this._animationMeshSourceComposites.Clear();
            }
            var selectedComposite = baseNode as CompositeDrawable;
            if (selectedComposite != null)
            {
                // A new composite load will establish its own verified context only after geometry resolves.
                this._animationMeshSourceComposite = null;
                this._animationMeshSourceP3D = null;
                this._animationMeshSourceComposites.Clear();
                this._preferredSkeletonName = null;
                if (!string.IsNullOrWhiteSpace(selectedComposite.SkeletonName))
                    this._preferredSkeletonName = CleanNodeName(selectedComposite.SkeletonName);
            }
            else if (!(baseNode is Animation) || !this._loadingAnimationFromCombo)
            {
                // Do not carry a previous model's rig into an unrelated tree selection.
                // Only combo selections retain the CompositeDrawable context that filtered the list.
                this._preferredSkeletonName = null;
                this._animationMeshSourceComposite = null;
                this._animationMeshSourceP3D = null;
                this._animationMeshSourceComposites.Clear();
            }

            Skeleton resolvedAnimationSkeleton = null;
            var requestedAnimation = baseNode as Animation;
            bool wasAnimationPreview = this._lastNode is Animation && this._activeAnimation != null;
            bool previousAnimationMeshMode = this._animationMeshMode;
            bool preserveSkinnedMesh = false;
            int resolvedAnimationJointCount = 0;
            if (requestedAnimation != null)
            {
                bool cachedComboResolution = this._loadingAnimationFromCombo &&
                    object.ReferenceEquals(this._comboRequestedAnimation, requestedAnimation) &&
                    object.ReferenceEquals(this._comboRequestedP3D, p3d) &&
                    this._comboRequestedSkeleton != null;
                string skeletonHint = this._loadingAnimationFromCombo ? this._preferredSkeletonName : null;
                resolvedAnimationSkeleton = cachedComboResolution
                    ? this._comboRequestedSkeleton
                    : this.FindSkeletonForAnimation(requestedAnimation, p3d, skeletonHint);
                if (resolvedAnimationSkeleton == null)
                {
                    string requestedName = CleanNodeName(requestedAnimation.Name) ?? CleanNodeName(requestedAnimation.AnimationType) ?? this.L("viewer.animation.unnamed", "без имени");
                    this.SetAnimationPreviewFailure(() => this.LF("viewer.error.animationSkeletonNotFound",
                        "Не найден подходящий скелет для анимации '{0}'. Выберите CompositeDrawable нужной модели, затем выберите анимацию.", requestedName));
                    return false;
                }
                resolvedAnimationJointCount = this.ExtractJoints(resolvedAnimationSkeleton).Count;
                if (resolvedAnimationJointCount == 0)
                {
                    string skeletonName = CleanNodeName(resolvedAnimationSkeleton.Name) ?? this.L("viewer.animation.unnamed", "без имени");
                    this.SetAnimationPreviewFailure(() => this.LF("viewer.error.animationSkeletonJointsMissing",
                        "Скелет '{0}' не содержит распознанных SkeletonJoint. Текущий просмотр оставлен без изменений.", skeletonName));
                    return false;
                }
            }

            bool selectedFromCurrentComposite = requestedAnimation != null && this._loadingAnimationFromCombo &&
                object.ReferenceEquals(this._comboRequestedAnimation, requestedAnimation) &&
                object.ReferenceEquals(this._comboRequestedP3D, p3d) &&
                object.ReferenceEquals(this._comboRequestedSkeleton, resolvedAnimationSkeleton) &&
                object.ReferenceEquals(this._animationDropdownContextComposite, this._animationMeshSourceComposite);
            bool meshSkeletonMatches = this._animationMeshSourceComposite != null &&
                SkeletonNamesMatch(resolvedAnimationSkeleton == null ? null : resolvedAnimationSkeleton.Name,
                    this._animationMeshSourceComposite.SkeletonName);
            bool modelContextAllowsUnlabelledSkeleton = selectedFromCurrentComposite &&
                this._animationMeshSourceComposite != null &&
                string.IsNullOrWhiteSpace(this._animationMeshSourceComposite.SkeletonName);
            if (requestedAnimation != null && resolvedAnimationSkeleton != null &&
                object.ReferenceEquals(this._lastP3D, p3d) &&
                object.ReferenceEquals(this._animationMeshSourceP3D, p3d) &&
                this._animationMeshSourceComposite != null &&
                this.AreAnimationSourceCompositesUsingSameRig() &&
                (meshSkeletonMatches || modelContextAllowsUnlabelledSkeleton) &&
                this._meshRenderInfos.Any(info => info != null && info.Model != null && info.Mesh != null &&
                    HasUsableSkinning(info, resolvedAnimationJointCount, resolvedAnimationSkeleton.Name)))
            {
                preserveSkinnedMesh = true;
            }

            // Capture the current view before the next node replaces the scene. When the
            // next model is built, its center and fit distance will change, but the user's
            // orbit, zoom ratio, and pan offset relative to the current model are preserved.
            // Internal preview reloads use their separate exact-camera preservation path.
            this.CaptureCameraTransitionForNextScene();

            this.StopAnimationPlayback();
            this._activeAnimation = null;
            this._activeAnimationFrameCount = 1.0f;
            this._activeSkeleton = null;
            this._activeAnimationTracks.Clear();
            this.SetAnimationControlsEnabled(false);
            this._animationMeshModeAvailable = false;
            this._animationMeshMode = false;
            this.UpdateAnimationDisplayModeButton();
            if (this.animationDisplayModeButton != null) this.animationDisplayModeButton.Enabled = false;
            this._lastNode = baseNode;
            bool documentChanged = !object.ReferenceEquals(this._lastP3D, p3d);
            if (documentChanged)
            {
                this._lastP3D = p3d;
                this.PopulateTextureDropdown(p3d);
            }
            CompositeDrawable animationContext = selectedComposite;
            if (animationContext == null && this._loadingAnimationFromCombo &&
                object.ReferenceEquals(this._animationMeshSourceP3D, p3d))
                animationContext = this._animationMeshSourceComposite;
            bool requireAnimationMesh = this._loadingAnimationFromCombo && animationContext != null &&
                object.ReferenceEquals(this._animationMeshSourceComposite, animationContext) &&
                object.ReferenceEquals(this._animationMeshSourceP3D, p3d);
            this.PopulateAnimationDropdown(p3d, animationContext, requireAnimationMesh);
            this.SyncAnimationSelection(baseNode);

            List<MeshRenderInfo> retainedMeshInfos = preserveSkinnedMesh
                ? this._meshRenderInfos.Where(info => info != null && info.Model != null && info.Mesh != null).ToList()
                : new List<MeshRenderInfo>();
            var retainedModels = new HashSet<Model3D>(retainedMeshInfos.Select(info => (Model3D)info.Model));
            foreach (var m in this._loadedModels.ToList())
            {
                if (!retainedModels.Contains(m)) this.rootGroup.Children.Remove(m);
            }
            this._loadedModels.Clear();
            foreach (var model in retainedModels)
            {
                if (!this.rootGroup.Children.Contains(model)) this.rootGroup.Children.Add(model);
                this._loadedModels.Add(model);
            }
            this._meshRenderInfos.Clear();
            this._meshRenderInfos.AddRange(retainedMeshInfos);
            if (!preserveSkinnedMesh)
                this._sourceSkeletonPoseStates.Clear();

            if (baseNode == null)
            {
                this.SetStatsText(null);
                return false;
            }

            // Анимация отображается на найденном совместимом скелете. Сначала строим
            // bind pose, затем применяем к нему каналы выбранной Animation.
            if (baseNode is Animation)
            {
                var animation = (Animation)baseNode;
                var skeleton = resolvedAnimationSkeleton ?? this.FindSkeletonForAnimation(animation, p3d, null);
                if (skeleton == null)
                {
                    this.SetAnimationPreviewFailure(() => this.L("viewer.error.animationSkeletonMissing",
                        "Для этой анимации не найден подходящий Skeleton/SkeletonJoint."));
                    return false;
                }
                if (!this.LoadSkeleton(skeleton))
                {
                    this.SetAnimationPreviewFailure(() => this.L("viewer.error.animationSkeletonBuildFailed",
                        "Не удалось построить скелет для выбранной анимации. Текущий просмотр оставлен без изменений."));
                    return false;
                }
                // Decode external keyframes on a deep-cloned animation node. Previewing a model
                // must not turn AnimationDataReference channels into inline-frame channels in
                // the live P3D tree, otherwise a later save could rewrite the source structure.
                Animation previewAnimation;
                bool isIsolatedExternalClip = this._loadingAnimationFromCombo &&
                    this._materializedExternalAnimationOption != null &&
                    ReferenceEquals(this._materializedExternalAnimationOption.Node, animation) &&
                    ReferenceEquals(this._comboRequestedAnimation, animation);
                try
                {
                    // This external clip was just deserialized from its own RCF root-chunk
                    // stream and is not part of a live editable P3D tree. Preview it in place
                    // to avoid a second full clone of potentially huge keyframe arrays.
                    previewAnimation = isIsolatedExternalClip ? animation : animation.Clone() as Animation;
                }
                catch (Exception ex)
                {
                    this.SetStatsText(() => this.LF("viewer.stats.animationCloneError", "Не удалось подготовить копию анимации для просмотра: {0}", ex.Message));
                    return false;
                }
                if (previewAnimation == null)
                {
                    this.SetStatsText(() => this.L("viewer.stats.animationCloneFailed", "Не удалось подготовить копию анимации для просмотра."));
                    return false;
                }
                this.ConfigureAnimationPreview(previewAnimation);
                this._animationMeshModeAvailable = preserveSkinnedMesh && this._meshRenderInfos.Any(info => info != null &&
                    HasUsableSkinning(info, this._activeSkeleton == null ? 0 : this._activeSkeleton.Joints.Count,
                        this._activeSkeleton == null || this._activeSkeleton.SkeletonNode == null ? null : this._activeSkeleton.SkeletonNode.Name));
                // If the user chose a clip while viewing its CompositeDrawable, keep that
                // character mesh as the preview. When changing clips, preserve the current mode.
                this._animationMeshMode = this._animationMeshModeAvailable &&
                    (wasAnimationPreview ? previousAnimationMeshMode : preserveSkinnedMesh);
                this.UpdateAnimationDisplayModeButton();
                if (this._animationMeshModeAvailable) this.ApplyAnimationDisplayMode();
                if (preserveSkinnedMesh) this.FitCameraToMeshBounds(!this._hasCameraView);
                return true;
            }

            var selectedSkeleton = baseNode as Skeleton ?? baseNode.GetParentNode<Skeleton>();
            if (selectedSkeleton != null &&
                (baseNode is Skeleton || baseNode is SkeletonJoint || baseNode is SkeletonPartition || baseNode is SkeletonLimb))
            {
                return this.LoadSkeleton(selectedSkeleton);
            }

            // 2. 3D МОДЕЛИ
            int totalVerts = 0, totalTris = 0;
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
            string lastError = null;

            List<CompositePreviewSource> geometrySources = this._compositePreviewBatchForLoad != null && this._compositePreviewBatchForLoad.Count > 0
                ? this._compositePreviewBatchForLoad
                : new List<CompositePreviewSource> { new CompositePreviewSource { Node = baseNode, Composite = selectedComposite, P3D = p3d } };

            foreach (CompositePreviewSource geometrySource in geometrySources)
            {
                Pure3DFile geometryP3D = geometrySource.P3D;
                try
                {
                    List<Prototype1GeometryReader.PrimitiveGroupSource> p1Groups =
                        Prototype1GeometryReader.ResolveGroups(geometrySource.Node, geometryP3D);

                    foreach (var pair in p1Groups)
                    {
                    var pg = pair.Group;
                    string contextName = pair.OwnerName;

                    try
                    {
                        Prototype1GeometryData geometry = Prototype1GeometryReader.Read(pg);
                        var rawVerts = geometry.Positions;
                        var rawFaces = geometry.Triangles;
                        var rawUVs = geometry.UVs;
                        var rawNormals = geometry.Normals;
                        if (rawVerts == null || rawFaces == null || rawVerts.Count == 0 || rawFaces.Count == 0)
                            continue;

                        var mesh = new MeshGeometry3D();
                        var positions = new Point3DCollection(rawVerts.Count);
                        PointCollection uvs = (rawUVs != null && rawUVs.Count == rawVerts.Count && rawUVs.All(u => u != null))
                            ? new PointCollection(rawUVs.Count) : null;
                        Vector3DCollection normals = (rawNormals != null && rawNormals.Count == rawVerts.Count && rawNormals.All(n => n != null))
                            ? new Vector3DCollection(rawNormals.Count) : null;

                        for (int i = 0; i < rawVerts.Count; i++)
                        {
                            double x = rawVerts[i].X * 1000.0, y = rawVerts[i].Z * -1000.0, z = rawVerts[i].Y * 1000.0;
                            if (x < minX) minX = x; if (x > maxX) maxX = x;
                            if (y < minY) minY = y; if (y > maxY) maxY = y;
                            if (z < minZ) minZ = z; if (z > maxZ) maxZ = z;
                            positions.Add(new Point3D(x, y, z));

                            if (uvs != null)
                            {
                                double u = rawUVs[i].U;
                                double v = this._flipUv ? (1.0 - rawUVs[i].V) : rawUVs[i].V;
                                uvs.Add(new System.Windows.Point(u, v));
                            }

                            if (normals != null)
                            {
                                // Apply the same axis conversion as positions. Triangle winding below is kept
                                // compatible with the legacy viewer, so do not invert these source normals.
                                double nx = rawNormals[i].X;
                                double ny = -rawNormals[i].Z;
                                double nz = rawNormals[i].Y;
                                double length = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                                if (length > 1e-12 && !double.IsNaN(length) && !double.IsInfinity(length))
                                    normals.Add(new Vector3D(nx / length, ny / length, nz / length));
                                else
                                    normals.Add(new Vector3D(0, 0, 1));
                            }
                        }
                        mesh.Positions = positions;
                        if (uvs != null) mesh.TextureCoordinates = uvs;
                        if (normals != null) mesh.Normals = normals;

                        var indices = new Int32Collection(rawFaces.Count * 3);
                        for (int i = 0; i < rawFaces.Count; i++)
                        {
                            // The reader validates every index against the decoded vertex array.
                            indices.Add(rawFaces[i].A);
                            indices.Add(rawFaces[i].C);
                            indices.Add(rawFaces[i].B);
                        }
                        mesh.TriangleIndices = indices;

                        Material solidMaterial = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 195, 202)));
                        ImageSource textureSource = geometryP3D != null ? ResolveTexture(pg, contextName, geometryP3D) : null;
                        Material texturedMaterial = textureSource != null && uvs != null
                            ? CreateTexturedMaterial(textureSource)
                            : solidMaterial;
                        Material initialMaterial = this._shadingMode == ViewerShadingMode.Textured ? texturedMaterial : solidMaterial;

                        var model = new GeometryModel3D(mesh, initialMaterial) { BackMaterial = initialMaterial };
                        this.rootGroup.Children.Add(model);
                        this._loadedModels.Add(model);
                        PrimitiveMatrix primitiveMatrix = pg.GetChildNode<PrimitiveMatrix>();
                        var renderInfo = new MeshRenderInfo
                        {
                            Model = model,
                            Mesh = mesh,
                            SolidMaterial = solidMaterial,
                            TexturedMaterial = texturedMaterial,
                            Weights = geometry.Weights ?? new List<Nixson.Common.Vector3>(),
                            Groups = geometry.Groups ?? new List<byte[]>(),
                            MatrixIndices = primitiveMatrix != null && primitiveMatrix.Indices != null ? primitiveMatrix.Indices : new uint[0],
                            RigidBoneIndex = pair.RigidBoneIndex
                        };
                        this.PrepareP1SkinningData(renderInfo, geometry, pg, rawVerts, rawNormals, geometryP3D);
                        this._meshRenderInfos.Add(renderInfo);

                        totalVerts += rawVerts.Count;
                        totalTris += rawFaces.Count;
                    }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                    }
                }
                }
                catch (Exception ex)
                {
                    lastError = lastError == null ? ex.Message : lastError + "; " + ex.Message;
                }
            }

            // Prototype 2 stores vertex, skin and index streams in separate P2Primitive nodes.
            // EditP3D supports this representation; this WPF implementation intentionally
            // previews static geometry first and does not yet apply P2 skeletal weights.
            foreach (CompositePreviewSource geometrySource in geometrySources)
            {
                Pure3DFile geometryP3D = geometrySource.P3D;
                try
                {
                    foreach (P2PreviewMeshData p2Data in BuildP2PreviewMeshes(geometrySource.Node, geometryP3D))
                    {
                    if (p2Data.Positions == null || p2Data.Positions.Count == 0 ||
                        p2Data.TriangleIndices == null || p2Data.TriangleIndices.Count < 3)
                    {
                        continue;
                    }

                    var mesh = new MeshGeometry3D();
                    var positions = new Point3DCollection(p2Data.Positions.Count);
                    PointCollection uvs = p2Data.UVs != null && p2Data.UVs.Count == p2Data.Positions.Count
                        ? new PointCollection(p2Data.UVs.Count)
                        : null;
                    Vector3DCollection normals = p2Data.Normals != null && p2Data.Normals.Count == p2Data.Positions.Count
                        ? new Vector3DCollection(p2Data.Normals.Count)
                        : null;

                    for (int i = 0; i < p2Data.Positions.Count; i++)
                    {
                        var v = p2Data.Positions[i];
                        double x = v.X * 1000.0;
                        double y = v.Z * -1000.0;
                        double z = v.Y * 1000.0;
                        if (x < minX) minX = x; if (x > maxX) maxX = x;
                        if (y < minY) minY = y; if (y > maxY) maxY = y;
                        if (z < minZ) minZ = z; if (z > maxZ) maxZ = z;
                        positions.Add(new Point3D(x, y, z));

                        if (uvs != null)
                        {
                            double u = p2Data.UVs[i].U;
                            double vCoord = this._flipUv ? (1.0 - p2Data.UVs[i].V) : p2Data.UVs[i].V;
                            uvs.Add(new System.Windows.Point(u, vCoord));
                        }

                        if (normals != null)
                        {
                            var n = p2Data.Normals[i];
                            var normal = new Vector3D(n.X, -n.Z, n.Y);
                            if (normal.LengthSquared > 0.000001) normal.Normalize();
                            normals.Add(normal);
                        }
                    }

                    mesh.Positions = positions;
                    var triangleIndices = new Int32Collection(p2Data.TriangleIndices.Count);
                    foreach (int triangleIndex in p2Data.TriangleIndices) triangleIndices.Add(triangleIndex);
                    mesh.TriangleIndices = triangleIndices;
                    if (uvs != null) mesh.TextureCoordinates = uvs;
                    if (normals != null) mesh.Normals = normals;

                    Material solidMaterial = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromRgb(190, 195, 202)));
                    ImageSource textureSource = geometryP3D != null
                        ? this.ResolveTextureByShaderName(p2Data.ShaderName, p2Data.Name, geometryP3D)
                        : null;
                    Material texturedMaterial = textureSource != null && uvs != null
                        ? CreateTexturedMaterial(textureSource)
                        : solidMaterial;
                    Material initialMaterial = this._shadingMode == ViewerShadingMode.Textured ? texturedMaterial : solidMaterial;

                    var model = new GeometryModel3D(mesh, initialMaterial) { BackMaterial = initialMaterial };
                    this.rootGroup.Children.Add(model);
                    this._loadedModels.Add(model);
                    this._meshRenderInfos.Add(new MeshRenderInfo
                    {
                        Model = model,
                        Mesh = mesh,
                        SolidMaterial = solidMaterial,
                        TexturedMaterial = texturedMaterial
                    });
                    totalVerts += p2Data.Positions.Count;
                    totalTris += p2Data.TriangleIndices.Count / 3;
                    }
                }
                catch (Exception ex)
                {
                    lastError = lastError == null ? ex.Message : lastError + "; " + ex.Message;
                }
            }

            if (this._loadedModels.Count == 0)
            {
                if (lastError != null)
                    this.SetStatsText(() => this.LF("viewer.stats.geometryError", "Ошибка: {0}", lastError));
                else
                    this.SetStatsText(() => this.L("viewer.stats.geometryEmpty", "В этом узле нет 3D геометрии"));
                return false;
            }

            this._sceneCenterX = this._targetX = (minX + maxX) * 0.5;
            this._sceneCenterY = this._targetY = (minY + maxY) * 0.5;
            this._sceneCenterZ = this._targetZ = (minZ + maxZ) * 0.5;

            double dx = maxX - minX, dy = maxY - minY, dz = maxZ - minZ;
            this._boundingRadius = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (this._boundingRadius < 0.001) this._boundingRadius = 1000.0;

            bool missingSkinningData = this.ApplyShadingModeToMeshes();
            this.FitCameraToBounds(!this._hasCameraView);
            this.SetStatsText(() =>
            {
                string text = this.LF("viewer.stats.geometry", "Вершин: {0:N0} | Полигонов: {1:N0}", totalVerts, totalTris);
                if (geometrySources.Count > 1)
                    text += this.LF("viewer.stats.compositeBatchCount", " | CompositeDrawable: {0}", geometrySources.Count);
                if (this._shadingMode == ViewerShadingMode.Skinning && missingSkinningData)
                    text += this.L("viewer.stats.geometryMissingSkinning", " | Скиннинг: нет весов для части мешей");
                return text;
            });
            if (selectedComposite != null)
            {
                this._animationMeshSourceComposite = selectedComposite;
                this._animationMeshSourceP3D = p3d;
                this._animationMeshSourceComposites = geometrySources
                    .Where(source => source != null && source.Composite != null && source.P3D != null)
                    .GroupBy(source => source.Composite)
                    .Select(group => group.Last())
                    .ToList();
                this.PopulateAnimationDropdown(p3d, selectedComposite, true);
                this.SyncAnimationSelection(baseNode);
            }
            return true;
        }

        private bool AreAnimationSourceCompositesUsingSameRig()
        {
            if (this._animationMeshSourceComposites == null || this._animationMeshSourceComposites.Count <= 1)
                return true;
            string firstName = CleanNodeName(this._animationMeshSourceComposites[0].Composite == null
                ? null : this._animationMeshSourceComposites[0].Composite.SkeletonName);
            foreach (CompositePreviewSource source in this._animationMeshSourceComposites.Skip(1))
            {
                string currentName = CleanNodeName(source.Composite == null ? null : source.Composite.SkeletonName);
                if (!SkeletonNamesMatch(firstName, currentName)) return false;
            }
            return true;
        }

        private static bool HasUsableSkinning(MeshRenderInfo info, int boneCount, string activeSkeletonName)
        {
            if (info == null || !info.HasSkinning || boneCount <= 0)
                return false;

            int sourceBoneCount = info.SourceSkeletonJoints == null ? 0 : info.SourceSkeletonJoints.Count;
            if (sourceBoneCount == 0 && !string.IsNullOrWhiteSpace(info.SourceSkeletonName) &&
                !string.IsNullOrWhiteSpace(activeSkeletonName) &&
                !SkeletonNamesMatch(info.SourceSkeletonName, activeSkeletonName))
                return false;

            // PrimitiveMatrix ids are expressed in the PolySkin's source skeleton. A cutscene
            // skeleton may have more joints than the selected in-game animation rig, so validate
            // against the source rig and prefer its own hierarchy during deformation.
            int paletteBoneCount = sourceBoneCount > 0 ? sourceBoneCount : boneCount;

            // Rigidly attached Geometry from CompositeDrawable references has no per-vertex weights.
            if (info.RigidBoneIndex >= 0 && info.RigidBoneIndex < paletteBoneCount &&
                info.SkinUsesRigidBoneOrigin != null && info.SkinUsesRigidBoneOrigin.Any(value => value))
                return true;

            if (info.SkinBoneIndices == null || info.SkinWeights == null)
                return false;
            int vertexCount = Math.Min(info.SkinBoneIndices.Length, info.SkinWeights.Length);
            for (int vertex = 0; vertex < vertexCount; vertex++)
            {
                int[] bones = info.SkinBoneIndices[vertex];
                double[] weights = info.SkinWeights[vertex];
                if (bones == null || weights == null) continue;
                int influenceCount = Math.Min(bones.Length, weights.Length);
                for (int influence = 0; influence < influenceCount; influence++)
                    if (weights[influence] > 0.000001 && bones[influence] >= 0 && bones[influence] < paletteBoneCount)
                        return true;
            }
            return false;
        }

        private void PrepareP1SkinningData(
            MeshRenderInfo info,
            Prototype1GeometryData geometry,
            PrimitiveGroup primitiveGroup,
            IList<Nixson.Common.Vector3> sourcePositions,
            IList<Nixson.Common.Vector3> sourceNormals,
            Pure3DFile sourceP3D)
        {
            if (info == null || geometry == null || sourcePositions == null || sourcePositions.Count == 0)
                return;

            int vertexCount = sourcePositions.Count;
            info.BindPositions = new Vector3D[vertexCount];
            info.BindNormals = sourceNormals != null && sourceNormals.Count == vertexCount
                ? new Vector3D[vertexCount]
                : new Vector3D[0];
            info.SkinBoneIndices = new int[vertexCount][];
            info.SkinWeights = new double[vertexCount][];
            info.SkinUsesRigidBoneOrigin = new bool[vertexCount];
            RawWeightBuffer rawWeightBuffer = primitiveGroup == null ? null : primitiveGroup.GetChildNode<RawWeightBuffer>();
            RawGroupBuffer rawGroupBuffer = primitiveGroup == null ? null : primitiveGroup.GetChildNode<RawGroupBuffer>();
            bool legacyRawSkinLayout = rawWeightBuffer != null && rawWeightBuffer.Weights != null &&
                rawWeightBuffer.Weights.Length == vertexCount && rawWeightBuffer.Weights.All(IsFiniteSkinWeightVector) &&
                rawGroupBuffer != null && rawGroupBuffer.Groups != null && rawGroupBuffer.Groups.Length == vertexCount &&
                rawGroupBuffer.Groups.All(groups => groups != null && groups.Length >= 4);
            info.LegacyRawSkinLayout = legacyRawSkinLayout;
            this.PrepareSourceSkeletonBinding(info, primitiveGroup, sourceP3D);

            for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
            {
                var position = sourcePositions[vertexIndex];
                info.BindPositions[vertexIndex] = new Vector3D(position.X, position.Y, position.Z);
                if (info.BindNormals.Length == vertexCount)
                {
                    var normal = sourceNormals[vertexIndex];
                    info.BindNormals[vertexIndex] = new Vector3D(normal.X, normal.Y, normal.Z);
                }
                info.SkinBoneIndices[vertexIndex] = new[] { -1, -1, -1, -1 };
                info.SkinWeights[vertexIndex] = new[] { 0.0, 0.0, 0.0, 0.0 };

                bool hasVertexSkinStreams = geometry.Weights != null && geometry.Groups != null &&
                    vertexIndex < geometry.Weights.Count && vertexIndex < geometry.Groups.Count &&
                    geometry.Weights[vertexIndex] != null && geometry.Groups[vertexIndex] != null &&
                    geometry.Groups[vertexIndex].Length >= 3;
                if (!hasVertexSkinStreams)
                {
                    // A referenced Geometry can be rigidly attached through Unknown5 even when
                    // its legacy attribute streams are absent or don't match this position stream.
                    AssignRigidSkinning(info, vertexIndex);
                    continue;
                }

                Nixson.Common.Vector3 vertexWeights = geometry.Weights[vertexIndex];
                byte[] groups = geometry.Groups[vertexIndex];

                double weight0 = SafeSkinWeight(vertexWeights.X);
                double weight1 = SafeSkinWeight(vertexWeights.Y);
                double weight2 = SafeSkinWeight(vertexWeights.Z);
                double weight3 = groups.Length >= 4 ? Math.Max(0.0, 1.0 - weight0 - weight1 - weight2) : 0.0;
                byte[] localBoneSlots;
                if (legacyRawSkinLayout && groups.Length >= 4)
                    localBoneSlots = new[] { groups[3], groups[2], groups[1], groups[0] };
                else
                    localBoneSlots = new[] { groups[0], groups[1], groups[2], groups.Length >= 4 ? groups[3] : (byte)0 };

                double[] weights = new[] { weight0, weight1, weight2, weight3 };
                double total = weights.Sum();
                if (total <= 0.000001)
                {
                    // Some CompositeDrawable Geometry records contain placeholder weight/group
                    // arrays (all zero weights). They still belong to the reference's rigid bone.
                    AssignRigidSkinning(info, vertexIndex);
                    continue;
                }

                bool anyInfluence = false;
                for (int influence = 0; influence < 4; influence++)
                {
                    if (weights[influence] <= 0.000001) continue;
                    int boneIndex = ResolveP1PaletteBone(info, localBoneSlots[influence]);
                    if (boneIndex < 0) continue;
                    info.SkinBoneIndices[vertexIndex][influence] = boneIndex;
                    info.SkinWeights[vertexIndex][influence] = weights[influence] / total;
                    anyInfluence = true;
                }
                if (anyInfluence)
                {
                    info.HasSkinning = true;
                }
                else
                {
                    // Invalid palette entries must not leave a rigidly bound mesh behind in bind pose.
                    AssignRigidSkinning(info, vertexIndex);
                }
            }
        }

        private static void AssignRigidSkinning(MeshRenderInfo info, int vertexIndex)
        {
            if (info == null || vertexIndex < 0 ||
                info.SkinBoneIndices == null || vertexIndex >= info.SkinBoneIndices.Length ||
                info.SkinWeights == null || vertexIndex >= info.SkinWeights.Length ||
                info.SkinUsesRigidBoneOrigin == null || vertexIndex >= info.SkinUsesRigidBoneOrigin.Length ||
                info.RigidBoneIndex < 0)
                return;

            info.SkinBoneIndices[vertexIndex] = new[] { info.RigidBoneIndex, -1, -1, -1 };
            info.SkinWeights[vertexIndex] = new[] { 1.0, 0.0, 0.0, 0.0 };
            info.SkinUsesRigidBoneOrigin[vertexIndex] = true;
            info.HasSkinning = true;
        }

        private static bool IsFiniteSkinWeightVector(Nixson.Common.Vector3 value)
        {
            return value != null && !float.IsNaN(value.X) && !float.IsInfinity(value.X) &&
                !float.IsNaN(value.Y) && !float.IsInfinity(value.Y) &&
                !float.IsNaN(value.Z) && !float.IsInfinity(value.Z);
        }

        private static double SafeSkinWeight(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0.0;
            return Math.Max(0.0, value);
        }

        private static int ResolveP1PaletteBone(MeshRenderInfo info, byte localBoneSlot)
        {
            if (info != null && info.MatrixIndices != null && localBoneSlot < info.MatrixIndices.Length)
            {
                uint paletteBone = info.MatrixIndices[localBoneSlot];
                return paletteBone <= (uint)int.MaxValue ? (int)paletteBone : -1;
            }
            // EditP3D uses the group index directly when no matching PrimitiveMatrix entry exists.
            return localBoneSlot;
        }

        private void PrepareSourceSkeletonBinding(MeshRenderInfo info, PrimitiveGroup primitiveGroup, Pure3DFile sourceP3D)
        {
            if (info == null || primitiveGroup == null || sourceP3D == null || sourceP3D.Nodes == null)
                return;

            PolySkin polySkin = primitiveGroup.GetParentNode<PolySkin>();
            string skeletonName = polySkin == null ? null : CleanNodeName(polySkin.SkeletonName);
            if (string.IsNullOrWhiteSpace(skeletonName))
            {
                CompositeDrawable composite = primitiveGroup.GetParentNode<CompositeDrawable>();
                skeletonName = composite == null ? null : CleanNodeName(composite.SkeletonName);
            }
            if (string.IsNullOrWhiteSpace(skeletonName))
                return;

            info.SourceSkeletonName = skeletonName;
            Skeleton sourceSkeleton = FlattenNodes(sourceP3D.Nodes).OfType<Skeleton>()
                .FirstOrDefault(candidate => SkeletonNamesMatch(candidate.Name, skeletonName));
            if (sourceSkeleton == null)
                return;

            info.SourceSkeletonNode = sourceSkeleton;
            List<JointData> sourceJoints = this.ExtractJoints(sourceSkeleton);
            if (sourceJoints == null || sourceJoints.Count == 0)
                return;

            info.SourceSkeletonJoints = sourceJoints;
        }

        private static int FindMappedActiveJoint(IList<JointData> sourceJoints, IList<JointData> activeJoints,
            int sourceIndex, int[] sourceToActiveMap)
        {
            if (sourceJoints == null || activeJoints == null || sourceIndex < 0 || sourceIndex >= sourceJoints.Count)
                return -1;

            // First try the source joint itself, then walk toward its closest ancestor present
            // in the active rig. This handles cutscene-only eyelid/lip/brow/collar joints.
            int candidateSource = sourceIndex;
            while (candidateSource >= 0 && candidateSource < sourceJoints.Count)
            {
                JointData sourceJoint = sourceJoints[candidateSource];
                string sourceName = CleanNodeName(sourceJoint == null ? null : sourceJoint.Name);
                if (!string.IsNullOrWhiteSpace(sourceName))
                {
                    int mappedParent = -1;
                    int sourceParent = sourceJoint.ParentIndex;
                    if (sourceParent >= 0 && sourceParent < candidateSource &&
                        sourceToActiveMap != null && sourceParent < sourceToActiveMap.Length)
                        mappedParent = sourceToActiveMap[sourceParent];

                    var exact = Enumerable.Range(0, activeJoints.Count)
                        .Where(i => string.Equals(sourceName, CleanNodeName(activeJoints[i].Name), StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (exact.Count == 1) return exact[0];
                    if (exact.Count > 1 && mappedParent >= 0)
                    {
                        var byParent = exact.Where(i => activeJoints[i].ParentIndex == mappedParent).ToList();
                        if (byParent.Count == 1) return byParent[0];
                    }

                    var suffixMatches = Enumerable.Range(0, activeJoints.Count)
                        .Where(i => BoneNamesMatch(sourceName, activeJoints[i].Name))
                        .ToList();
                    if (suffixMatches.Count == 1) return suffixMatches[0];
                    if (suffixMatches.Count > 1 && mappedParent >= 0)
                    {
                        var byParent = suffixMatches.Where(i => activeJoints[i].ParentIndex == mappedParent).ToList();
                        if (byParent.Count == 1) return byParent[0];
                    }
                }

                int parentIndex = sourceJoints[candidateSource].ParentIndex;
                if (parentIndex < 0 || parentIndex >= candidateSource) break;
                candidateSource = parentIndex;
            }
            return -1;
        }

        private static void EnsureSourceBoneMap(MeshRenderInfo info, SkeletonPreviewState state)
        {
            if (info == null || state == null || info.SourceSkeletonJoints == null || info.SourceSkeletonJoints.Count == 0)
                return;
            if (ReferenceEquals(info.BoneMapSkeletonNode, state.SkeletonNode) &&
                info.SourceToActiveBoneMap != null && info.SourceToActiveBoneMap.Length == info.SourceSkeletonJoints.Count)
                return;

            int sourceCount = info.SourceSkeletonJoints.Count;
            info.SourceToActiveBoneMap = Enumerable.Repeat(-1, sourceCount).ToArray();
            for (int i = 0; i < sourceCount; i++)
                info.SourceToActiveBoneMap[i] = FindMappedActiveJoint(
                    info.SourceSkeletonJoints, state.Joints, i, info.SourceToActiveBoneMap);
            info.BoneMapSkeletonNode = state.SkeletonNode;
        }

        private void UpdateSkinnedMeshes(float frame)
        {
            SkeletonPreviewState activeState = this._activeSkeleton;
            if (activeState == null || activeState.BindGamePositions == null || activeState.BindAxisX == null ||
                activeState.BindAxisY == null || activeState.BindAxisZ == null || activeState.WorldGamePositions == null ||
                activeState.BindGamePositions.Length != activeState.Joints.Count ||
                activeState.WorldGamePositions.Length != activeState.Joints.Count)
                return;

            // Evaluate each mesh's own source skeleton once per update. This matters for cutscene
            // meshes whose additional eyelid/lip joints have different pivots than the gameplay rig.
            var sourceStates = new Dictionary<Skeleton, SkeletonPreviewState>();
            foreach (MeshRenderInfo info in this._meshRenderInfos)
            {
                if (info == null || !info.HasSkinning || info.SourceSkeletonNode == null || sourceStates.ContainsKey(info.SourceSkeletonNode))
                    continue;
                SkeletonPreviewState sourceState = this.GetOrCreateSourceSkeletonPose(info.SourceSkeletonNode);
                if (sourceState == null) continue;
                this.EvaluateSkeletonPoseState(sourceState, frame, this._activeAnimationTracks, true, false, true);
                sourceStates[info.SourceSkeletonNode] = sourceState;
            }

            // DX12 performs vertex deformation in VSMain. Keep the decoded bind-pose vertex data
            // immutable here and only evaluate the small per-bone pose on the CPU. The WPF fallback
            // still uses the established CPU deformation path below.
            if (this._dx12RendererActive)
                return;

            foreach (MeshRenderInfo info in this._meshRenderInfos)
            {
                if (info == null || !info.HasSkinning || info.Mesh == null ||
                    info.BindPositions == null || info.SkinBoneIndices == null || info.SkinWeights == null ||
                    info.SkinUsesRigidBoneOrigin == null ||
                    info.BindPositions.Length != info.Mesh.Positions.Count ||
                    info.SkinBoneIndices.Length != info.BindPositions.Length ||
                    info.SkinWeights.Length != info.BindPositions.Length ||
                    info.SkinUsesRigidBoneOrigin.Length != info.BindPositions.Length)
                    continue;

                SkeletonPreviewState skinState = activeState;
                bool useSourceSkeletonPose = false;
                if (info.SourceSkeletonNode != null)
                {
                    SkeletonPreviewState sourceState;
                    if (sourceStates.TryGetValue(info.SourceSkeletonNode, out sourceState) &&
                        sourceState.BindGamePositions != null && sourceState.BindAxisX != null &&
                        sourceState.BindAxisY != null && sourceState.BindAxisZ != null &&
                        sourceState.BindGamePositions.Length == sourceState.Joints.Count &&
                        sourceState.WorldGamePositions != null && sourceState.WorldGamePositions.Length == sourceState.Joints.Count)
                    {
                        skinState = sourceState;
                        useSourceSkeletonPose = true;
                    }
                }

                if (!useSourceSkeletonPose) EnsureSourceBoneMap(info, activeState);
                bool hasSourceSkeleton = info.SourceSkeletonJoints != null && info.SourceSkeletonJoints.Count > 0;
                bool hasSourceRigMapping = hasSourceSkeleton &&
                    info.SourceToActiveBoneMap != null && info.SourceToActiveBoneMap.Length == info.SourceSkeletonJoints.Count;
                bool sourceRigNameMismatch = !string.IsNullOrWhiteSpace(info.SourceSkeletonName) &&
                    skinState.SkeletonNode != null && !SkeletonNamesMatch(info.SourceSkeletonName, skinState.SkeletonNode.Name);
                int vertexCount = info.BindPositions.Length;
                var positions = new Point3DCollection(vertexCount);
                bool hasNormals = info.BindNormals != null && info.BindNormals.Length == vertexCount;
                Vector3DCollection normals = hasNormals ? new Vector3DCollection(vertexCount) : null;

                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    Vector3D bindPosition = info.BindPositions[vertexIndex];
                    Vector3D skinnedPosition = new Vector3D();
                    Vector3D skinnedNormal = new Vector3D();
                    double totalWeight = 0.0;
                    Vector3D bindNormal = hasNormals ? info.BindNormals[vertexIndex] : new Vector3D();

                    int[] boneIndices = info.SkinBoneIndices[vertexIndex];
                    double[] weights = info.SkinWeights[vertexIndex];
                    if (boneIndices != null && weights != null)
                    {
                        int influenceCount = Math.Min(boneIndices.Length, weights.Length);
                        for (int influence = 0; influence < influenceCount; influence++)
                        {
                            int sourceBoneIndex = boneIndices[influence];
                            int boneIndex = sourceBoneIndex;
                            double weight = weights[influence];
                            if (weight <= 0.000001 || sourceBoneIndex < 0)
                                continue;

                            if (useSourceSkeletonPose)
                            {
                                // Palette indices are interpreted in the mesh's own source rig.
                                // Do not collapse cutscene-only joints (e.g. Eyelid_L/R) onto an ancestor.
                                boneIndex = sourceBoneIndex;
                            }
                            else if (hasSourceSkeleton)
                            {
                                // Fallback for assets whose source skeleton node was not available.
                                if (!hasSourceRigMapping || sourceBoneIndex >= info.SourceToActiveBoneMap.Length)
                                    continue;
                                boneIndex = info.SourceToActiveBoneMap[sourceBoneIndex];
                                if (boneIndex < 0)
                                    continue;
                            }
                            else if (sourceRigNameMismatch)
                            {
                                // A raw numeric id is unsafe when the mesh's named rig is missing.
                                continue;
                            }

                            if (boneIndex < 0 || boneIndex >= skinState.Joints.Count ||
                                boneIndex >= skinState.BindGamePositions.Length || boneIndex >= skinState.BindAxisX.Length ||
                                boneIndex >= skinState.BindAxisY.Length || boneIndex >= skinState.BindAxisZ.Length ||
                                boneIndex >= skinState.WorldAxisX.Length || boneIndex >= skinState.WorldAxisY.Length ||
                                boneIndex >= skinState.WorldAxisZ.Length)
                                continue;

                            Vector3D localPosition;
                            Vector3D localNormal;
                            if (info.SkinUsesRigidBoneOrigin[vertexIndex])
                            {
                                // EditP3D's rigid Geometry binding treats source coordinates as bone-local.
                                localPosition = bindPosition;
                                localNormal = bindNormal;
                            }
                            else
                            {
                                Vector3D bindOffset = bindPosition - skinState.BindGamePositions[boneIndex];
                                localPosition = new Vector3D(
                                    Vector3D.DotProduct(bindOffset, skinState.BindAxisX[boneIndex]),
                                    Vector3D.DotProduct(bindOffset, skinState.BindAxisY[boneIndex]),
                                    Vector3D.DotProduct(bindOffset, skinState.BindAxisZ[boneIndex]));
                                localNormal = new Vector3D(
                                    Vector3D.DotProduct(bindNormal, skinState.BindAxisX[boneIndex]),
                                    Vector3D.DotProduct(bindNormal, skinState.BindAxisY[boneIndex]),
                                    Vector3D.DotProduct(bindNormal, skinState.BindAxisZ[boneIndex]));
                            }
                            Vector3D currentPosition = skinState.WorldGamePositions[boneIndex]
                                + skinState.WorldAxisX[boneIndex] * localPosition.X
                                + skinState.WorldAxisY[boneIndex] * localPosition.Y
                                + skinState.WorldAxisZ[boneIndex] * localPosition.Z;
                            skinnedPosition += currentPosition * weight;

                            if (hasNormals)
                            {
                                skinnedNormal += (skinState.WorldAxisX[boneIndex] * localNormal.X
                                    + skinState.WorldAxisY[boneIndex] * localNormal.Y
                                    + skinState.WorldAxisZ[boneIndex] * localNormal.Z) * weight;
                            }
                            totalWeight += weight;
                        }
                    }

                    if (totalWeight > 0.000001)
                    {
                        bindPosition = skinnedPosition / totalWeight;
                        if (hasNormals)
                        {
                            skinnedNormal /= totalWeight;
                            if (skinnedNormal.LengthSquared > 0.000001) skinnedNormal.Normalize();
                            else skinnedNormal = bindNormal;
                        }
                    }
                    else if (hasNormals)
                    {
                        skinnedNormal = bindNormal;
                    }
                    positions.Add(ToViewerPoint(bindPosition));
                    if (hasNormals)
                    {
                        Vector3D convertedNormal = new Vector3D(skinnedNormal.X, -skinnedNormal.Z, skinnedNormal.Y);
                        if (convertedNormal.LengthSquared > 0.000001) convertedNormal.Normalize();
                        normals.Add(convertedNormal);
                    }
                }

                info.Mesh.Positions = positions;
                if (hasNormals) info.Mesh.Normals = normals;
            }
            this.RefreshAnimationMeshOverlays();
        }

        private void RefreshAnimationMeshOverlays()
        {
            if (this._dx12RendererActive) return;
            if (!this._animationMeshModeAvailable || !this._animationMeshMode ||
                (this._shadingMode != ViewerShadingMode.Wireframe &&
                 this._shadingMode != ViewerShadingMode.Normals &&
                 this._shadingMode != ViewerShadingMode.Skinning))
                return;

            var meshModels = new HashSet<Model3D>(this._meshRenderInfos
                .Where(info => info != null && info.Model != null)
                .Select(info => (Model3D)info.Model));
            var skeletonModels = new HashSet<Model3D>();
            if (this._activeSkeleton != null && this._activeSkeleton.JointMarkers != null)
                foreach (var model in this._activeSkeleton.JointMarkers) if (model != null) skeletonModels.Add(model);
            if (this._activeSkeleton != null && this._activeSkeleton.BoneModels != null)
                foreach (var model in this._activeSkeleton.BoneModels) if (model != null) skeletonModels.Add(model);

            foreach (Model3D model in this._loadedModels.ToList())
            {
                if (!meshModels.Contains(model) && !skeletonModels.Contains(model))
                {
                    this.rootGroup.Children.Remove(model);
                    this._loadedModels.Remove(model);
                }
            }
            this.ApplyShadingModeToMeshes();
        }

        private void UpdateAnimationDisplayModeButton()
        {
            if (this.animationDisplayModeButton == null) return;
            this.animationDisplayModeButton.Text = this.L(
                this._animationMeshMode ? "viewer.animation.displayModeMesh" : "viewer.animation.displayModeSkeleton",
                this._animationMeshMode ? "Меш" : "Скелет");
            this.animationDisplayModeButton.Checked = this._animationMeshMode;
            this.animationDisplayModeButton.ToolTipText = this.L(
                "viewer.animation.displayMode.tooltip", "Переключить между деформируемым мешем и скелетом");
            this.animationDisplayModeButton.AccessibleName = this.animationDisplayModeButton.Text;
            this.animationDisplayModeButton.Enabled = this._animationMeshModeAvailable &&
                this._activeAnimation != null && this._activeSkeleton != null;
        }

        private void ApplyAnimationDisplayMode()
        {
            if (!this._animationMeshModeAvailable || this._activeAnimation == null || this._activeSkeleton == null)
            {
                this.UpdateAnimationDisplayModeButton();
                return;
            }

            var meshModels = new HashSet<Model3D>(this._meshRenderInfos
                .Where(info => info != null && info.Model != null)
                .Select(info => (Model3D)info.Model));
            var skeletonModels = new HashSet<Model3D>();
            if (this._activeSkeleton.JointMarkers != null)
                foreach (var model in this._activeSkeleton.JointMarkers) if (model != null) skeletonModels.Add(model);
            if (this._activeSkeleton.BoneModels != null)
                foreach (var model in this._activeSkeleton.BoneModels) if (model != null) skeletonModels.Add(model);

            // Wireframe/normal/weight overlays are rebuilt from the current deformed vertex positions.
            foreach (Model3D model in this._loadedModels.ToList())
            {
                if (!meshModels.Contains(model) && !skeletonModels.Contains(model))
                {
                    this.rootGroup.Children.Remove(model);
                    this._loadedModels.Remove(model);
                }
            }

            if (this._animationMeshMode)
            {
                foreach (Model3D model in skeletonModels) this.rootGroup.Children.Remove(model);
                foreach (Model3D model in meshModels)
                    if (!this.rootGroup.Children.Contains(model)) this.rootGroup.Children.Add(model);
                this.ApplyShadingModeToMeshes();
            }
            else
            {
                foreach (Model3D model in meshModels) this.rootGroup.Children.Remove(model);
                foreach (Model3D model in skeletonModels)
                    if (!this.rootGroup.Children.Contains(model)) this.rootGroup.Children.Add(model);
            }

            this.UpdateAnimationDisplayModeButton();
            if (this.host != null) this.host.Invalidate(true);
        }

        private bool ApplyShadingModeToMeshes()
        {
            bool missingSkinningData = false;
            foreach (MeshRenderInfo info in this._meshRenderInfos)
            {
                if (info == null || info.Model == null || info.Mesh == null) continue;

                // These materials are now retained as source metadata for the DX12 scene collector.
                // The WPF viewport is not visible and no WPF 3D overlay meshes are created.
                Material baseMaterial = info.SolidMaterial ?? info.TexturedMaterial;
                info.Model.Material = baseMaterial;
                info.Model.BackMaterial = baseMaterial;
                if (this._shadingMode == ViewerShadingMode.Skinning &&
                    !HasUsableSkinning(info, this._activeSkeleton == null ? 0 : this._activeSkeleton.Joints.Count,
                        this._activeSkeleton == null || this._activeSkeleton.SkeletonNode == null ? null : this._activeSkeleton.SkeletonNode.Name))
                    missingSkinningData = true;
            }
            return missingSkinningData;
        }

        private void InitializeDx12Renderer()
        {
            if (this.IsDisposed || this._dx12Panel == null || this._dx12Panel.IsDisposed || !this._dx12Panel.IsHandleCreated)
                return;
            if (this._dx12Renderer != null || (this._dx12RenderThread != null && this._dx12RenderThread.IsAlive))
                return;

            try
            {
                this._dx12InitialRenderWidth = Math.Max(1, this._dx12Panel.ClientSize.Width);
                this._dx12InitialRenderHeight = Math.Max(1, this._dx12Panel.ClientSize.Height);
                this._dx12RenderHwnd = this._dx12Panel.Handle;
                // DX12 device/swap-chain and shader/PSO setup can take a noticeable amount of time.
                // Run initialization on the same worker that will own Render/Resize/Dispose so the UI
                // stays responsive and no D3D12 object is used concurrently from multiple threads.
                this._dx12RendererActive = true;
                this.host.Visible = false;
                this._dx12Panel.Visible = true;
                this._dx12FpsLabel.Text = "D3D12: инициализация…";
                this._dx12LastFpsLabelUpdateUtc = DateTime.UtcNow;
                this.PositionDx12Overlays();
                this.StartDx12RenderThread();
                this._dx12RenderTimer.Start();
            }
            catch (Exception ex)
            {
                string context = DescribeDx12Failure("Start renderer thread", ex) +
                    "; HWND=0x" + (this._dx12Panel == null || !this._dx12Panel.IsHandleCreated ? 0L : this._dx12Panel.Handle.ToInt64()).ToString("X", CultureInfo.InvariantCulture) +
                    "; HandleCreated=" + (this._dx12Panel != null && this._dx12Panel.IsHandleCreated) +
                    "; ClientSize=" + (this._dx12Panel == null ? "<null>" : this._dx12Panel.ClientSize.Width + "x" + this._dx12Panel.ClientSize.Height) +
                    "; AppBase=" + AppContext.BaseDirectory;
                ApplicationLog.Write("DX12_INIT_FAILED", context, ex);
                this._dx12RendererActive = false;
                if (this.host != null) this.host.Visible = true;
                if (this._dx12Panel != null) this._dx12Panel.Visible = false;
                if (this._dx12FpsLabel != null) this._dx12FpsLabel.Text = "D3D12: ошибка";
                if (this._sceneStatsOverlay != null)
                    this._sceneStatsOverlay.Text = "Direct3D 12 не удалось инициализировать\r\n" + context;
                this.SetStatsText(() => this.LF("viewer.dx12.initFailed", "Не удалось инициализировать DirectX 12: {0}", ex.Message));
            }
        }

        private static string DescribeDx12Failure(string operation, Exception exception)
        {
            Exception root = exception;
            int depth = 0;
            while (root != null && root.InnerException != null && depth++ < 16) root = root.InnerException;
            if (root == null) root = exception;
            uint rootHresult = root == null ? 0u : unchecked((uint)root.HResult);
            string apiCode = rootHresult == 0x80070057u ? "E_INVALIDARG / Invalid arguments" : "<not mapped>";
            return string.Format(CultureInfo.InvariantCulture,
                "Stage={0}; Exception={1}; HRESULT=0x{2:X8}; ExceptionMessage={3}; RootException={4}; RootHRESULT=0x{5:X8}; RootApiCode={6}; RootMessage={7}",
                operation ?? "unknown",
                exception == null ? "<none>" : exception.GetType().FullName,
                exception == null ? 0u : unchecked((uint)exception.HResult),
                exception == null ? "<none>" : exception.Message,
                root == null ? "<none>" : root.GetType().FullName,
                rootHresult,
                apiCode,
                root == null ? "<none>" : root.Message);
        }

        private void BuildDx12Scene()
        {
            this._dx12MeshScratch.Clear();
            this.RefreshDx12RenderInfoLookup();
            if (this.rootGroup == null) return;
            foreach (Model3D model in this.rootGroup.Children)
                this.CollectDx12SceneMeshes(model, Matrix3D.Identity);
        }

        private void RefreshDx12RenderInfoLookup()
        {
            bool unchanged = this._dx12RenderInfoLookupModels.Length == this._meshRenderInfos.Count;
            if (unchanged)
            {
                for (int i = 0; i < this._meshRenderInfos.Count; i++)
                {
                    MeshRenderInfo info = this._meshRenderInfos[i];
                    GeometryModel3D model = info == null ? null : info.Model;
                    if (!ReferenceEquals(model, this._dx12RenderInfoLookupModels[i]))
                    {
                        unchanged = false;
                        break;
                    }
                }
            }
            if (unchanged) return;

            this._dx12GeometrySnapshotCache.Clear();
            this._dx12FrozenTextureCache.Clear();
            this._dx12RenderInfoLookup.Clear();
            this._dx12RenderInfoLookupModels = new GeometryModel3D[this._meshRenderInfos.Count];
            for (int i = 0; i < this._meshRenderInfos.Count; i++)
            {
                MeshRenderInfo info = this._meshRenderInfos[i];
                GeometryModel3D model = info == null ? null : info.Model;
                this._dx12RenderInfoLookupModels[i] = model;
                if (model != null) this._dx12RenderInfoLookup[model] = info;
            }
        }

        private void CollectDx12SceneMeshes(Model3D model, Matrix3D parentTransform)
        {
            if (model == null) return;
            Matrix3D localTransform = model.Transform == null ? Matrix3D.Identity : model.Transform.Value;
            localTransform.Append(parentTransform);

            Model3DGroup group = model as Model3DGroup;
            if (group != null)
            {
                foreach (Model3D child in group.Children) this.CollectDx12SceneMeshes(child, localTransform);
                return;
            }

            GeometryModel3D geometryModel = model as GeometryModel3D;
            if (geometryModel == null) return;
            MeshGeometry3D mesh = geometryModel.Geometry as MeshGeometry3D;
            if (mesh == null || mesh.Positions == null || mesh.Positions.Count == 0 ||
                mesh.TriangleIndices == null || mesh.TriangleIndices.Count < 3)
                return;

            MeshRenderInfo renderInfo;
            this._dx12RenderInfoLookup.TryGetValue(geometryModel, out renderInfo);
            Material material = geometryModel.Material ?? geometryModel.BackMaterial;
            if (renderInfo != null)
            {
                if (this._shadingMode == ViewerShadingMode.Textured)
                    material = renderInfo.TexturedMaterial ?? renderInfo.SolidMaterial ?? material;
                else
                    material = renderInfo.SolidMaterial ?? material;
            }

            var sceneMesh = new Dx12SceneMesh
            {
                Key = geometryModel,
                Mesh = mesh,
                Transform = localTransform,
                BaseColor = ExtractDx12MaterialColor(material),
                Texture = this._shadingMode == ViewerShadingMode.Textured ? ExtractDx12Texture(material) : null
            };

            if (renderInfo != null && renderInfo.HasSkinning)
                this.PrepareDx12GpuSkinning(sceneMesh, renderInfo, localTransform);
            if (this._shadingMode == ViewerShadingMode.Skinning && renderInfo != null)
                sceneMesh.VertexColors = GetDx12SkinningColors(renderInfo, mesh.Positions.Count);

            // The rendering worker must never read live Dispatcher-affine WPF Freezables.
            // Cache an immutable, frozen geometry snapshot and a frozen texture on the UI thread.
            sceneMesh.Mesh = this.GetFrozenDx12MeshSnapshot(geometryModel, mesh);
            sceneMesh.Texture = this.GetFrozenDx12TextureSnapshot(sceneMesh.Texture);
            // Bone matrices are refreshed on the UI thread as animation poses change. Copy their
            // compact palette so a render already in progress cannot observe a partially-written pose.
            if (sceneMesh.BoneMatrices != null)
                sceneMesh.BoneMatrices = (System.Numerics.Matrix4x4[])sceneMesh.BoneMatrices.Clone();
            this._dx12MeshScratch.Add(sceneMesh);
        }

        private MeshGeometry3D GetFrozenDx12MeshSnapshot(GeometryModel3D key, MeshGeometry3D source)
        {
            if (key == null || source == null)
                return source;

            Dx12GeometrySnapshotCache cached;
            bool unchanged = this._dx12GeometrySnapshotCache.TryGetValue(key, out cached) && cached != null &&
                ReferenceEquals(cached.SourceMesh, source) &&
                ReferenceEquals(cached.SourcePositions, source.Positions) &&
                ReferenceEquals(cached.SourceNormals, source.Normals) &&
                ReferenceEquals(cached.SourceTextureCoordinates, source.TextureCoordinates) &&
                ReferenceEquals(cached.SourceTriangleIndices, source.TriangleIndices);
            if (unchanged && cached.FrozenMesh != null)
                return cached.FrozenMesh;

            MeshGeometry3D snapshot = source.CloneCurrentValue();
            if (!snapshot.IsFrozen && snapshot.CanFreeze)
                snapshot.Freeze();
            if (!snapshot.IsFrozen)
            {
                // Rebuild from plain value collections if the original Freezable tree cannot freeze.
                snapshot = new MeshGeometry3D
                {
                    Positions = source.Positions == null ? new Point3DCollection() : new Point3DCollection(source.Positions),
                    Normals = source.Normals == null ? new Vector3DCollection() : new Vector3DCollection(source.Normals),
                    TextureCoordinates = source.TextureCoordinates == null ? new PointCollection() : new PointCollection(source.TextureCoordinates),
                    TriangleIndices = source.TriangleIndices == null ? new Int32Collection() : new Int32Collection(source.TriangleIndices)
                };
                if (snapshot.CanFreeze) snapshot.Freeze();
            }
            if (!snapshot.IsFrozen)
                throw new InvalidOperationException("DX12 scene geometry could not be frozen for cross-thread rendering.");

            if (cached == null)
                cached = new Dx12GeometrySnapshotCache();
            cached.SourceMesh = source;
            cached.SourcePositions = source.Positions;
            cached.SourceNormals = source.Normals;
            cached.SourceTextureCoordinates = source.TextureCoordinates;
            cached.SourceTriangleIndices = source.TriangleIndices;
            cached.FrozenMesh = snapshot;
            this._dx12GeometrySnapshotCache[key] = cached;
            return snapshot;
        }

        private BitmapSource GetFrozenDx12TextureSnapshot(BitmapSource source)
        {
            if (source == null || source.IsFrozen)
                return source;

            BitmapSource cached;
            if (this._dx12FrozenTextureCache.TryGetValue(source, out cached) && cached != null && cached.IsFrozen)
                return cached;

            BitmapSource snapshot = source.CloneCurrentValue();
            if (!snapshot.IsFrozen && snapshot.CanFreeze)
                snapshot.Freeze();
            if (!snapshot.IsFrozen)
            {
                // Last-resort value copy ensures even a dynamically-backed source is safe to read
                // on the render thread. BGRA32 is also directly supported by the texture packer.
                BitmapSource converted = source.Format == PixelFormats.Bgra32
                    ? source
                    : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0.0);
                int width = Math.Max(1, converted.PixelWidth);
                int height = Math.Max(1, converted.PixelHeight);
                int stride = checked(width * 4);
                byte[] pixels = new byte[checked(stride * height)];
                converted.CopyPixels(pixels, stride, 0);
                snapshot = BitmapSource.Create(width, height, converted.DpiX, converted.DpiY,
                    PixelFormats.Bgra32, null, pixels, stride);
                snapshot.Freeze();
            }
            this._dx12FrozenTextureCache[source] = snapshot;
            return snapshot;
        }

        private void PrepareDx12GpuSkinning(Dx12SceneMesh sceneMesh, MeshRenderInfo info, Matrix3D objectTransform)
        {
            if (sceneMesh == null || info == null || !info.HasSkinning || info.SkinBoneIndices == null ||
                info.SkinWeights == null || info.BindPositions == null || info.SkinBoneIndices.Length != info.BindPositions.Length ||
                info.SkinWeights.Length != info.BindPositions.Length)
                return;

            SkeletonPreviewState skinState = null;
            bool useSourceSkeleton = false;
            if (info.SourceSkeletonNode != null)
            {
                skinState = this.GetOrCreateSourceSkeletonPose(info.SourceSkeletonNode);
                useSourceSkeleton = skinState != null && skinState.BindGamePositions != null &&
                    skinState.BindAxisX != null && skinState.BindAxisY != null && skinState.BindAxisZ != null &&
                    skinState.WorldGamePositions != null && skinState.WorldAxisX != null && skinState.WorldAxisY != null &&
                    skinState.WorldAxisZ != null && skinState.BindGamePositions.Length == skinState.Joints.Count &&
                    skinState.WorldGamePositions.Length == skinState.Joints.Count;
            }
            if (!useSourceSkeleton)
                skinState = this._activeSkeleton;

            if (skinState == null || skinState.Joints == null || skinState.Joints.Count == 0 ||
                skinState.BindGamePositions == null || skinState.BindAxisX == null || skinState.BindAxisY == null ||
                skinState.BindAxisZ == null || skinState.WorldGamePositions == null || skinState.WorldAxisX == null ||
                skinState.WorldAxisY == null || skinState.WorldAxisZ == null)
                return;

            int jointCount = skinState.Joints.Count;
            if (skinState.BindGamePositions.Length != jointCount || skinState.BindAxisX.Length != jointCount ||
                skinState.BindAxisY.Length != jointCount || skinState.BindAxisZ.Length != jointCount ||
                skinState.WorldGamePositions.Length != jointCount || skinState.WorldAxisX.Length != jointCount ||
                skinState.WorldAxisY.Length != jointCount || skinState.WorldAxisZ.Length != jointCount)
                return;

            int[][] skinBoneIndices = info.SkinBoneIndices;
            if (!useSourceSkeleton && info.SourceSkeletonJoints != null && info.SourceSkeletonJoints.Count > 0)
            {
                EnsureSourceBoneMap(info, skinState);
                if (info.SourceToActiveBoneMap == null || info.SourceToActiveBoneMap.Length != info.SourceSkeletonJoints.Count)
                    return;
                if (info.Dx12MappedSkinBoneIndices == null ||
                    !ReferenceEquals(info.Dx12MappedSkinBoneMap, info.SourceToActiveBoneMap) ||
                    info.Dx12MappedSkinBoneIndices.Length != info.SkinBoneIndices.Length)
                {
                    info.Dx12MappedSkinBoneIndices = new int[info.SkinBoneIndices.Length][];
                    for (int vertex = 0; vertex < info.SkinBoneIndices.Length; vertex++)
                    {
                        int[] source = info.SkinBoneIndices[vertex];
                        if (source == null) continue;
                        int[] mapped = new int[source.Length];
                        for (int influence = 0; influence < source.Length; influence++)
                        {
                            int sourceBone = source[influence];
                            mapped[influence] = sourceBone >= 0 && sourceBone < info.SourceToActiveBoneMap.Length
                                ? info.SourceToActiveBoneMap[sourceBone] : -1;
                        }
                        info.Dx12MappedSkinBoneIndices[vertex] = mapped;
                    }
                    info.Dx12MappedSkinBoneMap = info.SourceToActiveBoneMap;
                }
                skinBoneIndices = info.Dx12MappedSkinBoneIndices;
            }
            else if (!useSourceSkeleton && !string.IsNullOrWhiteSpace(info.SourceSkeletonName) &&
                !SkeletonNamesMatch(info.SourceSkeletonName, skinState.SkeletonNode == null ? null : skinState.SkeletonNode.Name))
            {
                // Without a source joint map, numeric ids from a different rig are unsafe.
                return;
            }

            int matrixCount = checked(jointCount * 2);
            System.Numerics.Matrix4x4[] matrices = info.Dx12BoneMatricesCache;
            bool updateMatrices = matrices == null || matrices.Length != matrixCount ||
                info.Dx12BoneMatrixCachedPoseVersion != skinState.PoseVersion ||
                !ReferenceEquals(info.Dx12BoneMatrixCachedSkeletonNode, skinState.SkeletonNode) ||
                !info.Dx12BoneMatrixCachedObjectTransform.Equals(objectTransform);
            if (matrices == null || matrices.Length != matrixCount)
            {
                matrices = new System.Numerics.Matrix4x4[matrixCount];
                info.Dx12BoneMatricesCache = matrices;
                updateMatrices = true;
            }
            if (updateMatrices)
            {
                for (int bone = 0; bone < jointCount; bone++)
                {
                    matrices[bone] = CreateDx12SkinMatrix(skinState, bone, false, objectTransform);
                    matrices[jointCount + bone] = CreateDx12SkinMatrix(skinState, bone, true, objectTransform);
                }
                info.Dx12BoneMatrixCachedPoseVersion = skinState.PoseVersion;
                unchecked { info.Dx12BoneMatrixRevision++; }
                info.Dx12BoneMatrixCachedSkeletonNode = skinState.SkeletonNode;
                info.Dx12BoneMatrixCachedObjectTransform = objectTransform;
            }

            sceneMesh.SkinBoneIndices = skinBoneIndices;
            sceneMesh.SkinWeights = info.SkinWeights;
            sceneMesh.SkinUsesRigidBoneOrigin = info.SkinUsesRigidBoneOrigin;
            sceneMesh.BoneMatrices = matrices;
            sceneMesh.BoneMatrixVersion = info.Dx12BoneMatrixRevision;
            sceneMesh.HasGpuSkinning = true;
        }

        private static System.Numerics.Matrix4x4 CreateDx12SkinMatrix(SkeletonPreviewState state, int boneIndex,
            bool rigidBoneOrigin, Matrix3D objectTransform)
        {
            System.Numerics.Vector3 origin = TransformDx12SkinPoint(state, boneIndex, System.Numerics.Vector3.Zero, rigidBoneOrigin, objectTransform);
            System.Numerics.Vector3 unitX = TransformDx12SkinPoint(state, boneIndex, System.Numerics.Vector3.UnitX, rigidBoneOrigin, objectTransform);
            System.Numerics.Vector3 unitY = TransformDx12SkinPoint(state, boneIndex, System.Numerics.Vector3.UnitY, rigidBoneOrigin, objectTransform);
            System.Numerics.Vector3 unitZ = TransformDx12SkinPoint(state, boneIndex, System.Numerics.Vector3.UnitZ, rigidBoneOrigin, objectTransform);
            return new System.Numerics.Matrix4x4(
                unitX.X - origin.X, unitX.Y - origin.Y, unitX.Z - origin.Z, 0.0f,
                unitY.X - origin.X, unitY.Y - origin.Y, unitY.Z - origin.Z, 0.0f,
                unitZ.X - origin.X, unitZ.Y - origin.Y, unitZ.Z - origin.Z, 0.0f,
                origin.X, origin.Y, origin.Z, 1.0f);
        }

        private static System.Numerics.Vector3 TransformDx12SkinPoint(SkeletonPreviewState state, int boneIndex,
            System.Numerics.Vector3 viewerPosition, bool rigidBoneOrigin, Matrix3D objectTransform)
        {
            // Mesh positions and skeleton display coordinates are scaled by ToViewerPoint's 1000x factor.
            // Convert the viewer-space sample back to raw game units before applying bind/current joint bases.
            Vector3D gamePosition = new Vector3D(
                viewerPosition.X / 1000.0,
                viewerPosition.Z / 1000.0,
                -viewerPosition.Y / 1000.0);
            Vector3D localPosition;
            if (rigidBoneOrigin)
            {
                localPosition = gamePosition;
            }
            else
            {
                Vector3D offset = gamePosition - state.BindGamePositions[boneIndex];
                localPosition = new Vector3D(
                    Vector3D.DotProduct(offset, state.BindAxisX[boneIndex]),
                    Vector3D.DotProduct(offset, state.BindAxisY[boneIndex]),
                    Vector3D.DotProduct(offset, state.BindAxisZ[boneIndex]));
            }

            Vector3D currentGamePosition = state.WorldGamePositions[boneIndex]
                + state.WorldAxisX[boneIndex] * localPosition.X
                + state.WorldAxisY[boneIndex] * localPosition.Y
                + state.WorldAxisZ[boneIndex] * localPosition.Z;
            // Mesh vertices in the DX12 vertex buffer use viewer units (game units * 1000).
            // The input sample above is converted back to game units to evaluate the rig, so
            // convert the resulting animated point back to viewer units before objectTransform.
            // Without this inverse scale, GPU-skinned meshes collapse to roughly 1/1000 size.
            Point3D viewerPoint = new Point3D(
                currentGamePosition.X * 1000.0,
                -currentGamePosition.Z * 1000.0,
                currentGamePosition.Y * 1000.0);
            Point3D transformed = objectTransform.Transform(viewerPoint);
            return new System.Numerics.Vector3((float)transformed.X, (float)transformed.Y, (float)transformed.Z);
        }

        private static System.Numerics.Vector4 ExtractDx12MaterialColor(Material material)
        {
            SolidColorBrush solidBrush = FindSolidBrush(material);
            if (solidBrush == null) return new System.Numerics.Vector4(0.82f, 0.84f, 0.88f, 1.0f);
            System.Windows.Media.Color color = solidBrush.Color;
            return new System.Numerics.Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        private static SolidColorBrush FindSolidBrush(Material material)
        {
            DiffuseMaterial diffuse = material as DiffuseMaterial;
            if (diffuse != null)
                return diffuse.Brush as SolidColorBrush;
            MaterialGroup group = material as MaterialGroup;
            if (group != null)
            {
                foreach (Material child in group.Children)
                {
                    SolidColorBrush brush = FindSolidBrush(child);
                    if (brush != null) return brush;
                }
            }
            return null;
        }

        private static BitmapSource ExtractDx12Texture(Material material)
        {
            DiffuseMaterial diffuse = material as DiffuseMaterial;
            if (diffuse != null)
            {
                ImageBrush imageBrush = diffuse.Brush as ImageBrush;
                if (imageBrush != null) return imageBrush.ImageSource as BitmapSource;
            }
            MaterialGroup group = material as MaterialGroup;
            if (group != null)
            {
                foreach (Material child in group.Children)
                {
                    BitmapSource source = ExtractDx12Texture(child);
                    if (source != null) return source;
                }
            }
            return null;
        }

        private static readonly System.Numerics.Vector4[] Dx12SkinningPalette = new System.Numerics.Vector4[]
        {
            new System.Numerics.Vector4(0.96f, 0.25f, 0.23f, 1),
            new System.Numerics.Vector4(0.20f, 0.77f, 0.36f, 1),
            new System.Numerics.Vector4(0.22f, 0.52f, 0.98f, 1),
            new System.Numerics.Vector4(0.98f, 0.70f, 0.18f, 1),
            new System.Numerics.Vector4(0.75f, 0.32f, 0.91f, 1),
            new System.Numerics.Vector4(0.12f, 0.82f, 0.82f, 1),
            new System.Numerics.Vector4(0.98f, 0.43f, 0.16f, 1),
            new System.Numerics.Vector4(0.58f, 0.72f, 0.20f, 1)
        };

        private static System.Numerics.Vector4[] GetDx12SkinningColors(MeshRenderInfo info, int vertexCount)
        {
            if (info == null || vertexCount <= 0 || info.SkinBoneIndices == null || info.SkinWeights == null ||
                info.SkinBoneIndices.Length != vertexCount || info.SkinWeights.Length != vertexCount)
                return null;

            if (info.Dx12SkinColorCache == null || info.Dx12SkinColorVertexCount != vertexCount)
            {
                var colors = new System.Numerics.Vector4[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    int[] indices = info.SkinBoneIndices[i];
                    double[] weights = info.SkinWeights[i];
                    int dominantBone = -1;
                    double dominantWeight = 0;
                    if (indices != null && weights != null)
                    {
                        for (int influence = 0; influence < Math.Min(indices.Length, weights.Length); influence++)
                        {
                            if (weights[influence] > dominantWeight && indices[influence] >= 0)
                            {
                                dominantWeight = weights[influence];
                                dominantBone = indices[influence];
                            }
                        }
                    }
                    if (dominantBone < 0 || dominantWeight <= 0.0001)
                        colors[i] = new System.Numerics.Vector4(0.38f, 0.40f, 0.43f, 1);
                    else
                    {
                        System.Numerics.Vector4 boneColor = Dx12SkinningPalette[dominantBone % Dx12SkinningPalette.Length];
                        float strength = (float)Math.Max(0.15, Math.Min(1.0, dominantWeight));
                        colors[i] = System.Numerics.Vector4.Lerp(new System.Numerics.Vector4(0.35f, 0.36f, 0.38f, 1), boneColor, strength);
                    }
                }
                info.Dx12SkinColorCache = colors;
                info.Dx12SkinColorVertexCount = vertexCount;
            }
            return info.Dx12SkinColorCache;
        }

        private void StartDx12RenderThread()
        {
            if (this._dx12RenderThread != null && this._dx12RenderThread.IsAlive)
                return;
            this._dx12RenderStopEvent.Reset();
            this._dx12RenderThread = new Thread(this.Dx12RenderThreadMain)
            {
                IsBackground = true,
                Name = "PrototypeUltimateTool DX12 Render"
            };
            // Texture packing uses WPF imaging Freezables on the worker; keep it STA-compatible.
            this._dx12RenderThread.SetApartmentState(ApartmentState.STA);
            this._dx12RenderThread.Start();
        }

        private void RenderDx12Frame()
        {
            if (!this._dx12RendererActive || this._dx12Renderer == null || this._dx12Panel == null ||
                this._dx12Panel.ClientSize.Width <= 0 || this._dx12Panel.ClientSize.Height <= 0)
                return;

            try
            {
                long sceneBuildStart = System.Diagnostics.Stopwatch.GetTimestamp();
                this.BuildDx12Scene();
                this._dx12SceneCollectionMilliseconds = (System.Diagnostics.Stopwatch.GetTimestamp() - sceneBuildStart) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;

                var frame = new Dx12RenderFrameRequest
                {
                    Meshes = this._dx12MeshScratch.ToArray(),
                    ShadingMode = (Dx12ShadingMode)this._shadingMode,
                    Yaw = this._yaw,
                    Pitch = this._pitch,
                    Distance = this._distance,
                    TargetX = this._targetX,
                    TargetY = this._targetY,
                    TargetZ = this._targetZ,
                    BoundingRadius = this._boundingRadius,
                    Width = this._dx12Panel.ClientSize.Width,
                    Height = this._dx12Panel.ClientSize.Height
                };
                lock (this._dx12FrameRequestSync)
                    this._dx12LatestFrameRequest = frame;

                if ((DateTime.UtcNow - this._dx12LastFpsLabelUpdateUtc).TotalMilliseconds >= 250)
                {
                    Dx12MeshRenderer renderer = this._dx12Renderer;
                    this._dx12FpsLabel.Text = renderer == null
                        ? "D3D12: запуск…"
                        : string.Format(CultureInfo.InvariantCulture, "D3D12: {0:0} FPS", renderer.Fps);
                    this.UpdateDx12StatsOverlay();
                    this._dx12LastFpsLabelUpdateUtc = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                string context = DescribeDx12Failure("Prepare frame snapshot", ex) +
                    "; SceneMeshes=" + this._dx12MeshScratch.Count +
                    "; ClientSize=" + this._dx12Panel.ClientSize.Width + "x" + this._dx12Panel.ClientSize.Height;
                ApplicationLog.Write("DX12_FRAME_SNAPSHOT_FAILED", context, ex);
                this._dx12RenderTimer.Stop();
                this._dx12FpsLabel.Text = "D3D12: сбой";
                if (this._sceneStatsOverlay != null)
                    this._sceneStatsOverlay.Text = "Direct3D 12 остановлен\r\n" + context;
                this.SetStatsText(() => this.LF("viewer.dx12.renderFailed", "Ошибка DX12-рендера: {0}", ex.Message));
            }
        }

        private void Dx12RenderThreadMain()
        {
            Dx12MeshRenderer renderer = null;
            bool timerResolutionRequested = false;
            IntPtr waitableTimer = IntPtr.Zero;
            int renderWidth = Math.Max(1, this._dx12InitialRenderWidth);
            int renderHeight = Math.Max(1, this._dx12InitialRenderHeight);
            try
            {
                try { timerResolutionRequested = timeBeginPeriod(1) == 0; }
                catch { timerResolutionRequested = false; }

                // Prefer a true high-resolution timer over repeated relative 16 ms waits.
                // A relative WaitOne(16) accumulates scheduler oversleep and was producing
                // a ~40 Hz loop even though each individual measured interval often looked ~17 ms.
                try
                {
                    waitableTimer = CreateWaitableTimerExW(IntPtr.Zero, null, CreateWaitableTimerHighResolution, TimerAllAccess);
                    if (waitableTimer == IntPtr.Zero)
                        waitableTimer = CreateWaitableTimerExW(IntPtr.Zero, null, 0, TimerAllAccess);
                }
                catch { waitableTimer = IntPtr.Zero; }

                if (this._dx12RenderHwnd == IntPtr.Zero)
                    throw new InvalidOperationException("DX12 render thread started without a valid panel HWND.");
                renderer = new Dx12MeshRenderer(this._dx12RenderHwnd, renderWidth, renderHeight);
                this._dx12Renderer = renderer;

                const double TargetFramesPerSecond = 60.0;
                long framePeriodTicks = Math.Max(1L, (long)Math.Round(System.Diagnostics.Stopwatch.Frequency / TargetFramesPerSecond));
                long nextFrameDeadlineTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                long previousLoopStartTicks = 0;
                IntPtr[] waitHandles = waitableTimer != IntPtr.Zero
                    ? new[] { this._dx12RenderStopEvent.SafeWaitHandle.DangerousGetHandle(), waitableTimer }
                    : null;

                while (!this._dx12RenderStopEvent.WaitOne(0))
                {
                    long loopStartTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                    if (previousLoopStartTicks != 0)
                    {
                        Volatile.Write(ref this._dx12RenderLoopIntervalMilliseconds,
                            (loopStartTicks - previousLoopStartTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                    }
                    previousLoopStartTicks = loopStartTicks;

                    Dx12RenderFrameRequest frame;
                    lock (this._dx12FrameRequestSync)
                        frame = this._dx12LatestFrameRequest;

                    if (frame != null && frame.Width > 0 && frame.Height > 0)
                    {
                        if (frame.Width != renderWidth || frame.Height != renderHeight)
                        {
                            renderer.Resize(frame.Width, frame.Height);
                            renderWidth = frame.Width;
                            renderHeight = frame.Height;
                        }

                        renderer.Render(frame.Meshes, frame.ShadingMode,
                            frame.Yaw, frame.Pitch, frame.Distance,
                            frame.TargetX, frame.TargetY, frame.TargetZ, frame.BoundingRadius);
                    }

                    // Pace against an absolute Stopwatch deadline. This avoids adding the render
                    // work duration to a fixed 16 ms wait and measures timer overshoot directly.
                    nextFrameDeadlineTicks += framePeriodTicks;
                    long nowTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                    if (nextFrameDeadlineTicks <= nowTicks)
                        nextFrameDeadlineTicks = nowTicks + framePeriodTicks; // Drop a missed deadline; never busy-loop to catch up.

                    if (!WaitForDx12FrameDeadline(waitableTimer, waitHandles, nextFrameDeadlineTicks))
                        break;
                }
            }
            catch (Exception ex)
            {
                string context = DescribeDx12Failure(renderer == null ? "Initialize renderer on background thread" : "Dedicated render thread", ex);
                ApplicationLog.Write(renderer == null ? "DX12_INIT_FAILED" : "DX12_RENDER_THREAD_FAILED", context, ex);
                try
                {
                    bool initializationFailure = renderer == null;
                    if (!this.IsDisposed && this.IsHandleCreated)
                    {
                        this.BeginInvoke((Action)(() => this.HandleDx12RenderThreadFailure(context, ex, initializationFailure)));
                    }
                }
                catch (Exception postException)
                {
                    ApplicationLog.Write("DX12_RENDER_THREAD_ERROR_POST_FAILED", "Could not post DX12 render-thread failure to the UI.", postException);
                }
            }
            finally
            {
                if (waitableTimer != IntPtr.Zero)
                {
                    try { CloseHandle(waitableTimer); } catch { }
                }
                if (timerResolutionRequested)
                {
                    try { timeEndPeriod(1); } catch { }
                }
                if (renderer != null)
                {
                    try { renderer.Dispose(); }
                    catch (Exception ex) { ApplicationLog.Write("DX12_RENDER_THREAD_DISPOSE_FAILED", "DX12 renderer disposal failed on the render thread.", ex); }
                }
                if (ReferenceEquals(this._dx12Renderer, renderer))
                    this._dx12Renderer = null;
            }
        }

        private bool WaitForDx12FrameDeadline(IntPtr waitableTimer, IntPtr[] waitHandles, long deadlineTicks)
        {
            long waitStartTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            long remainingTicks = deadlineTicks - waitStartTicks;
            if (remainingTicks > 0)
            {
                if (waitableTimer != IntPtr.Zero && waitHandles != null)
                {
                    long dueTime100ns = -Math.Max(1L, (long)Math.Ceiling(
                        remainingTicks * (10_000_000.0 / System.Diagnostics.Stopwatch.Frequency)));
                    if (SetWaitableTimer(waitableTimer, ref dueTime100ns, 0, IntPtr.Zero, IntPtr.Zero, false))
                    {
                        uint waitResult = WaitForMultipleObjects((uint)waitHandles.Length, waitHandles, false, Infinite);
                        if (waitResult == WaitObject0) return false; // Stop event.
                        if (waitResult != WaitObject0 + 1 && waitResult != WaitFailed)
                        {
                            // Unexpected wait result: fall back below rather than terminate rendering.
                        }
                        else if (waitResult == WaitObject0 + 1)
                        {
                            long wakeTicks = System.Diagnostics.Stopwatch.GetTimestamp();
                            Volatile.Write(ref this._dx12RenderWaitMilliseconds,
                                (wakeTicks - waitStartTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
                            Volatile.Write(ref this._dx12RenderWaitOvershootMilliseconds,
                                Math.Max(0.0, (wakeTicks - deadlineTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency));
                            return !this._dx12RenderStopEvent.WaitOne(0);
                        }
                    }
                }

                // Fallback path for systems without the high-resolution waitable timer.
                // Sleep most of the remaining duration, then spin only for the final <1 ms.
                while (!_dx12RenderStopEvent.WaitOne(0))
                {
                    remainingTicks = deadlineTicks - System.Diagnostics.Stopwatch.GetTimestamp();
                    if (remainingTicks <= 0) break;
                    double remainingMs = remainingTicks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                    if (remainingMs > 1.0)
                    {
                        int sleepMs = Math.Max(1, (int)Math.Floor(remainingMs - 0.5));
                        if (this._dx12RenderStopEvent.WaitOne(sleepMs)) return false;
                    }
                    else
                    {
                        Thread.SpinWait(32);
                    }
                }
                if (this._dx12RenderStopEvent.WaitOne(0)) return false;
            }

            long wakeTimeTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            Volatile.Write(ref this._dx12RenderWaitMilliseconds,
                Math.Max(0.0, (wakeTimeTicks - waitStartTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency));
            Volatile.Write(ref this._dx12RenderWaitOvershootMilliseconds,
                Math.Max(0.0, (wakeTimeTicks - deadlineTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency));
            return !this._dx12RenderStopEvent.WaitOne(0);
        }

        private void HandleDx12RenderThreadFailure(string context, Exception exception, bool initializationFailure)
        {
            if (this.IsDisposed) return;
            this._dx12RendererActive = false;
            if (this._dx12RenderTimer != null) this._dx12RenderTimer.Stop();
            if (initializationFailure)
            {
                // Match the old synchronous-init fallback: if DX12 initialization fails, show the WPF host.
                if (this.host != null) this.host.Visible = true;
                if (this._dx12Panel != null) this._dx12Panel.Visible = false;
            }
            if (this._dx12FpsLabel != null) this._dx12FpsLabel.Text = initializationFailure ? "D3D12: ошибка" : "D3D12: сбой потока";
            if (this._sceneStatsOverlay != null) this._sceneStatsOverlay.Text = "Direct3D 12 остановлен\r\n" + context;
            this.SetStatsText(() => this.LF("viewer.dx12.renderFailed", "Ошибка DX12-рендера: {0}", exception.Message));
        }

        private void UpdateDx12StatsOverlay()
        {
            if (this._sceneStatsOverlay == null || this._sceneStatsOverlay.IsDisposed) return;
            DateTime nowUtc = DateTime.UtcNow;
            if ((nowUtc - this._lastDx12StatsOverlayUpdateUtc).TotalMilliseconds < 100.0)
                return;
            this._lastDx12StatsOverlayUpdateUtc = nowUtc;
            Dx12MeshRenderer renderer = this._dx12Renderer;
            if (renderer == null) return;
            string mode = this.GetShadingModeLabel(this._shadingMode);
            this._sceneStatsOverlay.Text = string.Format(CultureInfo.InvariantCulture,
                "PrototypeUltimateTool  |  Direct3D 12\r\n" +
                "FPS {0:0.0}   Call Δ {1:0.00} ms   Submit Δ {2:0.00} ms\r\n" +
                "CPU {3:0.00} ms   Prep {4:0.00} ms   Present+Signal {5:0.00} ms\r\n" +
                "Loop {6:0.0} Hz (avg Δ {19:0.00} ms)   Calls/Frames {7:N0}/{8:N0}   Fence skips {9:N0}\r\n" +
                "Mode: {10}   MSAA {11}x\r\n" +
                "Objects {12:N0}   Draw calls {13:N0}\r\n" +
                "Vertices {14:N0}   Edges {15:N0}\r\n" +
                "Faces {16:N0}   Triangles {17:N0}\r\n" +
                "Texture data {18:0.0} MiB\r\n" +
                "Last Δ {20:0.00} ms   Wait {21:0.00} ms   Late {22:0.00} ms",
                renderer.Fps, renderer.FrameTimeMilliseconds, renderer.SubmittedFrameIntervalMilliseconds,
                renderer.CpuRenderMilliseconds, this._dx12SceneCollectionMilliseconds, renderer.PresentAndSignalMilliseconds,
                renderer.RenderCallRate, renderer.RenderCallCount, renderer.SubmittedFrameCount,
                renderer.SkippedFrames, mode, renderer.MsaaSampleCount, renderer.MeshCount, renderer.DrawCallCount,
                renderer.VertexCount, renderer.EdgeCount, renderer.FaceCount, renderer.TriangleCount,
                renderer.TextureMemoryBytes / (1024.0 * 1024.0),
                renderer.RenderCallRate > 0.01 ? 1000.0 / renderer.RenderCallRate : 0.0,
                Volatile.Read(ref this._dx12RenderLoopIntervalMilliseconds),
                Volatile.Read(ref this._dx12RenderWaitMilliseconds),
                Volatile.Read(ref this._dx12RenderWaitOvershootMilliseconds));
        }

        private void PositionDx12Overlays()
        {
            if (this._sceneStatsOverlay != null)
                this._sceneStatsOverlay.UpdateAnchorPosition();
            if (this._dx12GizmoOverlay != null && this._dx12Panel != null)
                this._dx12GizmoOverlay.Location = new System.Drawing.Point(
                    Math.Max(4, this._dx12Panel.ClientSize.Width - this._dx12GizmoOverlay.Width - 8), 8);
        }

        private void PaintDx12Gizmo(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.Clear(System.Drawing.Color.FromArgb(34, 37, 43));
            float cx = this._dx12GizmoOverlay.ClientSize.Width * 0.5f;
            float cy = this._dx12GizmoOverlay.ClientSize.Height * 0.5f + 2.0f;
            double yaw = this._yaw * Math.PI / 180.0;
            double pitch = this._pitch * Math.PI / 180.0;
            float length = 27.0f;
            float xX = (float)(cx - length * Math.Cos(pitch) * Math.Cos(yaw));
            float xY = (float)(cy + length * Math.Sin(pitch) * Math.Sin(yaw));
            float yX = (float)(cx + length * Math.Cos(pitch) * Math.Sin(yaw));
            float yY = (float)(cy + length * Math.Sin(pitch) * Math.Cos(yaw));
            float zX = cx;
            float zY = (float)(cy - length * Math.Cos(pitch));
            DrawGizmoAxis(graphics, cx, cy, xX, xY, System.Drawing.Color.FromArgb(242, 82, 82), "X");
            DrawGizmoAxis(graphics, cx, cy, yX, yY, System.Drawing.Color.FromArgb(80, 205, 120), "Y");
            DrawGizmoAxis(graphics, cx, cy, zX, zY, System.Drawing.Color.FromArgb(82, 150, 255), "Z");
        }

        private static void DrawGizmoAxis(Graphics graphics, float cx, float cy, float ex, float ey, System.Drawing.Color color, string label)
        {
            using (var pen = new System.Drawing.Pen(color, 2.4f)) graphics.DrawLine(pen, cx, cy, ex, ey);
            using (var brush = new System.Drawing.SolidBrush(color))
            using (var font = new System.Drawing.Font("Segoe UI", 8.0f, System.Drawing.FontStyle.Bold))
                graphics.DrawString(label, font, brush, ex - 4.0f, ey - 8.0f);
        }

        private void OnDx12PanelMouseDown(object sender, MouseEventArgs e)
        {
            this._dx12LastMousePosition = e.Location;
            this._dx12MouseOrbiting = e.Button == MouseButtons.Left;
            this._dx12MousePanning = e.Button == MouseButtons.Right;
            if (this._dx12MouseOrbiting || this._dx12MousePanning) this._dx12Panel.Capture = true;
        }

        private void OnDx12PanelMouseMove(object sender, MouseEventArgs e)
        {
            if (!this._dx12MouseOrbiting && !this._dx12MousePanning) return;
            int dx = e.X - this._dx12LastMousePosition.X;
            int dy = e.Y - this._dx12LastMousePosition.Y;
            this._dx12LastMousePosition = e.Location;
            if (this._dx12MouseOrbiting)
            {
                this._yaw += dx * 0.5;
                this._pitch = Math.Max(-89.0, Math.Min(89.0, this._pitch + dy * 0.5));
            }
            else if (this._dx12MousePanning)
            {
                double factor = this._distance * 0.0015;
                double radYaw = this._yaw * Math.PI / 180.0;
                this._targetX += dx * factor * Math.Cos(radYaw);
                this._targetY -= dx * factor * Math.Sin(radYaw);
                this._targetZ += dy * factor;
            }
            this.UpdateCamera();
            if (this._dx12GizmoOverlay != null) this._dx12GizmoOverlay.Invalidate();
        }

        private void OnDx12PanelMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) this._dx12MouseOrbiting = false;
            if (e.Button == MouseButtons.Right) this._dx12MousePanning = false;
            if (!this._dx12MouseOrbiting && !this._dx12MousePanning) this._dx12Panel.Capture = false;
        }

        private void OnDx12PanelMouseWheel(object sender, MouseEventArgs e)
        {
            double factor = e.Delta > 0 ? 0.85 : 1.15;
            double minDistance = Math.Max(0.001, this._boundingRadius * 0.004);
            double maxDistance = Math.Max(minDistance * 100.0, this._boundingRadius * 80.0);
            this._distance = Math.Max(minDistance, Math.Min(maxDistance, this._distance * factor));
            this.UpdateCamera();
            if (this._dx12GizmoOverlay != null) this._dx12GizmoOverlay.Invalidate();
        }

        private void AddWireframeOverlay(MeshRenderInfo info)
        {
            MeshGeometry3D edgeMesh = new MeshGeometry3D();
            MeshGeometry3D vertexMesh = new MeshGeometry3D();
            HashSet<long> seenEdges = new HashSet<long>();
            Int32Collection indices = info.Mesh.TriangleIndices;
            double width = Math.Max(0.0001, this._boundingRadius * 0.00045);

            for (int i = 0; i + 2 < indices.Count; i += 3)
            {
                AddUniqueEdge(edgeMesh, info.Mesh, seenEdges, indices[i], indices[i + 1], width);
                AddUniqueEdge(edgeMesh, info.Mesh, seenEdges, indices[i + 1], indices[i + 2], width);
                AddUniqueEdge(edgeMesh, info.Mesh, seenEdges, indices[i + 2], indices[i], width);
            }

            double markerRadius = width * 2.0;
            for (int i = 0; i < info.Mesh.Positions.Count; i++)
                AddOctahedron(vertexMesh, info.Mesh.Positions[i], markerRadius);

            System.Windows.Media.Color edgeColor = System.Windows.Media.Color.FromRgb(218, 223, 230);
            System.Windows.Media.Color vertexColor = System.Windows.Media.Color.FromRgb(72, 202, 255);
            this.AddOverlayModel(edgeMesh, edgeColor);
            this.AddOverlayModel(vertexMesh, vertexColor);
        }

        private static void AddUniqueEdge(MeshGeometry3D target, MeshGeometry3D source, HashSet<long> seen, int a, int b, double width)
        {
            if (a < 0 || b < 0 || a >= source.Positions.Count || b >= source.Positions.Count || a == b) return;
            int low = Math.Min(a, b), high = Math.Max(a, b);
            long key = ((long)low << 32) | (uint)high;
            if (!seen.Add(key)) return;
            AddLineRibbon(target, source.Positions[a], source.Positions[b], width);
        }

        private void AddNormalsOverlay(MeshRenderInfo info)
        {
            Vector3D[] normals = GetMeshNormals(info.Mesh);
            if (normals.Length != info.Mesh.Positions.Count) return;

            MeshGeometry3D[] buckets = new MeshGeometry3D[6];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = new MeshGeometry3D();
            double lineLength = Math.Max(1.0, this._boundingRadius * 0.025);
            double lineWidth = Math.Max(0.0001, this._boundingRadius * 0.00035);

            for (int i = 0; i < normals.Length; i++)
            {
                Vector3D n = normals[i];
                if (n.LengthSquared < 0.000001) continue;
                n.Normalize();
                int bucket = GetNormalAxisBucket(n);
                Point3D start = info.Mesh.Positions[i];
                AddLineRibbon(buckets[bucket], start, start + n * lineLength, lineWidth);
            }

            System.Windows.Media.Color[] colors = new System.Windows.Media.Color[]
            {
                System.Windows.Media.Color.FromRgb(255, 86, 86), System.Windows.Media.Color.FromRgb(150, 45, 45),
                System.Windows.Media.Color.FromRgb(91, 235, 135), System.Windows.Media.Color.FromRgb(42, 132, 72),
                System.Windows.Media.Color.FromRgb(80, 177, 255), System.Windows.Media.Color.FromRgb(45, 95, 170)
            };
            for (int i = 0; i < buckets.Length; i++) this.AddOverlayModel(buckets[i], colors[i]);
        }

        private static int GetNormalAxisBucket(Vector3D n)
        {
            double ax = Math.Abs(n.X), ay = Math.Abs(n.Y), az = Math.Abs(n.Z);
            if (ax >= ay && ax >= az) return n.X >= 0 ? 0 : 1;
            if (ay >= ax && ay >= az) return n.Y >= 0 ? 2 : 3;
            return n.Z >= 0 ? 4 : 5;
        }

        private static Vector3D[] GetMeshNormals(MeshGeometry3D mesh)
        {
            int count = mesh == null || mesh.Positions == null ? 0 : mesh.Positions.Count;
            Vector3D[] result = new Vector3D[count];
            if (count == 0) return result;

            if (mesh.Normals != null && mesh.Normals.Count == count)
            {
                for (int i = 0; i < count; i++) result[i] = mesh.Normals[i];
            }
            else
            {
                for (int i = 0; i + 2 < mesh.TriangleIndices.Count; i += 3)
                {
                    int a = mesh.TriangleIndices[i], b = mesh.TriangleIndices[i + 1], c = mesh.TriangleIndices[i + 2];
                    if (a < 0 || b < 0 || c < 0 || a >= count || b >= count || c >= count) continue;
                    Vector3D face = Vector3D.CrossProduct(mesh.Positions[b] - mesh.Positions[a], mesh.Positions[c] - mesh.Positions[a]);
                    if (face.LengthSquared < 0.000001) continue;
                    result[a] += face; result[b] += face; result[c] += face;
                }
            }
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].LengthSquared > 0.000001) result[i].Normalize();
                else result[i] = new Vector3D(0, 0, 1);
            }
            return result;
        }

        private bool AddSkinningOverlay(MeshRenderInfo info)
        {
            int vertexCount = info.Mesh.Positions.Count;
            if (vertexCount == 0 || info.Weights == null || info.Groups == null ||
                info.Weights.Count != vertexCount || info.Groups.Count != vertexCount)
                return false;

            Dictionary<uint, MeshGeometry3D> meshesByBone = new Dictionary<uint, MeshGeometry3D>();
            MeshGeometry3D source = info.Mesh;
            for (int i = 0; i + 2 < source.TriangleIndices.Count; i += 3)
            {
                int a = source.TriangleIndices[i], b = source.TriangleIndices[i + 1], c = source.TriangleIndices[i + 2];
                uint bone;
                if (!TryGetTriangleDominantBone(info, a, b, c, out bone)) continue;

                MeshGeometry3D target;
                if (!meshesByBone.TryGetValue(bone, out target))
                {
                    target = new MeshGeometry3D();
                    meshesByBone[bone] = target;
                }
                AddSourceVertex(target, source, a);
                AddSourceVertex(target, source, b);
                AddSourceVertex(target, source, c);
                int last = target.Positions.Count - 1;
                target.TriangleIndices.Add(last - 2);
                target.TriangleIndices.Add(last - 1);
                target.TriangleIndices.Add(last);
            }

            if (meshesByBone.Count == 0) return false;
            foreach (KeyValuePair<uint, MeshGeometry3D> pair in meshesByBone)
                this.AddOverlayModel(pair.Value, GetBoneDisplayColor(pair.Key));
            return true;
        }

        private static bool TryGetTriangleDominantBone(MeshRenderInfo info, int a, int b, int c, out uint dominantBone)
        {
            dominantBone = 0;
            Dictionary<uint, double> totals = new Dictionary<uint, double>();
            if (!AccumulateVertexWeights(info, a, totals) || !AccumulateVertexWeights(info, b, totals) || !AccumulateVertexWeights(info, c, totals))
                return false;
            double best = 0.0;
            foreach (KeyValuePair<uint, double> pair in totals)
            {
                if (pair.Value > best) { best = pair.Value; dominantBone = pair.Key; }
            }
            return best > 0.000001;
        }

        private static bool AccumulateVertexWeights(MeshRenderInfo info, int vertexIndex, Dictionary<uint, double> totals)
        {
            if (vertexIndex < 0 || vertexIndex >= info.Weights.Count || vertexIndex >= info.Groups.Count) return false;
            Nixson.Common.Vector3 weights = info.Weights[vertexIndex];
            byte[] groups = info.Groups[vertexIndex];
            if (weights == null || groups == null || groups.Length == 0) return false;
            double[] values = new double[]
            {
                Math.Max(0.0, weights.X), Math.Max(0.0, weights.Y), Math.Max(0.0, weights.Z),
                Math.Max(0.0, 1.0 - weights.X - weights.Y - weights.Z)
            };
            double sum = values.Sum();
            if (sum <= 0.000001) return false;
            int influenceCount = Math.Min(4, groups.Length);
            for (int i = 0; i < influenceCount; i++)
            {
                if (values[i] <= 0.000001) continue;
                int groupSlot = info.LegacyRawSkinLayout && groups.Length >= 4 ? 3 - i : i;
                uint localIndex = groups[groupSlot];
                uint bone = localIndex;
                if (info.MatrixIndices != null && localIndex < info.MatrixIndices.Length)
                    bone = info.MatrixIndices[localIndex];
                double oldValue;
                totals.TryGetValue(bone, out oldValue);
                totals[bone] = oldValue + values[i] / sum;
            }
            return true;
        }

        private static System.Windows.Media.Color GetBoneDisplayColor(uint bone)
        {
            System.Windows.Media.Color[] palette = new System.Windows.Media.Color[]
            {
                System.Windows.Media.Color.FromRgb(232, 78, 75),
                System.Windows.Media.Color.FromRgb(76, 180, 105),
                System.Windows.Media.Color.FromRgb(70, 142, 235),
                System.Windows.Media.Color.FromRgb(235, 180, 65),
                System.Windows.Media.Color.FromRgb(195, 94, 218),
                System.Windows.Media.Color.FromRgb(53, 198, 199),
                System.Windows.Media.Color.FromRgb(233, 123, 63),
                System.Windows.Media.Color.FromRgb(138, 157, 74),
                System.Windows.Media.Color.FromRgb(107, 104, 224),
                System.Windows.Media.Color.FromRgb(222, 106, 150),
                System.Windows.Media.Color.FromRgb(103, 182, 165),
                System.Windows.Media.Color.FromRgb(175, 138, 92)
            };
            int paletteIndex = (int)(bone % (uint)palette.Length);
            return palette[paletteIndex];
        }

        private static void AddSourceVertex(MeshGeometry3D target, MeshGeometry3D source, int index)
        {
            target.Positions.Add(source.Positions[index]);
            if (source.Normals != null && source.Normals.Count == source.Positions.Count)
                target.Normals.Add(source.Normals[index]);
            if (source.TextureCoordinates != null && source.TextureCoordinates.Count == source.Positions.Count)
                target.TextureCoordinates.Add(source.TextureCoordinates[index]);
        }

        private void AddOverlayModel(MeshGeometry3D mesh, System.Windows.Media.Color color)
        {
            if (mesh == null || mesh.Positions.Count == 0 || mesh.TriangleIndices.Count == 0) return;
            Material material = CreateOverlayMaterial(color);
            GeometryModel3D model = new GeometryModel3D(mesh, material) { BackMaterial = material };
            this.rootGroup.Children.Add(model);
            this._loadedModels.Add(model);
        }

        private static Material CreateOverlayMaterial(System.Windows.Media.Color color)
        {
            MaterialGroup group = new MaterialGroup();
            group.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
            group.Children.Add(new EmissiveMaterial(new SolidColorBrush(System.Windows.Media.Color.FromArgb(85, color.R, color.G, color.B))));
            return group;
        }

        private static void AddLineRibbon(MeshGeometry3D mesh, Point3D start, Point3D end, double halfWidth)
        {
            // Build a small six-sided tube instead of a flat ribbon. The old ribbon was
            // aligned to world Z, so from some camera angles it became nearly edge-on and
            // appeared dotted or broken even when zoomed in. A tube has a stable silhouette
            // regardless of camera orientation and does not need to be rebuilt while orbiting.
            Vector3D direction = end - start;
            if (direction.LengthSquared < 0.000001 || halfWidth <= 0.0) return;
            direction.Normalize();

            Vector3D reference = Math.Abs(direction.Z) < 0.9
                ? new Vector3D(0, 0, 1)
                : new Vector3D(0, 1, 0);
            Vector3D sideA = Vector3D.CrossProduct(direction, reference);
            if (sideA.LengthSquared < 0.000001)
                sideA = Vector3D.CrossProduct(direction, new Vector3D(1, 0, 0));
            if (sideA.LengthSquared < 0.000001) return;
            sideA.Normalize();

            Vector3D sideB = Vector3D.CrossProduct(direction, sideA);
            if (sideB.LengthSquared < 0.000001) return;
            sideB.Normalize();

            const int sides = 6;
            if (mesh.Normals == null)
                mesh.Normals = new Vector3DCollection();
            int first = mesh.Positions.Count;
            for (int i = 0; i < sides; i++)
            {
                double angle = (Math.PI * 2.0 * i) / sides;
                Vector3D radial = sideA * Math.Cos(angle) + sideB * Math.Sin(angle);
                Vector3D offset = radial * halfWidth;

                mesh.Positions.Add(start + offset);
                mesh.Positions.Add(end + offset);
                if (mesh.Normals != null)
                {
                    mesh.Normals.Add(radial);
                    mesh.Normals.Add(radial);
                }
            }

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                int startA = first + i * 2;
                int endA = startA + 1;
                int startB = first + next * 2;
                int endB = startB + 1;

                mesh.TriangleIndices.Add(startA);
                mesh.TriangleIndices.Add(startB);
                mesh.TriangleIndices.Add(endA);
                mesh.TriangleIndices.Add(startB);
                mesh.TriangleIndices.Add(endB);
                mesh.TriangleIndices.Add(endA);
            }
        }

        private static void AddOctahedron(MeshGeometry3D mesh, Point3D center, double radius)
        {
            int first = mesh.Positions.Count;
            mesh.Positions.Add(new Point3D(center.X + radius, center.Y, center.Z));
            mesh.Positions.Add(new Point3D(center.X - radius, center.Y, center.Z));
            mesh.Positions.Add(new Point3D(center.X, center.Y + radius, center.Z));
            mesh.Positions.Add(new Point3D(center.X, center.Y - radius, center.Z));
            mesh.Positions.Add(new Point3D(center.X, center.Y, center.Z + radius));
            mesh.Positions.Add(new Point3D(center.X, center.Y, center.Z - radius));
            int[] tri = new int[]
            {
                0,2,4, 2,1,4, 1,3,4, 3,0,4,
                2,0,5, 1,2,5, 3,1,5, 0,3,5
            };
            for (int i = 0; i < tri.Length; i++) mesh.TriangleIndices.Add(first + tri[i]);
        }

        private sealed class P2PreviewMeshData
        {
            public string Name;
            public string ShaderName;
            public List<Nixson.Common.Vector3> Positions = new List<Nixson.Common.Vector3>();
            public List<Nixson.Common.Vector3> Normals = new List<Nixson.Common.Vector3>();
            public List<UVCoordinate> UVs = new List<UVCoordinate>();
            public List<int> TriangleIndices = new List<int>();
        }

        private static List<P2PreviewMeshData> BuildP2PreviewMeshes(BaseNode selectedNode, Pure3DFile p3d)
        {
            var result = new List<P2PreviewMeshData>();
            if (selectedNode == null) return result;

            var allNodes = p3d != null && p3d.Nodes != null
                ? FlattenNodes(p3d.Nodes).ToList()
                : FlattenNodes(new[] { selectedNode }).ToList();
            var selectedSubtree = FlattenNodes(new[] { selectedNode }).ToList();
            var skins = selectedSubtree.OfType<P2PolySkin>().ToList();

            // When metadata itself is selected, recover its owning skin if available.
            if (selectedNode is P2PolySkinMetadata)
            {
                var parentSkin = selectedNode.GetParentNode<P2PolySkin>();
                if (parentSkin != null && !skins.Contains(parentSkin)) skins.Add(parentSkin);
            }

            foreach (var skin in skins)
            {
                var metadata = skin.Children == null ? null : skin.Children.OfType<P2PolySkinMetadata>().FirstOrDefault();
                if (metadata == null) continue;

                var indexPrimitive = FindP2Primitive(allNodes, metadata.IndicesName);
                var vertexPrimitive = FindP2Primitive(allNodes, metadata.VerticesName);
                var skinPrimitive = FindP2Primitive(allNodes, metadata.SkinName) ?? vertexPrimitive;
                var data = BuildP2PreviewMeshData(skinPrimitive, vertexPrimitive, indexPrimitive, skin.Name, metadata.ShaderName);
                if (data != null) result.Add(data);
            }

            // Direct P2Primitive selection is useful for inspecting standalone vertex/index chunks.
            if (selectedNode is P2Primitive selectedPrimitive)
            {
                var data = BuildP2PreviewMeshData(selectedPrimitive, selectedPrimitive, null,
                    string.IsNullOrWhiteSpace(selectedPrimitive.Name) ? "P2Primitive" : selectedPrimitive.Name, null);
                if (data != null) result.Add(data);
            }

            return result;
        }

        private static P2PreviewMeshData BuildP2PreviewMeshData(
            P2Primitive skinPrimitive,
            P2Primitive vertexPrimitive,
            P2Primitive indexPrimitive,
            string ownerName,
            string shaderName)
        {
            if (skinPrimitive == null && vertexPrimitive == null) return null;

            var positionItems = GetP2BufferItems(skinPrimitive, DescriptionTypeEnum.Position);
            if (positionItems.Count == 0 && !ReferenceEquals(skinPrimitive, vertexPrimitive))
                positionItems = GetP2BufferItems(vertexPrimitive, DescriptionTypeEnum.Position);
            if (positionItems.Count == 0) return null;

            var result = new P2PreviewMeshData { Name = ownerName, ShaderName = shaderName };
            foreach (var item in positionItems)
            {
                var value = SafeGetP2Value(item);
                if (value is Nixson.Common.Vector3 position) result.Positions.Add(position);
            }
            if (result.Positions.Count == 0) return null;

            var normalItems = GetP2BufferItems(skinPrimitive, DescriptionTypeEnum.Normal);
            if (normalItems.Count == 0 && !ReferenceEquals(skinPrimitive, vertexPrimitive))
                normalItems = GetP2BufferItems(vertexPrimitive, DescriptionTypeEnum.Normal);
            foreach (var item in normalItems)
            {
                var value = SafeGetP2Value(item);
                if (value is Nixson.Common.Vector3 normal) result.Normals.Add(normal);
            }
            if (result.Normals.Count != result.Positions.Count) result.Normals.Clear();

            var uvItems = GetP2BufferItems(vertexPrimitive, DescriptionTypeEnum.UV);
            if (uvItems.Count == 0) uvItems = GetP2BufferItems(vertexPrimitive, DescriptionTypeEnum.UV1);
            if (uvItems.Count == 0) uvItems = GetP2BufferItems(skinPrimitive, DescriptionTypeEnum.UV);
            if (uvItems.Count == 0) uvItems = GetP2BufferItems(skinPrimitive, DescriptionTypeEnum.UV1);
            foreach (var item in uvItems)
            {
                var value = SafeGetP2Value(item);
                if (value is UVCoordinate uv) result.UVs.Add(uv);
            }
            if (result.UVs.Count != result.Positions.Count) result.UVs.Clear();

            var indexItems = GetP2BufferItems(indexPrimitive, DescriptionTypeEnum.Group);
            // Older P2 dumps do not always identify index items with the known Group token;
            // EditP3D's fallback semantics are to inspect all items in that primitive.
            if (indexItems.Count == 0 && indexPrimitive != null)
                indexItems = GetAllP2BufferItems(indexPrimitive);

            if (indexItems.Count >= 3)
            {
                int vertexCount = result.Positions.Count;
                for (int i = 0; i + 2 < indexItems.Count; i += 3)
                {
                    int a = ReadP2Index(indexItems[i]);
                    int b = ReadP2Index(indexItems[i + 1]);
                    int c = ReadP2Index(indexItems[i + 2]);
                    if (a >= 0 && b >= 0 && c >= 0 && a < vertexCount && b < vertexCount && c < vertexCount)
                    {
                        result.TriangleIndices.Add(a);
                        result.TriangleIndices.Add(b);
                        result.TriangleIndices.Add(c);
                    }
                }
            }
            else if (indexPrimitive == null || ReferenceEquals(indexPrimitive, skinPrimitive))
            {
                for (int i = 0; i + 2 < result.Positions.Count; i += 3)
                {
                    result.TriangleIndices.Add(i);
                    result.TriangleIndices.Add(i + 1);
                    result.TriangleIndices.Add(i + 2);
                }
            }

            return result.TriangleIndices.Count >= 3 ? result : null;
        }

        private static List<BufferItem> GetP2BufferItems(P2Primitive primitive, DescriptionTypeEnum type)
        {
            var result = new List<BufferItem>();
            if (primitive == null || primitive.Children == null) return result;
            foreach (var buffer in primitive.Children.OfType<P2Buffer>())
            {
                if (buffer.BufferItems == null) continue;
                foreach (var item in buffer.BufferItems)
                {
                    if (item == null) continue;
                    if (item.BufferType != null && item.BufferType.EnumValue == type) result.Add(item);
                }
            }
            return result;
        }

        private static object SafeGetP2Value(BufferItem item)
        {
            if (item == null || item.Data == null) return null;
            try { return item.GetValueP2(); }
            catch { return null; }
        }

        private static List<BufferItem> GetAllP2BufferItems(P2Primitive primitive)
        {
            var result = new List<BufferItem>();
            if (primitive == null || primitive.Children == null) return result;
            foreach (var buffer in primitive.Children.OfType<P2Buffer>())
            {
                if (buffer.BufferItems != null)
                    result.AddRange(buffer.BufferItems.Where(item => item != null));
            }
            return result;
        }

        private static int ReadP2Index(BufferItem item)
        {
            if (item == null || item.Data == null || item.Data.Length == 0) return -1;
            try
            {
                if (item.Data.Length >= 2) return BitConverter.ToUInt16(item.Data, 0);
                return item.Data[0];
            }
            catch { return -1; }
        }

        private static P2Primitive FindP2Primitive(IEnumerable<BaseNode> nodes, string name)
        {
            if (nodes == null || string.IsNullOrWhiteSpace(name)) return null;
            return nodes.OfType<P2Primitive>().FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<BaseNode> FlattenNodes(IEnumerable<BaseNode> nodes)
        {
            if (nodes == null) yield break;
            foreach (var node in nodes)
            {
                if (node == null) continue;
                yield return node;
                if (node.Children != null)
                {
                    foreach (var child in FlattenNodes(node.Children)) yield return child;
                }
            }
        }

        /// <summary>
        /// Build a WPF 3D texture brush using absolute UV coordinates. Game meshes commonly
        /// contain UVs outside 0..1 to repeat a texture; the default relative ImageBrush viewport
        /// can remap the coordinate range and make those meshes look stretched or inconsistent.
        /// </summary>
        private static Material CreateTexturedMaterial(ImageSource textureSource)
        {
            var brush = new ImageBrush(textureSource)
            {
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0.0, 0.0, 1.0, 1.0),
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };
            if (brush.CanFreeze)
                brush.Freeze();
            return new DiffuseMaterial(brush);
        }

        // Existing Prototype 1 wrapper; Prototype 2 metadata supplies the shader name separately.
        private ImageSource ResolveTexture(PrimitiveGroup pg, string shapeName, Pure3DFile p3d)
        {
            return this.ResolveTextureByShaderName(pg != null ? pg.ShaderName : null, shapeName, p3d);
        }

        // Shared texture resolution for P1 PrimitiveGroups and P2 PolySkin metadata.
        private ImageSource ResolveTextureByShaderName(string shaderName, string shapeName, Pure3DFile p3d)
        {
            if (p3d == null || p3d.Nodes == null) return null;

            if (this.textureOverrideCombo.SelectedIndex > 0)
            {
                string picked = this.textureOverrideCombo.SelectedItem.ToString();
                foreach (var n in p3d.Nodes)
                {
                    if (GetCleanName(n).Equals(picked, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var bmp = DdsDecoder.ExtractBitmap(n, false)) return BitmapToImageSource(bmp);
                    }
                }
            }

            // 1. Resolve a shader texture when the format node provides a shader name.
            if (!string.IsNullOrWhiteSpace(shaderName))
            {
                foreach (var n in p3d.Nodes)
                {
                    if (n.ToString().Contains("Shader") && GetCleanName(n).Equals(shaderName, StringComparison.OrdinalIgnoreCase))
                    {
                        string texName = ExtractTextureNameFromShader(n);
                        if (!string.IsNullOrEmpty(texName))
                        {
                            foreach (var texNode in p3d.Nodes)
                            {
                                if (GetCleanName(texNode).IndexOf(texName, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    using (var bmp = DdsDecoder.ExtractBitmap(texNode, false)) return BitmapToImageSource(bmp);
                                }
                            }
                        }
                    }
                }
            }

            // 2. Умное сопоставление по персонажу и частям тела
            string[] shapeTokens = (shapeName ?? string.Empty).ToLowerInvariant().Split(new char[] { '_', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);

            BaseNode bestTexNode = null;
            int bestScore = 0;

            foreach (var node in p3d.Nodes)
            {
                if (node.Exportable || node.ToString().Contains("Texture"))
                {
                    string texName = GetCleanName(node).ToLowerInvariant();
                    if (!texName.Contains("_dm") && !texName.Contains("diffuse") && !texName.Contains("color")) continue;

                    int score = 0;
                    foreach (var tok in shapeTokens)
                    {
                        if (tok.Length < 3) continue;
                        if (texName.Contains(tok)) score += 10;
                    }

                    // Приоритеты анатомии
                    string shapeLower = (shapeName ?? string.Empty).ToLowerInvariant();
                    if ((shapeLower.Contains("head") || shapeLower.Contains("face")) && texName.Contains("head")) score += 25;
                    if ((shapeLower.Contains("body") || shapeLower.Contains("arm") || shapeLower.Contains("vest") || shapeLower.Contains("torso") || shapeLower.Contains("leg")) && texName.Contains("body")) score += 20;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTexNode = node;
                    }
                }
            }

            if (bestTexNode != null)
            {
                using (var bmp = DdsDecoder.ExtractBitmap(bestTexNode, false)) return BitmapToImageSource(bmp);
            }

            return null;
        }

        private static string ExtractTextureNameFromShader(BaseNode shaderNode)
        {
            if (shaderNode.Children == null) return null;
            foreach (var c in shaderNode.Children)
            {
                var pVal = c.GetType().GetProperty("Value") ?? c.GetType().GetProperty("TextureName");
                if (pVal != null)
                {
                    string s = pVal.GetValue(c, null) as string;
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
                string str = c.ToString();
                if (str.Contains(".dds"))
                {
                    int o = str.IndexOf('('), cl = str.LastIndexOf(')');
                    if (o >= 0 && cl > o) return str.Substring(o + 1, cl - o - 1);
                }
            }
            return null;
        }

        private static ImageSource BitmapToImageSource(Bitmap bmp)
        {
            if (bmp == null) return null;
            IntPtr hBitmap = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        private bool LoadSkeleton(Skeleton skeletonNode)
        {
            var joints = this.ExtractJoints(skeletonNode);
            if (joints.Count == 0)
            {
                this.SetStatsText(() => this.L("viewer.stats.skeletonEmpty", "Скелет пуст: SkeletonJoint не найдены."));
                return false;
            }

            var state = new SkeletonPreviewState
            {
                SkeletonNode = skeletonNode,
                Joints = joints,
                JointMarkers = new GeometryModel3D[joints.Count],
                BoneModels = new GeometryModel3D[joints.Count],
                WorldPositions = new Point3D[joints.Count],
                WorldGamePositions = new Vector3D[joints.Count],
                WorldAxisX = new Vector3D[joints.Count],
                WorldAxisY = new Vector3D[joints.Count],
                WorldAxisZ = new Vector3D[joints.Count]
            };
            this._activeSkeleton = state;

            this.UpdateSkeletonPose(0.0f);
            state.BindGamePositions = (Vector3D[])state.WorldGamePositions.Clone();
            state.BindAxisX = (Vector3D[])state.WorldAxisX.Clone();
            state.BindAxisY = (Vector3D[])state.WorldAxisY.Clone();
            state.BindAxisZ = (Vector3D[])state.WorldAxisZ.Clone();
            int boneCount = 0;
            for (int i = 0; i < joints.Count; i++)
            {
                Point3D position = state.WorldPositions[i];
                var marker = CreateBox(position, position, 7.0, System.Windows.Media.Color.FromRgb(64, 196, 255));
                state.JointMarkers[i] = marker;
                this.rootGroup.Children.Add(marker);
                this._loadedModels.Add(marker);

                int parent = joints[i].ParentIndex;
                if (parent >= 0 && parent < joints.Count && parent < i && parent != i)
                {
                    Point3D parentPosition = state.WorldPositions[parent];
                    double length = (position - parentPosition).Length;
                    double width = Math.Max(3.0, Math.Min(18.0, length * 0.12));
                    var bone = CreateBlenderBone(parentPosition, position, width, System.Windows.Media.Color.FromRgb(230, 175, 70));
                    state.BoneModels[i] = bone;
                    this.rootGroup.Children.Add(bone);
                    this._loadedModels.Add(bone);
                    boneCount++;
                }
            }

            if (!this.FitCameraToMeshBounds(!this._hasCameraView))
                this.FitCameraToSkeleton(state);
            this.SetStatsText(() => this.LF("viewer.stats.skeleton", "🦴 Скелет: {0} суставов | {1} костей", joints.Count, boneCount));
            return true;
        }

        private sealed class JointData
        {
            public string Name;
            public int ParentIndex;
            public Vector3D LocalPosition;
            public Vector3D LocalAxisX;
            public Vector3D LocalAxisY;
            public Vector3D LocalAxisZ;
        }

        private List<JointData> ExtractJoints(BaseNode skeletonNode)
        {
            if (skeletonNode == null) return new List<JointData>();
            Skeleton actualSkeleton = skeletonNode as Skeleton ?? skeletonNode.GetParentNode<Skeleton>();
            if (actualSkeleton != null)
            {
                AnimationSourceIndex index;
                if (this._skeletonSourceIndexes.TryGetValue(actualSkeleton, out index))
                {
                    List<JointData> cached;
                    if (index.Joints.TryGetValue(actualSkeleton, out cached)) return cached;
                    cached = ExtractJointsUncached(actualSkeleton);
                    index.Joints[actualSkeleton] = cached;
                    return cached;
                }
            }
            return ExtractJointsUncached((BaseNode)actualSkeleton ?? skeletonNode);
        }

        private static List<JointData> ExtractJointsUncached(BaseNode skeletonNode)
        {
            var result = new List<JointData>();
            if (skeletonNode == null) return result;
            BaseNode actualSkeleton = skeletonNode as Skeleton ?? skeletonNode.GetParentNode<Skeleton>();
            if (actualSkeleton == null) actualSkeleton = skeletonNode;

            // SkeletonJoint nodes are usually direct children, but some asset variants wrap
            // them in intermediate containers. Preserve preorder so ParentIndex remains stable.
            var serializedJoints = actualSkeleton.Children == null
                ? new List<SkeletonJoint>()
                : FlattenNodes(actualSkeleton.Children).OfType<SkeletonJoint>().ToList();
            var skeleton = actualSkeleton as Skeleton;
            if (skeleton != null && skeleton.NumJoints > 0 && skeleton.NumJoints < int.MaxValue)
                serializedJoints = serializedJoints.Take((int)skeleton.NumJoints).ToList();

            if (serializedJoints.Count > 0)
            {
                foreach (var joint in serializedJoints)
                {
                    if (joint == null) continue;
                    var position = joint.Position ?? new Nixson.Common.Vector3();
                    var axisX = joint.AxisX ?? new Nixson.Common.Vector3 { X = 1.0f };
                    var axisY = joint.AxisY ?? new Nixson.Common.Vector3 { Y = 1.0f };
                    var axisZ = joint.AxisZ ?? new Nixson.Common.Vector3 { Z = 1.0f };
                    var x = NormalizeBasis(new Vector3D(axisX.X, axisX.Y, axisX.Z), new Vector3D(1, 0, 0));
                    var y = NormalizeBasis(new Vector3D(axisY.X, axisY.Y, axisY.Z), new Vector3D(0, 1, 0));
                    var z = NormalizeBasis(new Vector3D(axisZ.X, axisZ.Y, axisZ.Z), new Vector3D(0, 0, 1));
                    result.Add(new JointData
                    {
                        Name = CleanNodeName(joint.Name),
                        ParentIndex = joint.ParentIndex,
                        LocalPosition = new Vector3D(position.X, position.Y, position.Z),
                        LocalAxisX = x,
                        LocalAxisY = y,
                        LocalAxisZ = z
                    });
                }
                NormalizeJointParents(result);
                return result;
            }

            // Совместимость с иными/старыми типами скелетов: достаём координаты через reflection.
            IEnumerable list = null;
            var prop = actualSkeleton.GetType().GetProperty("Joints") ?? actualSkeleton.GetType().GetProperty("SkeletonJoints");
            if (prop != null) list = prop.GetValue(actualSkeleton, null) as IEnumerable;
            if (list == null && actualSkeleton.Children != null) list = actualSkeleton.Children;
            if (list == null) return result;

            int idx = 0;
            foreach (var item in list)
            {
                if (item == null) continue;
                var itemType = item.GetType();
                if (item is BaseNode && !(item is SkeletonJoint) && !(itemType.Name.IndexOf("Joint", StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                int parentIdx = -1;
                var pParent = itemType.GetProperty("ParentIndex") ?? itemType.GetProperty("ParentID") ?? itemType.GetProperty("Parent");
                if (pParent != null) { try { parentIdx = Convert.ToInt32(pParent.GetValue(item, null)); } catch { } }
                Vector3D pos = new Vector3D(0, idx * 0.03, 0);
                var pPos = itemType.GetProperty("Position") ?? itemType.GetProperty("Translation") ?? itemType.GetProperty("RestPose");
                if (pPos != null)
                {
                    try
                    {
                        object v = pPos.GetValue(item, null);
                        if (v != null)
                        {
                            var px = v.GetType().GetProperty("X");
                            var py = v.GetType().GetProperty("Y");
                            var pz = v.GetType().GetProperty("Z");
                            if (px != null && py != null && pz != null)
                                pos = new Vector3D(Convert.ToDouble(px.GetValue(v, null)), Convert.ToDouble(py.GetValue(v, null)), Convert.ToDouble(pz.GetValue(v, null)));
                        }
                    }
                    catch { }
                }
                string name = null;
                var pName = itemType.GetProperty("Name");
                if (pName != null) { try { name = pName.GetValue(item, null) as string; } catch { } }
                result.Add(new JointData
                {
                    Name = CleanNodeName(name) ?? item.ToString(),
                    ParentIndex = parentIdx,
                    LocalPosition = pos,
                    LocalAxisX = new Vector3D(1, 0, 0),
                    LocalAxisY = new Vector3D(0, 1, 0),
                    LocalAxisZ = new Vector3D(0, 0, 1)
                });
                idx++;
            }
            NormalizeJointParents(result);
            return result;
        }

        private static void NormalizeJointParents(List<JointData> joints)
        {
            if (joints == null) return;
            for (int i = 0; i < joints.Count; i++)
            {
                int parent = joints[i].ParentIndex;
                if (parent < 0 || parent >= i) joints[i].ParentIndex = -1;
            }
        }

        private void RefreshAnimationDropdownForCurrentContext()
        {
            if (this.animationCombo == null || this._suppressAnimationSelection) return;
            Pure3DFile contextP3D = this._animationMeshSourceP3D ?? this._lastP3D;
            CompositeDrawable contextComposite = this._animationMeshSourceComposite;
            if (contextP3D == null) return;

            AnimationOption current = this.animationCombo.SelectedItem as AnimationOption;
            Animation preserve = current != null ? current.Node : null;
            if (contextComposite != null)
            {
                this.PopulateAnimationDropdown(contextP3D, contextComposite, true, preserve);
                if (preserve != null) this.SyncAnimationSelection(preserve);
            }
            else
            {
                this.PopulateAnimationDropdown(contextP3D, null, false, preserve);
                if (preserve != null) this.SyncAnimationSelection(preserve);
            }
        }

        private bool IsAnimationDropdownCatalogReusable(Pure3DFile p3d, string rigKey, bool hasCompositeContext,
            bool showIncompatible, bool sameRig)
        {
            return this._animationDropdownCatalogValid &&
                ReferenceEquals(this._animationDropdownCatalogP3D, p3d) &&
                this._animationDropdownCatalogSourceGeneration == this._animationSourceSetGeneration &&
                string.Equals(this._animationDropdownCatalogRigKey, rigKey, StringComparison.OrdinalIgnoreCase) &&
                this._animationDropdownCatalogCompositeContext == hasCompositeContext &&
                this._animationDropdownCatalogShowIncompatible == showIncompatible &&
                this._animationDropdownCatalogSameRig == sameRig;
        }

        private void RememberAnimationDropdownCatalog(Pure3DFile p3d, string rigKey, bool hasCompositeContext,
            bool showIncompatible, bool sameRig)
        {
            this._animationDropdownCatalogP3D = p3d;
            this._animationDropdownCatalogRigKey = rigKey;
            this._animationDropdownCatalogSourceGeneration = this._animationSourceSetGeneration;
            this._animationDropdownCatalogCompositeContext = hasCompositeContext;
            this._animationDropdownCatalogShowIncompatible = showIncompatible;
            this._animationDropdownCatalogSameRig = sameRig;
            this._animationDropdownCatalogValid = true;
        }

        private void PopulateAnimationDropdown(Pure3DFile p3d, CompositeDrawable contextComposite, bool requireMeshSupport = false, Animation preserveSelection = null)
        {
            if (!ReferenceEquals(this._animationDropdownContextComposite, contextComposite) &&
                this._materializedExternalAnimationOption != null)
            {
                this._materializedExternalAnimationOption.Node = this._materializedExternalAnimationOption.CatalogNode;
                this._materializedExternalAnimationOption = null;
            }
            this._animationDropdownContextComposite = contextComposite;
            this._animationDropdownMeshUnavailable = false;
            string preferredSkeletonName = contextComposite == null ? null : CleanNodeName(contextComposite.SkeletonName);
            string catalogRigKey = string.IsNullOrWhiteSpace(preferredSkeletonName) ? "<auto>" : preferredSkeletonName.Trim();
            bool hasCompositeContext = contextComposite != null;
            bool sameRig = !hasCompositeContext || this.AreAnimationSourceCompositesUsingSameRig();
            bool showIncompatible = this.showIncompatibleAnimationsCheckBox != null && this.showIncompatibleAnimationsCheckBox.Checked;

            // Reusing the same populated ComboBox is significantly faster than rechecking and
            // recreating thousands of AnimationOption rows when only the selected CompositeDrawable
            // changes inside the same P3D/RCF source set and rig.
            if (this.animationCombo.Items.Count > 0 &&
                this.IsAnimationDropdownCatalogReusable(p3d, catalogRigKey, hasCompositeContext, showIncompatible, sameRig))
            {
                this._suppressAnimationSelection = true;
                try
                {
                    if (!sameRig)
                    {
                        this.animationCombo.SelectedIndex = 0;
                        this.animationCombo.Enabled = false;
                    }
                    else
                    {
                        AnimationOption onlyOption = this.animationCombo.Items.Count == 1
                            ? this.animationCombo.Items[0] as AnimationOption : null;
                        bool isOnlyPlaceholder = onlyOption != null && onlyOption.IsPlaceholder;
                        int selectedIndex = -1;
                        if (preserveSelection != null)
                        {
                            for (int i = 0; i < this.animationCombo.Items.Count; i++)
                            {
                                AnimationOption option = this.animationCombo.Items[i] as AnimationOption;
                                if (option != null && ReferenceEquals(option.Node, preserveSelection))
                                {
                                    selectedIndex = i;
                                    break;
                                }
                            }
                        }
                        this.animationCombo.SelectedIndex = isOnlyPlaceholder ? 0 : selectedIndex;
                        this.animationCombo.Enabled = !isOnlyPlaceholder;
                    }
                }
                finally { this._suppressAnimationSelection = false; }
                this.UpdateAnimationComboToolTip();
                this.UpdateAnimationComboLayout();
                return;
            }

            this._animationDropdownCatalogValid = false;
            this._suppressAnimationSelection = true;
            try
            {
                this.animationCombo.Items.Clear();
                if (!sameRig)
                {
                    this.animationCombo.Items.Add(new AnimationOption
                    {
                        IsPlaceholder = true,
                        IsMultiCompositeRigMismatchPlaceholder = true,
                        LanguageCode = this._uiLanguage
                    });
                    this.animationCombo.SelectedIndex = 0;
                    this.animationCombo.Enabled = false;
                    this.RememberAnimationDropdownCatalog(p3d, catalogRigKey, hasCompositeContext, showIncompatible, sameRig);
                    this.UpdateAnimationComboToolTip();
                    return;
                }

                // For an RCF-backed CompositeDrawable, animations and their skeletons may live
                // in sibling P3D/P3D.RZ entries. For a standalone P3D or a non-composite node,
                // keep the historical scope limited to the active document.
                var sources = new List<KeyValuePair<Pure3DFile, string>>();
                var seenFiles = new HashSet<Pure3DFile>();
                if (contextComposite != null)
                {
                    if (p3d != null && seenFiles.Add(p3d))
                    {
                        string currentLabel = this._animationSourceFiles.FirstOrDefault(source => object.ReferenceEquals(source.Key, p3d)).Value;
                        sources.Add(new KeyValuePair<Pure3DFile, string>(p3d, currentLabel ?? string.Empty));
                    }
                    foreach (var source in this._animationSourceFiles)
                    {
                        if (source.Key == null || !seenFiles.Add(source.Key)) continue;
                        sources.Add(source);
                    }
                }
                else if (p3d != null)
                {
                    sources.Add(new KeyValuePair<Pure3DFile, string>(p3d, string.Empty));
                }

                var nodesBySource = new List<Tuple<Pure3DFile, string, AnimationSourceIndex>>();
                var sourcesToIndexInBackground = new List<Pure3DFile>();
                foreach (var source in sources)
                {
                    bool isActiveP3D = ReferenceEquals(source.Key, p3d);
                    AnimationSourceIndex index = source.Key == null ? null : this.GetAnimationSourceIndex(source.Key, isActiveP3D);
                    if (index == null && source.Key != null)
                        sourcesToIndexInBackground.Add(source.Key);
                    nodesBySource.Add(Tuple.Create(source.Key, source.Value ?? string.Empty, index));
                }
                this.ScheduleAnimationSourceIndexBuilds(sourcesToIndexInBackground);
                bool hasPendingSourceIndexes = nodesBySource.Any(source => source.Item1 != null && source.Item3 == null &&
                    this._animationSourceIndexBuildPending.Contains(source.Item1));

                // Source indexes retain only animation/skeleton references plus compact lookup
                // tables. Do not keep a second flattened list of every node in each source.
                var skeletons = new List<Skeleton>();
                var seenSkeletons = new HashSet<Skeleton>();
                foreach (var source in nodesBySource)
                    if (source.Item3 != null)
                        foreach (Skeleton skeleton in source.Item3.Skeletons)
                            if (skeleton != null && seenSkeletons.Add(skeleton)) skeletons.Add(skeleton);

                var options = new List<AnimationOption>();

                foreach (var source in nodesBySource)
                {
                    IEnumerable<Animation> sourceAnimations = source.Item3 != null ? source.Item3.Animations : Enumerable.Empty<Animation>();
                    foreach (var animation in sourceAnimations)
                    {
                        List<AnimationBone> cachedTrackBones;
                        if (!source.Item3.TrackBones.TryGetValue(animation, out cachedTrackBones))
                            cachedTrackBones = GetAnimationTrackBones(animation);
                        Skeleton compatibleSkeleton = this.GetCachedAnimationSkeletonMatch(source.Item3, animation, skeletons, preferredSkeletonName, false);
                        if (compatibleSkeleton == null && contextComposite != null && string.IsNullOrWhiteSpace(preferredSkeletonName))
                        {
                            Skeleton bestCompositeCandidate = this.GetCachedAnimationSkeletonMatch(source.Item3, animation, skeletons, null, true);
                            if (bestCompositeCandidate != null &&
                                ScoreAnimationTracksForSkeleton(cachedTrackBones, this.ExtractJoints(bestCompositeCandidate)) > 0)
                                compatibleSkeleton = bestCompositeCandidate;
                        }
                        bool isCompatible = compatibleSkeleton != null;
                        Skeleton candidateSkeleton = compatibleSkeleton;
                        if (candidateSkeleton == null && showIncompatible)
                            candidateSkeleton = this.GetCachedAnimationSkeletonMatch(source.Item3, animation, skeletons, preferredSkeletonName, true);

                        // Do not hide otherwise valid skeletal clips just because a particular mesh
                        // lacks weights. Skeleton playback remains useful and was supported before skinning.
                        if (!isCompatible && !showIncompatible) continue;

                        string sourceLabel = string.Empty;
                        if (contextComposite != null && !object.ReferenceEquals(source.Item1, p3d))
                            sourceLabel = source.Item2;
                        options.Add(new AnimationOption
                        {
                            Node = animation,
                            CatalogNode = this._animationNodeLoaders.ContainsKey(animation) ? animation : null,
                            CompatibleSkeleton = candidateSkeleton,
                            SourceP3D = source.Item1,
                            SourceLabel = sourceLabel,
                            IsCompatible = isCompatible,
                            LanguageCode = this._uiLanguage
                        });
                    }
                }

                if (options.Count > 0)
                    this.animationCombo.Items.AddRange(options.Cast<object>().ToArray());

                if (this.animationCombo.Items.Count == 0)
                {
                    this.animationCombo.Items.Add(new AnimationOption
                    {
                        IsPlaceholder = true,
                        IsSearchingPlaceholder = hasPendingSourceIndexes,
                        IsMeshUnavailablePlaceholder = false,
                        LanguageCode = this._uiLanguage
                    });
                    this.animationCombo.SelectedIndex = 0;
                    this.animationCombo.Enabled = false;
                    this.animationCombo.ToolTipText = string.Empty;
                    this.UpdateAnimationComboToolTip();
                }
                else
                {
                    int selectedIndex = -1;
                    if (preserveSelection != null)
                    {
                        for (int i = 0; i < this.animationCombo.Items.Count; i++)
                        {
                            AnimationOption option = this.animationCombo.Items[i] as AnimationOption;
                            if (option != null && object.ReferenceEquals(option.Node, preserveSelection))
                            {
                                selectedIndex = i;
                                break;
                            }
                        }
                    }
                    this.animationCombo.SelectedIndex = selectedIndex;
                    this.animationCombo.Enabled = true;
                }
                this.RememberAnimationDropdownCatalog(p3d, catalogRigKey, hasCompositeContext, showIncompatible, sameRig);
            }
            finally { this._suppressAnimationSelection = false; }
            this.UpdateAnimationComboToolTip();
            this.UpdateAnimationComboLayout();
        }

        private void SyncAnimationSelection(BaseNode node)
        {
            this._suppressAnimationSelection = true;
            try
            {
                int selected = -1;
                var animation = node as Animation;
                if (animation != null)
                {
                    for (int i = 0; i < this.animationCombo.Items.Count; i++)
                    {
                        var option = this.animationCombo.Items[i] as AnimationOption;
                        if (option != null && object.ReferenceEquals(option.Node, animation)) { selected = i; break; }
                    }
                }
                if (selected < 0 && this.animationCombo.Items.Count == 1 &&
                    this.animationCombo.Items[0] is AnimationOption &&
                    ((AnimationOption)this.animationCombo.Items[0]).IsPlaceholder)
                    selected = 0;
                this.animationCombo.SelectedIndex = selected;
            }
            finally { this._suppressAnimationSelection = false; }
            this.UpdateAnimationComboToolTip();
        }

        private Skeleton FindSkeletonForAnimation(Animation animation, Pure3DFile p3d, string preferredSkeletonName)
        {
            if (animation == null || p3d == null || p3d.Nodes == null) return null;
            var skeletons = FlattenNodes(p3d.Nodes).OfType<Skeleton>().ToList();
            return this.FindSkeletonForAnimation(animation, skeletons, preferredSkeletonName);
        }

        private Skeleton FindSkeletonForAnimation(Animation animation, IList<Skeleton> skeletons, string preferredSkeletonName)
        {
            return this.FindSkeletonForAnimation(GetAnimationTrackBones(animation), skeletons, preferredSkeletonName);
        }

        private Skeleton FindSkeletonForAnimation(IList<AnimationBone> trackBones, IList<Skeleton> skeletons, string preferredSkeletonName)
        {
            if (trackBones == null || skeletons == null || skeletons.Count == 0 || trackBones.Count == 0) return null;

            // A CompositeDrawable's SkeletonName defines the rig for its mesh. Never silently
            // substitute another rig if the declared one is present but the clip cannot drive it.
            if (!string.IsNullOrWhiteSpace(preferredSkeletonName))
            {
                var preferred = skeletons.FirstOrDefault(candidate => SkeletonNamesMatch(candidate.Name, preferredSkeletonName));
                if (preferred == null) return null;
                var joints = this.ExtractJoints(preferred);
                return joints.Count > 0 && HasAnimationTracksForSkeleton(trackBones, joints) ? preferred : null;
            }

            Skeleton best = null;
            int bestScore = 0;
            int bestCandidates = 0;
            foreach (var candidate in skeletons)
            {
                var joints = this.ExtractJoints(candidate);
                if (joints.Count == 0) continue;
                int score = ScoreAnimationTracksForSkeleton(trackBones, joints);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                    bestCandidates = 1;
                }
                else if (score > 0 && score == bestScore)
                {
                    bestCandidates++;
                }
            }
            if (best != null && bestScore > 0 && bestCandidates == 1) return best;
            return null;
        }

        private Skeleton FindSkeletonCandidateForAnimation(Animation animation, IList<Skeleton> skeletons, string preferredSkeletonName)
        {
            return this.FindSkeletonCandidateForAnimation(GetAnimationTrackBones(animation), skeletons, preferredSkeletonName);
        }

        private Skeleton FindSkeletonCandidateForAnimation(IList<AnimationBone> trackBones, IList<Skeleton> skeletons, string preferredSkeletonName)
        {
            if (trackBones == null || skeletons == null || skeletons.Count == 0) return null;
            if (!string.IsNullOrWhiteSpace(preferredSkeletonName))
            {
                Skeleton preferred = skeletons.FirstOrDefault(candidate => SkeletonNamesMatch(candidate.Name, preferredSkeletonName));
                if (preferred != null && this.ExtractJoints(preferred).Count > 0) return preferred;
            }

            if (trackBones.Count == 0) return null;
            Skeleton best = null;
            int bestScore = 0;
            foreach (Skeleton candidate in skeletons)
            {
                var joints = this.ExtractJoints(candidate);
                if (joints.Count == 0) continue;
                int score = ScoreAnimationTracksForSkeleton(trackBones, joints);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        private AnimationSourceIndex GetAnimationSourceIndex(Pure3DFile source, bool allowSynchronousBuild)
        {
            if (source == null) return null;
            AnimationSourceIndex index;
            if (this._animationSourceIndexCache.TryGetValue(source, out index)) return index;
            if (this._animationSourceIndexBuildPending.Contains(source) || this._animationSourceIndexBuildFailed.Contains(source))
                return null;
            if (!allowSynchronousBuild) return null;

            try
            {
                index = BuildAnimationSourceIndex(source);
                this.RegisterAnimationSourceIndex(index);
                return index;
            }
            catch
            {
                this._animationSourceIndexBuildFailed.Add(source);
                return null;
            }
        }

        public static PreparedAnimationSourceIndex PrepareAnimationSourceIndexForCatalog(Pure3DFile source)
        {
            if (source == null) return null;
            return new PreparedAnimationSourceIndex(BuildAnimationSourceIndex(source));
        }

        /// <summary>Registers indexes prepared by the RCF-open worker after source references are installed.</summary>
        public void RegisterPreparedAnimationSourceIndexes(IEnumerable<PreparedAnimationSourceIndex> preparedIndexes)
        {
            if (preparedIndexes == null) return;
            bool registered = false;
            foreach (PreparedAnimationSourceIndex prepared in preparedIndexes)
            {
                AnimationSourceIndex preparedIndex = prepared == null ? null : prepared.IndexObject as AnimationSourceIndex;
                if (preparedIndex == null || preparedIndex.Source == null) continue;
                Pure3DFile source = preparedIndex.Source;
                // The complete RCF catalog may be prepared before a CompositeDrawable is selected.
                // Keep those indexes cached; SetAnimationSourceFiles will retain them when the
                // catalog source set is installed later.
                preparedIndex.MatchingResults.Clear();
                this.RegisterAnimationSourceIndex(preparedIndex);
                this._preparedRcfAnimationSourceIndexSources.Add(source);
                this._animationSourceIndexBuildPending.Remove(source);
                this._animationSourceIndexBuildFailed.Remove(source);
                registered = true;
            }
            if (registered)
                this._animationDropdownCatalogValid = false;
        }

        private static AnimationSourceIndex BuildAnimationSourceIndex(Pure3DFile source)
        {
            var index = new AnimationSourceIndex { Source = source };
            IEnumerable<BaseNode> nodes = source == null || source.Nodes == null
                ? Enumerable.Empty<BaseNode>()
                : FlattenNodes(source.Nodes);
            index.Animations = nodes.OfType<Animation>().Where(animation => animation != null).ToList();
            index.Skeletons = nodes.OfType<Skeleton>().Where(skeleton => skeleton != null).ToList();
            foreach (Animation animation in index.Animations)
            {
                try { index.TrackBones[animation] = GetAnimationTrackBones(animation); }
                catch { index.TrackBones[animation] = new List<AnimationBone>(); }
            }
            foreach (Skeleton skeleton in index.Skeletons)
            {
                try { index.Joints[skeleton] = ExtractJointsUncached(skeleton); }
                catch { index.Joints[skeleton] = new List<JointData>(); }
            }
            return index;
        }

        private void RegisterAnimationSourceIndex(AnimationSourceIndex index)
        {
            if (index == null || index.Source == null) return;
            this._animationSourceIndexCache[index.Source] = index;
            foreach (Skeleton skeleton in index.Skeletons)
                this._skeletonSourceIndexes[skeleton] = index;
        }

        private bool IsAnimationSourceCurrentlyAvailable(Pure3DFile source)
        {
            return source != null &&
                (ReferenceEquals(this._animationMeshSourceP3D, source) ||
                 this._animationSourceFiles.Any(item => ReferenceEquals(item.Key, source)));
        }

        private void ScheduleAnimationSourceIndexBuilds(IEnumerable<Pure3DFile> sources)
        {
            var requested = new List<Pure3DFile>();
            if (sources != null)
            {
                foreach (Pure3DFile source in sources)
                {
                    if (source == null || this._animationSourceIndexCache.ContainsKey(source) ||
                        this._animationSourceIndexBuildPending.Contains(source) ||
                        this._animationSourceIndexBuildFailed.Contains(source) || requested.Contains(source))
                        continue;
                    this._animationSourceIndexBuildPending.Add(source);
                    requested.Add(source);
                }
            }
            if (requested.Count == 0) return;

            Task.Run(() =>
            {
                var results = new List<Tuple<Pure3DFile, AnimationSourceIndex, string>>(requested.Count);
                foreach (Pure3DFile source in requested)
                {
                    try { results.Add(Tuple.Create(source, BuildAnimationSourceIndex(source), (string)null)); }
                    catch (Exception ex) { results.Add(Tuple.Create(source, (AnimationSourceIndex)null, ex.Message)); }
                }
                return results;
            }).ContinueWith(task =>
            {
                if (this.IsDisposed || !this.IsHandleCreated) return;
                try
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        if (this.IsDisposed) return;
                        bool shouldRefresh = false;
                        foreach (Pure3DFile source in requested)
                        {
                            this._animationSourceIndexBuildPending.Remove(source);
                            if (this.IsAnimationSourceCurrentlyAvailable(source)) shouldRefresh = true;
                        }

                        if (task.IsFaulted || task.IsCanceled)
                        {
                            foreach (Pure3DFile source in requested)
                                if (this.IsAnimationSourceCurrentlyAvailable(source)) this._animationSourceIndexBuildFailed.Add(source);
                        }
                        else
                        {
                            foreach (var result in task.Result)
                            {
                                if (!this.IsAnimationSourceCurrentlyAvailable(result.Item1)) continue;
                                if (result.Item2 == null)
                                    this._animationSourceIndexBuildFailed.Add(result.Item1);
                                else if (!this._animationSourceIndexCache.ContainsKey(result.Item1))
                                    this.RegisterAnimationSourceIndex(result.Item2);
                            }
                        }

                        if (shouldRefresh && this._animationMeshSourceComposite != null && this._animationMeshSourceP3D != null)
                        {
                            this._animationDropdownCatalogValid = false;
                            this.RefreshAnimationDropdownForCurrentContext();
                        }
                    }));
                }
                catch (InvalidOperationException)
                {
                    // The viewer can be disposing while the worker finishes. No UI update is needed then.
                }
            }, TaskScheduler.Default);
        }

        private Skeleton GetCachedAnimationSkeletonMatch(AnimationSourceIndex sourceIndex, Animation animation,
            IList<Skeleton> skeletons, string preferredSkeletonName, bool allowBestCandidate)
        {
            if (sourceIndex == null || animation == null) return null;
            string cleanRigName = CleanNodeName(preferredSkeletonName);
            string rigKey = string.IsNullOrWhiteSpace(cleanRigName)
                ? "<auto>"
                : cleanRigName.ToUpperInvariant();
            string queryKey = this._animationSourceSetGeneration.ToString(CultureInfo.InvariantCulture) + "|" +
                (allowBestCandidate ? "candidate|" : "strict|") + rigKey;
            Dictionary<Animation, Skeleton> results;
            if (!sourceIndex.MatchingResults.TryGetValue(queryKey, out results))
            {
                // Keep only a small number of rig/query caches per source. Each cache can contain
                // one entry per animation in a large RCF; an unbounded rig-key dictionary caused
                // memory to grow as the user switched between many CompositeDrawable variants.
                const int MaxCachedMatchQueriesPerSource = 2;
                if (sourceIndex.MatchingResults.Count >= MaxCachedMatchQueriesPerSource)
                    sourceIndex.MatchingResults.Clear();
                results = new Dictionary<Animation, Skeleton>();
                sourceIndex.MatchingResults[queryKey] = results;
            }
            Skeleton cached;
            if (results.TryGetValue(animation, out cached)) return cached;

            List<AnimationBone> tracks;
            if (!sourceIndex.TrackBones.TryGetValue(animation, out tracks))
                tracks = GetAnimationTrackBones(animation);
            Skeleton resolved = allowBestCandidate
                ? this.FindSkeletonCandidateForAnimation(tracks, skeletons, preferredSkeletonName)
                : this.FindSkeletonForAnimation(tracks, skeletons, preferredSkeletonName);
            results[animation] = resolved;
            return resolved;
        }

        private static List<AnimationBone> GetAnimationTrackBones(Animation animation)
        {
            if (animation == null) return new List<AnimationBone>();
            return FlattenNodes(new[] { animation }).OfType<AnimationBone>()
                .Where(HasRecognizedAnimationTrack).ToList();
        }

        private static bool HasRecognizedAnimationTrack(AnimationBone bone)
        {
            if (bone == null || bone.Children == null) return false;
            var subtree = FlattenNodes(bone.Children).ToList();
            if (subtree.OfType<AnimationChannel>().Any(channel =>
                IsTranslationType(channel, "ROT") || IsTranslationType(channel, "TRAN")))
                return true;
            if (subtree.OfType<BoneRotationData>().Any(data => data.Frames != null && data.Values != null && data.Frames.Length > 0 && data.Values.Length > 0))
                return true;
            if (subtree.OfType<BoneTranslationData>().Any(data => data.Frames != null && data.Values != null && data.Frames.Length > 0 && data.Values.Length > 0))
                return true;
            return false;
        }

        private static bool HasAnimationTracksForSkeleton(IList<AnimationBone> trackBones, IList<JointData> joints)
        {
            return trackBones != null && joints != null && ScoreAnimationTracksForSkeleton(trackBones, joints) > 0;
        }

        private static int ScoreAnimationTracksForSkeleton(IList<AnimationBone> trackBones, IList<JointData> joints)
        {
            if (trackBones == null || joints == null || joints.Count == 0 || trackBones.Count == 0) return 0;
            int score = 0;
            int matchedTracks = 0;
            var exactJointNameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (JointData joint in joints)
            {
                string jointName = CleanNodeName(joint == null ? null : joint.Name);
                if (string.IsNullOrWhiteSpace(jointName)) continue;
                int count;
                exactJointNameCounts.TryGetValue(jointName, out count);
                exactJointNameCounts[jointName] = count + 1;
            }
            foreach (var bone in trackBones)
            {
                if (bone == null) continue;
                string boneName = CleanNodeName(bone.Name);
                if (string.IsNullOrWhiteSpace(boneName))
                {
                    // BoneID fallback is valid only for nameless tracks and must stay in range.
                    if (bone.BoneID < (uint)joints.Count)
                    {
                        score += 1;
                        matchedTracks++;
                    }
                    continue;
                }

                int exactMatches;
                if (exactJointNameCounts.TryGetValue(boneName, out exactMatches) && exactMatches > 0)
                {
                    score += 100;
                    matchedTracks++;
                    continue;
                }
                int suffixMatches = joints.Count(joint => BoneNamesMatch(boneName, joint.Name));
                if (suffixMatches == 1)
                {
                    score += 50;
                    matchedTracks++;
                    continue;
                }

                // Real game clips can contain auxiliary tracks from another rig or aliases that
                // are not present in this skeleton. Ignore those tracks; ConfigureAnimationPreview
                // already skips unmatched bones, while the matching tracks can still drive the rig.
                continue;
            }
            return matchedTracks > 0 ? score : 0;
        }

        private static bool SkeletonNamesMatch(string left, string right)
        {
            string a = NormalizeSkeletonName(left);
            string b = NormalizeSkeletonName(right);
            return !string.IsNullOrEmpty(a) && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeSkeletonName(string value)
        {
            value = CleanNodeName(value);
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Replace('\\', '/');
            int slash = value.LastIndexOf('/');
            if (slash >= 0 && slash + 1 < value.Length) value = value.Substring(slash + 1);
            if (value.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase))
                value = value.Substring(0, value.Length - 4);
            return value.Trim();
        }

        private void ConfigureAnimationPreview(Animation animation)
        {
            if (animation == null || this._activeSkeleton == null) return;
            var dataResult = AnimationPreviewDataReader.Prepare(animation);
            var jointLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < this._activeSkeleton.Joints.Count; i++)
            {
                string name = CleanNodeName(this._activeSkeleton.Joints[i].Name);
                if (!string.IsNullOrEmpty(name) && !jointLookup.ContainsKey(name)) jointLookup.Add(name, i);
            }

            this._activeAnimationTracks.Clear();
            foreach (var bone in FlattenNodes(new[] { animation }).OfType<AnimationBone>())
            {
                int jointIndex = -1;
                string boneName = CleanNodeName(bone.Name);
                int namedIndex;
                if (!string.IsNullOrEmpty(boneName) && jointLookup.TryGetValue(boneName, out namedIndex)) jointIndex = namedIndex;
                if (jointIndex < 0 && !string.IsNullOrEmpty(boneName))
                {
                    var suffixMatches = Enumerable.Range(0, this._activeSkeleton.Joints.Count)
                        .Where(i => BoneNamesMatch(boneName, this._activeSkeleton.Joints[i].Name))
                        .ToList();
                    if (suffixMatches.Count == 1) jointIndex = suffixMatches[0];
                }
                // BoneID can be a channel/group ID instead of a skeleton index. Only use it
                // positionally for unnamed tracks; a named but unmatched bone indicates a rig mismatch.
                if (jointIndex < 0 && string.IsNullOrEmpty(boneName) && bone.BoneID < (uint)this._activeSkeleton.Joints.Count)
                    jointIndex = (int)bone.BoneID;

                // Most files store channel nodes directly below the bone; some variants
                // wrap data in intermediate nodes. Search recursively but keep the scope
                // within this bone so tracks cannot leak between neighbouring bones.
                var boneSubtree = FlattenNodes(bone.Children).ToList();
                var channels = boneSubtree.OfType<AnimationChannel>().ToList();
                var rotation = channels.Where(c => IsTranslationType(c, "ROT") && c.Frames != null && c.Frames.Count > 0)
                    .OrderByDescending(c => c is Quaternion6CompressedChannel ? 6 : c is Quaternion3CompressedChannel ? 3 : 1)
                    .FirstOrDefault();
                var translation = channels.Where(c => IsTranslationType(c, "TRAN") && c.Frames != null && c.Frames.Count > 0)
                    .OrderByDescending(c => c is Vector3DOFChannel || c is Vector3DOFCompressedChannel ? 3 : c is Vector2DOFChannel || c is Vector2DOFCompressedChannel ? 2 : 1)
                    .FirstOrDefault();
                var legacyRotation = boneSubtree.OfType<BoneRotationData>()
                    .FirstOrDefault(d => d.Frames != null && d.Values != null && d.Frames.Length > 0 && d.Values.Length > 0);
                var legacyTranslation = boneSubtree.OfType<BoneTranslationData>()
                    .FirstOrDefault(d => d.Frames != null && d.Values != null && d.Frames.Length > 0 && d.Values.Length > 0);
                if (rotation == null && translation == null && legacyRotation == null && legacyTranslation == null) continue;
                // Keep named tracks even when the active (gameplay) skeleton lacks that joint;
                // a cutscene mesh can have a more detailed source rig with that exact joint.
                if ((jointIndex < 0 || jointIndex >= this._activeSkeleton.Joints.Count) && string.IsNullOrWhiteSpace(boneName)) continue;
                var track = new AnimationBoneTrack
                {
                    JointIndex = jointIndex,
                    BoneName = boneName,
                    RotationChannel = rotation,
                    TranslationChannel = translation,
                    LegacyRotationData = legacyRotation,
                    LegacyTranslationData = legacyTranslation
                };
                if (translation != null)
                {
                    ushort[] translationKeys = translation.OrderedFrameKeys;
                    if (translationKeys.Length > 0)
                        track.FirstTranslation = translation.Frames[translationKeys[0]];
                }
                else if (legacyTranslation != null && legacyTranslation.Values.Length > 0)
                {
                    var first = legacyTranslation.Values[0];
                    track.FirstTranslation = new Vector4 { X = first.X, Y = first.Y, Z = first.Z };
                }
                this._activeAnimationTracks.Add(track);
            }

            this._activeAnimation = animation;
            this._animationFrame = 0.0f;
            // Loop is a viewer preference; switching clips must not overwrite the user's checkbox state.
            // Some P3D clips have an unreliable/zero Animation.NumFrames field while the
            // actual channel keys span multiple frames. Infer the minimum playable length
            // from the decoded tracks, so playback does not stop on frame zero.
            double safeNumFrames = animation.NumFrames;
            if (double.IsNaN(safeNumFrames) || double.IsInfinity(safeNumFrames) || safeNumFrames < 1.0) safeNumFrames = 1.0;
            double inferredFrameCount = 1.0;
            foreach (var track in this._activeAnimationTracks)
            {
                if (track.RotationChannel != null)
                {
                    ushort[] keys = track.RotationChannel.OrderedFrameKeys;
                    if (keys.Length > 0) inferredFrameCount = Math.Max(inferredFrameCount, (double)keys[keys.Length - 1] + 1.0);
                }
                if (track.TranslationChannel != null)
                {
                    ushort[] keys = track.TranslationChannel.OrderedFrameKeys;
                    if (keys.Length > 0) inferredFrameCount = Math.Max(inferredFrameCount, (double)keys[keys.Length - 1] + 1.0);
                }
                if (track.LegacyRotationData != null && track.LegacyRotationData.Frames != null && track.LegacyRotationData.Frames.Length > 0)
                    inferredFrameCount = Math.Max(inferredFrameCount, (double)track.LegacyRotationData.Frames.Max() + 1.0);
                if (track.LegacyTranslationData != null && track.LegacyTranslationData.Frames != null && track.LegacyTranslationData.Frames.Length > 0)
                    inferredFrameCount = Math.Max(inferredFrameCount, (double)track.LegacyTranslationData.Frames.Max() + 1.0);
            }
            safeNumFrames = Math.Max(safeNumFrames, inferredFrameCount);
            this._activeAnimationFrameCount = (float)Math.Min(100000.0, safeNumFrames);
            int frames = Math.Max(1, (int)Math.Min(100000.0, Math.Ceiling(this._activeAnimationFrameCount)));
            this._suppressAnimationFrameChange = true;
            try
            {
                this.animationTrackBar.Minimum = 0;
                this.animationTrackBar.Maximum = Math.Max(1, frames - 1);
                this.animationTrackBar.Value = 0;
                this.animationTrackBar.Enabled = frames > 1 && this._activeAnimationTracks.Count > 0;
            }
            finally { this._suppressAnimationFrameChange = false; }
            this.SetAnimationControlsEnabled(this._activeAnimationTracks.Count > 0);
            this.UpdateSkeletonPose(0.0f);
            this.UpdateAnimationFrameLabel();

            string animName = CleanNodeName(animation.Name);
            if (string.IsNullOrEmpty(animName)) animName = CleanNodeName(animation.AnimationType);
            if (string.IsNullOrEmpty(animName)) animName = this.L("viewer.animation.unnamed", "без имени");
            int expectedChannels = FlattenNodes(new[] { animation }).OfType<AnimationChannel>().Count()
                + FlattenNodes(new[] { animation }).OfType<BoneRotationData>().Count()
                + FlattenNodes(new[] { animation }).OfType<BoneTranslationData>().Count();
            this.SetStatsText(() =>
            {
                string warning = dataResult.Warnings.Count > 0
                    ? this.LF("viewer.stats.animationWarningCountSuffix", " | предупреждений: {0}", dataResult.Warnings.Count)
                    : string.Empty;
                if (this._activeAnimationTracks.Count == 0)
                {
                    warning += this.L("viewer.stats.animationNoChannels", " | нет распознанных кадровых каналов: проверьте тип анимации/структуру скелета");
                    if (dataResult.Warnings.Count > 0)
                        warning += " | " + dataResult.Warnings[0];
                    else if (expectedChannels > 0 && dataResult.ChannelsWithFrames == 0)
                        warning += this.L("viewer.stats.animationNoDecodedFrames", " | каналы не содержат декодированных кадров");
                    else if (expectedChannels == 0)
                        warning += this.L("viewer.stats.animationNoChannelsInNodes", " | в узлах анимации нет AnimationChannel/BoneROT/BoneTRAN");
                }
                return this.LF("viewer.stats.animation", "Анимация: {0} | костей с каналами: {1} | кадры: {2} | каналы: {3}/{4}{5}",
                    animName, this._activeAnimationTracks.Count, frames, dataResult.ChannelsWithFrames, expectedChannels, warning);
            });
            this.UpdateAnimationComboToolTip();
        }

        private void SetAnimationPreviewFailure(Func<string> messageFactory)
        {
            // Avoid clearing the existing model when this animation cannot be matched to a rig.
            this.StopAnimationPlayback();
            this._activeAnimation = null;
            this._activeAnimationTracks.Clear();
            this._activeSkeleton = null;
            this._animationMeshMode = false;
            this._animationMeshModeAvailable = false;
            this.SetAnimationControlsEnabled(false);
            this.UpdateAnimationDisplayModeButton();
            this.UpdateAnimationFrameLabel();
            this.SetStatsText(messageFactory ?? (() => this.L("viewer.stats.animationPreviewFailed", "Не удалось загрузить предпросмотр анимации.")));
            this.UpdateAnimationComboToolTip();
        }

        private void SetAnimationControlsEnabled(bool enabled)
        {
            if (this.playAnimationButton != null) this.playAnimationButton.Enabled = enabled;
            if (this.loopAnimationButton != null) this.loopAnimationButton.Enabled = enabled;
            if (this.animationTrackBar != null && !enabled) this.animationTrackBar.Enabled = false;
            if (this.animationFrameLabel != null && !enabled) this.animationFrameLabel.Text = this.L("viewer.animation.framePlaceholder", "кадр —");
        }

        private void ToggleAnimationPlayback()
        {
            if (this._animationPlaying)
            {
                this.StopAnimationPlayback();
                return;
            }
            if (this._activeAnimation == null || this._activeAnimationTracks.Count == 0) return;
            this._animationPlaying = true;
            this._lastAnimationTickUtc = DateTime.UtcNow;
            this.playAnimationButton.Text = "Ⅱ";
            this._animationTimer.Start();
        }

        private void StopAnimationPlayback()
        {
            this._animationPlaying = false;
            if (this._animationTimer != null) this._animationTimer.Stop();
            if (this.playAnimationButton != null) this.playAnimationButton.Text = "▶";
        }

        private void OnAnimationTimerTick(object sender, EventArgs e)
        {
            if (!this._animationPlaying || this._activeAnimation == null) { this.StopAnimationPlayback(); return; }
            DateTime now = DateTime.UtcNow;
            double elapsed = (now - this._lastAnimationTickUtc).TotalSeconds;
            this._lastAnimationTickUtc = now;
            elapsed = Math.Max(0.0, Math.Min(0.2, elapsed));
            double rate = this._activeAnimation.FrameRate > 0.001f ? this._activeAnimation.FrameRate : 30.0;
            float frameCount = this._activeAnimationFrameCount;
            if (float.IsNaN(frameCount) || float.IsInfinity(frameCount) || frameCount < 1.0f) frameCount = 1.0f;
            this._animationFrame += (float)(elapsed * rate);
            if (this._animationFrame >= frameCount)
            {
                if (this.loopAnimationButton.Checked)
                    this._animationFrame %= frameCount;
                else
                {
                    this._animationFrame = Math.Max(0.0f, frameCount - 1.0f);
                    this.StopAnimationPlayback();
                }
            }
            int slider = Math.Max(this.animationTrackBar.Minimum, Math.Min(this.animationTrackBar.Maximum, (int)Math.Round(this._animationFrame)));
            this._suppressAnimationFrameChange = true;
            try { this.animationTrackBar.Value = slider; }
            finally { this._suppressAnimationFrameChange = false; }
            this.UpdateSkeletonPose(this._animationFrame);
            this.UpdateAnimationFrameLabel();
        }

        private void UpdateAnimationFrameLabel()
        {
            if (this.animationFrameLabel == null) return;
            if (this._activeAnimation == null) { this.animationFrameLabel.Text = this.L("viewer.animation.framePlaceholder", "кадр —"); return; }
            double frameCount = this._activeAnimationFrameCount;
            if (double.IsNaN(frameCount) || double.IsInfinity(frameCount) || frameCount < 1.0) frameCount = 1.0;
            int total = Math.Max(1, (int)Math.Min(100000.0, Math.Ceiling(frameCount)));
            this.animationFrameLabel.Text = this.LF("viewer.animation.frameValue", "кадр {0}/{1}", Math.Max(0, (int)Math.Floor(this._animationFrame)), total - 1);
        }

        private void UpdateSkeletonPose(float frame)
        {
            var state = this._activeSkeleton;
            if (state == null || state.Joints == null || state.Joints.Count == 0) return;
            this.EvaluateSkeletonPoseState(state, frame, this._activeAnimationTracks, false, true);
            this.UpdateSkinnedMeshes(frame);
        }

        private static int FindTrackJointIndexByName(AnimationBoneTrack track, IList<JointData> joints)
        {
            if (track == null || joints == null || joints.Count == 0) return -1;
            string boneName = CleanNodeName(track.BoneName);
            if (string.IsNullOrWhiteSpace(boneName)) return -1;

            var exact = Enumerable.Range(0, joints.Count)
                .Where(i => string.Equals(boneName, CleanNodeName(joints[i] == null ? null : joints[i].Name), StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (exact.Count == 1) return exact[0];
            if (exact.Count > 1) return -1;

            var suffix = Enumerable.Range(0, joints.Count)
                .Where(i => BoneNamesMatch(boneName, joints[i] == null ? null : joints[i].Name))
                .ToList();
            return suffix.Count == 1 ? suffix[0] : -1;
        }

        private static bool TryRetargetFacialControlRotation(SkeletonPreviewState state,
            IList<AnimationBoneTrack> tracks, AnimationBoneTrack track, int controlIndex, double[] quaternion,
            Vector3D[] localX, Vector3D[] localY, Vector3D[] localZ)
        {
            if (state == null || state.Joints == null || tracks == null || track == null ||
                controlIndex < 0 || controlIndex >= state.Joints.Count)
                return false;

            string controlName = CleanNodeName(track.BoneName);
            string[] targetNames;
            if (string.Equals(controlName, "EyeLid", StringComparison.OrdinalIgnoreCase))
                targetNames = new[] { "Eyelid_R", "Eyelid_L" };
            else if (string.Equals(controlName, "Brow", StringComparison.OrdinalIgnoreCase))
                targetNames = new[] { "Brow_01_R", "Brow_02_R", "Brow_01_L", "Brow_02_L" };
            else
                return false;

            var targetNameSet = new HashSet<string>(targetNames, StringComparer.OrdinalIgnoreCase);
            var targets = Enumerable.Range(0, state.Joints.Count)
                .Where(i => i != controlIndex && state.Joints[i] != null &&
                    state.Joints[i].ParentIndex == controlIndex &&
                    targetNameSet.Contains(CleanNodeName(state.Joints[i].Name)))
                .ToList();
            if (targets.Count == 0)
                return false;

            JointData control = state.Joints[controlIndex];
            Vector3D bindX = control.LocalAxisX;
            Vector3D bindY = control.LocalAxisY;
            Vector3D bindZ = control.LocalAxisZ;
            Vector3D animatedX = RotateByQuaternion(quaternion, new Vector3D(1, 0, 0));
            Vector3D animatedY = RotateByQuaternion(quaternion, new Vector3D(0, 1, 0));
            Vector3D animatedZ = RotateByQuaternion(quaternion, new Vector3D(0, 0, 1));

            // D = inverse(controlBind) * controlAnimated. Convert that delta into each
            // deform joint's own bind basis: childCurrent = childBind * D. The projection
            // uses the orthonormal bind axes already normalized by ExtractJoints().
            Vector3D deltaX = new Vector3D(
                Vector3D.DotProduct(animatedX, bindX),
                Vector3D.DotProduct(animatedX, bindY),
                Vector3D.DotProduct(animatedX, bindZ));
            Vector3D deltaY = new Vector3D(
                Vector3D.DotProduct(animatedY, bindX),
                Vector3D.DotProduct(animatedY, bindY),
                Vector3D.DotProduct(animatedY, bindZ));
            Vector3D deltaZ = new Vector3D(
                Vector3D.DotProduct(animatedZ, bindX),
                Vector3D.DotProduct(animatedZ, bindY),
                Vector3D.DotProduct(animatedZ, bindZ));

            foreach (int targetIndex in targets)
            {
                JointData target = state.Joints[targetIndex];
                string targetName = CleanNodeName(target.Name);
                bool hasAuthoredRotation = tracks.Any(candidate => candidate != null &&
                    string.Equals(targetName, CleanNodeName(candidate.BoneName), StringComparison.OrdinalIgnoreCase) &&
                    (candidate.RotationChannel != null || candidate.LegacyRotationData != null));
                if (hasAuthoredRotation)
                    continue;

                localX[targetIndex] = NormalizeBasis(
                    TransformDirection(deltaX, target.LocalAxisX, target.LocalAxisY, target.LocalAxisZ), target.LocalAxisX);
                localY[targetIndex] = NormalizeBasis(
                    TransformDirection(deltaY, target.LocalAxisX, target.LocalAxisY, target.LocalAxisZ), target.LocalAxisY);
                localZ[targetIndex] = NormalizeBasis(
                    TransformDirection(deltaZ, target.LocalAxisX, target.LocalAxisY, target.LocalAxisZ), target.LocalAxisZ);
            }
            // A matching deform rig means the control rotation has been handled here;
            // keep the control joint at bind orientation to avoid rotating children around
            // the controller pivot as well as around their own pivots.
            return true;
        }

        private void EvaluateSkeletonPoseState(SkeletonPreviewState state, float frame,
            IList<AnimationBoneTrack> tracks, bool resolveTracksByName, bool updateSkeletonGeometry,
            bool retargetFacialControlsToDeformBones = false)
        {
            if (state == null || state.Joints == null || state.Joints.Count == 0) return;
            int count = state.Joints.Count;
            if (state.WorldPositions == null || state.WorldPositions.Length != count) state.WorldPositions = new Point3D[count];
            if (state.WorldGamePositions == null || state.WorldGamePositions.Length != count) state.WorldGamePositions = new Vector3D[count];
            if (state.WorldAxisX == null || state.WorldAxisX.Length != count) state.WorldAxisX = new Vector3D[count];
            if (state.WorldAxisY == null || state.WorldAxisY.Length != count) state.WorldAxisY = new Vector3D[count];
            if (state.WorldAxisZ == null || state.WorldAxisZ.Length != count) state.WorldAxisZ = new Vector3D[count];

            var localPositions = new Vector3D[count];
            var localX = new Vector3D[count];
            var localY = new Vector3D[count];
            var localZ = new Vector3D[count];
            for (int i = 0; i < count; i++)
            {
                var joint = state.Joints[i];
                localPositions[i] = joint.LocalPosition;
                localX[i] = joint.LocalAxisX;
                localY[i] = joint.LocalAxisY;
                localZ[i] = joint.LocalAxisZ;
            }

            if (tracks != null)
            {
                foreach (var track in tracks)
                {
                    int index = resolveTracksByName
                        ? FindTrackJointIndexByName(track, state.Joints)
                        : (track == null ? -1 : track.JointIndex);
                    if (index < 0 || index >= count) continue;
                    if (track.RotationChannel != null || track.LegacyRotationData != null)
                    {
                        Vector4 value = track.RotationChannel != null
                            ? track.RotationChannel.CalculateValue(frame)
                            : SampleLegacyRotation(track.LegacyRotationData, frame);
                        var q = NormalizeQuaternion(value.W, value.X, value.Y, value.Z);

                        // Gameplay clips expose a single EyeLid/Brow control, whereas the NIS
                        // Alex skin weights the visible eyelids/brows to their child deform joints.
                        // In that rig, rotating only the unweighted control moves the skin around
                        // the wrong pivot. Retarget its local rotation relative to the control's
                        // bind basis onto the child joints, leaving the control at bind orientation.
                        bool retargeted = retargetFacialControlsToDeformBones && resolveTracksByName &&
                            TryRetargetFacialControlRotation(state, tracks, track, index, q, localX, localY, localZ);
                        if (!retargeted)
                        {
                            localX[index] = RotateByQuaternion(q, new Vector3D(1, 0, 0));
                            localY[index] = RotateByQuaternion(q, new Vector3D(0, 1, 0));
                            localZ[index] = RotateByQuaternion(q, new Vector3D(0, 0, 1));
                        }
                    }
                    if (track.TranslationChannel != null || track.LegacyTranslationData != null)
                    {
                        Vector4 translation;
                        if (track.TranslationChannel != null)
                        {
                            translation = track.TranslationChannel.CalculateValue(frame);
                        }
                        else
                        {
                            var sampled = SampleLegacyTranslation(track.LegacyTranslationData, frame);
                            translation = new Vector4 { X = sampled.X, Y = sampled.Y, Z = sampled.Z };
                        }

                        if (state.Joints[index].ParentIndex < 0)
                        {
                            localPositions[index] = state.Joints[index].LocalPosition + new Vector3D(
                                translation.X - track.FirstTranslation.X,
                                translation.Y - track.FirstTranslation.Y,
                                translation.Z - track.FirstTranslation.Z);
                        }
                        else
                        {
                            localPositions[index] = new Vector3D(translation.X, translation.Y, translation.Z);
                        }
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                int parent = state.Joints[i].ParentIndex;
                if (parent >= 0 && parent < i)
                {
                    state.WorldGamePositions[i] = state.WorldGamePositions[parent] + TransformDirection(
                        localPositions[i], state.WorldAxisX[parent], state.WorldAxisY[parent], state.WorldAxisZ[parent]);
                    state.WorldAxisX[i] = NormalizeBasis(TransformDirection(localX[i], state.WorldAxisX[parent], state.WorldAxisY[parent], state.WorldAxisZ[parent]), new Vector3D(1, 0, 0));
                    state.WorldAxisY[i] = NormalizeBasis(TransformDirection(localY[i], state.WorldAxisX[parent], state.WorldAxisY[parent], state.WorldAxisZ[parent]), new Vector3D(0, 1, 0));
                    state.WorldAxisZ[i] = NormalizeBasis(TransformDirection(localZ[i], state.WorldAxisX[parent], state.WorldAxisY[parent], state.WorldAxisZ[parent]), new Vector3D(0, 0, 1));
                }
                else
                {
                    state.WorldGamePositions[i] = localPositions[i];
                    state.WorldAxisX[i] = NormalizeBasis(localX[i], new Vector3D(1, 0, 0));
                    state.WorldAxisY[i] = NormalizeBasis(localY[i], new Vector3D(0, 1, 0));
                    state.WorldAxisZ[i] = NormalizeBasis(localZ[i], new Vector3D(0, 0, 1));
                }
                state.WorldPositions[i] = ToViewerPoint(state.WorldGamePositions[i]);
            }

            if (updateSkeletonGeometry)
            {
                // Updating Geometry keeps the scene graph stable while the displayed skeleton pose changes.
                for (int i = 0; i < count; i++)
                {
                    if (state.JointMarkers != null && i < state.JointMarkers.Length && state.JointMarkers[i] != null)
                    {
                        var marker = CreateBox(state.WorldPositions[i], state.WorldPositions[i], 7.0, System.Windows.Media.Color.FromRgb(64, 196, 255));
                        state.JointMarkers[i].Geometry = marker.Geometry;
                    }
                    if (state.BoneModels != null && i < state.BoneModels.Length && state.BoneModels[i] != null)
                    {
                        int parent = state.Joints[i].ParentIndex;
                        double length = (state.WorldPositions[i] - state.WorldPositions[parent]).Length;
                        double width = Math.Max(3.0, Math.Min(18.0, length * 0.12));
                        var bone = CreateBlenderBone(state.WorldPositions[parent], state.WorldPositions[i], width, System.Windows.Media.Color.FromRgb(230, 175, 70));
                        state.BoneModels[i].Geometry = bone.Geometry;
                    }
                }
            }

            unchecked { state.PoseVersion++; }
        }

        private SkeletonPreviewState GetOrCreateSourceSkeletonPose(Skeleton skeleton)
        {
            if (skeleton == null) return null;
            SkeletonPreviewState existing;
            if (this._sourceSkeletonPoseStates.TryGetValue(skeleton, out existing)) return existing;

            List<JointData> joints = this.ExtractJoints(skeleton);
            if (joints == null || joints.Count == 0) return null;
            var state = new SkeletonPreviewState
            {
                SkeletonNode = skeleton,
                Joints = joints,
                WorldPositions = new Point3D[joints.Count],
                WorldGamePositions = new Vector3D[joints.Count],
                WorldAxisX = new Vector3D[joints.Count],
                WorldAxisY = new Vector3D[joints.Count],
                WorldAxisZ = new Vector3D[joints.Count]
            };

            // Capture the actual source rig's rest pose before applying a clip from another rig.
            this.EvaluateSkeletonPoseState(state, 0.0f, new List<AnimationBoneTrack>(), true, false);
            state.BindGamePositions = (Vector3D[])state.WorldGamePositions.Clone();
            state.BindAxisX = (Vector3D[])state.WorldAxisX.Clone();
            state.BindAxisY = (Vector3D[])state.WorldAxisY.Clone();
            state.BindAxisZ = (Vector3D[])state.WorldAxisZ.Clone();
            this._sourceSkeletonPoseStates[skeleton] = state;
            return state;
        }

        private static Vector4 SampleLegacyRotation(BoneRotationData data, float frame)
        {
            if (data == null || data.Frames == null || data.Values == null || data.Frames.Length == 0 || data.Values.Length == 0)
                return new Vector4 { W = 1.0f };

            int count = Math.Min(data.Frames.Length, data.Values.Length);
            var order = Enumerable.Range(0, count).OrderBy(i => data.Frames[i]).ToArray();
            if (frame <= data.Frames[order[0]]) return data.Values[order[0]];
            if (frame >= data.Frames[order[order.Length - 1]]) return data.Values[order[order.Length - 1]];
            for (int i = 0; i < order.Length - 1; i++)
            {
                int a = order[i], b = order[i + 1];
                float start = data.Frames[a], end = data.Frames[b];
                if (frame < start || frame > end) continue;
                if (end <= start) return data.Values[b];
                float t = (frame - start) / (end - start);
                return Vector4.QuaternionSlerp(data.Values[a], data.Values[b], t);
            }
            return data.Values[order[order.Length - 1]];
        }

        private static Nixson.Common.Vector3 SampleLegacyTranslation(BoneTranslationData data, float frame)
        {
            if (data == null || data.Frames == null || data.Values == null || data.Frames.Length == 0 || data.Values.Length == 0)
                return new Nixson.Common.Vector3();

            int count = Math.Min(data.Frames.Length, data.Values.Length);
            var order = Enumerable.Range(0, count).OrderBy(i => data.Frames[i]).ToArray();
            if (frame <= data.Frames[order[0]]) return data.Values[order[0]];
            if (frame >= data.Frames[order[order.Length - 1]]) return data.Values[order[order.Length - 1]];
            for (int i = 0; i < order.Length - 1; i++)
            {
                int a = order[i], b = order[i + 1];
                float start = data.Frames[a], end = data.Frames[b];
                if (frame < start || frame > end) continue;
                if (end <= start) return data.Values[b];
                float t = (frame - start) / (end - start);
                return new Nixson.Common.Vector3
                {
                    X = data.Values[a].X + (data.Values[b].X - data.Values[a].X) * t,
                    Y = data.Values[a].Y + (data.Values[b].Y - data.Values[a].Y) * t,
                    Z = data.Values[a].Z + (data.Values[b].Z - data.Values[a].Z) * t
                };
            }
            return data.Values[order[order.Length - 1]];
        }

        private bool FitCameraToMeshBounds(bool resetOrientation)
        {
            if (this._meshRenderInfos == null || this._meshRenderInfos.Count == 0) return false;
            double oldCenterX = this._sceneCenterX, oldCenterY = this._sceneCenterY, oldCenterZ = this._sceneCenterZ;
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
            int pointCount = 0;
            foreach (MeshRenderInfo info in this._meshRenderInfos)
            {
                if (info == null || info.Mesh == null || info.Mesh.Positions == null) continue;
                foreach (Point3D point in info.Mesh.Positions)
                {
                    if (double.IsNaN(point.X) || double.IsInfinity(point.X) ||
                        double.IsNaN(point.Y) || double.IsInfinity(point.Y) ||
                        double.IsNaN(point.Z) || double.IsInfinity(point.Z)) continue;
                    minX = Math.Min(minX, point.X); maxX = Math.Max(maxX, point.X);
                    minY = Math.Min(minY, point.Y); maxY = Math.Max(maxY, point.Y);
                    minZ = Math.Min(minZ, point.Z); maxZ = Math.Max(maxZ, point.Z);
                    pointCount++;
                }
            }
            if (pointCount == 0) return false;

            this._sceneCenterX = (minX + maxX) * 0.5;
            this._sceneCenterY = (minY + maxY) * 0.5;
            this._sceneCenterZ = (minZ + maxZ) * 0.5;
            double dx = maxX - minX, dy = maxY - minY, dz = maxZ - minZ;
            this._boundingRadius = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (this._boundingRadius < 0.001) this._boundingRadius = 1200.0;

            // When the animated pose moves the bounds, move the target with the mesh center
            // while preserving the user's pan offset relative to the character.
            if (!resetOrientation && this._hasCameraView && !this._cameraTransitionPending)
            {
                this._targetX += this._sceneCenterX - oldCenterX;
                this._targetY += this._sceneCenterY - oldCenterY;
                this._targetZ += this._sceneCenterZ - oldCenterZ;
            }
            this.FitCameraToBounds(resetOrientation);
            return true;
        }

        private void FitCameraToSkeleton(SkeletonPreviewState state)
        {
            if (state == null || state.WorldPositions == null || state.WorldPositions.Length == 0) return;
            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
            foreach (var p in state.WorldPositions)
            {
                minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
                minZ = Math.Min(minZ, p.Z); maxZ = Math.Max(maxZ, p.Z);
            }
            this._sceneCenterX = this._targetX = (minX + maxX) * 0.5;
            this._sceneCenterY = this._targetY = (minY + maxY) * 0.5;
            this._sceneCenterZ = this._targetZ = (minZ + maxZ) * 0.5;
            double dx = maxX - minX, dy = maxY - minY, dz = maxZ - minZ;
            this._boundingRadius = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (this._boundingRadius < 0.001) this._boundingRadius = 1200.0;
            this.FitCameraToBounds(!this._hasCameraView);
        }

        private static Point3D ToViewerPoint(Vector3D value)
        {
            return new Point3D(value.X * 1000.0, -value.Z * 1000.0, value.Y * 1000.0);
        }

        private static Vector3D TransformDirection(Vector3D value, Vector3D x, Vector3D y, Vector3D z)
        {
            return x * value.X + y * value.Y + z * value.Z;
        }

        private static Vector3D NormalizeBasis(Vector3D value, Vector3D fallback)
        {
            double length = value.Length;
            if (length < 0.000001 || double.IsNaN(length) || double.IsInfinity(length)) return fallback;
            return value / length;
        }

        private static double[] NormalizeQuaternion(float w, float x, float y, float z)
        {
            double length = Math.Sqrt(w * w + x * x + y * y + z * z);
            if (length < 0.000001 || double.IsNaN(length) || double.IsInfinity(length)) return new[] { 1.0, 0.0, 0.0, 0.0 };
            return new[] { w / length, x / length, y / length, z / length };
        }

        private static Vector3D RotateByQuaternion(double[] q, Vector3D value)
        {
            var u = new Vector3D(q[1], q[2], q[3]);
            var uv = Vector3D.CrossProduct(u, value);
            var uuv = Vector3D.CrossProduct(u, uv);
            return value + uv * (2.0 * q[0]) + uuv * 2.0;
        }

        private static bool IsTranslationType(AnimationChannel channel, string type)
        {
            return channel != null && string.Equals(CleanNodeName(channel.TranslationType), type, StringComparison.OrdinalIgnoreCase);
        }

        private static bool BoneNamesMatch(string left, string right)
        {
            left = CleanNodeName(left);
            right = CleanNodeName(right);
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return false;
            return left.EndsWith("_" + right, StringComparison.OrdinalIgnoreCase) ||
                   right.EndsWith("_" + left, StringComparison.OrdinalIgnoreCase);
        }

        private static string CleanNodeName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim().TrimEnd('\0', ' ');
        }

        private static string GetCleanName(BaseNode node)
        {
            if (node == null) return "";
            var p = node.GetType().GetProperty("Name");
            if (p != null && p.GetValue(node, null) is string n && !string.IsNullOrWhiteSpace(n)) return n;
            string s = node.ToString() ?? "";
            int o = s.IndexOf('('), c = s.LastIndexOf(')');
            return (o >= 0 && c > o) ? s.Substring(o + 1, c - o - 1) : s;
        }

        private void ResetCamera()
        {
            // An explicit reset should ignore any pending model-switch transition.
            this._cameraTransitionPending = false;
            this._pendingInitialCameraFit = false;
            this.FitCameraToBounds(true);
            if (this._dx12GizmoOverlay != null) this._dx12GizmoOverlay.Invalidate();
        }

        private bool _initialCameraFitScheduled;

        private double GetEffectiveViewportWidth()
        {
            if (this._dx12Panel != null && !this._dx12Panel.IsDisposed && this._dx12Panel.ClientSize.Width > 1)
                return this._dx12Panel.ClientSize.Width;
            return this.viewport == null ? 0.0 : this.viewport.ActualWidth;
        }

        private double GetEffectiveViewportHeight()
        {
            if (this._dx12Panel != null && !this._dx12Panel.IsDisposed && this._dx12Panel.ClientSize.Height > 1)
                return this._dx12Panel.ClientSize.Height;
            return this.viewport == null ? 0.0 : this.viewport.ActualHeight;
        }

        private void OnViewportSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (!this._pendingInitialCameraFit || this._hasCameraView ||
                this.GetEffectiveViewportWidth() <= 1.0 || this.GetEffectiveViewportHeight() <= 1.0)
                return;

            this.ScheduleInitialCameraFit();
        }

        private void ScheduleInitialCameraFit()
        {
            if (!this._pendingInitialCameraFit || this._initialCameraFitScheduled || this.viewport == null)
                return;

            this._initialCameraFitScheduled = true;
            try
            {
                this.viewport.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new Action(() =>
                    {
                        this._initialCameraFitScheduled = false;
                        if (this.IsDisposed || this.viewport == null || this.wpfGrid == null || this._hasCameraView)
                            return;

                        // Force one layout pass before reading ActualWidth/ActualHeight.
                        // This also handles an initial non-zero but stale size during first display.
                        this.wpfGrid.UpdateLayout();
                        if (this.GetEffectiveViewportWidth() > 1.0 && this.GetEffectiveViewportHeight() > 1.0)
                            this.FitCameraToBounds(true, true);
                    }));
            }
            catch (InvalidOperationException)
            {
                this._initialCameraFitScheduled = false;
            }
        }

        private void CaptureCameraTransitionForNextScene()
        {
            if (this._preserveCameraOnReload || !this._hasCameraView)
            {
                this._cameraTransitionPending = false;
                return;
            }

            this._pendingZoomFactor = this._fitDistance > 0.000001
                ? this._distance / this._fitDistance
                : 1.0;
            if (double.IsNaN(this._pendingZoomFactor) || double.IsInfinity(this._pendingZoomFactor))
                this._pendingZoomFactor = 1.0;
            this._pendingZoomFactor = Math.Max(0.001, Math.Min(80.0, this._pendingZoomFactor));

            this._pendingPanX = this._targetX - this._sceneCenterX;
            this._pendingPanY = this._targetY - this._sceneCenterY;
            this._pendingPanZ = this._targetZ - this._sceneCenterZ;
            this._pendingReferenceRadius = Math.Max(0.001, this._boundingRadius);
            this._cameraTransitionPending = true;
        }

        private void FitCameraToBounds(bool resetOrientation, bool afterLayout = false)
        {
            if (this._preserveCameraOnReload)
            {
                this.UpdateCamera();
                return;
            }

            bool hadCameraView = this._hasCameraView;
            bool hasEffectiveLayout = this.GetEffectiveViewportWidth() > 1.0 && this.GetEffectiveViewportHeight() > 1.0;
            if (!hadCameraView && !afterLayout && !hasEffectiveLayout)
            {
                // Defer only while neither the visible DX12 surface nor the WPF host has a useful size.
                if (resetOrientation)
                {
                    this._cameraTransitionPending = false;
                    this._yaw = -45.0;
                    this._pitch = 15.0;
                    this._targetX = this._sceneCenterX;
                    this._targetY = this._sceneCenterY;
                    this._targetZ = this._sceneCenterZ;
                }
                this._pendingInitialCameraFit = true;
                this.UpdateCamera();
                this.ScheduleInitialCameraFit();
                return;
            }

            if (afterLayout && !hasEffectiveLayout)
            {
                this._pendingInitialCameraFit = true;
                return;
            }

            this._pendingInitialCameraFit = false;
            if (resetOrientation)
            {
                this._cameraTransitionPending = false;
                this._yaw = -45.0;
                this._pitch = 15.0;
                this._targetX = this._sceneCenterX;
                this._targetY = this._sceneCenterY;
                this._targetZ = this._sceneCenterZ;
            }

            double diagonal = Math.Max(0.001, this._boundingRadius);
            double sphereRadius = diagonal * 0.5;
            double aspect = 1.0;
            double viewportWidth = this.GetEffectiveViewportWidth();
            double viewportHeight = this.GetEffectiveViewportHeight();
            if (viewportWidth > 1.0 && viewportHeight > 1.0)
                aspect = viewportWidth / viewportHeight;

            // WPF PerspectiveCamera.FieldOfView is horizontal. Use the narrower of the
            // horizontal and vertical half-angles so tall or wide models fit the viewport.
            double horizontalHalfFov = Math.Max(1.0, this.camera.FieldOfView) * Math.PI / 360.0;
            double verticalHalfFov = Math.Atan(Math.Tan(horizontalHalfFov) / Math.Max(0.1, aspect));
            double limitingHalfFov = Math.Max(0.01, Math.Min(horizontalHalfFov, verticalHalfFov));
            double fitDistance = Math.Max(diagonal * 0.02, sphereRadius / Math.Sin(limitingHalfFov) * 1.12);

            if (resetOrientation || !hadCameraView)
            {
                // First model or an explicit reset: use the default view and fit the object.
                this._targetX = this._sceneCenterX;
                this._targetY = this._sceneCenterY;
                this._targetZ = this._sceneCenterZ;
                this._distance = fitDistance;
                this._cameraTransitionPending = false;
            }
            else if (this._cameraTransitionPending)
            {
                // Preserve the user's relative zoom and pan when moving to a model of a
                // different size. Scale pan by the bounds ratio, avoiding a world-space
                // offset that would be too small or too large for the next object.
                double radiusScale = this._pendingReferenceRadius > 0.001
                    ? diagonal / this._pendingReferenceRadius
                    : 1.0;
                radiusScale = Math.Max(0.001, Math.Min(1000.0, radiusScale));
                this._targetX = this._sceneCenterX + this._pendingPanX * radiusScale;
                this._targetY = this._sceneCenterY + this._pendingPanY * radiusScale;
                this._targetZ = this._sceneCenterZ + this._pendingPanZ * radiusScale;
                this._distance = fitDistance * this._pendingZoomFactor;
                this._cameraTransitionPending = false;
            }
            else
            {
                // If bounds are refitted without a captured transition, retain the current
                // zoom ratio rather than forcing the model back to its default fit distance.
                double zoomFactor = this._fitDistance > 0.000001
                    ? this._distance / this._fitDistance
                    : 1.0;
                if (double.IsNaN(zoomFactor) || double.IsInfinity(zoomFactor)) zoomFactor = 1.0;
                zoomFactor = Math.Max(0.001, Math.Min(80.0, zoomFactor));
                this._distance = fitDistance * zoomFactor;
            }

            this._fitDistance = fitDistance;
            this._hasCameraView = true;
            this.UpdateCamera();
        }

        private void UpdateCamera()
        {
            double radYaw = this._yaw * Math.PI / 180.0;
            double radPitch = this._pitch * Math.PI / 180.0;

            // Z-up orbit: yaw rotates around Z in the XY plane, pitch raises the camera along Z.
            double camX = this._targetX + this._distance * Math.Cos(radPitch) * Math.Sin(radYaw);
            double camY = this._targetY + this._distance * Math.Cos(radPitch) * Math.Cos(radYaw);
            double camZ = this._targetZ + this._distance * Math.Sin(radPitch);

            this.camera.Position = new Point3D(camX, camY, camZ);
            this.camera.LookDirection = new Vector3D(this._targetX - camX, this._targetY - camY, this._targetZ - camZ);
            this.camera.NearPlaneDistance = Math.Max(0.000001, Math.Min(1.0, this._distance * 0.002));
            this.camera.FarPlaneDistance = Math.Max(1000.0, this._distance * 8.0 + this._boundingRadius * 2.0);

            double gzDist = 3.6;
            double gzX = gzDist * Math.Cos(radPitch) * Math.Sin(radYaw);
            double gzY = gzDist * Math.Cos(radPitch) * Math.Cos(radYaw);
            double gzZ = gzDist * Math.Sin(radPitch);

            this.gizmoCamera.Position = new Point3D(gzX, gzY, gzZ);
            this.gizmoCamera.LookDirection = new Vector3D(-gzX, -gzY, -gzZ);

            double cosY = Math.Cos(radYaw), sinY = Math.Sin(radYaw);
            double cosP = Math.Cos(radPitch), sinP = Math.Sin(radPitch);

            // Project world axes into the gizmo camera's screen plane for Z-up.
            Canvas.SetLeft(this.xLabel, 45.0 - 32.0 * cosP * cosY);
            Canvas.SetTop(this.xLabel, 45.0 + 32.0 * sinP * sinY);

            Canvas.SetLeft(this.yLabel, 45.0 + 32.0 * cosP * sinY);
            Canvas.SetTop(this.yLabel, 45.0 + 32.0 * sinP * cosY);

            Canvas.SetLeft(this.zLabel, 45.0);
            Canvas.SetTop(this.zLabel, 45.0 - 32.0 * cosP);
        }

        private void OnWpfMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._lastMousePos = e.GetPosition(this.wpfGrid);
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this._isOrbiting = true;
                this.wpfGrid.CaptureMouse();
            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this._isPanning = true;
                this.wpfGrid.CaptureMouse();
            }
        }

        private void OnWpfMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!this._isOrbiting && !this._isPanning) return;

            System.Windows.Point cur = e.GetPosition(this.wpfGrid);
            double dx = cur.X - this._lastMousePos.X;
            double dy = cur.Y - this._lastMousePos.Y;
            this._lastMousePos = cur;

            if (this._isOrbiting)
            {
                // Dragging right should rotate the scene in the same intuitive direction as Blender.
                this._yaw += dx * 0.5;
                this._pitch += dy * 0.5;
                this._pitch = Math.Max(-89.0, Math.Min(89.0, this._pitch));
                this.UpdateCamera();
            }
            else if (this._isPanning)
            {
                double factor = this._distance * 0.0015;
                double radYaw = this._yaw * Math.PI / 180.0;
                // Horizontal right-button drag follows the cursor instead of moving the view in the opposite direction.
                // Keep vertical panning on Z and invert both horizontal-plane components together.
                this._targetX += dx * factor * Math.Cos(radYaw);
                this._targetY -= dx * factor * Math.Sin(radYaw);
                this._targetZ += dy * factor;
                this.UpdateCamera();
            }
        }

        private void OnWpfMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released) this._isOrbiting = false;
            if (e.RightButton == System.Windows.Input.MouseButtonState.Released) this._isPanning = false;

            if (!this._isOrbiting && !this._isPanning) this.wpfGrid.ReleaseMouseCapture();
        }

        private void OnWpfMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double factor = e.Delta > 0 ? 0.85 : 1.15;
            double minDistance = Math.Max(0.001, this._boundingRadius * 0.004);
            double maxDistance = Math.Max(minDistance * 100.0, this._boundingRadius * 80.0);
            this._distance = Math.Max(minDistance, Math.Min(maxDistance, this._distance * factor));
            this.UpdateCamera();
        }

        private sealed class ViewerToolStripColorTable : ProfessionalColorTable
        {
            private readonly Func<bool> _isDark;
            public ViewerToolStripColorTable(Func<bool> isDark) { _isDark = isDark; }
            private bool Dark => _isDark != null && _isDark();

            public override System.Drawing.Color ToolStripDropDownBackground => Dark ? System.Drawing.Color.FromArgb(37, 37, 38) : base.ToolStripDropDownBackground;
            public override System.Drawing.Color ImageMarginGradientBegin => Dark ? System.Drawing.Color.FromArgb(37, 37, 38) : base.ImageMarginGradientBegin;
            public override System.Drawing.Color ImageMarginGradientMiddle => Dark ? System.Drawing.Color.FromArgb(37, 37, 38) : base.ImageMarginGradientMiddle;
            public override System.Drawing.Color ImageMarginGradientEnd => Dark ? System.Drawing.Color.FromArgb(37, 37, 38) : base.ImageMarginGradientEnd;
            public override System.Drawing.Color ToolStripBorder => Dark ? System.Drawing.Color.FromArgb(48, 48, 50) : base.ToolStripBorder;
            public override System.Drawing.Color ToolStripGradientBegin => Dark ? System.Drawing.Color.FromArgb(32, 32, 34) : base.ToolStripGradientBegin;
            public override System.Drawing.Color ToolStripGradientMiddle => Dark ? System.Drawing.Color.FromArgb(32, 32, 34) : base.ToolStripGradientMiddle;
            public override System.Drawing.Color ToolStripGradientEnd => Dark ? System.Drawing.Color.FromArgb(32, 32, 34) : base.ToolStripGradientEnd;
            public override System.Drawing.Color ButtonSelectedHighlight => Dark ? System.Drawing.Color.FromArgb(52, 52, 56) : base.ButtonSelectedHighlight;
            public override System.Drawing.Color ButtonSelectedHighlightBorder => Dark ? System.Drawing.Color.FromArgb(80, 80, 86) : base.ButtonSelectedHighlightBorder;
            public override System.Drawing.Color ButtonPressedHighlight => Dark ? System.Drawing.Color.FromArgb(42, 42, 46) : base.ButtonPressedHighlight;
            public override System.Drawing.Color ButtonPressedHighlightBorder => Dark ? System.Drawing.Color.FromArgb(0, 122, 204) : base.ButtonPressedHighlightBorder;
            public override System.Drawing.Color ButtonCheckedGradientBegin => Dark ? System.Drawing.Color.FromArgb(0, 122, 204) : base.ButtonCheckedGradientBegin;
            public override System.Drawing.Color ButtonCheckedGradientMiddle => Dark ? System.Drawing.Color.FromArgb(0, 122, 204) : base.ButtonCheckedGradientMiddle;
            public override System.Drawing.Color ButtonCheckedGradientEnd => Dark ? System.Drawing.Color.FromArgb(0, 122, 204) : base.ButtonCheckedGradientEnd;
            public override System.Drawing.Color MenuItemSelected => Dark ? System.Drawing.Color.FromArgb(62, 62, 64) : base.MenuItemSelected;
            public override System.Drawing.Color MenuItemSelectedGradientBegin => Dark ? System.Drawing.Color.FromArgb(62, 62, 64) : base.MenuItemSelectedGradientBegin;
            public override System.Drawing.Color MenuItemSelectedGradientEnd => Dark ? System.Drawing.Color.FromArgb(62, 62, 64) : base.MenuItemSelectedGradientEnd;
        }

        private sealed class ViewerToolStripRenderer : ToolStripProfessionalRenderer
        {
            private readonly Func<bool> _isDark;
            public ViewerToolStripRenderer(Func<bool> isDark) : base(new ViewerToolStripColorTable(isDark))
            {
                _isDark = isDark;
            }

            private bool Dark => _isDark != null && _isDark();

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                System.Drawing.Color color = Dark
                    ? System.Drawing.Color.FromArgb(32, 32, 34)
                    : System.Drawing.SystemColors.Control;
                using (var brush = new SolidBrush(color))
                    e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                System.Drawing.Color color = Dark
                    ? System.Drawing.Color.FromArgb(48, 48, 50)
                    : System.Drawing.SystemColors.ControlDark;
                using (var pen = new System.Drawing.Pen(color))
                    e.Graphics.DrawLine(pen, 0, Math.Max(0, e.ToolStrip.Height - 1), e.ToolStrip.Width, Math.Max(0, e.ToolStrip.Height - 1));
            }

            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                var button = e.Item as ToolStripButton;
                if (button == null)
                {
                    base.OnRenderButtonBackground(e);
                    return;
                }

                bool checkedState = button.Checked;
                bool hoverState = button.Selected || button.Pressed;
                if (!checkedState && !hoverState) return;

                System.Drawing.Color fill = checkedState
                    ? (Dark ? System.Drawing.Color.FromArgb(0, 122, 204) : System.Drawing.SystemColors.Highlight)
                    : (Dark ? System.Drawing.Color.FromArgb(52, 52, 56) : System.Drawing.SystemColors.ControlLight);
                System.Drawing.Color border = checkedState
                    ? (Dark ? System.Drawing.Color.FromArgb(60, 160, 225) : System.Drawing.SystemColors.Highlight)
                    : (Dark ? System.Drawing.Color.FromArgb(80, 80, 86) : System.Drawing.SystemColors.ControlDark);
                var bounds = new System.Drawing.Rectangle(0, 0, Math.Max(0, e.Item.Width - 1), Math.Max(0, e.Item.Height - 1));
                using (var brush = new SolidBrush(fill)) e.Graphics.FillRectangle(brush, bounds);
                using (var pen = new System.Drawing.Pen(border)) e.Graphics.DrawRectangle(pen, bounds);
            }

            protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
            {
                // ToolStripButton reserves an internal image layout rectangle even when
                // Padding and Margin are empty. With a button exactly the same size as
                // the thumbnail, that rectangle is smaller than the bitmap and clips it.
                // Shading thumbnails are designed to fill their button edge-to-edge, so
                // draw them directly into the complete item bounds instead of the reduced
                // default ImageRectangle. All other toolbar images keep standard layout.
                var button = e.Item as ToolStripButton;
                if (button != null && button.Tag is ViewerShadingMode && button.Image != null)
                {
                    var bounds = new System.Drawing.Rectangle(0, 0,
                        Math.Max(1, e.Item.Width), Math.Max(1, e.Item.Height));
                    e.Graphics.DrawImage(
                        button.Image,
                        bounds,
                        0,
                        0,
                        button.Image.Width,
                        button.Image.Height,
                        System.Drawing.GraphicsUnit.Pixel);
                    return;
                }

                base.OnRenderItemImage(e);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (!e.Item.Enabled)
                    e.TextColor = Dark ? System.Drawing.Color.FromArgb(120, 120, 120) : System.Drawing.SystemColors.GrayText;
                else if (e.Item is ToolStripButton && ((ToolStripButton)e.Item).Checked)
                    e.TextColor = Dark ? System.Drawing.Color.White : System.Drawing.SystemColors.HighlightText;
                else
                    e.TextColor = e.Item.ForeColor;
                base.OnRenderItemText(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._animationTimer != null)
                {
                    this._animationTimer.Stop();
                    this._animationTimer.Dispose();
                    this._animationTimer = null;
                }
                if (this._sceneStatsOverlay != null)
                {
                    this._sceneStatsOverlay.Dispose();
                    this._sceneStatsOverlay = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}