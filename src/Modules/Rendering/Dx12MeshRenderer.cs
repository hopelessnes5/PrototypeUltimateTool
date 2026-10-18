using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D12.D3D12;
using static Vortice.DXGI.DXGI;

namespace PrototypeUltimateTool.Modules.Rendering
{
    internal enum Dx12ShadingMode
    {
        Wireframe = 0,
        Solid = 1,
        Textured = 2,
        Normals = 3,
        Skinning = 4,
        Deformation = 5
    }

    /// <summary>Renderable view of a model currently present in the editor scene graph.</summary>
    internal sealed class Dx12SceneMesh
    {
        public object Key;
        public MeshGeometry3D Mesh;
        public Matrix3D Transform = Matrix3D.Identity;
        public Vector4 BaseColor = new Vector4(0.745f, 0.765f, 0.792f, 1.0f);
        public BitmapSource Texture;
        public Vector4[] VertexColors;
        // Immutable mesh bind-pose skin data; BoneMatrices are refreshed per frame and consumed by the VS.
        public int[][] SkinBoneIndices;
        public double[][] SkinWeights;
        public bool[] SkinUsesRigidBoneOrigin;
        public Matrix4x4[] BoneMatrices;
        public long BoneMatrixVersion;
        public bool HasGpuSkinning;
    }

    /// <summary>
    /// Main DX12 viewport renderer. It draws the scene graph data collected by Model3DViewer;
    /// it does not generate per-edge tube geometry or use WPF Viewport3D for visible rendering.
    /// </summary>
    internal sealed class Dx12MeshRenderer : IDisposable
    {
        private const int BufferCount = 2;
        private const int ConstantBufferBytes = 256;
        private const int MaxTextureDimension = 1024;
        private const int MaxTexturePixels = 8 * 1024 * 1024;
        private readonly uint[] _msaaSampleCandidates = new uint[] { 4, 2 };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct GpuVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public Vector4 Color;
            public uint TextureOffset;
            public uint TextureWidth;
            public uint TextureHeight;
            public uint HasTexture;
            public Vector3 RestPosition;
            public uint Bone0;
            public uint Bone1;
            public uint Bone2;
            public uint Bone3;
            public Vector4 BoneWeights;
            public uint IsSkinned;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = ConstantBufferBytes)]
        private struct SceneConstants
        {
            public Matrix4x4 ViewProjection;
        }

        private sealed class TextureLocation
        {
            public uint Offset;
            public uint Width;
            public uint Height;
            public bool Valid;
        }

        private readonly IDXGIFactory4 _factory;
        private readonly ID3D12Device _device;
        private readonly ID3D12CommandQueue _queue;
        private readonly IDXGISwapChain3 _swapChain;
        private readonly ID3D12DescriptorHeap _rtvHeap;
        private readonly ID3D12DescriptorHeap _dsvHeap;
        private readonly ID3D12DescriptorHeap _srvHeap;
        private readonly ID3D12CommandAllocator[] _allocators = new ID3D12CommandAllocator[BufferCount];
        private readonly ID3D12Resource[] _backBuffers = new ID3D12Resource[BufferCount];
        private readonly ulong[] _frameFenceValues = new ulong[BufferCount];
        private readonly long[] _frameBoneMatrixVersions = new long[BufferCount];
        private readonly ID3D12Fence _fence;
        private readonly AutoResetEvent _fenceEvent = new AutoResetEvent(false);
        private readonly ID3D12RootSignature _rootSignature;
        private readonly ID3D12PipelineState _solidPipeline;
        private readonly ID3D12PipelineState _wireframePipeline;
        private readonly ID3D12PipelineState _texturedPipeline;
        private readonly ID3D12PipelineState _normalsPipeline;
        private readonly ID3D12PipelineState _skinningPipeline;
        private ID3D12PipelineState _deformationPipeline;
        private string _shaderSource;
        private ReadOnlyMemory<byte> _vertexShader;
        private readonly ID3D12GraphicsCommandList _commandList;
        private readonly ID3D12Resource[] _vertexBuffers = new ID3D12Resource[BufferCount];
        private readonly ID3D12Resource[] _boneMatrixBuffers = new ID3D12Resource[BufferCount];
        private readonly ID3D12Resource[] _constantBuffers = new ID3D12Resource[BufferCount];
        private readonly SceneConstants[] _sceneConstantsScratch = new SceneConstants[1];
        private readonly uint _rtvDescriptorSize;
        private readonly uint _srvDescriptorSize;
        private int _sampleCount = 1;
        private ID3D12Resource _msaaColorBuffer;
        private readonly Stopwatch _fpsClock = Stopwatch.StartNew();
        private readonly Stopwatch _renderClock = Stopwatch.StartNew();
        private long _fpsFrames;
        private long _fpsStartTicks = Stopwatch.GetTimestamp();
        private long _lastRenderTicks;
        private ulong _nextFenceValue;
        private int _width;
        private int _height;
        private ID3D12Resource _indexBuffer;
        private ID3D12Resource _texturePixelBuffer;
        private ID3D12Resource _depthBuffer;
        private Dx12SceneMesh[] _sceneMeshes = new Dx12SceneMesh[0];
        private int[] _sceneVertexOffsets = new int[0];
        private int[] _sceneVertexCounts = new int[0];
        private int[] _sceneIndexCounts = new int[0];
        private int[] _sceneBoneOffsets = new int[0];
        private int _boneMatrixCount;
        private Matrix4x4[] _boneMatrixScratch = new Matrix4x4[0];
        private Point3DCollection[] _positionCollections = new Point3DCollection[0];
        private Vector3DCollection[] _normalCollections = new Vector3DCollection[0];
        private PointCollection[] _uvCollections = new PointCollection[0];
        private Vector4[][] _vertexColorArrays = new Vector4[0][];
        private TextureLocation[] _textureLocations = new TextureLocation[0];
        private long[] _vertexBufferVersions = new long[BufferCount];
        private long _geometryVersion;
        private GpuVertex[] _vertexScratch = new GpuVertex[0];
        private uint[] _texturePixelScratch = new uint[] { 0xFFFFFFFFu };
        private int _vertexCount;
        private int _indexCount;
        private int _edgeCount;
        private int _faceCount;
        private int _meshCount;
        private int _drawCalls;
        private long _textureMemoryBytes;
        // These metrics are written by the dedicated render thread and read by the UI thread.
        // Use volatile publication for floating-point samples and interlocked counters so the
        // overlay never combines stale/partially published values from different render ticks.
        private double _fps;
        private double _frameTimeMilliseconds;
        private double _cpuRenderMilliseconds;
        private double _presentAndSignalMilliseconds;
        private double _submittedFrameIntervalMilliseconds;
        private double _renderCallRate;
        private long _skippedFrames;
        private long _renderCallCount;
        private long _submittedFrameCount;
        private long _renderRateWindowStartTicks = Stopwatch.GetTimestamp();
        private long _renderCallsInWindow;
        private long _lastSuccessfulSubmitTicks;
        private bool _disposed;

        public double Fps { get { return Volatile.Read(ref _fps); } }
        public double FrameTimeMilliseconds { get { return Volatile.Read(ref _frameTimeMilliseconds); } }
        public double CpuRenderMilliseconds { get { return Volatile.Read(ref _cpuRenderMilliseconds); } }
        public double PresentAndSignalMilliseconds { get { return Volatile.Read(ref _presentAndSignalMilliseconds); } }
        public double SubmittedFrameIntervalMilliseconds { get { return Volatile.Read(ref _submittedFrameIntervalMilliseconds); } }
        public double RenderCallRate { get { return Volatile.Read(ref _renderCallRate); } }
        public long RenderCallCount { get { return Interlocked.Read(ref _renderCallCount); } }
        public long SubmittedFrameCount { get { return Interlocked.Read(ref _submittedFrameCount); } }
        public long SkippedFrames { get { return Interlocked.Read(ref _skippedFrames); } }
        public int VertexCount { get { return _vertexCount; } }
        public int TriangleCount { get { return _indexCount / 3; } }
        public int EdgeCount { get { return _edgeCount; } }
        public int FaceCount { get { return _faceCount; } }
        public int MeshCount { get { return _meshCount; } }
        public int DrawCallCount { get { return _drawCalls; } }
        public long TextureMemoryBytes { get { return _textureMemoryBytes; } }
        public int MsaaSampleCount { get { return _sampleCount; } }

        public Dx12MeshRenderer(IntPtr hwnd, int width, int height)
        {
            try
            {
            if (hwnd == IntPtr.Zero) throw new ArgumentException("A valid WinForms HWND is required.", nameof(hwnd));
            _width = Math.Max(1, width);
            _height = Math.Max(1, height);

            _factory = AtInitializationStage("CreateDXGIFactory2", () => CreateDXGIFactory2<IDXGIFactory4>(false));
            _device = AtInitializationStage("D3D12CreateDevice", () => D3D12CreateDevice<ID3D12Device>(null, FeatureLevel.Level_11_0));
            _sampleCount = AtInitializationStage("Check multisample support", () => GetBestSupportedSampleCount());
            _queue = AtInitializationStage("Create direct command queue", () => _device.CreateCommandQueue(CommandListType.Direct));

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
            using (IDXGISwapChain1 swapChain1 = AtInitializationStage("CreateSwapChainForHwnd", () =>
                _factory.CreateSwapChainForHwnd(_queue, hwnd, swapChainDescription)))
                _swapChain = AtInitializationStage("QueryInterface IDXGISwapChain3", () => swapChain1.QueryInterface<IDXGISwapChain3>());
            AtInitializationActionStage("MakeWindowAssociation", () => { _factory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAltEnter); });

            // The swap chain stays single-sampled (required by flip-model DXGI). The extra RTV
            // belongs to an MSAA color target which is resolved into each back buffer per frame.
            _rtvHeap = AtInitializationStage("Create RTV descriptor heap", () => _device.CreateDescriptorHeap(new DescriptorHeapDescription(DescriptorHeapType.RenderTargetView, BufferCount + 1)));
            _dsvHeap = AtInitializationStage("Create DSV descriptor heap", () => _device.CreateDescriptorHeap(new DescriptorHeapDescription(DescriptorHeapType.DepthStencilView, 1)));
            // Each frame owns a contiguous [texture SRV, bone-matrix SRV] pair so the vertex and pixel
            // stages can read frame-safe resources without rewriting descriptors still in flight.
            _srvHeap = AtInitializationStage("Create SRV descriptor heap", () => _device.CreateDescriptorHeap(new DescriptorHeapDescription(
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView, BufferCount * 2, DescriptorHeapFlags.ShaderVisible)));
            _rtvDescriptorSize = AtInitializationStage("Get RTV descriptor increment", () => _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView));
            _srvDescriptorSize = AtInitializationStage("Get SRV descriptor increment", () => _device.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView));
            AtInitializationActionStage("Create command allocators", () =>
            {
                for (int i = 0; i < BufferCount; i++)
                    _allocators[i] = _device.CreateCommandAllocator(CommandListType.Direct);
            });

            const string shaderResourceName = "PrototypeUltimateTool.Shaders.Dx12Wireframe.hlsl";
            _shaderSource = AtInitializationStage("Read embedded HLSL shader source", () =>
            {
                System.Reflection.Assembly assembly = typeof(Dx12MeshRenderer).Assembly;
                using System.IO.Stream shaderStream = assembly.GetManifestResourceStream(shaderResourceName)
                    ?? throw new InvalidOperationException($"Embedded DX12 shader resource '{shaderResourceName}' was not found in {assembly.GetName().Name}.");
                using var shaderReader = new System.IO.StreamReader(shaderStream);
                return shaderReader.ReadToEnd();
            });
            _vertexShader = AtInitializationStage("Compile VSMain (vs_5_1)", () => Compiler.Compile(_shaderSource, "VSMain", "Dx12Wireframe.hlsl", "vs_5_1", ShaderFlags.OptimizationLevel3));
            ReadOnlyMemory<byte> solidShader = AtInitializationStage("Compile PSSolid (ps_5_1)", () => Compiler.Compile(_shaderSource, "PSSolid", "Dx12Wireframe.hlsl", "ps_5_1", ShaderFlags.OptimizationLevel3));
            ReadOnlyMemory<byte> texturedShader = AtInitializationStage("Compile PSTextured (ps_5_1)", () => Compiler.Compile(_shaderSource, "PSTextured", "Dx12Wireframe.hlsl", "ps_5_1", ShaderFlags.OptimizationLevel3));
            ReadOnlyMemory<byte> normalsShader = AtInitializationStage("Compile PSNormals (ps_5_1)", () => Compiler.Compile(_shaderSource, "PSNormals", "Dx12Wireframe.hlsl", "ps_5_1", ShaderFlags.OptimizationLevel3));
            ReadOnlyMemory<byte> skinningShader = AtInitializationStage("Compile PSSkinning (ps_5_1)", () => Compiler.Compile(_shaderSource, "PSSkinning", "Dx12Wireframe.hlsl", "ps_5_1", ShaderFlags.OptimizationLevel3));
            ReadOnlyMemory<byte> vertexShader = _vertexShader;

            var textureRange = new DescriptorRange1(DescriptorRangeType.ShaderResourceView, 2, 0);
            var textureTable = new RootDescriptorTable1(textureRange);
            var rootParameters = new[]
            {
                new RootParameter1(RootParameterType.ConstantBufferView, new RootDescriptor1(0, 0), ShaderVisibility.All),
                new RootParameter1(textureTable, ShaderVisibility.All)
            };
            _rootSignature = AtInitializationStage("Create root signature", () => _device.CreateRootSignature(new RootSignatureDescription1(
                RootSignatureFlags.AllowInputAssemblerInputLayout,
                rootParameters,
                Array.Empty<StaticSamplerDescription>())));

            _solidPipeline = AtInitializationStage("Create Solid graphics PSO", () => CreatePipeline(vertexShader, solidShader, FillMode.Solid));
            _wireframePipeline = AtInitializationStage("Create Wireframe graphics PSO", () => CreatePipeline(vertexShader, solidShader, FillMode.Wireframe));
            _texturedPipeline = AtInitializationStage("Create Textured graphics PSO", () => CreatePipeline(vertexShader, texturedShader, FillMode.Solid));
            _normalsPipeline = AtInitializationStage("Create Normals graphics PSO", () => CreatePipeline(vertexShader, normalsShader, FillMode.Solid));
            _skinningPipeline = AtInitializationStage("Create Skinning graphics PSO", () => CreatePipeline(vertexShader, skinningShader, FillMode.Solid));

            AtInitializationActionStage("Create per-frame constant buffers", () =>
            {
                for (int i = 0; i < BufferCount; i++)
                    _constantBuffers[i] = _device.CreateCommittedResource(
                        HeapType.Upload, ResourceDescription.Buffer(ConstantBufferBytes), ResourceStates.GenericRead);
            });

            _commandList = AtInitializationStage("Create graphics command list", () =>
                _device.CreateCommandList<ID3D12GraphicsCommandList>(CommandListType.Direct, _allocators[0], _solidPipeline));
            AtInitializationActionStage("Close initial command list", () => { _commandList.Close(); });
            _fence = AtInitializationStage("Create GPU fence", () => _device.CreateFence(0));
            AtInitializationActionStage("Create texture pixel buffer", () => { CreateTexturePixelBuffer(_texturePixelScratch); });
            AtInitializationActionStage("Create initial bone matrix buffers", () => { CreateBoneMatrixBuffers(0); });
            AtInitializationActionStage("Create swap-chain render targets", () => { CreateRenderTargets(); });
            AtInitializationActionStage("Create multisample color target", () => { CreateMsaaColorBuffer(); });
            AtInitializationActionStage("Create depth-stencil buffer", () => { CreateDepthBuffer(); });
            }
            catch
            {
                DisposePartialConstruction();
                throw;
            }
        }

        public void Render(
            IReadOnlyList<Dx12SceneMesh> meshes,
            Dx12ShadingMode shadingMode,
            double yaw,
            double pitch,
            double distance,
            double targetX,
            double targetY,
            double targetZ,
            double boundingRadius)
        {
            ThrowIfDisposed();
            long cpuStartTicks = Stopwatch.GetTimestamp();
            long nowTicks = cpuStartTicks;
            if (_lastRenderTicks != 0)
                Volatile.Write(ref _frameTimeMilliseconds, (nowTicks - _lastRenderTicks) * 1000.0 / Stopwatch.Frequency);
            _lastRenderTicks = nowTicks;
            Interlocked.Increment(ref _renderCallCount);
            UpdateRenderCallRate(nowTicks);

            int frameIndex = (int)_swapChain.CurrentBackBufferIndex;
            // Never block the WinForms UI thread waiting for a frame resource. If the GPU has
            // not retired this back buffer yet, skip this tick and let a later tick retry.
            // This preserves frame-resource safety without freezing the rest of the editor.
            ulong frameFenceValue = _frameFenceValues[frameIndex];
            if (frameFenceValue != 0 && _fence.CompletedValue < frameFenceValue)
            {
                Interlocked.Increment(ref _skippedFrames);
                return;
            }
            EnsureScene(meshes);
            UpdateBoneMatrixBuffer(frameIndex);
            ID3D12PipelineState pipeline = GetPipeline(shadingMode);

            if (_vertexCount > 0 && _indexCount > 0 && _vertexBuffers[frameIndex] != null &&
                _vertexBufferVersions[frameIndex] != _geometryVersion)
            {
                UpdateDynamicVertices();
                _vertexBuffers[frameIndex].SetData<GpuVertex>(_vertexScratch);
                _vertexBufferVersions[frameIndex] = _geometryVersion;
            }

            _allocators[frameIndex].Reset();
            _commandList.Reset(_allocators[frameIndex], pipeline);

            double radYaw = yaw * Math.PI / 180.0;
            double radPitch = pitch * Math.PI / 180.0;
            float eyeX = (float)(targetX + distance * Math.Cos(radPitch) * Math.Sin(radYaw));
            float eyeY = (float)(targetY + distance * Math.Cos(radPitch) * Math.Cos(radYaw));
            float eyeZ = (float)(targetZ + distance * Math.Sin(radPitch));
            var eye = new Vector3(eyeX, eyeY, eyeZ);
            var target = new Vector3((float)targetX, (float)targetY, (float)targetZ);
            var view = Matrix4x4.CreateLookAt(eye, target, Vector3.UnitZ);
            float aspect = _width / (float)Math.Max(1, _height);

            // Keep the depth range tightly around the current scene. The former far plane
            // (distance * 8 + radius * 2) wastes most of D32 depth precision when zoomed out.
            // Grow the near plane with camera distance whenever the scene bounds allow it.
            double safeDistance = Math.Max(0.01, distance);
            double safeRadius = Math.Max(0.0, boundingRadius);
            double nearestBoundDistance = safeDistance - safeRadius;
            double nearCandidate = safeDistance * 0.02;
            if (nearestBoundDistance > 0.01)
                nearCandidate = Math.Min(nearCandidate, nearestBoundDistance * 0.5);
            else
                nearCandidate = Math.Min(nearCandidate, Math.Max(0.001, safeDistance * 0.001));
            float nearPlane = (float)Math.Max(0.001, nearCandidate);

            double farCandidate = safeDistance + safeRadius * 1.25 + Math.Max(1.0, safeRadius * 0.05);
            float farPlane = (float)Math.Max(1000.0, farCandidate);
            if (farPlane <= nearPlane + 1.0f) farPlane = nearPlane + 1000.0f;

            // Keep the same horizontal 45-degree field of view as the former WPF PerspectiveCamera.
            // System.Numerics.CreatePerspectiveFieldOfView expects a VERTICAL FOV, so convert it
            // for the current aspect ratio. Use reverse-Z (near -> 1, far -> 0) with D32_FLOAT
            // to preserve depth precision when a very large camera range is unavoidable.
            const double horizontalFovRadians = Math.PI / 4.0;
            double verticalFovRadians = 2.0 * Math.Atan(
                Math.Tan(horizontalFovRadians * 0.5) / Math.Max(0.0001f, aspect));
            var projection = CreateReverseZPerspectiveFieldOfView(
                (float)verticalFovRadians, aspect, nearPlane, farPlane);
            _sceneConstantsScratch[0] = new SceneConstants { ViewProjection = view * projection };
            _constantBuffers[frameIndex].SetData<SceneConstants>(_sceneConstantsScratch);

            ID3D12Resource backBuffer = _backBuffers[frameIndex];
            CpuDescriptorHandle backBufferRtv = GetRtvHandle(frameIndex);
            CpuDescriptorHandle rtvHandle = _sampleCount > 1 ? GetRtvHandle(BufferCount) : backBufferRtv;
            CpuDescriptorHandle dsvHandle = _dsvHeap.GetCPUDescriptorHandleForHeapStart();
            if (_sampleCount <= 1)
                _commandList.ResourceBarrierTransition(backBuffer, ResourceStates.Present, ResourceStates.RenderTarget);
            _commandList.OMSetRenderTargets(rtvHandle, dsvHandle);
            _commandList.ClearRenderTargetView(rtvHandle, new Color4(0.055f, 0.067f, 0.09f, 1.0f));
            // Reverse-Z uses 0.0 as the farthest depth value.
            _commandList.ClearDepthStencilView(dsvHandle, ClearFlags.Depth, 0.0f, 0);
            _commandList.RSSetViewport(new Viewport(0, 0, _width, _height, 0.0f, 1.0f));
            _commandList.RSSetScissorRect(new RectI(0, 0, _width, _height));
            _commandList.SetGraphicsRootSignature(_rootSignature);
            _commandList.SetGraphicsRootConstantBufferView(0, _constantBuffers[frameIndex].GPUVirtualAddress);
            _commandList.SetDescriptorHeaps(new[] { _srvHeap });
            _commandList.SetGraphicsRootDescriptorTable(1, GetSrvGpuHandle(frameIndex * 2));

            if (_vertexCount > 0 && _indexCount > 0 && _indexBuffer != null && _vertexBuffers[frameIndex] != null)
            {
                _commandList.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
                _commandList.IASetVertexBuffers(0, new VertexBufferView
                {
                    BufferLocation = _vertexBuffers[frameIndex].GPUVirtualAddress,
                    SizeInBytes = checked((uint)(Marshal.SizeOf<GpuVertex>() * _vertexCount)),
                    StrideInBytes = (uint)Marshal.SizeOf<GpuVertex>()
                });
                _commandList.IASetIndexBuffer(new IndexBufferView
                {
                    BufferLocation = _indexBuffer.GPUVirtualAddress,
                    SizeInBytes = checked((uint)(_indexCount * sizeof(uint))),
                    Format = Format.R32_UInt
                });
                _commandList.DrawIndexedInstanced((uint)_indexCount, 1, 0, 0, 0);
            }

            if (_sampleCount > 1 && _msaaColorBuffer != null)
            {
                _commandList.ResourceBarrierTransition(_msaaColorBuffer, ResourceStates.RenderTarget, ResourceStates.ResolveSource);
                _commandList.ResourceBarrierTransition(backBuffer, ResourceStates.Present, ResourceStates.ResolveDest);
                _commandList.ResolveSubresource(backBuffer, 0, _msaaColorBuffer, 0, Format.R8G8B8A8_UNorm);
                _commandList.ResourceBarrierTransition(backBuffer, ResourceStates.ResolveDest, ResourceStates.Present);
                _commandList.ResourceBarrierTransition(_msaaColorBuffer, ResourceStates.ResolveSource, ResourceStates.RenderTarget);
            }
            else
            {
                _commandList.ResourceBarrierTransition(backBuffer, ResourceStates.RenderTarget, ResourceStates.Present);
            }
            _commandList.Close();
            _queue.ExecuteCommandList(_commandList);
            Volatile.Write(ref _cpuRenderMilliseconds, (Stopwatch.GetTimestamp() - cpuStartTicks) * 1000.0 / Stopwatch.Frequency);
            long presentStartTicks = Stopwatch.GetTimestamp();
            // Rendering is paced by the dedicated worker. Keep Present non-blocking so UI and
            // frame timing do not inherit a second wait for vertical sync.
            _swapChain.Present(0, PresentFlags.None);

            ulong submittedFenceValue = ++_nextFenceValue;
            _queue.Signal(_fence, submittedFenceValue);
            _frameFenceValues[frameIndex] = submittedFenceValue;
            Volatile.Write(ref _presentAndSignalMilliseconds, (Stopwatch.GetTimestamp() - presentStartTicks) * 1000.0 / Stopwatch.Frequency);
            long submittedTicks = Stopwatch.GetTimestamp();
            long previousSubmittedTicks = Interlocked.Exchange(ref _lastSuccessfulSubmitTicks, submittedTicks);
            if (previousSubmittedTicks != 0)
                Volatile.Write(ref _submittedFrameIntervalMilliseconds,
                    (submittedTicks - previousSubmittedTicks) * 1000.0 / Stopwatch.Frequency);
            Interlocked.Increment(ref _submittedFrameCount);
            UpdateFps(submittedTicks);
        }

        public void Resize(int width, int height)
        {
            if (_disposed || width <= 0 || height <= 0 || (width == _width && height == _height)) return;
            WaitForGpu();
            for (int i = 0; i < BufferCount; i++)
            {
                if (_backBuffers[i] != null) _backBuffers[i].Dispose();
                _backBuffers[i] = null;
            }
            if (_depthBuffer != null) _depthBuffer.Dispose();
            _depthBuffer = null;
            if (_msaaColorBuffer != null) _msaaColorBuffer.Dispose();
            _msaaColorBuffer = null;
            _width = width;
            _height = height;
            _swapChain.ResizeBuffers(BufferCount, (uint)_width, (uint)_height, Format.R8G8B8A8_UNorm);
            Array.Clear(_frameFenceValues, 0, _frameFenceValues.Length);
            CreateRenderTargets();
            CreateMsaaColorBuffer();
            CreateDepthBuffer();
        }

        private static void SafeDispose(IDisposable resource)
        {
            if (resource == null) return;
            try { resource.Dispose(); } catch { }
        }

        private void DisposePartialConstruction()
        {
            _disposed = true;
            SafeDispose(_texturePixelBuffer); _texturePixelBuffer = null;
            SafeDispose(_depthBuffer); _depthBuffer = null;
            SafeDispose(_msaaColorBuffer); _msaaColorBuffer = null;
            for (int i = 0; i < BufferCount; i++)
            {
                SafeDispose(_vertexBuffers[i]); _vertexBuffers[i] = null;
                SafeDispose(_boneMatrixBuffers[i]); _boneMatrixBuffers[i] = null;
                SafeDispose(_backBuffers[i]); _backBuffers[i] = null;
                SafeDispose(_allocators[i]); _allocators[i] = null;
                SafeDispose(_constantBuffers[i]); _constantBuffers[i] = null;
            }
            SafeDispose(_indexBuffer); _indexBuffer = null;
            SafeDispose(_commandList);
            SafeDispose(_deformationPipeline);
            SafeDispose(_skinningPipeline);
            SafeDispose(_normalsPipeline);
            SafeDispose(_texturedPipeline);
            SafeDispose(_wireframePipeline);
            SafeDispose(_solidPipeline);
            SafeDispose(_srvHeap);
            SafeDispose(_dsvHeap);
            SafeDispose(_rtvHeap);
            SafeDispose(_rootSignature);
            SafeDispose(_fence);
            SafeDispose(_swapChain);
            SafeDispose(_queue);
            SafeDispose(_device);
            SafeDispose(_factory);
            SafeDispose(_fenceEvent);
        }

        private static T AtInitializationStage<T>(string stage, Func<T> action)
        {
            try { return action(); }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Direct3D 12 initialization stage '" + stage + "' failed.", ex);
            }
        }

        private static void AtInitializationActionStage(string stage, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Direct3D 12 initialization stage '" + stage + "' failed.", ex);
            }
        }

        private ID3D12PipelineState GetPipeline(Dx12ShadingMode mode)
        {
            switch (mode)
            {
                case Dx12ShadingMode.Wireframe: return _wireframePipeline;
                case Dx12ShadingMode.Textured: return _texturedPipeline;
                case Dx12ShadingMode.Normals: return _normalsPipeline;
                case Dx12ShadingMode.Skinning: return _skinningPipeline;
                case Dx12ShadingMode.Deformation: return EnsureDeformationPipeline();
                default: return _solidPipeline;
            }
        }

        private ID3D12PipelineState EnsureDeformationPipeline()
        {
            if (_deformationPipeline != null) return _deformationPipeline;

            ReadOnlyMemory<byte> geometryShader = AtInitializationStage("Compile GSDeformation (gs_5_1)",
                () => Compiler.Compile(_shaderSource, "GSDeformation", "Dx12Wireframe.hlsl", "gs_5_1", ShaderFlags.OptimizationLevel3));
            ReadOnlyMemory<byte> pixelShader = AtInitializationStage("Compile PSDeformation (ps_5_1)",
                () => Compiler.Compile(_shaderSource, "PSDeformation", "Dx12Wireframe.hlsl", "ps_5_1", ShaderFlags.OptimizationLevel3));
            _deformationPipeline = AtInitializationStage("Create Deformation debug graphics PSO",
                () => CreatePipeline(_vertexShader, pixelShader, FillMode.Solid, geometryShader));
            return _deformationPipeline;
        }

        private void EnsureScene(IReadOnlyList<Dx12SceneMesh> requestedMeshes)
        {
            var renderable = new List<Dx12SceneMesh>(requestedMeshes == null ? 0 : requestedMeshes.Count);
            if (requestedMeshes != null)
            {
                for (int i = 0; i < requestedMeshes.Count; i++)
                {
                    Dx12SceneMesh item = requestedMeshes[i];
                    if (item != null && item.Mesh != null && item.Mesh.Positions != null && item.Mesh.Positions.Count > 0 &&
                        item.Mesh.TriangleIndices != null && item.Mesh.TriangleIndices.Count >= 3)
                        renderable.Add(item);
                }
            }

            bool topologyUnchanged = renderable.Count == _sceneMeshes.Length;
            bool vertexDataChanged = false;
            bool texturesChanged = false;
            if (topologyUnchanged)
            {
                for (int i = 0; i < renderable.Count; i++)
                {
                    Dx12SceneMesh current = renderable[i];
                    Dx12SceneMesh previous = _sceneMeshes[i];
                    int currentBoneCount = current.BoneMatrices == null ? 0 : current.BoneMatrices.Length;
                    int previousBoneCount = previous.BoneMatrices == null ? 0 : previous.BoneMatrices.Length;
                    if (!ReferenceEquals(current.Key, previous.Key) ||
                        current.Mesh.Positions.Count != _sceneVertexCounts[i] ||
                        current.Mesh.TriangleIndices.Count != _sceneIndexCounts[i] ||
                        currentBoneCount != previousBoneCount)
                    {
                        topologyUnchanged = false;
                        break;
                    }
                    if (!ReferenceEquals(current.Mesh, previous.Mesh) ||
                        !ReferenceEquals(current.Mesh.Positions, _positionCollections[i]) ||
                        !ReferenceEquals(current.Mesh.Normals, _normalCollections[i]) ||
                        !ReferenceEquals(current.Mesh.TextureCoordinates, _uvCollections[i]) ||
                        !ReferenceEquals(current.VertexColors, _vertexColorArrays[i]) ||
                        !ReferenceEquals(current.SkinBoneIndices, previous.SkinBoneIndices) ||
                        !ReferenceEquals(current.SkinWeights, previous.SkinWeights) ||
                        !ReferenceEquals(current.SkinUsesRigidBoneOrigin, previous.SkinUsesRigidBoneOrigin) ||
                        current.HasGpuSkinning != previous.HasGpuSkinning ||
                        !current.Transform.Equals(previous.Transform) || !current.BaseColor.Equals(previous.BaseColor))
                        vertexDataChanged = true;
                    if (!ReferenceEquals(current.Texture, previous.Texture)) texturesChanged = true;
                }
            }

            if (topologyUnchanged)
            {
                for (int i = 0; i < renderable.Count; i++)
                {
                    Dx12SceneMesh current = renderable[i];
                    _sceneMeshes[i] = current;
                    // Update the snapshot after detecting a changed collection. Without this, an
                    // animated collection was considered dirty on every render tick until replaced again.
                    _positionCollections[i] = current.Mesh.Positions;
                    _normalCollections[i] = current.Mesh.Normals;
                    _uvCollections[i] = current.Mesh.TextureCoordinates;
                    _vertexColorArrays[i] = current.VertexColors;
                }
                if (texturesChanged)
                {
                    WaitForGpu();
                    BuildTexturePixelBuffer();
                    vertexDataChanged = true;
                }
                if (vertexDataChanged) _geometryVersion++;
                _meshCount = renderable.Count;
                _drawCalls = _indexCount > 0 && _vertexCount > 0 ? 1 : 0;
                return;
            }

            WaitForGpu();
            DisposeSceneBuffers();
            DisposeBoneMatrixBuffers();
            _sceneMeshes = renderable.ToArray();
            _sceneVertexOffsets = new int[_sceneMeshes.Length];
            _sceneVertexCounts = new int[_sceneMeshes.Length];
            _sceneIndexCounts = new int[_sceneMeshes.Length];
            _sceneBoneOffsets = new int[_sceneMeshes.Length];
            _positionCollections = new Point3DCollection[_sceneMeshes.Length];
            _normalCollections = new Vector3DCollection[_sceneMeshes.Length];
            _uvCollections = new PointCollection[_sceneMeshes.Length];
            _vertexColorArrays = new Vector4[_sceneMeshes.Length][];
            _textureLocations = new TextureLocation[_sceneMeshes.Length];
            _vertexBufferVersions = new long[BufferCount];

            _vertexCount = 0;
            int potentialIndices = 0;
            for (int i = 0; i < _sceneMeshes.Length; i++)
            {
                MeshGeometry3D mesh = _sceneMeshes[i].Mesh;
                _sceneVertexOffsets[i] = _vertexCount;
                _sceneVertexCounts[i] = mesh.Positions.Count;
                _sceneIndexCounts[i] = mesh.TriangleIndices.Count;
                _positionCollections[i] = mesh.Positions;
                _normalCollections[i] = mesh.Normals;
                _uvCollections[i] = mesh.TextureCoordinates;
                _vertexColorArrays[i] = _sceneMeshes[i].VertexColors;
                _vertexCount += _sceneVertexCounts[i];
                potentialIndices += _sceneIndexCounts[i];
            }

            BuildTexturePixelBuffer();
            _vertexScratch = new GpuVertex[_vertexCount];
            var indexList = new List<uint>(potentialIndices);
            var edgeCount = 0;
            var faceCount = 0;
            for (int meshIndex = 0; meshIndex < _sceneMeshes.Length; meshIndex++)
            {
                MeshGeometry3D mesh = _sceneMeshes[meshIndex].Mesh;
                int offset = _sceneVertexOffsets[meshIndex];
                Int32Collection indices = mesh.TriangleIndices;
                var uniqueEdges = new HashSet<long>();
                for (int i = 0; i + 2 < indices.Count; i += 3)
                {
                    int a = indices[i];
                    int b = indices[i + 1];
                    int c = indices[i + 2];
                    if (a < 0 || b < 0 || c < 0 || a >= mesh.Positions.Count || b >= mesh.Positions.Count || c >= mesh.Positions.Count)
                        continue;
                    indexList.Add((uint)(offset + a));
                    indexList.Add((uint)(offset + b));
                    indexList.Add((uint)(offset + c));
                    AddEdge(uniqueEdges, a, b);
                    AddEdge(uniqueEdges, b, c);
                    AddEdge(uniqueEdges, c, a);
                    faceCount++;
                }
                edgeCount += uniqueEdges.Count;
            }
            _boneMatrixCount = 0;
            for (int i = 0; i < _sceneMeshes.Length; i++)
            {
                _sceneBoneOffsets[i] = _boneMatrixCount;
                Matrix4x4[] matrices = _sceneMeshes[i].BoneMatrices;
                if (matrices != null) _boneMatrixCount = checked(_boneMatrixCount + matrices.Length);
            }
            CreateBoneMatrixBuffers(_boneMatrixCount);

            _indexCount = indexList.Count;
            _edgeCount = edgeCount;
            _faceCount = faceCount;
            _meshCount = _sceneMeshes.Length;
            _drawCalls = _indexCount > 0 && _vertexCount > 0 ? 1 : 0;
            _geometryVersion++;

            if (_vertexCount <= 0 || _indexCount <= 0)
            {
                _vertexCount = Math.Max(0, _vertexCount);
                _vertexScratch = new GpuVertex[_vertexCount];
                return;
            }

            for (int frame = 0; frame < BufferCount; frame++)
            {
                ulong vertexBytes = (ulong)(Marshal.SizeOf<GpuVertex>() * _vertexCount);
                _vertexBuffers[frame] = _device.CreateCommittedResource(
                    HeapType.Upload, ResourceDescription.Buffer(vertexBytes), ResourceStates.GenericRead);
            }
            ulong indexBytes = (ulong)(sizeof(uint) * _indexCount);
            _indexBuffer = _device.CreateCommittedResource(
                HeapType.Upload, ResourceDescription.Buffer(indexBytes), ResourceStates.GenericRead);
            _indexBuffer.SetData<uint>(indexList.ToArray());
        }

        private static void AddEdge(HashSet<long> edges, int a, int b)
        {
            if (a == b) return;
            int low = Math.Min(a, b);
            int high = Math.Max(a, b);
            edges.Add(((long)low << 32) | (uint)high);
        }

        private void BuildTexturePixelBuffer()
        {
            var pixels = new List<uint> { 0xFFFFFFFFu };
            var locations = new Dictionary<BitmapSource, TextureLocation>();
            _textureLocations = new TextureLocation[_sceneMeshes.Length];
            for (int i = 0; i < _sceneMeshes.Length; i++)
            {
                BitmapSource texture = _sceneMeshes[i].Texture;
                TextureLocation location;
                if (texture == null)
                {
                    _textureLocations[i] = new TextureLocation { Offset = 0, Width = 1, Height = 1, Valid = false };
                    continue;
                }
                if (!locations.TryGetValue(texture, out location))
                {
                    location = TryAppendTexturePixels(texture, pixels);
                    locations[texture] = location;
                }
                _textureLocations[i] = location;
            }

            _texturePixelScratch = pixels.ToArray();
            _textureMemoryBytes = (long)_texturePixelScratch.Length * sizeof(uint);
            CreateTexturePixelBuffer(_texturePixelScratch);
        }

        private TextureLocation TryAppendTexturePixels(BitmapSource original, List<uint> target)
        {
            var invalid = new TextureLocation { Offset = 0, Width = 1, Height = 1, Valid = false };
            try
            {
                BitmapSource source = original;
                double scale = Math.Min(1.0, Math.Min((double)MaxTextureDimension / Math.Max(1, source.PixelWidth),
                    (double)MaxTextureDimension / Math.Max(1, source.PixelHeight)));
                if (scale < 1.0)
                    source = new TransformedBitmap(source, new ScaleTransform(scale, scale));
                if (source.Format != PixelFormats.Bgra32)
                    source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0.0);

                int width = source.PixelWidth;
                int height = source.PixelHeight;
                if (width <= 0 || height <= 0 || (long)target.Count + (long)width * height > MaxTexturePixels)
                    return invalid;

                int stride = checked(width * 4);
                byte[] bytes = new byte[checked(stride * height)];
                source.CopyPixels(bytes, stride, 0);
                var location = new TextureLocation
                {
                    Offset = (uint)target.Count,
                    Width = (uint)width,
                    Height = (uint)height,
                    Valid = true
                };
                for (int i = 0; i < bytes.Length; i += 4)
                {
                    uint r = bytes[i + 2];
                    uint g = bytes[i + 1];
                    uint b = bytes[i];
                    uint a = bytes[i + 3];
                    target.Add(r | (g << 8) | (b << 16) | (a << 24));
                }
                return location;
            }
            catch
            {
                return invalid;
            }
        }

        private void CreateTexturePixelBuffer(uint[] pixels)
        {
            if (_texturePixelBuffer != null)
            {
                _texturePixelBuffer.Dispose();
                _texturePixelBuffer = null;
            }
            if (pixels == null || pixels.Length == 0) pixels = new uint[] { 0xFFFFFFFFu };
            _texturePixelBuffer = _device.CreateCommittedResource(
                HeapType.Upload,
                ResourceDescription.Buffer((ulong)pixels.Length * sizeof(uint)),
                ResourceStates.GenericRead);
            _texturePixelBuffer.SetData<uint>(pixels);
            var srv = new ShaderResourceViewDescription
            {
                Format = Format.Unknown,
                ViewDimension = Vortice.Direct3D12.ShaderResourceViewDimension.Buffer,
                // D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING from d3d12.h (encoded RGBA identity mapping).
                Shader4ComponentMapping = 5768,
                Buffer = new BufferShaderResourceView
                {
                    FirstElement = 0,
                    NumElements = (uint)pixels.Length,
                    StructureByteStride = sizeof(uint),
                    Flags = BufferShaderResourceViewFlags.None
                }
            };
            for (int frame = 0; frame < BufferCount; frame++)
                _device.CreateShaderResourceView(_texturePixelBuffer, srv, GetSrvCpuHandle(frame * 2));
        }

        private void CreateBoneMatrixShaderResourceView(int frameIndex, ID3D12Resource buffer, int matrixCount)
        {
            var description = new ShaderResourceViewDescription
            {
                Format = Format.Unknown,
                ViewDimension = Vortice.Direct3D12.ShaderResourceViewDimension.Buffer,
                Shader4ComponentMapping = 5768,
                Buffer = new BufferShaderResourceView
                {
                    FirstElement = 0,
                    NumElements = (uint)Math.Max(1, matrixCount),
                    StructureByteStride = (uint)Marshal.SizeOf<Matrix4x4>(),
                    Flags = BufferShaderResourceViewFlags.None
                }
            };
            _device.CreateShaderResourceView(buffer, description, GetSrvCpuHandle(frameIndex * 2 + 1));
        }

        private CpuDescriptorHandle GetSrvCpuHandle(int index)
        {
            return new CpuDescriptorHandle(_srvHeap.GetCPUDescriptorHandleForHeapStart(), index, _srvDescriptorSize);
        }

        private GpuDescriptorHandle GetSrvGpuHandle(int index)
        {
            return new GpuDescriptorHandle(_srvHeap.GetGPUDescriptorHandleForHeapStart(), index, _srvDescriptorSize);
        }

        private void UpdateBoneMatrixBuffer(int frameIndex)
        {
            // Each mesh increments its revision only when the CPU-computed joint pose changes.
            // Avoid uploading the whole matrix palette on render ticks between animation updates.
            long signature = unchecked((long)1469598103934665603UL);
            for (int i = 0; i < _sceneMeshes.Length; i++)
            {
                signature = unchecked((signature ^ _sceneMeshes[i].BoneMatrixVersion) * 1099511628211L);
                int length = _sceneMeshes[i].BoneMatrices == null ? 0 : _sceneMeshes[i].BoneMatrices.Length;
                signature = unchecked((signature ^ length) * 1099511628211L);
            }
            if (_frameBoneMatrixVersions[frameIndex] == signature) return;

            if (_boneMatrixScratch == null || _boneMatrixScratch.Length == 0)
                _boneMatrixScratch = new[] { Matrix4x4.Identity };
            Array.Fill(_boneMatrixScratch, Matrix4x4.Identity);
            for (int meshIndex = 0; meshIndex < _sceneMeshes.Length; meshIndex++)
            {
                Matrix4x4[] matrices = _sceneMeshes[meshIndex].BoneMatrices;
                if (matrices == null || matrices.Length == 0) continue;
                int offset = _sceneBoneOffsets[meshIndex];
                int copyCount = Math.Min(matrices.Length, _boneMatrixScratch.Length - offset);
                if (copyCount > 0) Array.Copy(matrices, 0, _boneMatrixScratch, offset, copyCount);
            }
            if (_boneMatrixBuffers[frameIndex] != null)
            {
                _boneMatrixBuffers[frameIndex].SetData<Matrix4x4>(_boneMatrixScratch);
                _frameBoneMatrixVersions[frameIndex] = signature;
            }
        }

        private static bool IsRenderableMesh(Dx12SceneMesh mesh)
        {
            return mesh != null && mesh.Mesh != null && mesh.Mesh.Positions != null && mesh.Mesh.Positions.Count > 0 &&
                mesh.Mesh.TriangleIndices != null && mesh.Mesh.TriangleIndices.Count >= 3;
        }

        private void UpdateDynamicVertices()
        {
            for (int meshIndex = 0; meshIndex < _sceneMeshes.Length; meshIndex++)
            {
                Dx12SceneMesh sceneMesh = _sceneMeshes[meshIndex];
                MeshGeometry3D mesh = sceneMesh.Mesh;
                int offset = _sceneVertexOffsets[meshIndex];
                int count = Math.Min(_sceneVertexCounts[meshIndex], mesh.Positions.Count);
                bool hasNormals = mesh.Normals != null && mesh.Normals.Count == mesh.Positions.Count;
                bool hasUvs = mesh.TextureCoordinates != null && mesh.TextureCoordinates.Count == mesh.Positions.Count;
                TextureLocation textureLocation = _textureLocations != null && meshIndex < _textureLocations.Length
                    ? _textureLocations[meshIndex] : null;
                bool hasTexture = textureLocation != null && textureLocation.Valid;
                Vector4[] vertexColors = sceneMesh.VertexColors;
                bool hasVertexColors = vertexColors != null && vertexColors.Length == mesh.Positions.Count;
                bool gpuSkinning = sceneMesh.HasGpuSkinning && sceneMesh.BoneMatrices != null && sceneMesh.BoneMatrices.Length >= 2;
                int jointCount = gpuSkinning ? sceneMesh.BoneMatrices.Length / 2 : 0;
                int bonePaletteOffset = _sceneBoneOffsets != null && meshIndex < _sceneBoneOffsets.Length ? _sceneBoneOffsets[meshIndex] : 0;

                for (int i = 0; i < count; i++)
                {
                    Point3D bindPoint = mesh.Positions[i];
                    bool vertexSkinned = false;
                    uint bone0 = 0, bone1 = 0, bone2 = 0, bone3 = 0;
                    Vector4 weights = Vector4.Zero;
                    if (gpuSkinning && sceneMesh.SkinBoneIndices != null && sceneMesh.SkinWeights != null &&
                        i < sceneMesh.SkinBoneIndices.Length && i < sceneMesh.SkinWeights.Length)
                    {
                        int[] boneIndices = sceneMesh.SkinBoneIndices[i];
                        double[] sourceWeights = sceneMesh.SkinWeights[i];
                        bool rigid = sceneMesh.SkinUsesRigidBoneOrigin != null && i < sceneMesh.SkinUsesRigidBoneOrigin.Length && sceneMesh.SkinUsesRigidBoneOrigin[i];
                        float weight0 = 0.0f, weight1 = 0.0f, weight2 = 0.0f, weight3 = 0.0f;
                        double totalWeight = 0.0;
                        for (int influence = 0; influence < 4; influence++)
                        {
                            if (boneIndices == null || sourceWeights == null || influence >= boneIndices.Length || influence >= sourceWeights.Length)
                                continue;
                            int boneIndex = boneIndices[influence];
                            double weight = sourceWeights[influence];
                            if (boneIndex < 0 || boneIndex >= jointCount || weight <= 0.000001 || double.IsNaN(weight) || double.IsInfinity(weight))
                                continue;
                            uint packedBone = checked((uint)(bonePaletteOffset + boneIndex + (rigid ? jointCount : 0)));
                            float packedWeight = (float)weight;
                            switch (influence)
                            {
                                case 0: bone0 = packedBone; weight0 = packedWeight; break;
                                case 1: bone1 = packedBone; weight1 = packedWeight; break;
                                case 2: bone2 = packedBone; weight2 = packedWeight; break;
                                case 3: bone3 = packedBone; weight3 = packedWeight; break;
                            }
                            totalWeight += weight;
                        }
                        if (totalWeight > 0.000001)
                        {
                            vertexSkinned = true;
                            weights = new Vector4(weight0, weight1, weight2, weight3);
                        }
                    }

                    Point3D point;
                    Vector3 normal;
                    if (vertexSkinned)
                    {
                        // Position and normal remain in the mesh bind pose. The per-bone transforms,
                        // object transform, and normal transform are evaluated by the GPU vertex shader.
                        point = bindPoint;
                        normal = Vector3.UnitZ;
                        if (hasNormals)
                        {
                            Vector3D n = mesh.Normals[i];
                            if (n.LengthSquared > 0.000001 && !double.IsNaN(n.LengthSquared) && !double.IsInfinity(n.LengthSquared))
                            {
                                n.Normalize();
                                normal = new Vector3((float)n.X, (float)n.Y, (float)n.Z);
                            }
                        }
                    }
                    else
                    {
                        point = sceneMesh.Transform.Transform(bindPoint);
                        normal = Vector3.UnitZ;
                        if (hasNormals)
                        {
                            Vector3D sourceNormal = sceneMesh.Transform.Transform(mesh.Normals[i]);
                            double lengthSquared = sourceNormal.LengthSquared;
                            if (lengthSquared > 0.000001 && !double.IsNaN(lengthSquared) && !double.IsInfinity(lengthSquared))
                            {
                                sourceNormal.Normalize();
                                normal = new Vector3((float)sourceNormal.X, (float)sourceNormal.Y, (float)sourceNormal.Z);
                            }
                        }
                    }
                    Point3D restPoint = sceneMesh.Transform.Transform(bindPoint);
                    System.Windows.Point uv = hasUvs ? mesh.TextureCoordinates[i] : new System.Windows.Point(0, 0);
                    Vector4 color = hasVertexColors ? vertexColors[i] : sceneMesh.BaseColor;
                    _vertexScratch[offset + i] = new GpuVertex
                    {
                        Position = new Vector3((float)point.X, (float)point.Y, (float)point.Z),
                        Normal = normal,
                        TextureCoordinate = new Vector2((float)uv.X, (float)uv.Y),
                        Color = color,
                        TextureOffset = hasTexture ? textureLocation.Offset : 0,
                        TextureWidth = hasTexture ? textureLocation.Width : 1,
                        TextureHeight = hasTexture ? textureLocation.Height : 1,
                        HasTexture = hasTexture ? 1u : 0u,
                        RestPosition = new Vector3((float)restPoint.X, (float)restPoint.Y, (float)restPoint.Z),
                        Bone0 = bone0, Bone1 = bone1, Bone2 = bone2, Bone3 = bone3,
                        BoneWeights = weights,
                        IsSkinned = vertexSkinned ? 1u : 0u
                    };
                }
            }
        }

        private static Matrix4x4 CreateReverseZPerspectiveFieldOfView(
            float verticalFovRadians, float aspectRatio, float nearPlane, float farPlane)
        {
            // System.Numerics.CreateLookAt is right-handed: visible points have negative view-space Z.
            // Use the matching right-handed D3D [0,1] reverse-Z projection: z=-near -> 1, z=-far -> 0.
            // For row-vector transforms, clipW = -viewZ, so M34 must be -1 (not +1).
            float safeAspect = Math.Max(0.0001f, aspectRatio);
            float yScale = 1.0f / (float)Math.Tan(verticalFovRadians * 0.5f);
            float xScale = yScale / safeAspect;
            float depthRange = Math.Max(1e-6f, farPlane - nearPlane);
            float depthScale = nearPlane / depthRange;
            float depthOffset = (nearPlane * farPlane) / depthRange;

            return new Matrix4x4(
                xScale, 0.0f, 0.0f, 0.0f,
                0.0f, yScale, 0.0f, 0.0f,
                0.0f, 0.0f, depthScale, -1.0f,
                0.0f, 0.0f, depthOffset, 0.0f);
        }

        private ID3D12PipelineState CreatePipeline(ReadOnlyMemory<byte> vertexShader, ReadOnlyMemory<byte> pixelShader,
            FillMode fillMode, ReadOnlyMemory<byte> geometryShader = default)
        {
            RasterizerDescription rasterizer = RasterizerDescription.CullNone;
            rasterizer.FillMode = fillMode;
            // MSAA has to be enabled in the rasterizer as well as in the PSO and render targets.
            // With MultisampleEnable set, D3D12 uses its multisampled line-rasterization algorithm
            // for wireframe edges; AntialiasedLineEnable is only used when MultisampleEnable is false.
            rasterizer.MultisampleEnable = _sampleCount > 1;
            rasterizer.AntialiasedLineEnable = false;
            InputElementDescription[] inputElements =
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 24, 0),
                new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 32, 0),
                new InputElementDescription("TEXINFO", 0, Format.R32G32B32A32_UInt, 48, 0),
                new InputElementDescription("RESTPOSITION", 0, Format.R32G32B32_Float, 64, 0),
                new InputElementDescription("BLENDINDICES", 0, Format.R32G32B32A32_UInt, 76, 0),
                new InputElementDescription("BLENDWEIGHT", 0, Format.R32G32B32A32_Float, 92, 0),
                new InputElementDescription("SKINFLAG", 0, Format.R32_UInt, 108, 0)
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
                DepthStencilState = new DepthStencilDescription(true, DepthWriteMask.All, ComparisonFunction.GreaterEqual),
                SampleMask = uint.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RenderTargetFormats = new[] { Format.R8G8B8A8_UNorm },
                DepthStencilFormat = Format.D32_Float,
                SampleDescription = new SampleDescription((uint)_sampleCount, 0)
            };
            return _device.CreateGraphicsPipelineState(description);
        }

        private int GetBestSupportedSampleCount()
        {
            foreach (uint requested in _msaaSampleCandidates)
            {
                try
                {
                    var colorSupport = new FeatureDataMultisampleQualityLevels
                    {
                        Format = Format.R8G8B8A8_UNorm,
                        SampleCount = requested,
                        Flags = MultisampleQualityLevelFlags.None
                    };
                    _device.CheckFeatureSupport(Vortice.Direct3D12.Feature.MultisampleQualityLevels, ref colorSupport);
                    if (colorSupport.NumQualityLevels == 0)
                        continue;

                    var depthSupport = new FeatureDataMultisampleQualityLevels
                    {
                        Format = Format.D32_Float,
                        SampleCount = requested,
                        Flags = MultisampleQualityLevelFlags.None
                    };
                    _device.CheckFeatureSupport(Vortice.Direct3D12.Feature.MultisampleQualityLevels, ref depthSupport);
                    if (depthSupport.NumQualityLevels > 0)
                        return (int)requested;
                }
                catch
                {
                    // Keep trying lower sample counts; some adapters expose limited MSAA support.
                }
            }
            return 1;
        }

        private void CreateMsaaColorBuffer()
        {
            if (_sampleCount <= 1)
                return;

            var description = ResourceDescription.Texture2D(
                Format.R8G8B8A8_UNorm,
                (uint)_width,
                (uint)_height,
                mipLevels: 1,
                sampleCount: (uint)_sampleCount,
                flags: ResourceFlags.AllowRenderTarget);
            var optimizedClearValue = new ClearValue(
                Format.R8G8B8A8_UNorm,
                new Color4(0.055f, 0.067f, 0.09f, 1.0f));
            _msaaColorBuffer = _device.CreateCommittedResource(
                HeapType.Default, description, ResourceStates.RenderTarget, optimizedClearValue);
            _device.CreateRenderTargetView(_msaaColorBuffer, null, GetRtvHandle(BufferCount));
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
                ResourceDescription.Texture2D(Format.D32_Float, (uint)_width, (uint)_height,
                    mipLevels: 1, sampleCount: (uint)_sampleCount, flags: ResourceFlags.AllowDepthStencil),
                ResourceStates.DepthWrite,
                // Match reverse-Z depth clears and PSO state (near=1, far=0).
                new ClearValue(Format.D32_Float, 0.0f, 0));
            var description = new DepthStencilViewDescription
            {
                Format = Format.D32_Float,
                ViewDimension = _sampleCount > 1
                    ? DepthStencilViewDimension.Texture2DMultisampled
                    : DepthStencilViewDimension.Texture2D
            };
            _device.CreateDepthStencilView(_depthBuffer, description, _dsvHeap.GetCPUDescriptorHandleForHeapStart());
        }

        private CpuDescriptorHandle GetRtvHandle(int index)
        {
            CpuDescriptorHandle start = _rtvHeap.GetCPUDescriptorHandleForHeapStart();
            return new CpuDescriptorHandle(start, index, _rtvDescriptorSize);
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
            if (_queue == null || _fence == null) return;
            ulong value = ++_nextFenceValue;
            _queue.Signal(_fence, value);
            if (_fence.CompletedValue < value)
            {
                _fence.SetEventOnCompletion(value, _fenceEvent.SafeWaitHandle.DangerousGetHandle());
                _fenceEvent.WaitOne();
            }
        }

        private void UpdateRenderCallRate(long nowTicks)
        {
            _renderCallsInWindow++;
            double elapsed = (nowTicks - _renderRateWindowStartTicks) / (double)Stopwatch.Frequency;
            if (elapsed >= 0.5)
            {
                Volatile.Write(ref _renderCallRate, _renderCallsInWindow / elapsed);
                _renderCallsInWindow = 0;
                _renderRateWindowStartTicks = nowTicks;
            }
        }

        private void UpdateFps(long nowTicks)
        {
            _fpsFrames++;
            double elapsed = (nowTicks - _fpsStartTicks) / (double)Stopwatch.Frequency;
            if (elapsed >= 0.5)
            {
                Volatile.Write(ref _fps, _fpsFrames / elapsed);
                _fpsFrames = 0;
                _fpsStartTicks = nowTicks;
            }
        }

        private void DisposeSceneBuffers()
        {
            for (int i = 0; i < BufferCount; i++)
            {
                if (_vertexBuffers[i] != null) _vertexBuffers[i].Dispose();
                _vertexBuffers[i] = null;
            }
            if (_indexBuffer != null) _indexBuffer.Dispose();
            _indexBuffer = null;
            _vertexCount = 0;
            _indexCount = 0;
            _vertexBufferVersions = new long[BufferCount];
        }

        private void CreateBoneMatrixBuffers(int matrixCount)
        {
            Array.Fill(_frameBoneMatrixVersions, long.MinValue);
            _boneMatrixCount = Math.Max(0, matrixCount);
            _boneMatrixScratch = new Matrix4x4[Math.Max(1, _boneMatrixCount)];
            Array.Fill(_boneMatrixScratch, Matrix4x4.Identity);
            for (int frame = 0; frame < BufferCount; frame++)
            {
                if (_boneMatrixBuffers[frame] != null) _boneMatrixBuffers[frame].Dispose();
                ulong boneBytes = (ulong)_boneMatrixScratch.Length * (ulong)Marshal.SizeOf<Matrix4x4>();
                _boneMatrixBuffers[frame] = _device.CreateCommittedResource(
                    HeapType.Upload, ResourceDescription.Buffer(boneBytes), ResourceStates.GenericRead);
                CreateBoneMatrixShaderResourceView(frame, _boneMatrixBuffers[frame], _boneMatrixScratch.Length);
            }
        }

        private void DisposeBoneMatrixBuffers()
        {
            for (int i = 0; i < BufferCount; i++)
            {
                if (_boneMatrixBuffers[i] != null) _boneMatrixBuffers[i].Dispose();
                _boneMatrixBuffers[i] = null;
            }
            _boneMatrixCount = 0;
            _boneMatrixScratch = new[] { Matrix4x4.Identity };
            Array.Fill(_frameBoneMatrixVersions, long.MinValue);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Dx12MeshRenderer));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { WaitForGpu(); } catch { }
            DisposeSceneBuffers();
            DisposeBoneMatrixBuffers();
            if (_texturePixelBuffer != null) _texturePixelBuffer.Dispose();
            if (_depthBuffer != null) _depthBuffer.Dispose();
            if (_msaaColorBuffer != null) _msaaColorBuffer.Dispose();
            for (int i = 0; i < BufferCount; i++)
            {
                if (_backBuffers[i] != null) _backBuffers[i].Dispose();
                if (_allocators[i] != null) _allocators[i].Dispose();
                if (_constantBuffers[i] != null) _constantBuffers[i].Dispose();
            }
            _commandList.Dispose();
            SafeDispose(_deformationPipeline);
            _deformationPipeline = null;
            _skinningPipeline.Dispose();
            _normalsPipeline.Dispose();
            _texturedPipeline.Dispose();
            _wireframePipeline.Dispose();
            _solidPipeline.Dispose();
            _srvHeap.Dispose();
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
}
