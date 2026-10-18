using System;
using System.Collections.Generic;
using System.Diagnostics;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using System.Runtime.InteropServices;
using System.Threading;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D12.D3D12;
using static Vortice.DXGI.DXGI;

namespace PrototypeUltimateTool.DirectX12SkinningPrototype;

/// <summary>
/// Standalone DX12 GPU-skinning proof of concept. It intentionally renders a generated two-bone
/// test mesh; the P3D adapter is a separate follow-up so this does not change the net48 editor.
/// </summary>
internal sealed class Dx12SkinnedMeshRenderer : IDisposable
{
    private const int BufferCount = 2;
    private const int ConstantBufferBytes = 256;
    private const uint RootParameterIndex = 0;

    internal enum RenderMode { Solid, Wireframe, Normals }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct GpuSkinVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Bone0, Bone1, Bone2, Bone3;
        public float Weight0, Weight1, Weight2, Weight3;

        public GpuSkinVertex(Vector3 position, Vector3 normal, uint bone)
        {
            Position = position;
            Normal = normal;
            Bone0 = bone; Bone1 = 0; Bone2 = 0; Bone3 = 0;
            Weight0 = 1.0f; Weight1 = 0.0f; Weight2 = 0.0f; Weight3 = 0.0f;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = ConstantBufferBytes)]
    private struct SceneConstants
    {
        public Matrix4x4 ViewProjection;
        public Matrix4x4 Bone0;
        public Matrix4x4 Bone1;
        public Vector4 BaseColor;
        public float NormalLength;
        public Vector3 Padding;
    }

    private readonly IDXGIFactory4 _factory;
    private readonly ID3D12Device _device;
    private readonly ID3D12CommandQueue _queue;
    private readonly IDXGISwapChain3 _swapChain;
    private readonly ID3D12DescriptorHeap _rtvHeap;
    private readonly ID3D12DescriptorHeap _dsvHeap;
    private readonly ID3D12CommandAllocator[] _allocators = new ID3D12CommandAllocator[BufferCount];
    private readonly ID3D12Resource?[] _backBuffers = new ID3D12Resource?[BufferCount];
    private readonly ulong[] _frameFenceValues = new ulong[BufferCount];
    private readonly ID3D12Fence _fence;
    private readonly AutoResetEvent _fenceEvent = new(false);
    private readonly ID3D12RootSignature _rootSignature;
    private readonly ID3D12PipelineState _solidPipeline;
    private readonly ID3D12PipelineState _wireframePipeline;
    private readonly ID3D12PipelineState _normalsPipeline;
    private readonly ID3D12GraphicsCommandList _commandList;
    private readonly ID3D12Resource _vertexBuffer;
    private readonly ID3D12Resource _indexBuffer;
    private readonly ID3D12Resource[] _constantBuffers = new ID3D12Resource[BufferCount];
    private readonly uint _rtvDescriptorSize;
    private readonly int _indexCount;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private long _fpsFrames;
    private long _fpsStartTicks = Stopwatch.GetTimestamp();
    private ulong _nextFenceValue;
    private int _width;
    private int _height;
    private ID3D12Resource? _depthBuffer;
    private RenderMode _mode = RenderMode.Solid;
    private double _animationTime;
    private bool _disposed;

    public string ModeLabel => _mode.ToString();
    public double Fps { get; private set; }

    public Dx12SkinnedMeshRenderer(IntPtr hwnd, int width, int height)
    {
        if (hwnd == IntPtr.Zero) throw new ArgumentException("A valid WinForms HWND is required.", nameof(hwnd));
        _width = Math.Max(1, width);
        _height = Math.Max(1, height);

        _factory = CreateDXGIFactory2<IDXGIFactory4>(false);
        _device = D3D12CreateDevice<ID3D12Device>(null, FeatureLevel.Level_11_0);
        _queue = _device.CreateCommandQueue(CommandListType.Direct);

        var swapChainDescription = new SwapChainDescription1
        {
            Width = (uint)_width,
            Height = (uint)_height,
            Format = Format.R8G8B8A8_UNorm,
            Stereo = false,
            SampleDescription = new SampleDescription(1, 0),
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = BufferCount,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipDiscard,
            AlphaMode = AlphaMode.Ignore
        };
        using (IDXGISwapChain1 swapChain1 = _factory.CreateSwapChainForHwnd(_queue, hwnd, swapChainDescription))
            _swapChain = swapChain1.QueryInterface<IDXGISwapChain3>();
        _factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAltEnter);

        _rtvHeap = _device.CreateDescriptorHeap(new DescriptorHeapDescription(DescriptorHeapType.RenderTargetView, BufferCount));
        _dsvHeap = _device.CreateDescriptorHeap(new DescriptorHeapDescription(DescriptorHeapType.DepthStencilView, 1));
        _rtvDescriptorSize = _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);

        for (int i = 0; i < BufferCount; i++)
            _allocators[i] = _device.CreateCommandAllocator(CommandListType.Direct);

        string shaderPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Shaders", "SkinnedMesh.hlsl");
        if (!System.IO.File.Exists(shaderPath))
            throw new System.IO.FileNotFoundException("HLSL shader file was not copied to the output directory.", shaderPath);
        string shaderSource = System.IO.File.ReadAllText(shaderPath);
        ReadOnlyMemory<byte> vertexShader = Compiler.Compile(shaderSource, "VSMain", "vs_5_1");
        ReadOnlyMemory<byte> pixelShader = Compiler.Compile(shaderSource, "PSMain", "ps_5_1");
        ReadOnlyMemory<byte> normalsGeometryShader = Compiler.Compile(shaderSource, "GSNormals", "gs_5_1");

        var rootParameter = new RootParameter1(
            RootParameterType.ConstantBufferView,
            new RootDescriptor1(0, 0),
            ShaderVisibility.All);
        var rootSignatureDescription = new RootSignatureDescription1(
            RootSignatureFlags.AllowInputAssemblerInputLayout,
            new[] { rootParameter },
            Array.Empty<StaticSamplerDescription>());
        _rootSignature = _device.CreateRootSignature(rootSignatureDescription);

        _solidPipeline = CreatePipeline(vertexShader, pixelShader, ReadOnlyMemory<byte>.Empty, false);
        _wireframePipeline = CreatePipeline(vertexShader, pixelShader, ReadOnlyMemory<byte>.Empty, true);
        _normalsPipeline = CreatePipeline(vertexShader, pixelShader, normalsGeometryShader, false);

        (GpuSkinVertex[] vertices, uint[] indices) = CreateTwoBoneSampleMesh();
        _indexCount = indices.Length;
        ulong vertexBytes = (ulong)(Marshal.SizeOf<GpuSkinVertex>() * vertices.Length);
        ulong indexBytes = (ulong)(sizeof(uint) * indices.Length);
        _vertexBuffer = _device.CreateCommittedResource(HeapType.Upload, ResourceDescription.Buffer(vertexBytes), ResourceStates.GenericRead);
        _indexBuffer = _device.CreateCommittedResource(HeapType.Upload, ResourceDescription.Buffer(indexBytes), ResourceStates.GenericRead);
        _vertexBuffer.SetData<GpuSkinVertex>(vertices);
        _indexBuffer.SetData<uint>(indices);
        for (int i = 0; i < BufferCount; i++)
            _constantBuffers[i] = _device.CreateCommittedResource(HeapType.Upload, ResourceDescription.Buffer(ConstantBufferBytes), ResourceStates.GenericRead);

        _commandList = _device.CreateCommandList<ID3D12GraphicsCommandList>(CommandListType.Direct, _allocators[0], _solidPipeline);
        _commandList.Close();
        _fence = _device.CreateFence(0);

        CreateRenderTargets();
        CreateDepthBuffer();
    }

    public void SetMode(RenderMode mode) => _mode = mode;

    public void Render(bool paused)
    {
        ThrowIfDisposed();
        int frameIndex = (int)_swapChain.CurrentBackBufferIndex;
        WaitForFrame(frameIndex);

        _allocators[frameIndex].Reset();
        ID3D12PipelineState pipeline = _mode switch
        {
            RenderMode.Wireframe => _wireframePipeline,
            RenderMode.Normals => _normalsPipeline,
            _ => _solidPipeline
        };
        _commandList.Reset(_allocators[frameIndex], pipeline);

        if (!paused) _animationTime += Math.Min(0.05, _clock.Elapsed.TotalSeconds);
        _clock.Restart();
        float angle = (float)(Math.Sin(_animationTime * 1.7) * 0.78);
        var view = Matrix4x4.CreateLookAt(new Vector3(3.0f, 2.2f, 4.8f), new Vector3(0.0f, 0.1f, 0.0f), Vector3.UnitY);
        float aspect = _width / (float)Math.Max(1, _height);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(0.78f, aspect, 0.1f, 100.0f);
        var constants = new SceneConstants
        {
            ViewProjection = view * projection,
            Bone0 = Matrix4x4.Identity,
            Bone1 = Matrix4x4.CreateRotationZ(angle),
            BaseColor = _mode == RenderMode.Wireframe
                ? new Vector4(0.75f, 0.86f, 1.0f, 1.0f)
                : new Vector4(0.42f, 0.70f, 0.96f, 1.0f),
            NormalLength = 0.18f,
            Padding = Vector3.Zero
        };
        _constantBuffers[frameIndex].SetData<SceneConstants>(new[] { constants });

        ID3D12Resource backBuffer = _backBuffers[frameIndex] ?? throw new InvalidOperationException("Back buffer is unavailable.");
        CpuDescriptorHandle rtvHandle = GetRtvHandle(frameIndex);
        CpuDescriptorHandle dsvHandle = _dsvHeap.GetCPUDescriptorHandleForHeapStart();

        _commandList.ResourceBarrierTransition(backBuffer, ResourceStates.Present, ResourceStates.RenderTarget);
        _commandList.OMSetRenderTargets(rtvHandle, dsvHandle);
        _commandList.ClearRenderTargetView(rtvHandle, new Color4(0.055f, 0.067f, 0.09f, 1.0f));
        _commandList.ClearDepthStencilView(dsvHandle, ClearFlags.Depth, 1.0f, 0);
        _commandList.RSSetViewport(new Viewport(0, 0, _width, _height, 0.0f, 1.0f));
        _commandList.RSSetScissorRect(new Rect(0, 0, _width, _height));
        _commandList.SetGraphicsRootSignature(_rootSignature);
        _commandList.SetGraphicsRootConstantBufferView(RootParameterIndex, _constantBuffers[frameIndex].GPUVirtualAddress);
        _commandList.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        _commandList.IASetVertexBuffers(0, new VertexBufferView
        {
            BufferLocation = _vertexBuffer.GPUVirtualAddress,
            SizeInBytes = checked((uint)_vertexBuffer.Description.Width),
            StrideInBytes = (uint)Marshal.SizeOf<GpuSkinVertex>()
        });
        _commandList.IASetIndexBuffer(new IndexBufferView
        {
            BufferLocation = _indexBuffer.GPUVirtualAddress,
            SizeInBytes = (uint)(_indexCount * sizeof(uint)),
            Format = Format.R32_UInt
        });
        _commandList.DrawIndexedInstanced((uint)_indexCount, 1, 0, 0, 0);
        _commandList.ResourceBarrierTransition(backBuffer, ResourceStates.RenderTarget, ResourceStates.Present);
        _commandList.Close();
        _queue.ExecuteCommandList(_commandList);
        _swapChain.Present(1, PresentFlags.None);

        ulong submittedFenceValue = ++_nextFenceValue;
        _queue.Signal(_fence, submittedFenceValue);
        _frameFenceValues[frameIndex] = submittedFenceValue;
        UpdateFps();
    }

    public void Resize(int width, int height)
    {
        if (_disposed || width <= 0 || height <= 0 || (width == _width && height == _height)) return;
        WaitForGpu();
        for (int i = 0; i < BufferCount; i++)
        {
            _backBuffers[i]?.Dispose();
            _backBuffers[i] = null;
        }
        _depthBuffer?.Dispose();
        _depthBuffer = null;
        _width = width;
        _height = height;
        _swapChain.ResizeBuffers(BufferCount, (uint)_width, (uint)_height, Format.R8G8B8A8_UNorm);
        Array.Clear(_frameFenceValues, 0, _frameFenceValues.Length);
        CreateRenderTargets();
        CreateDepthBuffer();
    }

    private ID3D12PipelineState CreatePipeline(ReadOnlyMemory<byte> vertexShader, ReadOnlyMemory<byte> pixelShader,
        ReadOnlyMemory<byte> geometryShader, bool wireframe)
    {
        RasterizerDescription rasterizer = RasterizerDescription.CullNone;
        if (wireframe) rasterizer.FillMode = FillMode.Wireframe;
        InputElementDescription[] inputElements =
        {
            new("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
            new("BLENDINDICES", 0, Format.R32G32B32A32_UInt, 24, 0),
            new("BLENDWEIGHT", 0, Format.R32G32B32A32_Float, 40, 0)
        };
        var description = new GraphicsPipelineStateDescription
        {
            RootSignature = _rootSignature,
            VertexShader = vertexShader,
            PixelShader = pixelShader,
            GeometryShader = geometryShader,
            InputLayout = new InputLayoutDescription(inputElements),
            RasterizerState = rasterizer,
            BlendState = BlendDescription.Opaque,
            DepthStencilState = DepthStencilDescription.Default,
            SampleMask = uint.MaxValue,
            PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
            RenderTargetFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthStencilFormat = Format.D32_Float,
            SampleDescription = new SampleDescription(1, 0)
        };
        return _device.CreateGraphicsPipelineState(description);
    }

    private void CreateRenderTargets()
    {
        for (int i = 0; i < BufferCount; i++)
        {
            ID3D12Resource backBuffer = _swapChain.GetBuffer<ID3D12Resource>((uint)i);
            _backBuffers[i] = backBuffer;
            _device.CreateRenderTargetView(backBuffer, null, GetRtvHandle(i));
        }
    }

    private void CreateDepthBuffer()
    {
        _depthBuffer = _device.CreateCommittedResource(
            HeapType.Default,
            ResourceDescription.Texture2D(Format.D32_Float, (uint)_width, (uint)_height, flags: ResourceFlags.AllowDepthStencil),
            ResourceStates.DepthWrite,
            new ClearValue(Format.D32_Float, 1.0f, 0));
        var description = new DepthStencilViewDescription
        {
            Format = Format.D32_Float,
            ViewDimension = DepthStencilViewDimension.Texture2D
        };
        _device.CreateDepthStencilView(_depthBuffer, description, _dsvHeap.GetCPUDescriptorHandleForHeapStart());
    }

    private CpuDescriptorHandle GetRtvHandle(int index)
    {
        CpuDescriptorHandle start = _rtvHeap.GetCPUDescriptorHandleForHeapStart();
        return new CpuDescriptorHandle(start.Ptr + (int)(_rtvDescriptorSize * index));
    }

    private void WaitForFrame(int frameIndex)
    {
        ulong value = _frameFenceValues[frameIndex];
        if (value == 0 || _fence.CompletedValue >= value) return;
        _fence.SetEventOnCompletion(value, _fenceEvent.SafeWaitHandle.DangerousGetHandle());
        _fenceEvent.WaitOne();
    }

    private void WaitForGpu()
    {
        ulong value = ++_nextFenceValue;
        _queue.Signal(_fence, value);
        if (_fence.CompletedValue < value)
        {
            _fence.SetEventOnCompletion(value, _fenceEvent.SafeWaitHandle.DangerousGetHandle());
            _fenceEvent.WaitOne();
        }
    }

    private void UpdateFps()
    {
        _fpsFrames++;
        long now = Stopwatch.GetTimestamp();
        double elapsed = (now - _fpsStartTicks) / (double)Stopwatch.Frequency;
        if (elapsed >= 0.5)
        {
            Fps = _fpsFrames / elapsed;
            _fpsFrames = 0;
            _fpsStartTicks = now;
        }
    }

    private static (GpuSkinVertex[] Vertices, uint[] Indices) CreateTwoBoneSampleMesh()
    {
        var vertices = new List<GpuSkinVertex>();
        var indices = new List<uint>();
        AddSegment(vertices, indices, -1.2f, 0.0f, 0, includeBottom: true, includeTop: false);
        AddSegment(vertices, indices, 0.0f, 1.35f, 1, includeBottom: false, includeTop: true);
        return (vertices.ToArray(), indices.ToArray());
    }

    private static void AddSegment(List<GpuSkinVertex> vertices, List<uint> indices, float bottom, float top,
        uint bone, bool includeBottom, bool includeTop)
    {
        const float x0 = -0.22f, x1 = 0.22f, z0 = -0.16f, z1 = 0.16f;
        Vector3 blf = new(x0, bottom, z1), brf = new(x1, bottom, z1), trf = new(x1, top, z1), tlf = new(x0, top, z1);
        Vector3 blb = new(x0, bottom, z0), brb = new(x1, bottom, z0), trb = new(x1, top, z0), tlb = new(x0, top, z0);
        AddFace(vertices, indices, new Vector3(0, 0, 1), blf, brf, trf, tlf, bone);
        AddFace(vertices, indices, new Vector3(0, 0, -1), brb, blb, tlb, trb, bone);
        AddFace(vertices, indices, new Vector3(-1, 0, 0), blb, blf, tlf, tlb, bone);
        AddFace(vertices, indices, new Vector3(1, 0, 0), brf, brb, trb, trf, bone);
        if (includeBottom) AddFace(vertices, indices, new Vector3(0, -1, 0), blb, brb, brf, blf, bone);
        if (includeTop) AddFace(vertices, indices, new Vector3(0, 1, 0), tlf, trf, trb, tlb, bone);
    }

    private static void AddFace(List<GpuSkinVertex> vertices, List<uint> indices, Vector3 normal,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d, uint bone)
    {
        uint start = (uint)vertices.Count;
        vertices.Add(new GpuSkinVertex(a, normal, bone));
        vertices.Add(new GpuSkinVertex(b, normal, bone));
        vertices.Add(new GpuSkinVertex(c, normal, bone));
        vertices.Add(new GpuSkinVertex(d, normal, bone));
        indices.Add(start); indices.Add(start + 1); indices.Add(start + 2);
        indices.Add(start); indices.Add(start + 2); indices.Add(start + 3);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(Dx12SkinnedMeshRenderer));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { WaitForGpu(); } catch { }
        _commandList.Dispose();
        _solidPipeline.Dispose();
        _wireframePipeline.Dispose();
        _normalsPipeline.Dispose();
        foreach (ID3D12Resource constantBuffer in _constantBuffers) constantBuffer.Dispose();
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();
        _depthBuffer?.Dispose();
        for (int i = 0; i < BufferCount; i++)
        {
            _backBuffers[i]?.Dispose();
            _allocators[i]?.Dispose();
        }
        _dsvHeap.Dispose();
        _rtvHeap.Dispose();
        _rootSignature.Dispose();
        _fence.Dispose();
        _swapChain.Dispose();
        _queue.Dispose();
        _device.Dispose();
        _factory.Dispose();
        _fenceEvent.Dispose();
    }
}
