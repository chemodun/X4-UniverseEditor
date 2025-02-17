# Changelog

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
