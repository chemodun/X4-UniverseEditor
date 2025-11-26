# Changelog

## [0.2.2](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor@v0.2.1...GalaxyEditor@v0.2.2) (2025-11-26)


### Code Refactoring

* **App:** enhance NLog configuration handling to check for changes before applying updates ([57a1d97](https://github.com/chemodun/X4-UniverseEditor/commit/57a1d97853ec20790763facd9184294f3051478d))
* **GalaxyMod:** replace FolderBrowserDialog with OpenFolderDialog ([abaf8ba](https://github.com/chemodun/X4-UniverseEditor/commit/abaf8ba79e4173460ea44fc7b18bc758d64e4d9c))
* **GalaxyMod:** set initial directory for mod data folder dialog ([d558243](https://github.com/chemodun/X4-UniverseEditor/commit/d5582439370aa2e3efa11151bd1c80877504fc87))
* **GalaxyMod:** update modAttributesToSave initialization syntax ([35011fa](https://github.com/chemodun/X4-UniverseEditor/commit/35011fad824ebc8a05a07ee5ec51a22db7f72626))
* **MainWindow.xaml:** update sector size bindings for internal size limits ([63b24df](https://github.com/chemodun/X4-UniverseEditor/commit/63b24df279f3a12dcaf36074f8249502fc9800a0))
* **MainWindow:** enhance universe ID handling and selection logic ([d558243](https://github.com/chemodun/X4-UniverseEditor/commit/d5582439370aa2e3efa11151bd1c80877504fc87))
* **MainWindow:** optimize JsonSerializerOptions usage ([35011fa](https://github.com/chemodun/X4-UniverseEditor/commit/35011fad824ebc8a05a07ee5ec51a22db7f72626))
* **MainWindow:** remove NLog configuration calls from property setters to streamline logging setup ([7f7960d](https://github.com/chemodun/X4-UniverseEditor/commit/7f7960d84aa0c57719c2c0f0523980f33b8f04b7))
* **MainWindow:** remove SectorRadius parameter from GalaxyMapViewer connection ([9e7e415](https://github.com/chemodun/X4-UniverseEditor/commit/9e7e41537e6c8b65ce74515b959717f4ac240550))
* **MainWindow:** replace FolderBrowserDialog with OpenFolderDialog ([abaf8ba](https://github.com/chemodun/X4-UniverseEditor/commit/abaf8ba79e4173460ea44fc7b18bc758d64e4d9c))
* remove unused using directives from App.xaml.cs files ([d0badce](https://github.com/chemodun/X4-UniverseEditor/commit/d0badce43f3854723d4cb193b2c058b5459491e5))

## [0.3.0](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor@v0.2.1...GalaxyEditor@v0.3.0) (2025-02-20)


### Features

* **GalaxyEditor:** enhance JSON serialization for UnifyItemCluster and related classes ([e46ad93](https://github.com/chemodun/X4-UniverseEditor/commit/e46ad93c285518ac8e79780fe03a9b08e213c7d5))
* **GalaxyEditor:** implement PostInit method for initialization and state management in GalaxyModInfo classes ([d920164](https://github.com/chemodun/X4-UniverseEditor/commit/d9201646989fc83482abeb8ca5ae44a71286d0ba))
* **GalaxyEditor:** implement UnifyItemCluster class for enhanced cluster management and initialization ([1340ec7](https://github.com/chemodun/X4-UniverseEditor/commit/1340ec7088c32bc6fb30a99d7ec5be804c883892))


### Bug Fixes

* **GalaxyUnify:** make GetItem return an UnifyItem and add GetAttribute to return and attribute ... ([943b84c](https://github.com/chemodun/X4-UniverseEditor/commit/943b84c96dc6cdb668a275400c5cb47e609dee19))


### Code Refactoring

* **GalaxyEditor:** enhance GalaxyUnifyItemAttribute to support multiple value types and improve state management ([1ed6709](https://github.com/chemodun/X4-UniverseEditor/commit/1ed670946f0a1aa0d60eec72a9620f6fa8a6c37c))
* **GalaxyEditor:** remove unused using directives from GalaxyUnifyBase and GalaxyUnifyPlanet for cleaner code ([e8091e9](https://github.com/chemodun/X4-UniverseEditor/commit/e8091e99b9ed3a7ea67a081266109e9f14bcadcc))
* **GalaxyEditor:** rename files for  UnifyItem classes to streamline codebase ([86090a7](https://github.com/chemodun/X4-UniverseEditor/commit/86090a7dab7297944a9a4164d7d08e93b53077f5))
* **GalaxyEditor:** return back separate lists for attributes and items in GalaxyUnifyItemAttribute ([4e5d7fd](https://github.com/chemodun/X4-UniverseEditor/commit/4e5d7fde48c574327d0ab955e50152b75ba84951))
* **GalaxyEditor:** simplify JSON read/write methods in GalaxyUnify classes for improved clarity and maintainability ([fe78101](https://github.com/chemodun/X4-UniverseEditor/commit/fe78101742b85b8ab983e0b76ff63f0105ad814e))
* **GalaxyEditor:** update  GalaxyUnifyItemJsonConverter and GalaxyUnifyItemAttributeConverter ([e6011d2](https://github.com/chemodun/X4-UniverseEditor/commit/e6011d290da4e21bd541c5c5d47e6adccf1fa6c5))
* **GalaxyEditor:** update GalaxyUnifyItemAttribute and GalaxyUnifyItem to use nullable types for better handling of optional values ([e6011d2](https://github.com/chemodun/X4-UniverseEditor/commit/e6011d290da4e21bd541c5c5d47e6adccf1fa6c5))
* **GalaxyEditor:** update GalaxyUnifyItemAttribute to support new Attribute type and streamline list handling. There a ValueList can handle as attributes as well an items too ([ee4121d](https://github.com/chemodun/X4-UniverseEditor/commit/ee4121dbb854e3db232202cc011982de36c5f1d0))
* **GalaxyEditor:** update GetList method to return a list of GalaxyUnifyItemAttribute and introduce GetListOfItems for improved item retrieval ([5304a4f](https://github.com/chemodun/X4-UniverseEditor/commit/5304a4ff1b04ed25c6f2d812d6c31c69c11111a2))
* **GalaxyUnify:** consolidate attribute setting methods into a single Set method for improved code clarity and maintainability ([943b84c](https://github.com/chemodun/X4-UniverseEditor/commit/943b84c96dc6cdb668a275400c5cb47e609dee19))

## [0.2.1](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor@v0.2.0...GalaxyEditor@v0.2.1) (2025-02-19)


### Miscellaneous Chores

* **GalaxyReferencesHolder:** add WorldParts and AtmosphereParts lists to catalog planets and moons ([843e990](https://github.com/chemodun/X4-UniverseEditor/commit/843e990b12e5db1c11c023efb489ea5857e750a0))

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
