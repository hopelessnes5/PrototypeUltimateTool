using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PrototypeUltimateTool.DirectX12SkinningPrototype;

internal sealed class SkinningPrototypeForm : Form
{
    private readonly System.Windows.Forms.Timer _timer;
    private Dx12SkinnedMeshRenderer? _renderer;
    private readonly Stopwatch _titleClock = Stopwatch.StartNew();
    private bool _paused;

    public SkinningPrototypeForm()
    {
        Text = "PrototypeUltimateTool — DirectX 12 GPU Skinning Prototype";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1120, 760);
        MinimumSize = new Size(640, 480);
        BackColor = Color.FromArgb(18, 21, 27);
        KeyPreview = true;
        DoubleBuffered = false;

        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += (_, _) => RenderFrame();
        Shown += (_, _) => InitializeRenderer();
        Resize += (_, _) =>
        {
            if (WindowState != FormWindowState.Minimized && _renderer != null)
                _renderer.Resize(ClientSize.Width, ClientSize.Height);
        };
        KeyDown += OnKeyDown;
        FormClosed += (_, _) =>
        {
            _timer.Stop();
            _renderer?.Dispose();
            _renderer = null;
            _timer.Dispose();
        };
    }

    private void InitializeRenderer()
    {
        try
        {
            _renderer = new Dx12SkinnedMeshRenderer(Handle, ClientSize.Width, ClientSize.Height);
            _timer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "Не удалось запустить прототип DirectX 12. Проверь наличие GPU/драйвера с поддержкой Direct3D 12 и восстановление NuGet-пакетов.\n\n" + ex,
                "DirectX 12 prototype", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void RenderFrame()
    {
        if (_renderer == null || WindowState == FormWindowState.Minimized) return;
        try
        {
            _renderer.Render(_paused);
            if (_titleClock.ElapsedMilliseconds >= 500)
            {
                Text = $"DirectX 12 GPU Skinning Prototype | {_renderer.ModeLabel} | {_renderer.Fps:0} FPS | F1 Solid · F2 Wireframe · F3 Normals · Space Pause";
                _titleClock.Restart();
            }
        }
        catch (Exception ex)
        {
            _timer.Stop();
            MessageBox.Show(this, "Ошибка при отрисовке DirectX 12:\n\n" + ex,
                "DirectX 12 prototype", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_renderer == null) return;
        if (e.KeyCode == Keys.F1) _renderer.SetMode(Dx12SkinnedMeshRenderer.RenderMode.Solid);
        else if (e.KeyCode == Keys.F2) _renderer.SetMode(Dx12SkinnedMeshRenderer.RenderMode.Wireframe);
        else if (e.KeyCode == Keys.F3) _renderer.SetMode(Dx12SkinnedMeshRenderer.RenderMode.Normals);
        else if (e.KeyCode == Keys.Space) _paused = !_paused;
        else return;
        e.Handled = true;
    }
}
