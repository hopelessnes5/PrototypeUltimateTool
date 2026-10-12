EN

This project is a decompilation, corrections, and modifications of the original EditP3D from Nixson.
I'm not claiming rights to his code, all edits were made using neural networks.

  - Abandonment of outdated SharpGL. It has been completely replaced by the native DirectX/Direct3D hardware engine.

  - Built-in 3D viewer directly in the main window:
    Instead of a separate drop-down window, seamless switching has been done [🖼️ 2D
    Texture] / [🧊 3D Model].
  - Applying original textures (UV mapping)

  - Optimization of the tree (acceleration by 100 times):
    In the original, large files with thousands of nodes (like alex.p3d.rz) were stuck when
opening and filtering. Lazy Loading and
metadata caching have been implemented — the tree opens and sorts instantly.
  - "🖼️Textures only" filter and quick search (🔍):
Allows you to hide hundreds of technical animations and work with the right ones at once
    materials.
  - Multi-criteria sorting:
By size (bytes), resolution (pixels), file name and type.



And there are many more changes that you can evaluate for yourself.

RU

Этот проект представляет из себя декомпиляцию, исправления и изменения исходного EditP3D от Nixson.
Я не заявляю прав на его код, все правки делались при помощи нейросетей.

  - Отказ от устаревшего SharpGL. Он полностью заменён на нативный аппаратный движок DirectX / Direct3D

  - Встроенный 3D-вьювер прямо в основном окне:
    Вместо отдельного выпадающего окна сделано бесшовное переключение [🖼️ 2D
    Текстура] / [🧊 3D Модель].
  - Наложение оригинальных текстур (UV-маппинг)

  - Оптимизация дерева (ускорение в 100 раз):
    В оригинале большие файлы с тысячами нод (как alex.p3d.rz) зависали при
    открытии и фильтрации. Внедрена ленивая загрузка (Lazy Loading) и
    кэширование метаданных — дерево открывается и сортируется мгновенно.
  - Фильтр «🖼️ Только текстуры» и быстрый поиск (🔍):
    Позволяет скрыть сотни технических анимаций и сразу работать с нужными
    материалами.
  - Многокритериальная сортировка:
    По размеру (байты), разрешению (пиксели), имени и типу файлов.



И ещё множество изменений, которые вы сами можете оценить

