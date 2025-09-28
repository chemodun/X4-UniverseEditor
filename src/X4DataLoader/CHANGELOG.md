# Changelog

## [0.11.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.6...X4DataLoader@v0.11.0) (2025-09-28)


### Features

* **PositionHelpers:** add PositionHelper class with IsSamePosition method ([8fc8c1a](https://github.com/chemodun/X4-UniverseEditor/commit/8fc8c1a9350ea9c1419732a65ca5dfe17c5afee8))


### Bug Fixes

* **Cluster:** update PositionSource assignment logic ([7e5f5f1](https://github.com/chemodun/X4-UniverseEditor/commit/7e5f5f1a495240ace1fad30f2b8728f56096ef05))


### Code Refactoring

* **Cluster:** update SetPosition method signature and logic ([7b793d7](https://github.com/chemodun/X4-UniverseEditor/commit/7b793d71264858eb6a981412ecf8fe658a978ac1))
* **DataLoader:** modify XML element handling ([6491f9d](https://github.com/chemodun/X4-UniverseEditor/commit/6491f9d732624d041fd64888d56467ba8967acf0))

## [0.10.6](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.5...X4DataLoader@v0.10.6) (2025-09-20)


### Code Refactoring

* **DataLoader:** enhance file collection and extension handling ([6108ddb](https://github.com/chemodun/X4-UniverseEditor/commit/6108ddb7ddee30c432becf8d657b13f32b247318))
* **DataLoader:** simplify file processing logic ([636fc0c](https://github.com/chemodun/X4-UniverseEditor/commit/636fc0c388cce04bfe43ddd122b2ff39fde2ff19))
* **DataLoader:** streamline content extraction logic ([892c3cb](https://github.com/chemodun/X4-UniverseEditor/commit/892c3cb13ce18b55c7b24d58d07146dacf0ee837))
* **Galaxy:** remove static DLCOrder list ([fdfe157](https://github.com/chemodun/X4-UniverseEditor/commit/fdfe157d9405ecb146111bcdfa0413a83703ec24))
* **Galaxy:** streamline XML loading ([636fc0c](https://github.com/chemodun/X4-UniverseEditor/commit/636fc0c388cce04bfe43ddd122b2ff39fde2ff19))
* **GameFilesStructureItem:** add countable property ([6108ddb](https://github.com/chemodun/X4-UniverseEditor/commit/6108ddb7ddee30c432becf8d657b13f32b247318))
* **Mod:** improve mod file processing ([6108ddb](https://github.com/chemodun/X4-UniverseEditor/commit/6108ddb7ddee30c432becf8d657b13f32b247318))
* **X4DataExtractionWindow:** replace ContentCopier with ContentExtractor ([892c3cb](https://github.com/chemodun/X4-UniverseEditor/commit/892c3cb13ce18b55c7b24d58d07146dacf0ee837))
* **X4DataExtractionWindow:** streamline extension loading logic ([6108ddb](https://github.com/chemodun/X4-UniverseEditor/commit/6108ddb7ddee30c432becf8d657b13f32b247318))

## [0.10.5](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.4...X4DataLoader@v0.10.5) (2025-07-18)


### Bug Fixes

* **DataLoader:** enhance file extraction with hash validation and BOM handling ([dd13f5a](https://github.com/chemodun/X4-UniverseEditor/commit/dd13f5acd2f99eb4373d913dae0650e3f5f822fd))

## [0.10.4](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.3...X4DataLoader@v0.10.4) (2025-03-05)


### Code Refactoring

* **data-loader:** add SourceName property and update method to handle source names ([3829f32](https://github.com/chemodun/X4-UniverseEditor/commit/3829f32c3e8ec99f6a62e201930360260dc5465e))

## [0.10.4](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.3...X4DataLoader@v0.10.4) (2025-03-05)


### Code Refactoring

* **data-loader:** add SourceName property and update method to handle source names ([3829f32](https://github.com/chemodun/X4-UniverseEditor/commit/3829f32c3e8ec99f6a62e201930360260dc5465e))

## [0.10.3](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.2...X4DataLoader@v0.10.3) (2025-03-04)


### Code Refactoring

* **Colors, Faction:** improve color handling by ensuring default alpha value and enhancing color loading logic directly, not via mapping colors ([4c0b95a](https://github.com/chemodun/X4-UniverseEditor/commit/4c0b95ae4ef61f417e65f41a26f89d531242e5c9))

## [0.10.2](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.1...X4DataLoader@v0.10.2) (2025-02-26)


### Code Refactoring

* **DataLoader:** add DefaultUniverseId constant for improved universe management ([509bdc7](https://github.com/chemodun/X4-UniverseEditor/commit/509bdc7f984b363a006ae7ec7ab757af513b92eb))
* **DataLoader:** widely use the ExtensionInfo class instead of ExtensionId string, to have more useful information in a logs ([9eed756](https://github.com/chemodun/X4-UniverseEditor/commit/9eed7561ba601623ded3e408c352c598a094f5d7))

## [0.10.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.10.0...X4DataLoader@v0.10.1) (2025-02-25)


### Bug Fixes

* **DataLoader:** fixed mods loading order calculation ([046af8c](https://github.com/chemodun/X4-UniverseEditor/commit/046af8cdf8ee4a9a7b4afd83c8eab0c211af8659))

## [0.10.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.9.0...X4DataLoader@v0.10.0) (2025-02-24)


### Features

* **DataLoader:** enhance mod loading with options for enabled mods and exclusions ([178c8e9](https://github.com/chemodun/X4-UniverseEditor/commit/178c8e9603e84f3e18b9cf63d24062423b696b7f))
* **DataLoader:** integrate ContentExtractor for improved file handling from catalog and directory ([02c65e2](https://github.com/chemodun/X4-UniverseEditor/commit/02c65e24c55cc391423b7ecd8312de51a62847f2))
* **Translation:** add ClearReference method to format reference IDs ([88308c7](https://github.com/chemodun/X4-UniverseEditor/commit/88308c7ef98ace2e2e99ff6283e2d6b92ba81cf3))


### Bug Fixes

* **Highway:** replace exceptions with warnings for EntryPointPath and ExitPointPath loading errors; improve zone error messages ([9546752](https://github.com/chemodun/X4-UniverseEditor/commit/9546752dc4a053960f787b171432d6de9d3a1dcb))


### Code Refactoring

* **Cluster, Planet:** replace integer ID properties with reference strings for improved clarity ([8cf8915](https://github.com/chemodun/X4-UniverseEditor/commit/8cf8915dca33095863c73ef3e40f5dc7c02f1058))
* **Cluster:** change properties from private to public setters for better accessibility ([e61875a](https://github.com/chemodun/X4-UniverseEditor/commit/e61875a01e30fb1151532884f196c12afb42c2da))
* **DataLoader:** streamline content extraction logic and improve folder handling ([a3d70df](https://github.com/chemodun/X4-UniverseEditor/commit/a3d70df00c131df9f075cfb81fb36c8f91e98481))

## [0.9.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.8.0...X4DataLoader@v0.9.0) (2025-02-20)


### Features

* **Galaxy:** add new DLC entry for mini expansion in Galaxy class ([f9dd9e2](https://github.com/chemodun/X4-UniverseEditor/commit/f9dd9e2c1262e4b6b3fff5aeb29414818a3ec96f))

## [0.8.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.7.0...X4DataLoader@v0.8.0) (2025-02-19)


### Features

* **Translation:** add methods for page-based translation and ID extraction ([2aaa8f3](https://github.com/chemodun/X4-UniverseEditor/commit/2aaa8f3c148d42da2231e793d4720e46c6624e70))
* **X4DataLoader:** add Moon and Planet classes for XML loading for Sectors ([3f665ab](https://github.com/chemodun/X4-UniverseEditor/commit/3f665ab082af9798d5770945c5a57a1425c8b8da))
* **X4DataLoader:** add MusicId property and extract music reference from XML ([58ce790](https://github.com/chemodun/X4-UniverseEditor/commit/58ce7906ee55713be3c3ee22364a8c6f96bde086))
* **X4DataLoader:** add support for loading sounds and icons from XML ([263ab8b](https://github.com/chemodun/X4-UniverseEditor/commit/263ab8b3d3ea97c7dea72a4cc9598266791d2668))
* **X4DataLoader:** add TranslateString method for nested reference resolution ([0147e99](https://github.com/chemodun/X4-UniverseEditor/commit/0147e997b009db7c5d11d54683202d68f8e4e447))


### Bug Fixes

* **X4DataLoader:** correct method name casing for LoadFromXML in Colors.cs ([8227d6c](https://github.com/chemodun/X4-UniverseEditor/commit/8227d6c87a9775eded163bbda7fc0930012b32ab))
* **X4DataLoader:** rename Settlement property to Settlements for consistency ([8d7f0d4](https://github.com/chemodun/X4-UniverseEditor/commit/8d7f0d41d8157d21fdf67dc6845199bce6782cb4))


### Code Refactoring

* **X4DataLoader:** enhance Cluster and Sector classes to utilize Galaxy for translation ([3f665ab](https://github.com/chemodun/X4-UniverseEditor/commit/3f665ab082af9798d5770945c5a57a1425c8b8da))


### Miscellaneous Chores

* **X4DataLoader:** enhance Cluster class with additional properties for translation and identification ([2215e08](https://github.com/chemodun/X4-UniverseEditor/commit/2215e086d55319a3e60a39f5e2b7540fda42373d))

## [0.7.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.6.2...X4DataLoader@v0.7.0) (2025-02-17)


### Features

* **DataLoader:** implemented processingOrder to have possibility to process some files several times for different data on different steps ([1cbf851](https://github.com/chemodun/X4-UniverseEditor/commit/1cbf851c1b2fb5e62b845cdee4c10b0cbb4689d9))


### Bug Fixes

* **DataLoader:** avoided the issue when the case in `mapdefaults.xml` is different from other files for `clusters` and `sectors` prevent the right work of a created mods. Now the trusted sources are -`galaxy.xml` for `clusters` and `clusters.xml` for `sectors`. I.e fixes #79. ([1cbf851](https://github.com/chemodun/X4-UniverseEditor/commit/1cbf851c1b2fb5e62b845cdee4c10b0cbb4689d9))


### Code Refactoring

* **Sectors:** now the `sectors` are assigned to `clusters` the right way, via `sector` connection, not via common parts in macros. ([1cbf851](https://github.com/chemodun/X4-UniverseEditor/commit/1cbf851c1b2fb5e62b845cdee4c10b0cbb4689d9))

## [0.6.2](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.6.1...X4DataLoader@v0.6.2) (2025-02-17)


### Bug Fixes

* **X4DataLoader:** add error handling and logging for GalaxyConnection loading ([384ccf2](https://github.com/chemodun/X4-UniverseEditor/commit/384ccf25d12200eecd5da74591f0b9b440d8fa81))
* **X4DataLoader:** enhance error handling and logging for Highway loading process ([28b4e87](https://github.com/chemodun/X4-UniverseEditor/commit/28b4e875daf52974f8cfdfe45fb9d391f5b19f65))
* **X4DataLoader:** log error instead of throwing exception for missing macro element in Gate zone ([f612f28](https://github.com/chemodun/X4-UniverseEditor/commit/f612f283220c3aaa07f6dbad4060ef24f0f852d5))


### Code Refactoring

* **DataLoader:** convert static class to instance class and enhance data loading with events ([ee833e1](https://github.com/chemodun/X4-UniverseEditor/commit/ee833e169652e1b700ae79b5b1ebcb0d23022f12))
* **Sector:** rename DominantOwnerColor to Color for consistency and clarity and set the default value to grey_128 ([4f3c118](https://github.com/chemodun/X4-UniverseEditor/commit/4f3c118d163bcb000707934ae7c3561c11d461bf))
* **X4DataLoader:** streamline color handling in Faction, Station, and Sector classes ([3750afc](https://github.com/chemodun/X4-UniverseEditor/commit/3750afca1f702050f8ce355ed756da6917c548c0))

## [0.6.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.6.0...X4DataLoader@v0.6.1) (2025-02-15)


### Code Refactoring

* **LoadData:** update LoadData method to accept gameFilesStructure as a parameter and remove "predefined" structure ([6a0c26d](https://github.com/chemodun/X4-UniverseEditor/commit/6a0c26dd01702c175ce40dcab58f1b22943debcd))
* **X4Galaxy:** move data validation method for X4 data folder inside ([056a168](https://github.com/chemodun/X4-UniverseEditor/commit/056a168e2ee5fda60de5517743c4e169e9bcf5e0))

## [0.6.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader-v0.5.1...X4DataLoader@v0.6.0) (2025-02-13)


### Features

* **Colors:** Add X4Color and X4MappedColor classes for color management and loading from XML ([5c8cd51](https://github.com/chemodun/X4-UniverseEditor/commit/5c8cd513c4ad8a0c68ed72fb5c0c7f962165bbe9))
* **Colors:** Replace Hex property with Color object for improved color management ([1878daa](https://github.com/chemodun/X4-UniverseEditor/commit/1878daae482d39782f87752ef11af1791284ec9e))
* **DataLoader:** Add duplicate ID and name checks for construction plans, factions, station categories, groups, modules, and module groups ([66c7ebf](https://github.com/chemodun/X4-UniverseEditor/commit/66c7ebfa735a08a93cf3bfac72d4f690540adbbd))
* **Faction:** Implement Faction class and integrate faction loading into data processing and sectors ownership ([e65fc6b](https://github.com/chemodun/X4-UniverseEditor/commit/e65fc6bc4ff6a11216a56e47fdeb68050f5c0e32))
* **Galaxy:** add LoadXML method to support loading from XML ([84e2ff8](https://github.com/chemodun/X4-UniverseEditor/commit/84e2ff86952bfca6450e44bfba6a8264fb933452))
* **Galaxy:** add static DLCOrder list ([b505bda](https://github.com/chemodun/X4-UniverseEditor/commit/b505bda3639a90e24d466909b18e53bfcccfd121))
* **Galaxy:** update LoadData method to support loading mods ([0d4da87](https://github.com/chemodun/X4-UniverseEditor/commit/0d4da873c95d4c99cf3c15795f5d1e578f57ea66))
* **MainWindow:** enhance UI with tab names and data binding improvements; add window loaded event ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))
* **Race:** Add Race class and implement loading logic for races from XML ([17d6ed8](https://github.com/chemodun/X4-UniverseEditor/commit/17d6ed8a8a88511f17265d102d133c5a3b2810c0))
* **Sector:** Add methods to retrieve stations by tag or type ([d600167](https://github.com/chemodun/X4-UniverseEditor/commit/d600167742cb9e770319c085ba72448bb42808a5))
* **Sector:** add properties for sunlight, economy, and security; update dominant owner faction logic ([ef46907](https://github.com/chemodun/X4-UniverseEditor/commit/ef469076172bbafa7e0bf9f4edbb8e50a4d65b5b))
* **Station:** Add Name property and get it from construction plan ([3789440](https://github.com/chemodun/X4-UniverseEditor/commit/37894408f2462da0455320f00a4d4450af83b9f2))
* **X4DataLoader:** Enhance data loading by enhancing  Cluster Highway connection logic ([53ba6a9](https://github.com/chemodun/X4-UniverseEditor/commit/53ba6a975215cf64759ea4780ecf558c6ac84bdf))
* **X4DataLoader:** Implement Station class and enhance data loading for stations in the galaxy from god.xml. Sector DominantOwner is calculated ([de3e835](https://github.com/chemodun/X4-UniverseEditor/commit/de3e8356377037aa1833af55796144a1e8aa8f36))
* **Zone:** implement LoadFromXML to support diff loading of zones ([4200250](https://github.com/chemodun/X4-UniverseEditor/commit/4200250cd3af37a149dfba3a6fc894a027b73a36))


### Bug Fixes

* **Connections:** Fix Sector to Cluster connection processing ([ea3800d](https://github.com/chemodun/X4-UniverseEditor/commit/ea3800db5d65a6a0f3b9154b0617ccfe46a8a44e))
* **DataLoader:** log warning instead of throwing exception for missing files ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))
* **DataLoader:** remove duplication of code for gathering a file sets. And fix real file name storing in set ([9b2f30f](https://github.com/chemodun/X4-UniverseEditor/commit/9b2f30fd809a268c07987ff8fab4bc5d1d8c63e8))
* **LoadX4Data:** Replace mandatory data load by warning and switching the ribbon on options tab ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))
* **Station:** Correct owner name assignment logic to ensure proper prefix handling ([b6615c5](https://github.com/chemodun/X4-UniverseEditor/commit/b6615c54f0143440c5d26ad8e17a77458b5bb6da))
* **Translation:** replace dictionary initialization with array and optimize regex usage ([3daddc4](https://github.com/chemodun/X4-UniverseEditor/commit/3daddc47093a3def65af7bb1ef965d2c0147d27c))
* **X4DataLoader:** extend SetPosition method to include source and filename parameters, as it differ from cluster and sector source file ([a5ec477](https://github.com/chemodun/X4-UniverseEditor/commit/a5ec477ed3fe1512af868f6592bb8a1735d2eb68))
* **Zone:** add macro element with reference and connection attributes to PositionXML ([37d6644](https://github.com/chemodun/X4-UniverseEditor/commit/37d6644194d77bc55c4d210f1938537b0e9a2bde))


### Code Refactoring

* **Connections:** enhance LoadFromXML to support diff loads ([084270c](https://github.com/chemodun/X4-UniverseEditor/commit/084270c79420f6a7a0258a52d1e85dc4d6db645e))
* **Connections:** Override ToString methods for Position and Quaternion classes to improve debugging output ([b3f7762](https://github.com/chemodun/X4-UniverseEditor/commit/b3f7762bf9e6e69e82f05cc4282928e8ed67f57c))
* **Connections:** Update variable declarations to use nullable types for improved safety ([8f01ace](https://github.com/chemodun/X4-UniverseEditor/commit/8f01aced8da6bc61a7e32a0b6898dd4a06440b7e))
* **DataLoader:** Add error handling for XML loading in DataLoader methods ([c4a26e8](https://github.com/chemodun/X4-UniverseEditor/commit/c4a26e81a81d70bdf09cc4518094645081549291))
* **DataLoader:** add possibility to load default 0001.xml ([5eabc90](https://github.com/chemodun/X4-UniverseEditor/commit/5eabc901c793ae9d65f09a0feaa103061252c8be))
* **DataLoader:** enhance LoadData method to support loading extensions by ID and improve clarity in DLC file handling ([a4c1312](https://github.com/chemodun/X4-UniverseEditor/commit/a4c1312b5573022dd3976fcfd02d805459783d68))
* **DataLoader:** Implement GatherFiles method for improved file organization and logging ([7e4027e](https://github.com/chemodun/X4-UniverseEditor/commit/7e4027e3d3741c8b608d77b57e4f2a14827d7824))
* **DataLoader:** Improve DLC folder scanning and logging for file identification ([0493c62](https://github.com/chemodun/X4-UniverseEditor/commit/0493c62f352d52ef6a3668533fd1a60d9c0c52a4))
* **DataLoader:** Improve variable declarations for clarity and consistency ([38013ed](https://github.com/chemodun/X4-UniverseEditor/commit/38013edb3f270a78e1fc93369d61d16f02dda777))
* **DataLoader:** refactor LoadAllData to accept Galaxy instance and streamline translation handling. As preparation to load mods, not only DLC's ([123badb](https://github.com/chemodun/X4-UniverseEditor/commit/123badb8db96193e654cb58b47263bdddbf86a90))
* **DataLoader:** replace Dictionary initializations with array literals ([b2aa20a](https://github.com/chemodun/X4-UniverseEditor/commit/b2aa20a1559edd03ed626b163c1cc180459470af))
* **Galaxy, DataLoader, Connections, Cluster:** update methods to use string IDs for clusters and sectors, improving consistency and clarity ([d2f3a08](https://github.com/chemodun/X4-UniverseEditor/commit/d2f3a081c1ddadc2e0d2c8875ccb6e32becb0220))
* **Galaxy:** Enhance LoadConnections method to support additional zones, to make it possible to use it for the mods data load ([5bfce9e](https://github.com/chemodun/X4-UniverseEditor/commit/5bfce9e2c3065a52c10ba98d2e6f166ac986d491))
* **Galaxy:** made a attribute Extensions, contains both - DLC's and Mods ([fddadec](https://github.com/chemodun/X4-UniverseEditor/commit/fddadecd256c141b53e66abf94803226168064bc))
* **GalaxyMapWindow:** Update background color to LightGray and improve variable declarations ([b367d6f](https://github.com/chemodun/X4-UniverseEditor/commit/b367d6fb94c63d227688febae18229ff8552ee96))
* **Highway:** Add Source and FileName properties to HighwayPoint for enhanced data tracking ([ad8109b](https://github.com/chemodun/X4-UniverseEditor/commit/ad8109bfcefa6f0b3c316fe099c51cadb18d7a08))
* **HighwayClusterLevel:** Change EntryPointPath and ExitPointPath to public access modifiers ([3fafeaa](https://github.com/chemodun/X4-UniverseEditor/commit/3fafeaa62470836627916fac989d479e22ce94ed))
* **Highway:** Enhance connection logic for entry and exit points in Highway class ([50e10c5](https://github.com/chemodun/X4-UniverseEditor/commit/50e10c5454ec9442e958476794724f369d7bbcbd))
* **Highway:** Exchange entry and exit point handling in HighwayClusterConnectionPath logic ([09a624f](https://github.com/chemodun/X4-UniverseEditor/commit/09a624f68d277b7b9587f63354cfb6a941fed166))
* **Logging:** Integrate logging functionality throughout data loading process ([caff28e](https://github.com/chemodun/X4-UniverseEditor/commit/caff28e3a727a11be1fa9c6d69da96c5ba33bc1a))
* **Mod:** make a mod loading fully operational ([e2225b5](https://github.com/chemodun/X4-UniverseEditor/commit/e2225b56433d516ffc1a65860d4c906dc0915573))
* Replace parsing logic with StringHelper methods for improved readability and error handling ([7a7e9fc](https://github.com/chemodun/X4-UniverseEditor/commit/7a7e9fc228e14ae3a27e8d841f021dd068eb93da))
* **Sector, Cluster, Zone:** Update properties to public and simplify initialization ([ea3800d](https://github.com/chemodun/X4-UniverseEditor/commit/ea3800db5d65a6a0f3b9154b0617ccfe46a8a44e))
* **Sector:** change initialization of ownerStationCount from new() to [] ([fd5c781](https://github.com/chemodun/X4-UniverseEditor/commit/fd5c7811ef85d0c67ac80552e6214ce48d4563dc))
* **Sector:** Remove unused owner and type ignore lists from ownership calculation ([c4b2a0e](https://github.com/chemodun/X4-UniverseEditor/commit/c4b2a0ea3c313ea766867a5899b32cdb5d8591c1))
* simplify XElement instantiation and initialize connections list ([b42b2bc](https://github.com/chemodun/X4-UniverseEditor/commit/b42b2bc8f574a1104f06b1b406122b494d2db689))
* **Station:** Clarify comment regarding refId usage in station category assignment ([49164a1](https://github.com/chemodun/X4-UniverseEditor/commit/49164a1aee2111066ab19968ab39d98e036f423a))
* **Station:** Enhance owner name assignment logic to include primary race name prefix handling ([b2cf2e1](https://github.com/chemodun/X4-UniverseEditor/commit/b2cf2e1f79b38176e9c6cc0380f5ceebef1f9f29))
* **Station:** Enhance station category assignment logic with refId handling and add macro property ([1944dc3](https://github.com/chemodun/X4-UniverseEditor/commit/1944dc3cfd72a97074f6af2664f39eb71566c7b3))
* **Station:** Improve position and rotation parsing with TryParse for better error handling and from right XMLElement ([7c84d0e](https://github.com/chemodun/X4-UniverseEditor/commit/7c84d0e92303c012c8ff1241e69d39b1d3acfba8))
* **Station:** Refactor owner property to OwnerId and add OwnerName; update loading logic ([2bfa888](https://github.com/chemodun/X4-UniverseEditor/commit/2bfa88868e387e15cefd9775ddcacb7d8aa8dbdf))
* **Translation:** change Regex fields to readonly for better performance and clarity ([5bbb572](https://github.com/chemodun/X4-UniverseEditor/commit/5bbb572fb1c1ec0407e1391451c12f5c871d8efb))
* **X4DataLoader:** add reading version from version.dat ([143b684](https://github.com/chemodun/X4-UniverseEditor/commit/143b684cd862ca849692cc8a4dadcb7601b15d9d))
* **X4DataLoader:** added special classes to work with game and extensions file structure: ([e8cc129](https://github.com/chemodun/X4-UniverseEditor/commit/e8cc1296fa60fdadd3ca4a382f17110da87b558c))
* **X4DataLoader:** improve formatting and consistency in logging methods ([1d068f1](https://github.com/chemodun/X4-UniverseEditor/commit/1d068f1aab528438be8f12ea9e6ad0e91ba69b2f))
* **X4DataLoader:** Integrate centralized logging utility across multiple classes ([10e8fea](https://github.com/chemodun/X4-UniverseEditor/commit/10e8feaf674dd3c521c7619a24f19fe5ce91e028))
* **X4DataLoader:** Introduce StationModule, StationModuleGroup, StationCategory, StationGroup, ConstructionPlans to get dig to the "claiming" possibility of stations ([3075296](https://github.com/chemodun/X4-UniverseEditor/commit/3075296c637e0e4debb6a0390b49efe7aa0c6592))
* **X4Data:** Update data loading logic and introduce version handling from patchactions.xml ([ce5efde](https://github.com/chemodun/X4-UniverseEditor/commit/ce5efdeba84d62b29323ddc11844a151b6827fb3))
* **XmlHelper:** add method to extract attribute and value from differential selection syntax ([ef67a8c](https://github.com/chemodun/X4-UniverseEditor/commit/ef67a8c803de6050544bedef445d0a332a519553))
* **XmlHelper:** Add method to retrieve attribute values as a list from XElement ([6dc339a](https://github.com/chemodun/X4-UniverseEditor/commit/6dc339aed113395eb78406ac2e6c7437564f0baa))
* **XmlHelper:** change Regex field to readonly and initialize tags list with empty array ([aebafd5](https://github.com/chemodun/X4-UniverseEditor/commit/aebafd518346a99ad4d59c732fa82f9668a28a8b))
* **XmlHelper:** Enhance GetAttributeAsList method to accept a custom separator for splitting attribute values ([e18bd6b](https://github.com/chemodun/X4-UniverseEditor/commit/e18bd6bfd34a09e2dee04734c79f68e9fa475cde))
* **XmlHelper:** Improve variable declarations for clarity and consistency ([88adda4](https://github.com/chemodun/X4-UniverseEditor/commit/88adda4d94866f4da3b60c3e2f19f0f6775eba08))


### Miscellaneous Chores

* add XMLPatch project reference to X4DataLoader ([8a2af90](https://github.com/chemodun/X4-UniverseEditor/commit/8a2af90fb12eff61fadc7c64d53c8d8b0216140b))

## [0.5.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.5.0...X4DataLoader@v0.5.1) (2025-02-13)


### Miscellaneous Chores

* add XMLPatch project reference to X4DataLoader ([8a2af90](https://github.com/chemodun/X4-UniverseEditor/commit/8a2af90fb12eff61fadc7c64d53c8d8b0216140b))

## [0.5.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.4.0...X4DataLoader@v0.5.0) (2025-02-12)


### Features

* **Galaxy:** add static DLCOrder list ([b505bda](https://github.com/chemodun/X4-UniverseEditor/commit/b505bda3639a90e24d466909b18e53bfcccfd121))
* **Sector:** add properties for sunlight, economy, and security; update dominant owner faction logic ([ef46907](https://github.com/chemodun/X4-UniverseEditor/commit/ef469076172bbafa7e0bf9f4edbb8e50a4d65b5b))

## [0.4.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.3.6...X4DataLoader@v0.4.0) (2025-02-11)


### Features

* **Galaxy:** add LoadXML method to support loading from XML ([84e2ff8](https://github.com/chemodun/X4-UniverseEditor/commit/84e2ff86952bfca6450e44bfba6a8264fb933452))
* **Galaxy:** update LoadData method to support loading mods ([0d4da87](https://github.com/chemodun/X4-UniverseEditor/commit/0d4da873c95d4c99cf3c15795f5d1e578f57ea66))
* **Zone:** implement LoadFromXML to support diff loading of zones ([4200250](https://github.com/chemodun/X4-UniverseEditor/commit/4200250cd3af37a149dfba3a6fc894a027b73a36))


### Code Refactoring

* **Connections:** enhance LoadFromXML to support diff loads ([084270c](https://github.com/chemodun/X4-UniverseEditor/commit/084270c79420f6a7a0258a52d1e85dc4d6db645e))
* **DataLoader:** add possibility to load default 0001.xml ([5eabc90](https://github.com/chemodun/X4-UniverseEditor/commit/5eabc901c793ae9d65f09a0feaa103061252c8be))
* **DataLoader:** enhance LoadData method to support loading extensions by ID and improve clarity in DLC file handling ([a4c1312](https://github.com/chemodun/X4-UniverseEditor/commit/a4c1312b5573022dd3976fcfd02d805459783d68))
* **DataLoader:** refactor LoadAllData to accept Galaxy instance and streamline translation handling. As preparation to load mods, not only DLC's ([123badb](https://github.com/chemodun/X4-UniverseEditor/commit/123badb8db96193e654cb58b47263bdddbf86a90))
* **Galaxy, DataLoader, Connections, Cluster:** update methods to use string IDs for clusters and sectors, improving consistency and clarity ([d2f3a08](https://github.com/chemodun/X4-UniverseEditor/commit/d2f3a081c1ddadc2e0d2c8875ccb6e32becb0220))
* **Galaxy:** made a attribute Extensions, contains both - DLC's and Mods ([fddadec](https://github.com/chemodun/X4-UniverseEditor/commit/fddadecd256c141b53e66abf94803226168064bc))
* **Mod:** make a mod loading fully operational ([e2225b5](https://github.com/chemodun/X4-UniverseEditor/commit/e2225b56433d516ffc1a65860d4c906dc0915573))
* **Sector:** change initialization of ownerStationCount from new() to [] ([fd5c781](https://github.com/chemodun/X4-UniverseEditor/commit/fd5c7811ef85d0c67ac80552e6214ce48d4563dc))
* **Translation:** change Regex fields to readonly for better performance and clarity ([5bbb572](https://github.com/chemodun/X4-UniverseEditor/commit/5bbb572fb1c1ec0407e1391451c12f5c871d8efb))
* **X4DataLoader:** added special classes to work with game and extensions file structure: ([e8cc129](https://github.com/chemodun/X4-UniverseEditor/commit/e8cc1296fa60fdadd3ca4a382f17110da87b558c))
* **XmlHelper:** add method to extract attribute and value from differential selection syntax ([ef67a8c](https://github.com/chemodun/X4-UniverseEditor/commit/ef67a8c803de6050544bedef445d0a332a519553))
* **XmlHelper:** change Regex field to readonly and initialize tags list with empty array ([aebafd5](https://github.com/chemodun/X4-UniverseEditor/commit/aebafd518346a99ad4d59c732fa82f9668a28a8b))

## [0.3.6](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.3.5...X4DataLoader@v0.3.6) (2025-02-10)


### Code Refactoring

* **X4DataLoader:** add reading version from version.dat ([143b684](https://github.com/chemodun/X4-UniverseEditor/commit/143b684cd862ca849692cc8a4dadcb7601b15d9d))

## [0.3.5](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.3.4...X4DataLoader@v0.3.5) (2025-02-10)


### Code Refactoring

* simplify XElement instantiation and initialize connections list ([b42b2bc](https://github.com/chemodun/X4-UniverseEditor/commit/b42b2bc8f574a1104f06b1b406122b494d2db689))

## [0.3.4](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.3.3...X4DataLoader@v0.3.4) (2025-02-08)


### Code Refactoring

* **X4DataLoader:** improve formatting and consistency in logging methods ([1d068f1](https://github.com/chemodun/X4-UniverseEditor/commit/1d068f1aab528438be8f12ea9e6ad0e91ba69b2f))

## [0.3.3](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataLoader@v0.3.2...X4DataLoader@v0.3.3) (2025-02-05)


### Bug Fixes

* **X4DataLoader:** extend SetPosition method to include source and filename parameters, as it differ from cluster and sector source file ([a5ec477](https://github.com/chemodun/X4-UniverseEditor/commit/a5ec477ed3fe1512af868f6592bb8a1735d2eb68))

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
