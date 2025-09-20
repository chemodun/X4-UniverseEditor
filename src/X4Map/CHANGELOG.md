# Changelog

## [0.6.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.6.0...X4Map@v0.6.1) (2025-09-20)


### Bug Fixes

* **GalaxyMapViewer:** improve gate connection logic, fixes Bug in Gate matching Logic, i.e. Fixes [#140](https://github.com/chemodun/X4-UniverseEditor/issues/140) ([0ac01e7](https://github.com/chemodun/X4-UniverseEditor/commit/0ac01e787778f5c064521bdc4ce84564b3e7d21a))


### Code Refactoring

* **GalaxyMapSector:** expose Items property for sector items ([0ac01e7](https://github.com/chemodun/X4-UniverseEditor/commit/0ac01e787778f5c064521bdc4ce84564b3e7d21a))
* **GalaxyMapViewer:** simplify DLC processing logic ([e3cda97](https://github.com/chemodun/X4-UniverseEditor/commit/e3cda97157e359b1d16b14c5d5f321ccb7243e9c))
* **GalaxyMapViewer:** simplify GetUnprocessedClusters logic ([c15a326](https://github.com/chemodun/X4-UniverseEditor/commit/c15a3269b42b43417ff090607ad4d96019c802f7))
* **GalaxyMapViewer:** simplify PNG export logic ([d3af168](https://github.com/chemodun/X4-UniverseEditor/commit/d3af168474c0b8a3e5c3309ce785e4c773bbfb06))
* **GalaxyMapViewer:** update layout handling during PNG export ([c9d1dbb](https://github.com/chemodun/X4-UniverseEditor/commit/c9d1dbb87ee12b4b46bbcdfac563f601f3b9c481))
* **GalaxyMapWindow:** refactor busy overlay during export process ([d3af168](https://github.com/chemodun/X4-UniverseEditor/commit/d3af168474c0b8a3e5c3309ce785e4c773bbfb06))

## [0.6.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.5.3...X4Map@v0.6.0) (2025-07-18)


### Features

* **GalaxyMapSector:** add rotation functionality to image rendering with TransformGroup ([33dfe6d](https://github.com/chemodun/X4-UniverseEditor/commit/33dfe6d913a8540afb33b1864aac593df9f696dd))
* **SectorMap, SectorObject:** add rotation functionality for sector map items ([0e0480b](https://github.com/chemodun/X4-UniverseEditor/commit/0e0480b9abd15b89794813e02f2c8eee4c8d0d4d))


### Code Refactoring

* **SectorMap:** enhance tooltip for gate objects with pitch information ([873bd49](https://github.com/chemodun/X4-UniverseEditor/commit/873bd49bce42366117183fd90a642a66c7b6b179))

## [0.5.3](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.5.2...X4Map@v0.5.3) (2025-07-18)


### Code Refactoring

* **GalaxyMapViewer:** add unprocessed cluster logging, which can happened if cluster has coordinates not aligned to the hexagonal map ([c43e8fa](https://github.com/chemodun/X4-UniverseEditor/commit/c43e8fa91741da10b411822630bcc732a3515872))

## [0.5.2](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.5.1...X4Map@v0.5.2) (2025-03-05)


### Code Refactoring

* **GalaxyMap:** enhance UI to display mod versions alongside names ([c39d927](https://github.com/chemodun/X4-UniverseEditor/commit/c39d9276b578212f42efce99bdde94f1dc1baf19))

## [0.5.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.5.0...X4Map@v0.5.1) (2025-03-04)


### Code Refactoring

* **Constants:** update sector size constants for improved map configuration ([22d3a12](https://github.com/chemodun/X4-UniverseEditor/commit/22d3a12b3d3c378e0b69489cc503ac6159015770))
* **GalaxyMap:** update sector creation methods to return internal size and enhance logging ([82d96a5](https://github.com/chemodun/X4-UniverseEditor/commit/82d96a5389d0a76f65edf6577e649c7689362f0a))
* **GalaxyMapViewer:** replace sector radius parameter with minimum internal size constant ([116fed3](https://github.com/chemodun/X4-UniverseEditor/commit/116fed31df3cb925bdba50caaefeb574b7df8c94))
* **SectorMap, SectorObject:** enhance internal size management and add max coordinate calculation ([2bf8b4b](https://github.com/chemodun/X4-UniverseEditor/commit/2bf8b4b753dc2a1393bc91d85446086e07cd40e9))

## [0.5.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.4.0...X4Map@v0.5.0) (2025-02-24)


### Features

* **ClusterEditWindow:** add position parameter to Cluster initialization and update related methods ([ff5a253](https://github.com/chemodun/X4-UniverseEditor/commit/ff5a253cd7569d9445e8345c193278608ed0c6e0))
* **GalaxyMapCluster:** implement ReAssign method for cluster visualization and tooltip updates ([f57bb7c](https://github.com/chemodun/X4-UniverseEditor/commit/f57bb7c075b4277359ee99598c5716a224d9ef00))


### Code Refactoring

* **GalaxyMapCluster:** enhance tooltip display for new clusters with improved styling and alignment ([90915e1](https://github.com/chemodun/X4-UniverseEditor/commit/90915e11cd69f5fd717b1fc27ae5fead2a297681))

## [0.4.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.3.2...X4Map@v0.4.0) (2025-02-19)


### Features

* **GalaxyEditor:** add right-click event handling for galaxy map elements with empty context menu ([7ad9055](https://github.com/chemodun/X4-UniverseEditor/commit/7ad90558777d477f6fc74854adc52161f89498e1))


### Bug Fixes

* **X4Map:** initialize MapInfo to default values in GalaxyMapViewer ([11020dc](https://github.com/chemodun/X4-UniverseEditor/commit/11020dc1bd2f567d5e414e951070d9507e0b6b01))


### Code Refactoring

* **X4Map:** encapsulate map boundaries and positions in MapInfo and MapPosition classes ([e6061e4](https://github.com/chemodun/X4-UniverseEditor/commit/e6061e441adc3bbde65dd7c106b4ce21554536ee))
* **X4Map:** move GalaxyMapCluster and GalaxyMapSector into separate files ([f6508ea](https://github.com/chemodun/X4-UniverseEditor/commit/f6508eabd573ab32e96cfdff3e8a4ec7a7450400))

## [0.3.2](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.3.1...X4Map@v0.3.2) (2025-02-18)


### Code Refactoring

* **GalaxyMapViewer:** streamline ans simplify pressed hexagon(cluster, sector) handling and update event argument names for clarity ([150f81d](https://github.com/chemodun/X4-UniverseEditor/commit/150f81ddf509c7c9d57d98f164b8abbc51fd883f))

## [0.3.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.3.0...X4Map@v0.3.1) (2025-02-18)


### Bug Fixes

* **GalaxyMapViewer:** update event handlers to use GalaxyCanvas instead wrongly used GalaxyMapViewer for improved interaction ([d157a36](https://github.com/chemodun/X4-UniverseEditor/commit/d157a3662b9fea2239823686db11b358a57a2ca4))

## [0.3.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.2.1...X4Map@v0.3.0) (2025-02-17)


### Features

* **GalaxyMapViewer:** add editor mode support and enhance hexagon selection logic ([bcf1891](https://github.com/chemodun/X4-UniverseEditor/commit/bcf1891dc72f17c75b9189e44a3f80dbd3666b5c))
* **GalaxyMapViewer:** class to display Galaxy Map in Scroll Viewer ([16b3483](https://github.com/chemodun/X4-UniverseEditor/commit/16b3483e8ea901dba54929f96392f40e22659135))
* **GalaxyMapViewer:** enhance sector and cluster selection logic with new properties and events ([6080a8d](https://github.com/chemodun/X4-UniverseEditor/commit/6080a8dbb28f4250826f36205294e2072566a3be))
* **GalaxyMapViewer:** expose ColumnWidth and RowHeight as public static properties and add Column/Row to tooltips ([ffcad19](https://github.com/chemodun/X4-UniverseEditor/commit/ffcad19b237ba9927118a64c8630a4e037197df0))
* **GalaxyMapViewer:** implement panning and zooming functionality with mouse events ([8310c02](https://github.com/chemodun/X4-UniverseEditor/commit/8310c02d585213bcbecb46f19da3ba6ea5323254))


### Code Refactoring

* **GalaxyMapViewer:** enhance visibility management for DLCs and Mods options ([cbb92dc](https://github.com/chemodun/X4-UniverseEditor/commit/cbb92dc97a25fdf46e3d6b6fe377844e0e4f1d7f))
* **GalaxyMapViewer:** make it settable via xaml ([b484449](https://github.com/chemodun/X4-UniverseEditor/commit/b484449f7ecbbfa66d7aecb7d40e35be347e6088))
* **GalaxyMapViewer:** replace DominantOwnerColor with Color for consistency in color handling ([941a7c1](https://github.com/chemodun/X4-UniverseEditor/commit/941a7c1502d0384b152782ee90c7073729be9899))
* **GalaxyMapViewer:** some  GalaxyMapViewer usage in child objects is changed ([7d6dd7a](https://github.com/chemodun/X4-UniverseEditor/commit/7d6dd7a7bc2604e78f2fa9be4eac47939b121946))
* **GalaxyMapViewer:** update tooltip items syntax for consistency and clarity ([bb7fb89](https://github.com/chemodun/X4-UniverseEditor/commit/bb7fb89c62735ab39e3fa8901d0d332b06ffe5db))
* **SectorMap:** remove FactionColors dependency and streamline color assignment logic ([dbf5148](https://github.com/chemodun/X4-UniverseEditor/commit/dbf51480a1ec92b337d77fb7bd2a8bcdc1d8189b))


### Miscellaneous Chores

* **SectorMap:** add Remove method to facilitate canvas image removal ([aa5d452](https://github.com/chemodun/X4-UniverseEditor/commit/aa5d452162bc6c7f2cb13652acb9e6452c931bcc))
* **SectorMap:** add StationOwner attribute to sector objects and display owner in info ([c176d40](https://github.com/chemodun/X4-UniverseEditor/commit/c176d403f7e0d51679797beb4f65a4e9b8ddb85c))

## [0.2.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map@v0.2.0...X4Map@v0.2.1) (2025-02-15)


### Code Refactoring

* **X4Map:** move sector size limits to MapConstants and refactor SectorMap ([4a2808a](https://github.com/chemodun/X4-UniverseEditor/commit/4a2808a5baf43bff3452e27fe0843a84282334e6))

## [0.2.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4Map-v0.1.0...X4Map@v0.2.0) (2025-02-14)


### Features

* **X4Map:** add initial project files and versioning setup ([d22fd21](https://github.com/chemodun/X4-UniverseEditor/commit/d22fd21aee7475b2ff294c9a41b45951562f3142))


### Code Refactoring

* **X4Map:** move from ChemGateBuilder: SectorMap.cs as whole and some related objects, including HexagonPointsConverter ([d015bb2](https://github.com/chemodun/X4-UniverseEditor/commit/d015bb21b037cdc009f25b7c48a84a8c5cf44412))
