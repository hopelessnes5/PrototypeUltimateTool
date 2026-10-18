# PrototypeUltimateTool — Stage 4: RCF tree editor and P3D format registry

This source package builds on the user-confirmed UVFix2 and CompositeDrawableFix1 changes and carries forward the Elan-S/EditP3D animation-preview integration (Fix6).

## New in Stage 4

- RCF/CEMENT archives open directly in the application's left tree instead of requiring extraction first.
- Archive entries are grouped into virtual folders using their metadata paths. Duplicate entry hashes are retained and mapped through per-hash queues rather than a unique-key dictionary.
- `.p3d` and `.p3d.rz` entries are parsed on demand; their chunk trees can be expanded and edited through the same PropertyGrid and preview code as standalone P3D files.
- PropertyGrid changes to chunks and imports into supported chunks are serialized back into the active archive entry in memory immediately. Use **Save** to write the modified RCF to disk.
- Entire archive entries can be exported or replaced. Replacing a plain `.p3d` into a `.p3d.rz` entry compresses it; replacing a `.p3d.rz` into a plain `.p3d` entry decompresses it.
- The original **RCF Archives → Unpack** and **Pack** commands remain available.
- Saving updates entry sizes and payload offsets, preserves all duplicate-hash entries, and uses stable hash ordering.
- Opening a new document is transactional: an invalid input does not clear the previously open tree/preview.
- `NodeFactory` tries both P1 and P2 candidates when the file is opened as `PrototypeGame.Any`, and now includes legacy format classes that expose `KnownType` but omitted `KnownGame`.
- Added typed P1 readers for `Locator` (0x00014000), legacy payload records 0x00011020, 0x00011115, 0x00022005, and animation channel records `PVIS` (0x00121108) and `VRTX` (0x0012110E).
- Added named byte-preserving nodes for format IDs which the Elan-S/Gibbed catalogue identifies but this project does not fully decode yet. These are editable/exportable raw payloads, not claims that all fields are understood. Generic `Unknown` chunks can also be replaced using the existing Replace command.
- Added opaque fallback recognition for 0x00019008, 0x00022004, and 0x00022006. The reference sources contain placeholder classes for these IDs with their `KnownType` attributes commented out, so these IDs are deliberately represented as raw data rather than parsed with incomplete layouts.

## RCF edit workflow

1. Open an `.rcf` file using **Open**.
2. Expand a file entry. For `.p3d` / `.p3d.rz`, the entry expands into its Pure3D chunk hierarchy on demand.
3. Select a chunk and edit its properties or use **Replace** for an importable chunk. The embedded P3D is serialized back into the selected RCF entry in memory after the edit/import.
4. Select an archive file entry to rename it or edit its metadata date. Entry rename updates its path hash.
5. Choose **Save** to write the whole RCF. **Export selected** and **Replace** on a file entry operate on the entire stored entry data.

A malformed P3D entry exposes its parse error in the selected archive item's PropertyGrid instead of aborting the entire RCF open.

## P3D recognition scope

A static ID comparison covers all 115 active `KnownType` IDs in the provided Elan-S/EditP3D reference catalogue. The project currently registers 126 unique IDs, including project-specific registrations and three tentative IDs shown as raw payloads. IDs outside these registries remain `Unknown` and are preserved byte-for-byte when possible.

The raw-payload node improves labels, export/import and safe round-tripping but does not decode unknown fields. For additional IDs not present in the reference catalogue, representative P3D/RCF samples are needed to infer their binary layouts safely.

## Build

Open `PrototypeUltimateTool.sln` in Visual Studio 2022 on Windows with the .NET Framework 4.8 Developer Pack installed, then run **Build → Rebuild Solution** in `Debug | x64` (the project targets x64).

## Validation status

This environment has no `dotnet`, MSBuild, `csc`, or `mcs` toolchain. Static source checks and ZIP integrity checks are performed here, but this increment has **not** been compiled or runtime-tested in Visual Studio. Before using it to modify production archives, test on a copy of an RCF and verify that save/reopen preserves entry names, sizes, P3D child chunks, and textures.

No SharpGL/OpenGL dependency was added. Rendering remains on the existing WPF `Viewport3D`; the project remains a single C# application project with no new external compiled DLL dependency.

## UI Stage 1

See [`UI_STAGE1.md`](UI_STAGE1.md) for the compact Fluent tree update, dark/light theme toggle, smart initial RCF expansion, and validation status. This increment deliberately does not include global texture filtering or external-edit monitoring.

## UI Stage 2 — external files

See [`EXTERNAL_FILE_WORKFLOW_STAGE2.md`](EXTERNAL_FILE_WORKFLOW_STAGE2.md). File-tree context menus now support opening temporary working copies with the system-associated application or a chosen executable, monitoring changes, and importing supported edits back into the active document/RCF entry. `.p3d.rz` entries are decompressed for external editing and recompressed on import. Save the active document in Prototype Ultimate Tool to write imported changes to the original archive/file.

This increment was statically checked only; rebuild and validate external editing on a copy of a real RCF/P3D in Windows Visual Studio before modifying production game archives.


## UI Stage 3 — About dialog

- Added a toolbar button and a Help menu entry for **About Prototype Ultimate Tool**.
- The dialog shows the running assembly version, a short scope description, upstream attribution to Nixson's EditP3D, the current repository maintainer/account label (`hopelessnes5`), and a clickable GitHub repository link.
- The dialog follows the application's selected light/dark palette.
- The About screen intentionally distinguishes repository maintenance/modifications from authorship of the original EditP3D code.

Validation: source-level checks and ZIP integrity only; rebuild and test the WinForms dialog on Windows before using a release build.

## UI Stage 3 — About dialog

- Added **О программе** to the toolbar and the **Справка** menu.
- Shows the running assembly version and a short application description.
- Credits repository/modification maintenance to `hopelessnes5` and attributes the original EditP3D lineage to Nixson without claiming authorship of the original code.
- Provides a clickable GitHub repository link and follows the selected light/dark theme.

Validation: source-level checks and ZIP integrity only; rebuild and visually test this WinForms dialog on Windows before release.

## Explorer integration button

The toolbar now includes **🪟 Проводник**, also available under **Файл → Интеграция с Проводником**. The dialog installs or removes current-user Explorer context-menu entries for `.rcf`, `.p3d`, and `.rz`, registers the program under **Open with**, and links to Windows Default Apps settings. It does not require administrator privileges and intentionally does not alter the protected `FileExts\\UserChoice` setting; choosing the default app for double-click remains a Windows settings action. On Windows 11 the classic verb may appear under **Показать дополнительные параметры**.

Validation for this increment is limited to static source and ZIP integrity checks because a Windows/.NET Framework build toolchain is unavailable in this environment.
