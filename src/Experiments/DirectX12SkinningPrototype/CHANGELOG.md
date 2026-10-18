# DirectX 12 GPU skinning prototype — initial spike

- Added a standalone `net8.0-windows` WinForms project using Vortice Direct3D 12.
- Added a `<Compile Remove>` rule to the main `net48` project so SDK default globs cannot pull in the experiment sources; the solution and main app sources remain unchanged.
- Added an HLSL vertex shader that skins a procedural two-bone test mesh on the GPU.
- Added rasterizer wireframe mode and a geometry-shader normal-line pass so diagnostics do not create new CPU-side tube meshes per frame.
- Added frame pacing, swap-chain resizing, GPU fence synchronization, and keyboard controls for solid/wireframe/normals and pause.
- This is a sandbox only; P3D/P2 import, real materials/textures, arbitrary bone buffers and integration with `Model3DViewer` remain future steps.
- No Visual Studio/.NET build or runtime validation was available in the authoring environment.
