# PrototypeUltimateTool

**Редактор ресурсов Prototype / Pure3D** для просмотра и изменения файлов игры: контейнеров P3D, архивов RCF, текстур, геометрии и анимаций.

PrototypeUltimateTool — самостоятельная Windows-утилита с редактором дерева ресурсов, инструментами импорта/экспорта и 3D-вьювером на Direct3D 12. Приложение находится в активной разработке; поддержка отдельных типов чанков и графических особенностей может различаться.

## Возможности

### Файлы и ресурсы

- Открытие, создание и сохранение **P3D**, **P3D.RZ / RZ** и **RCF**.
- Просмотр иерархии Pure3D-чанков и редактирование доступных свойств через панель свойств.
- Добавление файлов и ресурсов в открытые P3D и RCF, множественное выделение, пакетный экспорт, поиск, сортировка и фильтрация.
- Импорт и экспорт сырых P3D-чанков (`.p3dchunk-*`), дамп содержимого P3D и инструменты для сохранения порядка корневых чанков при повторной сборке.
- Распаковка RCF в папку и сборка RCF из папки.
- Отмена и повтор поддерживаемых изменений.
- Отслеживание изменений файлов, открытых во внешних программах, с обратным импортом для поддерживаемых типов ресурсов.

### Текстуры и модели

- 2D-просмотр текстур с переключением каналов **R, G, B, A**, режимами RGB/RGBA и настройкой ориентации V.
- Замена текстурных данных и экспорт всех текстур.
- Экспорт поддерживаемой геометрии в **Wavefront OBJ** с данными материалов **MTL**, когда они доступны.
- Просмотр выбранной геометрии и CompositeDrawable в 3D-вьювере.

### 3D-вьювер и анимация

- Видимый рендеринг через **Direct3D 12 / DXGI**.
- Режимы: каркас, сплошная модель, текстуры, нормали, визуализация влияния костей и диагностика деформации.
- Камера с вращением, панорамированием, масштабированием и сбросом вида; гизмо осей.
- Выбор анимационных клипов, воспроизведение/пауза, зацикливание и перемещение по кадрам; переключение между просмотром меша и скелета там, где данные модели это поддерживают.
- GPU skinning для поддерживаемых мешей и MSAA с выбором доступного уровня сглаживания.
- Диагностический режим деформации окрашивает треугольники по изменению площади относительно исходной позы: сжатие, близкое к исходному состояние, растяжение и возможный переворот грани.
- Оверлей статистики: FPS, время кадра, показатели CPU/подготовки/Present, количество объектов и геометрии, draw calls, оценка памяти текстур и используемый уровень MSAA.

### Работа с большими архивами

- Фоновое сканирование содержимого RCF, включая вложенные P3D/P3D.RZ.
- Фильтрация по расширениям файлов архива и типам корневых секций вложенных P3D.
- Отложенный разбор записей и кэширование части индексных данных, чтобы уменьшить время ожидания при работе с большими архивами.

### Интерфейс

- Русский и английский языки интерфейса через XML-каталоги; предусмотрена архитектура для пользовательских переводов.
- Светлая и тёмная темы, сохранение настроек, список недавних файлов, журнал событий.
- Интеграция с Проводником Windows для открытия поддерживаемых файлов в приложении.
- Горячие клавиши для основных операций, включая `Ctrl+O`, `Ctrl+S`, `Ctrl+Shift+S`, `Ctrl+Z` и `Ctrl+Shift+Z`.

## Поддерживаемые форматы

| Формат | Назначение |
|---|---|
| `.p3d` | Контейнер Pure3D без RZ-сжатия |
| `.p3d.rz`, `.rz` | Pure3D с RZ-сжатием / RZ-контейнеры |
| `.rcf` | Архив ресурсов; просмотр, создание, изменение содержимого, упаковка и распаковка |
| `.p3dchunk-*` | Сырые чанки для точечного экспорта и повторного импорта |
| `.obj` / `.mtl` | Экспорт поддерживаемой геометрии и материалов для внешних 3D-инструментов |

Приложение содержит сериализаторы и модели данных для множества типов Pure3D, включая геометрию, PolySkin, скелеты, анимации, текстуры, шейдеры, CompositeDrawable и связанные структуры. Это не означает, что каждый вариант каждого чанка или каждый графический эффект полностью поддерживается.

## Технологии

- **C# 13**, **.NET 9 (`net9.0-windows`)**
- **Windows Forms** для основного интерфейса
- **WPF `Viewport3D`** как часть существующего конвейера данных сцены/анимации
- **Direct3D 12 / DXGI** через пакеты **Vortice.Direct3D12**, **Vortice.DXGI** и **Vortice.D3DCompiler** версии **3.8.3** для видимого 3D-рендеринга
- HLSL-шейдер DX12 включается в сборку как встроенный ресурс

## Запуск

Для запуска необходимы:

- Windows x64;
- **.NET 9 Windows Desktop Runtime (x64)**;
- видеокарта и драйвер с поддержкой Direct3D 12.

Сборка настраивается как **framework-dependent single-file EXE**: .NET Runtime не включён в приложение и устанавливается отдельно. Каталоги языковых XML и файл настроек остаются внешними данными приложения.

## Сборка из исходников

1. Установите Visual Studio 2022 с рабочей нагрузкой **Разработка классических приложений .NET** и .NET 9 SDK.
2. Откройте `PrototypeUltimateTool.csproj`.
3. Выполните восстановление NuGet-пакетов.
4. Соберите конфигурацию `Release` для `x64` (проект нацелен на Windows x64).

Обычная сборка проекта настроена на автоматическую публикацию framework-dependent single-file EXE в выходной каталог. Для работы из исходников и диагностики удобнее использовать конфигурацию `Debug`.

## Настройки и языки

- `settings.ini` хранит пользовательские параметры приложения рядом с его рабочими файлами.
- `Languages/ru.xml` и `Languages/en.xml` содержат встроенные каталоги русского и английского интерфейса.
- Для пользовательских переводов предусмотрен каталог `%LOCALAPPDATA%\PrototypeUltimateTool\Languages`.
- HLSL-шейдер DX12 встроен в сборку и не должен требовать отдельного файла в папке `Shaders` в опубликованном приложении.

## Известные ограничения

- DX12-вьювер и его совместимость с разнообразными P1/P2-ресурсами продолжают развиваться; проверка конкретной модели/анимации может потребоваться отдельно.
- Прозрачность и сортировка прозрачных материалов пока не полностью соответствуют возможностям исходного формата.
- Текущий путь упаковки текстур в DX12 ограничивает отдельную текстуру размером до **1024 × 1024** и суммарный пул примерно **8 миллионов пикселей**; ресурсы сверх лимита могут отображаться с текстурой-заполнителем.
- Режим деформации — визуальный диагностический инструмент на основе изменения площади граней, а не полноценный анализ объёмной деформации.
- Проект не заявляет о полной поддержке всех неизвестных/редких вариантов чанков; для низкоуровневой работы используйте экспорт/импорт сырых чанков и проверяйте сохранённые файлы в целевой игре.

## Благодарности и сообщество

Благодарность **Elan-S** и **Gibbed** за предшествующие инструменты, разбор форматов и работу, полезную для изучения ресурсов Prototype / Pure3D.

Присоединиться к сообществу: [Discord](https://discord.gg/XXNBqBSkNv).

---

# PrototypeUltimateTool (English)

A **Prototype / Pure3D resource editor** for browsing and modifying game assets, including P3D containers, RCF archives, textures, geometry and animations.

PrototypeUltimateTool is a standalone Windows utility that combines a resource-tree editor, import/export tools and a Direct3D 12 viewport. The project is under active development, and support for individual chunk variants and rendering features may vary.

## Features

### Files and resources

- Open, create and save **P3D**, **P3D.RZ / RZ**, and **RCF** files.
- Browse Pure3D chunk hierarchies and edit exposed properties in the property grid.
- Add resources to open P3D/RCF documents; multi-select, batch export, search, sort and filter the resource tree.
- Import and export raw P3D chunks (`.p3dchunk-*`), dump P3D contents, and preserve root-chunk ordering when rebuilding where supported.
- Extract RCF archives to folders and pack folders into RCF archives.
- Undo and redo supported edits.
- Watch files opened in external tools and re-import changes for supported resource types.

### Textures and models

- 2D texture preview with **R, G, B, A** channel controls, RGB/RGBA presets and V-axis orientation control.
- Replace texture data and export all textures.
- Export supported geometry to **Wavefront OBJ**, with **MTL** material data when available.
- Preview selected geometry and CompositeDrawable assets in 3D.

### 3D viewport and animation

- Visible rendering with **Direct3D 12 / DXGI**.
- Wireframe, solid, textured, normals, bone-influence visualization and deformation-debug modes.
- Orbit, pan, zoom and reset the camera; display an axis gizmo.
- Select animation clips, play/pause, loop and scrub frames; switch between mesh and skeleton previews where the asset data supports it.
- GPU skinning for supported meshes and MSAA with capability-based sample selection.
- The deformation-debug view colors triangles by area change from the source pose: compression, near-baseline area, stretch, and possible face flipping.
- A stats overlay reports FPS, frame timing, CPU/preparation/Present measurements, object and geometry counts, draw calls, estimated texture memory and the active MSAA level.

### Large archives

- Background scanning of RCF contents, including nested P3D/P3D.RZ resources.
- Filter RCF entries by extension and nested P3D entries by root chunk type.
- Deferred entry parsing and caching of selected index data to reduce stalls when working with large archives.

### Interface

- Russian and English UI catalogs stored as XML, with an architecture for user-defined translations.
- Light/dark themes, settings persistence, recent files and an application log.
- Windows Explorer integration for opening supported files in the tool.
- Keyboard shortcuts for common actions, including `Ctrl+O`, `Ctrl+S`, `Ctrl+Shift+S`, `Ctrl+Z` and `Ctrl+Shift+Z`.

## Supported formats

| Format | Purpose |
|---|---|
| `.p3d` | Uncompressed Pure3D container |
| `.p3d.rz`, `.rz` | RZ-compressed Pure3D / RZ containers |
| `.rcf` | Resource archive; browse, create, modify contents, pack and extract |
| `.p3dchunk-*` | Raw chunks for targeted export and re-import |
| `.obj` / `.mtl` | Export supported geometry and materials for external 3D tools |

The codebase contains serializers and data models for many Pure3D types, including geometry, PolySkin, skeletons, animations, textures, shaders, CompositeDrawable and related structures. This does not imply complete support for every chunk variant or every rendering effect.

## Technology stack

- **C# 13**, **.NET 9 (`net9.0-windows`)**
- **Windows Forms** for the main UI
- **WPF `Viewport3D`** retained as part of the scene/animation data pipeline
- **Direct3D 12 / DXGI** via **Vortice.Direct3D12**, **Vortice.DXGI** and **Vortice.D3DCompiler** packages, version **3.8.3**, for visible 3D rendering
- The DX12 HLSL shader is embedded as an application resource

## Requirements

- Windows x64
- **.NET 9 Windows Desktop Runtime (x64)**
- A GPU and driver supporting Direct3D 12

The project is configured for a **framework-dependent single-file EXE**. The .NET Runtime is installed separately; language catalogs and settings remain external application data.

## Build from source

1. Install Visual Studio 2022 with the **.NET desktop development** workload and the .NET 9 SDK.
2. Open `PrototypeUltimateTool.csproj`.
3. Restore NuGet packages.
4. Build the `Release` configuration for `x64` (the project targets Windows x64).

A normal project build is configured to publish a framework-dependent single-file EXE into the output directory. Use `Debug` when working from source and diagnosing issues.

## Settings and localization

- `settings.ini` stores user preferences alongside the application's working files.
- `Languages/ru.xml` and `Languages/en.xml` provide Russian and English UI catalogs.
- User translations are supported under `%LOCALAPPDATA%\PrototypeUltimateTool\Languages`.
- The DX12 HLSL shader is embedded and should not need a separate `Shaders` file in the published app.

## Current limitations

- The DX12 viewport and compatibility with the variety of P1/P2 assets are still evolving; individual models and animations may require separate validation.
- Transparency and transparent-material sorting are not yet fully equivalent to all features of the source format.
- The current DX12 texture-packing path is limited to **1024 × 1024 per texture** and about **8 million aggregate pixels**; assets exceeding those limits may fall back to a placeholder texture.
- Deformation visualization is a diagnostic based on triangle-area changes, not a full volumetric deformation analysis.
- Full support for every rare or unknown chunk variant is not claimed. For low-level work, use raw chunk import/export and validate saved files in the target game.

## Acknowledgements and community

Thanks to **Elan-S** and **Gibbed** for earlier tools, format research and work that has been useful for understanding Prototype / Pure3D assets.

Join the community on [Discord](https://discord.gg/XXNBqBSkNv).
