# Changelog

## [0.3.0](https://github.com/chemodun/X4-UniverseEditor/compare/GalaxyEditor@v0.2.1...GalaxyEditor@v0.3.0) (2025-09-20)


### Features

* **ClusterEditWindow:** add IsChanged and IsNew properties for state management and update button enablement ([509692b](https://github.com/chemodun/X4-UniverseEditor/commit/509692ba855ace249ff106f2011966333ba9f033))
* **ClusterEditWindow:** add position parameter to Cluster initialization and update related methods ([ff5a253](https://github.com/chemodun/X4-UniverseEditor/commit/ff5a253cd7569d9445e8345c193278608ed0c6e0))
* **ClusterEditWindow:** disable ClusterId editing and use instead generated one by template ([893027f](https://github.com/chemodun/X4-UniverseEditor/commit/893027f89e25e2642c2c6514817dd68174d30719))
* **ClusterEditWindow:** enhance cluster editing with unify cluster support and improved initialization ([969f3ba](https://github.com/chemodun/X4-UniverseEditor/commit/969f3ba573f49ca123214a68d40753c1568ca448))
* **GalaxyEditor:** add MoonEditWindow and PlanetEditWindow for editing moon and planet properties ([40c62c0](https://github.com/chemodun/X4-UniverseEditor/commit/40c62c042d53560b46a0036b43b87751244c8fc7))
* **GalaxyEditor:** enhance JSON serialization for UnifyItemCluster and related classes ([e46ad93](https://github.com/chemodun/X4-UniverseEditor/commit/e46ad93c285518ac8e79780fe03a9b08e213c7d5))
* **GalaxyEditor:** implement PostInit method for initialization and state management in GalaxyModInfo classes ([d920164](https://github.com/chemodun/X4-UniverseEditor/commit/d9201646989fc83482abeb8ca5ae44a71286d0ba))
* **GalaxyEditor:** implement UnifyItemCluster class for enhanced cluster management and initialization ([1340ec7](https://github.com/chemodun/X4-UniverseEditor/commit/1340ec7088c32bc6fb30a99d7ec5be804c883892))
* **GalaxyMod:** add Clusters property to manage UnifyItemCluster list ([cb809ec](https://github.com/chemodun/X4-UniverseEditor/commit/cb809ec8341916d43ee87941d51551a75b27bf48))
* **GalaxyMod:** add TemplateConfig for dynamic cluster and sector ID generation ([33407f2](https://github.com/chemodun/X4-UniverseEditor/commit/33407f2d9e66e745fb40eb6551bab7b47a6984f8))
* **GalaxyUnify:** add group attribute to GalaxyUnifyItemAttribute and implement IsHasValue and IsReady methods for validation ([46a4cd8](https://github.com/chemodun/X4-UniverseEditor/commit/46a4cd8aad7b045e5822983705b107ea2c7c7d3a))
* **GalaxyUnifyBase:** add isMandatory field for UnifyAttribute ([ab39e84](https://github.com/chemodun/X4-UniverseEditor/commit/ab39e8462fb6ef9a700bfba7abab65cd2c9f49c0))
* **GalaxyUnifyBase:** add IsModified and isUnset methods for attribute state checks ([697ab90](https://github.com/chemodun/X4-UniverseEditor/commit/697ab90c2f2ec75a9625d2d757b8d8aff6a8b9d2))
* **GalaxyUnifyBase:** implement UpdateFrom methods for GalaxyUnifyItem and GalaxyUnifyItemAttribute, enhance list item handling ([6d1631c](https://github.com/chemodun/X4-UniverseEditor/commit/6d1631c55b9ae5200f6dfd6a0dafed707a65f6dd))
* **GalaxyUnifyCluster:** add GetCluster method to initialize and populate cluster attributes ([b5506d0](https://github.com/chemodun/X4-UniverseEditor/commit/b5506d0db827bd0dbf24a96c3997979f2da31a6e))
* **GalaxyUnifyCluster:** add search methods for clusters by ID and position ([384f6ec](https://github.com/chemodun/X4-UniverseEditor/commit/384f6ec562aabe4856ea6e415d376aed4df40296))
* **GalaxyUnify:** enhance attribute initialization with mandatory field support and update serialization methods ([16958cb](https://github.com/chemodun/X4-UniverseEditor/commit/16958cba72e780a4a8303cad9f5a82c558a866f7))
* **GalaxyUnify:** implement CopyFrom method for item attribute copying and enhance JSON reading with type mappings ([5b0a3a8](https://github.com/chemodun/X4-UniverseEditor/commit/5b0a3a8dfb51b777e10b537e646acfeeb7b1aaa5))
* **MainWindow:** add AutoLoadLatestMod feature for automatic mod loading on startup ([8ebcb9e](https://github.com/chemodun/X4-UniverseEditor/commit/8ebcb9e562970a38117c9fe30546c55b002b9551))
* **MainWindow:** First try to create sector on map ([760e771](https://github.com/chemodun/X4-UniverseEditor/commit/760e771fafe88c741dd7598fefff3f284bdf91aa))


### Bug Fixes

* **ClusterEditWindow:** change EditVisibility to use Collapsed and add confirmation prompt on cancel ([fdf3bd9](https://github.com/chemodun/X4-UniverseEditor/commit/fdf3bd9e1feb6d0e31108627bb092e7823170fd1))
* **ClusterEditWindow:** correct deserialization type for clusters in JSON processing ([2502f2d](https://github.com/chemodun/X4-UniverseEditor/commit/2502f2d8a1f61da3739b206491d4dbe49130d924))
* **ClusterEditWindow:** position set on cluster add ([f19038b](https://github.com/chemodun/X4-UniverseEditor/commit/f19038be8eac7320639f99f9f78b2c892c16e09d))
* **GalaxyUnify:** make GetItem return an UnifyItem and add GetAttribute to return and attribute ... ([943b84c](https://github.com/chemodun/X4-UniverseEditor/commit/943b84c96dc6cdb668a275400c5cb47e609dee19))


### Code Refactoring

* **App:** enhance NLog configuration handling to check for changes before applying updates ([57a1d97](https://github.com/chemodun/X4-UniverseEditor/commit/57a1d97853ec20790763facd9184294f3051478d))
* **ClusterEditWindow:** rearrange UI elements for improved layout and accessibility ([260fc18](https://github.com/chemodun/X4-UniverseEditor/commit/260fc187c0da89e4e14ff09660b3c0013147f99f))
* **ClusterEditWindow:** replace IsChanged binding with IsReady for Save button and implement IsReady property for validation ([4d2229d](https://github.com/chemodun/X4-UniverseEditor/commit/4d2229d96273dfeeb9ece4a23fb5bed1ba29e82c))
* **ClusterEditWindow:** update bindings to use Cluster properties for improved data management ([8d07ba0](https://github.com/chemodun/X4-UniverseEditor/commit/8d07ba0b83a55fbb14009a8e2d3acb06aeb904c0))
* **ClusterEditWindow:** use CopyFrom instead of Update from, if UnifyCluster is exists ([b424ce7](https://github.com/chemodun/X4-UniverseEditor/commit/b424ce7e8ce456b6a06c1bfc3c4c6d8f14308ada))
* **GalaxyEditor:** add ViewCluster functionality and refactor to use text references for Sun and Environment ([9709706](https://github.com/chemodun/X4-UniverseEditor/commit/970970673d4134ba3497fb63d1bb3da2780db96b))
* **GalaxyEditor:** enhance GalaxyUnifyItemAttribute to support multiple value types and improve state management ([1ed6709](https://github.com/chemodun/X4-UniverseEditor/commit/1ed670946f0a1aa0d60eec72a9620f6fa8a6c37c))
* **GalaxyEditor:** remove unused using directives from GalaxyUnifyBase and GalaxyUnifyPlanet for cleaner code ([e8091e9](https://github.com/chemodun/X4-UniverseEditor/commit/e8091e99b9ed3a7ea67a081266109e9f14bcadcc))
* **GalaxyEditor:** rename files for  UnifyItem classes to streamline codebase ([86090a7](https://github.com/chemodun/X4-UniverseEditor/commit/86090a7dab7297944a9a4164d7d08e93b53077f5))
* **GalaxyEditor:** return back separate lists for attributes and items in GalaxyUnifyItemAttribute ([4e5d7fd](https://github.com/chemodun/X4-UniverseEditor/commit/4e5d7fde48c574327d0ab955e50152b75ba84951))
* **GalaxyEditor:** simplify JSON read/write methods in GalaxyUnify classes for improved clarity and maintainability ([fe78101](https://github.com/chemodun/X4-UniverseEditor/commit/fe78101742b85b8ab983e0b76ff63f0105ad814e))
* **GalaxyEditor:** update  GalaxyUnifyItemJsonConverter and GalaxyUnifyItemAttributeConverter ([e6011d2](https://github.com/chemodun/X4-UniverseEditor/commit/e6011d290da4e21bd541c5c5d47e6adccf1fa6c5))
* **GalaxyEditor:** update GalaxyUnifyItemAttribute and GalaxyUnifyItem to use nullable types for better handling of optional values ([e6011d2](https://github.com/chemodun/X4-UniverseEditor/commit/e6011d290da4e21bd541c5c5d47e6adccf1fa6c5))
* **GalaxyEditor:** update GalaxyUnifyItemAttribute to support new Attribute type and streamline list handling. There a ValueList can handle as attributes as well an items too ([ee4121d](https://github.com/chemodun/X4-UniverseEditor/commit/ee4121dbb854e3db232202cc011982de36c5f1d0))
* **GalaxyEditor:** update GetList method to return a list of GalaxyUnifyItemAttribute and introduce GetListOfItems for improved item retrieval ([5304a4f](https://github.com/chemodun/X4-UniverseEditor/commit/5304a4ff1b04ed25c6f2d812d6c31c69c11111a2))
* **GalaxyMod:** replace FolderBrowserDialog with OpenFolderDialog ([abaf8ba](https://github.com/chemodun/X4-UniverseEditor/commit/abaf8ba79e4173460ea44fc7b18bc758d64e4d9c))
* **GalaxyMod:** update modAttributesToSave initialization syntax ([35011fa](https://github.com/chemodun/X4-UniverseEditor/commit/35011fad824ebc8a05a07ee5ec51a22db7f72626))
* **GalaxyReferenceCatalogs:** replace integer ID properties with text references for improved clarity and functionality ([97fbeb7](https://github.com/chemodun/X4-UniverseEditor/commit/97fbeb722abc30802f2a181915cab662a17a6742))
* **GalaxyUnifyCluster:** add FromEmptyCell flag to track initialization from an empty cell ([f8f79d9](https://github.com/chemodun/X4-UniverseEditor/commit/f8f79d9f6079dd6888d25aed7e3795c2159d81a6))
* **GalaxyUnify:** consolidate attribute setting methods into a single Set method for improved code clarity and maintainability ([943b84c](https://github.com/chemodun/X4-UniverseEditor/commit/943b84c96dc6cdb668a275400c5cb47e609dee19))
* **GalaxyUnifyItemAttribute:** streamline JSON read/write methods and improve attribute handling ([edeaf4b](https://github.com/chemodun/X4-UniverseEditor/commit/edeaf4be2f1658fbcfc02c5313e3c61bb28e2814))
* **GalaxyUnifyPlanet:** update planet attributes define a group for name attributes ([a952f12](https://github.com/chemodun/X4-UniverseEditor/commit/a952f12ca22f24beaed06c8eafb9e6b859650f67))
* **GalaxyUnify:** replace integer properties with string references for improved clarity in clusters and planets ([8bb08a9](https://github.com/chemodun/X4-UniverseEditor/commit/8bb08a9469964c5add76d0d01a7d4d348e53a1aa))
* **GalaxyUnify:** update attribute types and improve JSON serialization methods for clusters and planets ([08c6915](https://github.com/chemodun/X4-UniverseEditor/commit/08c6915449605306f8feff8d6d2b7ea03d1e135f))
* **MainWindow.xaml:** update sector size bindings for internal size limits ([63b24df](https://github.com/chemodun/X4-UniverseEditor/commit/63b24df279f3a12dcaf36074f8249502fc9800a0))
* **MainWindow:** make edit and delete cluster available only for the newly created. Implement deleting new cluster ([31a89b5](https://github.com/chemodun/X4-UniverseEditor/commit/31a89b56cfa947d5fcc712f58b4ca2bcfe93bfb2))
* **MainWindow:** optimize JsonSerializerOptions usage ([35011fa](https://github.com/chemodun/X4-UniverseEditor/commit/35011fad824ebc8a05a07ee5ec51a22db7f72626))
* **MainWindow:** remove NLog configuration calls from property setters to streamline logging setup ([7f7960d](https://github.com/chemodun/X4-UniverseEditor/commit/7f7960d84aa0c57719c2c0f0523980f33b8f04b7))
* **MainWindow:** remove SectorRadius parameter from GalaxyMapViewer connection ([9e7e415](https://github.com/chemodun/X4-UniverseEditor/commit/9e7e41537e6c8b65ce74515b959717f4ac240550))
* **MainWindow:** replace FolderBrowserDialog with OpenFolderDialog ([abaf8ba](https://github.com/chemodun/X4-UniverseEditor/commit/abaf8ba79e4173460ea44fc7b18bc758d64e4d9c))
* **MoonEditWindow:** improve XAML formatting for better readability and maintainability ([d957298](https://github.com/chemodun/X4-UniverseEditor/commit/d9572983e810ac81071cfe6539f05c69b066132f))
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
