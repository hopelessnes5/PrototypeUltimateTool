# DirectX 12 GPU skinning prototype

This is an **isolated experiment**, not a replacement for `Model3DViewer`. The root `PrototypeUltimateTool.csproj` excludes this folder from its default recursive C# compile glob; open `DirectX12SkinningPrototype.csproj` separately in Visual Studio. The main application remains .NET Framework 4.8 and does not reference the prototype dependencies.

## Prerequisites

- Windows 10/11 with a Direct3D 12 capable GPU/driver.
- Visual Studio 2022 with the .NET 8 SDK and Windows desktop development components.
- NuGet restore access for `Vortice.Direct3D12`, `Vortice.DXGI`, and `Vortice.D3DCompiler` 3.8.3.

## Run

Open `DirectX12SkinningPrototype.csproj`, restore packages, build x64 and run. The app creates a procedural two-segment/two-bone model and deforms the upper segment on the GPU in the vertex shader.

- `F1`: solid shading.
- `F2`: rasterizer wireframe (the GPU draws the original indexed mesh edges; no tube mesh is generated on the CPU).
- `F3`: normal lines emitted by a geometry shader (no per-frame CPU tubes / `MeshGeometry3D`).
- `Space`: pause/resume the sample animation.

The window title reports approximate FPS. This is a sanity check, not a meaningful performance comparison against the editor yet: the prototype currently renders a small synthetic mesh, uses no imported texture/materials, and presents with VSync.

## What this validates

- Direct3D 12 device, command queue, swap chain, render/depth targets and pipeline states can live outside the legacy WPF `Viewport3D`.
- Vertex skinning and normal transformation are shader work; mesh positions are not read back to the CPU per frame.
- Wireframe uses the rasterizer fill mode and normals are generated in a GPU geometry-shader pass instead of recreating a high-triangle-count overlay from C# on every animation tick.

## Integration boundary with day4

The current `Model3DViewer` uses `System.Windows.Media.Media3D.MeshGeometry3D`. Its `UpdateSkinnedMeshes()` builds new `Point3DCollection` / `Vector3DCollection` objects per tick, then `RefreshAnimationMeshOverlays()` leads into CPU construction of wireframe and normals overlay meshes. This prototype avoids that path entirely, but is not yet wired to the project's P3D data.

Next integration step, only after this standalone project builds and runs:

1. Add an adapter from `MeshRenderInfo.BindPositions`, source normals, `SkinBoneIndices`, `SkinWeights`, palette mapping and rigid-bone flags into a packed GPU vertex buffer.
2. Derive skin matrices from the current bind/world bone transforms; upload only bone matrices each frame.
3. Expand the sample's two matrices to a structured GPU buffer for arbitrary P1/P2 rigs, rather than keeping the sample's two-bone constant-buffer limit.
4. Upload mesh/index/material resources once and make WPF and DirectX 12 backends selectable until feature parity and correctness tests pass.

Do not copy this sample's two-bone mapping as-is to game resources: P1 palette slots, P2 weight/group layout, inverse bind transforms and rigid `CompositeDrawable` references need to retain the meanings already decoded by the main project.

## Build-status note

The current execution environment has no .NET SDK/MSBuild/C# compiler, so this source has only had static review and cannot honestly be marked build-verified. This experimental project must be built on Windows before it is considered runnable. The main source files and solution are unchanged. The only main-project edit is a `<Compile Remove>` rule that prevents the nested experiment files from accidentally being compiled into the `net48` application.
