# Adding and customizing interface languages

The UI reads `*.xml` dictionaries at startup. Language files are data, not source code; a new language can be added without rebuilding the application.

## Recommended location

Create the folder `%LOCALAPPDATA%\PrototypeUltimateTool\Languages` and place a language file there, for example `de.xml`. User dictionaries are loaded before the bundled dictionaries, so they can add a new language or override selected entries of an existing language. Restart the application after editing a file.

The bundled dictionaries are also available in the application's `Languages` directory beside the executable.

## Minimal file structure

```xml
<?xml version="1.0" encoding="utf-8"?>
<language code="de" displayName="German" nativeName="Deutsch" culture="de-DE">
  <strings>
    <string key="menu.file">Datei</string>
    <string key="cmd.file.open">Öffnen...</string>
    <string key="cmd.file.open.tooltip">Datei öffnen</string>
  </strings>
</language>
```

- `code` is a unique language code such as `de`, `fr`, `pl`, or `ja`.
- `displayName` is the name in English; `nativeName` is what appears in the language menu.
- `culture` documents the matching .NET culture (for example `de-DE`).
- `<strings>` maps stable technical IDs (`menu.file`, `cmd.file.open`, `viewer.resetCamera`, and so on) to displayed captions. Keep these keys unchanged; translate only their values.
- Tooltip and accessibility overrides use the owning control/item ID plus `.tooltip`, `.accessibleName`, or `.accessibleDescription` (for example `cmd.file.open.tooltip`).
- An override file may contain only the keys that need to change. Omitted keys fall back to the bundled dictionary for that language (if one exists), then to the default Russian catalog or the caller's fallback text.

The old `<legacy>` phrase mapping is no longer loaded. Interface text must use technical IDs; this prevents accidental translation of resource names, paths, exception details, and other dynamic data.

Resource names, file paths, extensions, P3D type IDs, and contents shown from game files are data and must not be translated.
