# Changelog

## [0.3.2](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.3.1...X4DataLoader@v0.3.2) (2025-02-05)


### Bug Fixes

* **Translation:** replace dictionary initialization with array and optimize regex usage ([3daddc4](https://github.com/chemodun/X4-UniverseEditor/commit/3daddc47093a3def65af7bb1ef965d2c0147d27c))

## [0.3.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.3.0...X4DataLoader@v0.3.1) (2025-02-05)


### Bug Fixes

* **DataLoader:** remove duplication of code for gathering a file sets. And fix real file name storing in set ([9b2f30f](https://github.com/chemodun/X4-UniverseEditor/commit/9b2f30fd809a268c07987ff8fab4bc5d1d8c63e8))
* **Zone:** add macro element with reference and connection attributes to PositionXML ([37d6644](https://github.com/chemodun/X4-UniverseEditor/commit/37d6644194d77bc55c4d210f1938537b0e9a2bde))


### Code Refactoring

* **DataLoader:** replace Dictionary initializations with array literals ([b2aa20a](https://github.com/chemodun/X4-UniverseEditor/commit/b2aa20a1559edd03ed626b163c1cc180459470af))

## [0.3.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.2.0...X4DataLoader@v0.3.0) (2025-02-05)


### Features

* **MainWindow:** enhance UI with tab names and data binding improvements; add window loaded event ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))


### Bug Fixes

* **DataLoader:** log warning instead of throwing exception for missing files ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))
* **LoadX4Data:** Replace mandatory data load by warning and switching the ribbon on options tab ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))

## [0.2.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.1.1...X4DataLoader@v0.2.0) (2025-02-04)


### Features

* **Colors:** Add X4Color and X4MappedColor classes for color management and loading from XML ([5c8cd51](https://github.com/chemodun/X4-UniverseEditor/commit/5c8cd513c4ad8a0c68ed72fb5c0c7f962165bbe9))
* **Colors:** Replace Hex property with Color object for improved color management ([1878daa](https://github.com/chemodun/X4-UniverseEditor/commit/1878daae482d39782f87752ef11af1791284ec9e))
* **Connection, Zone:** enhance connection handling with Create method to create appropriate XML structures ([49331dc](https://github.com/chemodun/X4-UniverseEditor/commit/49331dc079c3b9839f153086bc9e056158d21070))
* **Connections:** add GetCoordinates method to retrieve position as float array ([49df004](https://github.com/chemodun/X4-UniverseEditor/commit/49df0040a22291ef23db5668c8efa6b8e7c19480))
* **DataLoader:** Add duplicate ID and name checks for construction plans, factions, station categories, groups, modules, and module groups ([66c7ebf](https://github.com/chemodun/X4-UniverseEditor/commit/66c7ebfa735a08a93cf3bfac72d4f690540adbbd))
* **Faction:** Implement Faction class and integrate faction loading into data processing and sectors ownership ([e65fc6b](https://github.com/chemodun/X4-UniverseEditor/commit/e65fc6bc4ff6a11216a56e47fdeb68050f5c0e32))
* **Galaxy:** add methods to retrieve clusters and sectors ([08ff854](https://github.com/chemodun/X4-UniverseEditor/commit/08ff854b2b5ddde79faf5eb2e0cb2eec2cf9e41d))
* **GateConnection:** add GateMacro and IsActive properties with XML parsing for gate connections ([00979ba](https://github.com/chemodun/X4-UniverseEditor/commit/00979ba62d7a2303b08d309736d0812344af315a))
* **Race:** Add Race class and implement loading logic for races from XML ([17d6ed8](https://github.com/chemodun/X4-UniverseEditor/commit/17d6ed8a8a88511f17265d102d133c5a3b2810c0))
* **Sector:** add GetConnection method to retrieve connection by ID ([a418c88](https://github.com/chemodun/X4-UniverseEditor/commit/a418c88c268fc3123a13d3209824033b9c351208))
* **Sector:** Add methods to retrieve stations by tag or type ([d600167](https://github.com/chemodun/X4-UniverseEditor/commit/d600167742cb9e770319c085ba72448bb42808a5))
* **Station:** Add Name property and get it from construction plan ([3789440](https://github.com/chemodun/X4-UniverseEditor/commit/37894408f2462da0455320f00a4d4450af83b9f2))
* **X4DataLoader:** Enhance data loading by enhancing  Cluster Highway connection logic ([53ba6a9](https://github.com/chemodun/X4-UniverseEditor/commit/53ba6a975215cf64759ea4780ecf558c6ac84bdf))
* **X4DataLoader:** Implement Station class and enhance data loading for stations in the galaxy from god.xml. Sector DominantOwner is calculated ([de3e835](https://github.com/chemodun/X4-UniverseEditor/commit/de3e8356377037aa1833af55796144a1e8aa8f36))


### Bug Fixes

* **Connections:** Fix Sector to Cluster connection processing ([ea3800d](https://github.com/chemodun/X4-UniverseEditor/commit/ea3800db5d65a6a0f3b9154b0617ccfe46a8a44e))
* **Station:** Correct owner name assignment logic to ensure proper prefix handling ([b6615c5](https://github.com/chemodun/X4-UniverseEditor/commit/b6615c54f0143440c5d26ad8e17a77458b5bb6da))


### Code Refactoring

* **Connections:** Override ToString methods for Position and Quaternion classes to improve debugging output ([b3f7762](https://github.com/chemodun/X4-UniverseEditor/commit/b3f7762bf9e6e69e82f05cc4282928e8ed67f57c))
* **Connections:** Update variable declarations to use nullable types for improved safety ([8f01ace](https://github.com/chemodun/X4-UniverseEditor/commit/8f01aced8da6bc61a7e32a0b6898dd4a06440b7e))
* **DataLoader:** Add error handling for XML loading in DataLoader methods ([c4a26e8](https://github.com/chemodun/X4-UniverseEditor/commit/c4a26e81a81d70bdf09cc4518094645081549291))
* **DataLoader:** Implement GatherFiles method for improved file organization and logging ([7e4027e](https://github.com/chemodun/X4-UniverseEditor/commit/7e4027e3d3741c8b608d77b57e4f2a14827d7824))
* **DataLoader:** Improve DLC folder scanning and logging for file identification ([0493c62](https://github.com/chemodun/X4-UniverseEditor/commit/0493c62f352d52ef6a3668533fd1a60d9c0c52a4))
* **DataLoader:** Improve variable declarations for clarity and consistency ([38013ed](https://github.com/chemodun/X4-UniverseEditor/commit/38013edb3f270a78e1fc93369d61d16f02dda777))
* **Galaxy:** add method to retrieve all opposite sectors from connections ([d33d0e9](https://github.com/chemodun/X4-UniverseEditor/commit/d33d0e9500e3fc425ad23ce5d8d269c31571f103))
* **Galaxy:** add Sectors property and update sector retrieval methods for improved data handling ([5a2d463](https://github.com/chemodun/X4-UniverseEditor/commit/5a2d463c4895516b71697ea93ff048d9a2cdb040))
* **GalaxyConnection:** refactor initialization and loading methods; add Create method for connection setup as for GalaxyConnection as for GalaxyConnectionPath ([3c97541](https://github.com/chemodun/X4-UniverseEditor/commit/3c97541d370fd8fca3b4acaf82ece47425c4778e))
* **Galaxy:** Enhance LoadConnections method to support additional zones, to make it possible to use it for the mods data load ([5bfce9e](https://github.com/chemodun/X4-UniverseEditor/commit/5bfce9e2c3065a52c10ba98d2e6f166ac986d491))
* **Galaxy:** implement method to retrieve opposite sector for gate connections ([d89c0df](https://github.com/chemodun/X4-UniverseEditor/commit/d89c0df4a01177b9d475a513c8ba964298dbc969))
* **GalaxyMapWindow:** Update background color to LightGray and improve variable declarations ([b367d6f](https://github.com/chemodun/X4-UniverseEditor/commit/b367d6fb94c63d227688febae18229ff8552ee96))
* **Highway:** Add Source and FileName properties to HighwayPoint for enhanced data tracking ([ad8109b](https://github.com/chemodun/X4-UniverseEditor/commit/ad8109bfcefa6f0b3c316fe099c51cadb18d7a08))
* **HighwayClusterLevel:** Change EntryPointPath and ExitPointPath to public access modifiers ([3fafeaa](https://github.com/chemodun/X4-UniverseEditor/commit/3fafeaa62470836627916fac989d479e22ce94ed))
* **Highway:** Enhance connection logic for entry and exit points in Highway class ([50e10c5](https://github.com/chemodun/X4-UniverseEditor/commit/50e10c5454ec9442e958476794724f369d7bbcbd))
* **Highway:** Exchange entry and exit point handling in HighwayClusterConnectionPath logic ([09a624f](https://github.com/chemodun/X4-UniverseEditor/commit/09a624f68d277b7b9587f63354cfb6a941fed166))
* **Logging:** Integrate logging functionality throughout data loading process ([caff28e](https://github.com/chemodun/X4-UniverseEditor/commit/caff28e3a727a11be1fa9c6d69da96c5ba33bc1a))
* Replace parsing logic with StringHelper methods for improved readability and error handling ([7a7e9fc](https://github.com/chemodun/X4-UniverseEditor/commit/7a7e9fc228e14ae3a27e8d841f021dd068eb93da))
* **Sector, Cluster, Zone:** Update properties to public and simplify initialization ([ea3800d](https://github.com/chemodun/X4-UniverseEditor/commit/ea3800db5d65a6a0f3b9154b0617ccfe46a8a44e))
* **Sector:** Remove unused owner and type ignore lists from ownership calculation ([c4b2a0e](https://github.com/chemodun/X4-UniverseEditor/commit/c4b2a0ea3c313ea766867a5899b32cdb5d8591c1))
* **Station:** Clarify comment regarding refId usage in station category assignment ([49164a1](https://github.com/chemodun/X4-UniverseEditor/commit/49164a1aee2111066ab19968ab39d98e036f423a))
* **Station:** Enhance owner name assignment logic to include primary race name prefix handling ([b2cf2e1](https://github.com/chemodun/X4-UniverseEditor/commit/b2cf2e1f79b38176e9c6cc0380f5ceebef1f9f29))
* **Station:** Enhance station category assignment logic with refId handling and add macro property ([1944dc3](https://github.com/chemodun/X4-UniverseEditor/commit/1944dc3cfd72a97074f6af2664f39eb71566c7b3))
* **Station:** Improve position and rotation parsing with TryParse for better error handling and from right XMLElement ([7c84d0e](https://github.com/chemodun/X4-UniverseEditor/commit/7c84d0e92303c012c8ff1241e69d39b1d3acfba8))
* **Station:** Refactor owner property to OwnerId and add OwnerName; update loading logic ([2bfa888](https://github.com/chemodun/X4-UniverseEditor/commit/2bfa88868e387e15cefd9775ddcacb7d8aa8dbdf))
* **X4DataLoader:** Integrate centralized logging utility across multiple classes ([10e8fea](https://github.com/chemodun/X4-UniverseEditor/commit/10e8feaf674dd3c521c7619a24f19fe5ce91e028))
* **X4DataLoader:** Introduce StationModule, StationModuleGroup, StationCategory, StationGroup, ConstructionPlans to get dig to the "claiming" possibility of stations ([3075296](https://github.com/chemodun/X4-UniverseEditor/commit/3075296c637e0e4debb6a0390b49efe7aa0c6592))
* **X4DataLoader:** refactor zone handling to use PositionId, Position and Position XML instead ConnectionId and add AddZone method to set these values ([7a5085a](https://github.com/chemodun/X4-UniverseEditor/commit/7a5085a7fbc91b65e1cb9485a16d36406e65740d))
* **X4DataLoader:** update Connections classes, Zone and Sector classes to use Position class and improve initialization ([696ee56](https://github.com/chemodun/X4-UniverseEditor/commit/696ee56d399bcd742155c8581b082f335c49cf54))
* **X4DataLoader:** Update properties to support nullable types for better null handling ([0364536](https://github.com/chemodun/X4-UniverseEditor/commit/03645364b82f98ca1915971cff0aa327f97e98c4))
* **X4Data:** Update data loading logic and introduce version handling from patchactions.xml ([ce5efde](https://github.com/chemodun/X4-UniverseEditor/commit/ce5efdeba84d62b29323ddc11844a151b6827fb3))
* **XmlHelper:** Add method to retrieve attribute values as a list from XElement ([6dc339a](https://github.com/chemodun/X4-UniverseEditor/commit/6dc339aed113395eb78406ac2e6c7437564f0baa))
* **XmlHelper:** Enhance GetAttributeAsList method to accept a custom separator for splitting attribute values ([e18bd6b](https://github.com/chemodun/X4-UniverseEditor/commit/e18bd6bfd34a09e2dee04734c79f68e9fa475cde))
* **XmlHelper:** Improve variable declarations for clarity and consistency ([88adda4](https://github.com/chemodun/X4-UniverseEditor/commit/88adda4d94866f4da3b60c3e2f19f0f6775eba08))

## [0.1.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.1.0...X4DataLoader@v0.1.1) (2025-01-19)


### Code Refactoring

* rename Class1 to X4Galaxy and update LoadData method to return Galaxy object ([97eda19](https://github.com/chemodun/X4-UniverseEditor/commit/97eda1922194f98ca09979e5698c6607d4843c4b))

## [0.1.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.0.1...X4DataLoader@v0.1.0) (2025-01-18)


### Features

* implemented load data not only from "vanilla", but from an extensions too, with storing information about sourcing of data ([87b4390](https://github.com/chemodun/X4-UniverseEditor/commit/87b4390125b57f12c117588c66b92b6ea959310a))


### Code Refactoring

* Organize helper methods into a new Helpers namespace and improve string comparison logic ([87b4390](https://github.com/chemodun/X4-UniverseEditor/commit/87b4390125b57f12c117588c66b92b6ea959310a))


### Documentation

* Change CHANGELOG.md location for X4DataLoader ([084abdc](https://github.com/chemodun/X4-UniverseEditor/commit/084abdce0bb14916d3c86dd70cda8b2475e100cf))

## 0.0.1 (2025-01-18)


### Features

* add source and filename properties to data loader classes as preparation for loading extensions data ([b413d95](https://github.com/chemodun/X4-UniverseEditor/commit/b413d95a36575d3253506426434f95df8c7f61b4))
* Initial load of the Universe data for vanilla ([56cf3c8](https://github.com/chemodun/X4-UniverseEditor/commit/56cf3c894a58c2899f58cd01b84e02ce60d1bfdb))


### Code Refactoring

* update relativePaths to use tuple for path and fileName ([057e0f7](https://github.com/chemodun/X4-UniverseEditor/commit/057e0f7f3883722146ec26bb761c25e1a75e5154))


### Miscellaneous Chores

* release 0.0.1 ([7efa89e](https://github.com/chemodun/X4-UniverseEditor/commit/7efa89e5fefe14be0435dd40d1539eaee93c5070))
