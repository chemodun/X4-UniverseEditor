# Changelog

## [0.2.0](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor@v0.1.2...GalaxyEditor@v0.2.0) (2025-02-19)


### Features

* **ClusterEditWindow:** Make planets and moons visible ([63ce126](https://github.com/chemodun/X4-UniverseEditor/commit/63ce1262097e56bd3b2457fb91374f85ed718156))
* **GalaxyEditor:** add ExtensionInfo and ExtensionsInfoList classes for mod extension management, and realized saving and loading them to/from config ([8469779](https://github.com/chemodun/X4-UniverseEditor/commit/84697799b0a170b1075aa4d2fb8aa8269dc1c84a))
* **GalaxyEditor:** add GalaxyReferencesHolder and catalog item classes for managing references for galaxy items attributes ([80214d1](https://github.com/chemodun/X4-UniverseEditor/commit/80214d12e48c7386ec410bb016e088b07e1d599e))
* **GalaxyEditor:** add mod options UI and functionality for mod management ([4e5442a](https://github.com/chemodun/X4-UniverseEditor/commit/4e5442ad2edea622c2e9e544e93346e99cd44181))
* **GalaxyEditor:** add right-click event handling for galaxy map elements with empty context menu ([7ad9055](https://github.com/chemodun/X4-UniverseEditor/commit/7ad90558777d477f6fc74854adc52161f89498e1))
* **GalaxyEditor:** implement ClusterEditWindow for editing and adding clusters ([e1637a7](https://github.com/chemodun/X4-UniverseEditor/commit/e1637a74fe0fe01d61c5e6a3df73c9d380826b67))
* **GalaxyEditor:** implement context menu for hexagon elements with edit, delete, and add options ([3194747](https://github.com/chemodun/X4-UniverseEditor/commit/3194747b7df570183b9e61c1c23112743a0bd003))
* **GalaxyEditor:** implement GalaxyMod class for mod management ([6c52b95](https://github.com/chemodun/X4-UniverseEditor/commit/6c52b95fe2472811ca4496e966daa1797bdf19a3))
* **GalaxyReferencesHolder:** add support for ClusterMusic and ClusterIcons properties, and some elements manually added (like None) ([9657c22](https://github.com/chemodun/X4-UniverseEditor/commit/9657c2250087132b93fa7deb41884df25a3b2186))
* **MainWindow:** update data structures to include sounds and icons, and refactor variable names for consistency ([63ce126](https://github.com/chemodun/X4-UniverseEditor/commit/63ce1262097e56bd3b2457fb91374f85ed718156))


### Code Refactoring

* **GalaxyEditor:** change the ribbon content height and adopt controls ([14efaac](https://github.com/chemodun/X4-UniverseEditor/commit/14efaac22cd61c5eccc7c9f740320fe91306f0c9))
* **GalaxyEditor:** update ClusterEditWindow to use CatalogItem types and integrate GalaxyReferencesHolder for improved data management ([bfbb8b3](https://github.com/chemodun/X4-UniverseEditor/commit/bfbb8b3b4293d1feb7ecf4e59937735971f3b03c))
* **GalaxyEditor:** update window title to reflect current mod details and improve property change notifications ([14efaac](https://github.com/chemodun/X4-UniverseEditor/commit/14efaac22cd61c5eccc7c9f740320fe91306f0c9))

## [0.1.2](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor@v0.1.1...GalaxyEditor@v0.1.2) (2025-02-18)


### Code Refactoring

* **MainWindow:** rename ribbon and tab items for consistency ([97f100b](https://github.com/chemodun/X4-UniverseEditor/commit/97f100b40db2dcc707e7a7100671fb22d29b68e5))


### Miscellaneous Chores

* **GalaxyEditor:** add detailed cell information display and update event handling for selection actions ([e997493](https://github.com/chemodun/X4-UniverseEditor/commit/e997493ca7a6e9a7a00cdb0a7e208eb758b41b23))

## [0.1.1](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor@v0.1.0...GalaxyEditor@v0.1.1) (2025-02-17)


### Miscellaneous Chores

* **MainWindow:** add processing order for X4 data loading ([f63b53c](https://github.com/chemodun/X4-UniverseEditor/commit/f63b53c32a3130be9cbf063857b7ee20d1837f99))

## [0.1.0](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor-v0.0.1...GalaxyEditor@v0.1.0) (2025-02-17)


### Features

* **App:** implement configuration loading, NLog setup on startup and X4 Data loading ([e9b9c64](https://github.com/chemodun/X4-UniverseEditor/commit/e9b9c64526441f913d599fdba304750b3f2f820b))
* **GalaxyEditor:** add ClusterItemInfo and SectorItemInfo classes for enhanced data representation and update event handling in MainWindow ([0f696cf](https://github.com/chemodun/X4-UniverseEditor/commit/0f696cf5b290a23f2a8236d2bbfb87b2ea76f2d2))
* **GalaxyEditor:** add new asset images for jump gates, superhighways, and stations ([58cc6cf](https://github.com/chemodun/X4-UniverseEditor/commit/58cc6cfe3a8655ae7cf3d99f1f33d4af274be539))
* **GalaxyEditor:** enhance MainWindow layout and integrate GalaxyMapViewer with dynamic resizing ([035b326](https://github.com/chemodun/X4-UniverseEditor/commit/035b3267cfe78656b54afd70df773a6ca8c59d40))
* **GalaxyEditor:** initialize project structure with App.xaml and MainWindow ([1bcf20d](https://github.com/chemodun/X4-UniverseEditor/commit/1bcf20dd6618ae5bfd8b0726ed3922d67976b5ae))
* **MainWindow:** implement MainWindow with Ribbon, data binding and resource dictionary for icons ([6edab72](https://github.com/chemodun/X4-UniverseEditor/commit/6edab725321ec550c7c542df5adfe006fbc5ce1f))
* **MainWindow:** integrate X4DataExtractionWindow ([723ca8e](https://github.com/chemodun/X4-UniverseEditor/commit/723ca8e119b85c78468cc37d3eea04465fb2816f))


### Code Refactoring

* **MainWindow:** add height to StatusBar and introduce main content area ([e9eba34](https://github.com/chemodun/X4-UniverseEditor/commit/e9eba34e55f856c9028517f0558d74a45d007d85))
* **MainWindow:** enable progress reporting in BackgroundWorker for improved user feedback ([5b72691](https://github.com/chemodun/X4-UniverseEditor/commit/5b72691f2fb7c059a9156e9b896fd4005dece6bb))
* **MainWindow:** improve data loading and initialization of GalaxyMapViewer to make it processing in background ([3c7dd2d](https://github.com/chemodun/X4-UniverseEditor/commit/3c7dd2d6fa68845a396615ccc174e245b525025b))
* **MainWindow:** remove _galaxyMapViewer field and update references to use GalaxyMapViewer directly from xaml ([81c49ff](https://github.com/chemodun/X4-UniverseEditor/commit/81c49ff28a788c6f5b782c46dcc8628dfb4283fa))
* **MainWindow:** remove unused main grid reference for cleaner code ([2e45a3d](https://github.com/chemodun/X4-UniverseEditor/commit/2e45a3dd698a6e70486452cf6238475fbfaca8a1))
* **MainWindow:** update GalaxyMapViewer initialization and bind properties for dynamic updates ([7d18b4e](https://github.com/chemodun/X4-UniverseEditor/commit/7d18b4e3b52216825660026acd97930136f1a4c0))
* **MainWindow:** use th application icon into data extraction and about windows ([723ca8e](https://github.com/chemodun/X4-UniverseEditor/commit/723ca8e119b85c78468cc37d3eea04465fb2816f))


### Miscellaneous Chores

* **MainWindow:** add editor icon and improve data binding for assembly info ([88e70b8](https://github.com/chemodun/X4-UniverseEditor/commit/88e70b867f0c819f477552072c97b3d7ef419527))
