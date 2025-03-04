# Changelog

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
