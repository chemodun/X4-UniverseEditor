# Changelog

## [0.8.7](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.8.6...ChemGateBuilder@v0.8.7) (2025-02-14)


### Code Refactoring

* **ChemGateBuilder:** integrate X4Map project and update references ([a826f87](https://github.com/chemodun/X4-UniverseEditor/commit/a826f87df0ef4d1e1ff25ab234ea7e935534ab19))
* **X4Map:** move to X4Map: SectorMap.cs and MapColors.cs as whole and some related objects, including HexagonPointsConverter ([a826f87](https://github.com/chemodun/X4-UniverseEditor/commit/a826f87df0ef4d1e1ff25ab234ea7e935534ab19))

## [0.8.6](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.8.5...ChemGateBuilder@v0.8.6) (2025-02-14)


### Bug Fixes

* **Mod:** not loading the mod if it uses other preloaded mods, not only DLC's ([6e568e2](https://github.com/chemodun/X4-UniverseEditor/commit/6e568e24907c696378b66d134eded78deb123eb3))


### Documentation

* **README:** update changelog for version 0.8.6 with mod loading fix ([a31ef9e](https://github.com/chemodun/X4-UniverseEditor/commit/a31ef9e8266e9e22b39c601abf943bd6093bb848))

## [0.8.5](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.8.4...ChemGateBuilder@v0.8.5) (2025-02-14)


### Documentation

* **README:** add changelog entry for version 0.8.3 ([a0b08c5](https://github.com/chemodun/X4-UniverseEditor/commit/a0b08c57e3232b37679c3d84e15e4a3447b394a2))
* **README:** update changelog for versions 0.8.4 and 0.8.5 ([ffe9ae0](https://github.com/chemodun/X4-UniverseEditor/commit/ffe9ae0a1ec45e6e229414880f3e5efcbe7e201c))
* **README:** Update README's files ([d2e8cc0](https://github.com/chemodun/X4-UniverseEditor/commit/d2e8cc0097aaf2783fb265d440aa89753d2fcadd))

## [0.8.4](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.8.3...ChemGateBuilder@v0.8.4) (2025-02-14)


### Bug Fixes

* **Mod:** fix connection comparison logic to identify the Mod change status ([7cadf1e](https://github.com/chemodun/X4-UniverseEditor/commit/7cadf1ea11f68f1cc4a0ce13dcfdc20ca7794e52))
* **SectorMap:** update source identification for sector objects and improve tooltip formatting ([1a23216](https://github.com/chemodun/X4-UniverseEditor/commit/1a232163740ff6ab81e067cc5d140a92776d77d7))


### Miscellaneous Chores

* **UI:** add "Save as" button functionality and update save logic ([acb95cd](https://github.com/chemodun/X4-UniverseEditor/commit/acb95cd455757d49a0f805083316a7bff536a11d))

## [0.8.3](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.8.2...ChemGateBuilder@v0.8.3) (2025-02-14)


### Miscellaneous Chores

* **Mod:** Enhance mod loading by applying the same XMLPatch approach as for the DLC's and mods ([dc269fd](https://github.com/chemodun/X4-UniverseEditor/commit/dc269fdc86aea4173fe28fb9697d5f30c4f866ff))

## [0.8.2](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.8.1...ChemGateBuilder@v0.8.2) (2025-02-13)


### Bug Fixes

* **X4DataExtractionWindow:** Hide Mods list in extraction window if Mods loading is not configured ([c3c5def](https://github.com/chemodun/X4-UniverseEditor/commit/c3c5def745474300326fc874d7ec23e2e56e4251))
* **X4DataExtractionWindow:** increase maximum height to improve usability ([70c6c2e](https://github.com/chemodun/X4-UniverseEditor/commit/70c6c2e2391920d4199ec1ada5696c5bd8817ffe))


### Documentation

* **README:** update changelog for version 0.8.2 ([4f46570](https://github.com/chemodun/X4-UniverseEditor/commit/4f46570293824c03342c6f21ddc30a86726e96c7))

## [0.8.1](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.8.0...ChemGateBuilder@v0.8.1) (2025-02-13)


### Features

* **X4DataExtractionWindow:** allow extract not only DLC's - mod extraction is available for now too ([038f3ef](https://github.com/chemodun/X4-UniverseEditor/commit/038f3ef34a69a89dc89de56107e483149c806bed))
* **GalaxyMapWindow:** bind visibility of Mods group to ModsOptions count ([8a247dd](https://github.com/chemodun/X4-UniverseEditor/commit/8a247dd600a98210680404a01d2103ba2e69c9b0))


### Documentation

* **README:** update instructions for loading mods and add changelog entry for version 0.8.1 ([dbe1f67](https://github.com/chemodun/X4-UniverseEditor/commit/dbe1f67a358b2c461c27c11042526da4b9624023))
* **README:** Update README's files ([8adf425](https://github.com/chemodun/X4-UniverseEditor/commit/8adf425abcecb0c14fe2b1554de177f8e80ab320))
* **README:** update section titles for clarity in sector selection instructions ([2fc222b](https://github.com/chemodun/X4-UniverseEditor/commit/2fc222b527d4e210dbc64a2fb17c738cc8211591))

## [0.8.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.7.0...ChemGateBuilder@v0.8.0) (2025-02-13)


### Features

* **GalaxyMap:** add Developer options group with binding for developer settings, to display the empty cells ([a27efdc](https://github.com/chemodun/X4-UniverseEditor/commit/a27efdcffa520792bf17d1faa5157cacd4e5d182))
* **GalaxyMap:** add tooltip functionality for sectors and clusters; update Create methods to accept window parameter ([33d2d49](https://github.com/chemodun/X4-UniverseEditor/commit/33d2d49dc6b3aa2ba71279a4eae3db8109c49184))
* **GalaxyMap:** prepare to support for show/hide DLCs and mods related objects with checkboxes in the UI ([ec59d69](https://github.com/chemodun/X4-UniverseEditor/commit/ec59d692e295646e85d15ed518ae7c720021b8f4))


### Bug Fixes

* **GalaxyMap:** optimize column calculation for hexagonal grid rendering ([0381efa](https://github.com/chemodun/X4-UniverseEditor/commit/0381efaf5f2378483d30efef10c215e4749f1b67))


### Code Refactoring

* **GalaxyMap:** enhance options visibility with toggle button and FontAwesome icons ([f67bc8e](https://github.com/chemodun/X4-UniverseEditor/commit/f67bc8e6f9e8fc74bca35390b8211c1f47374487))
* **GalaxyMap:** Map building is rewritten ([a27efdc](https://github.com/chemodun/X4-UniverseEditor/commit/a27efdcffa520792bf17d1faa5157cacd4e5d182))
* **GalaxyMap:** update DLC handling to use DLCOrder for improved consistency and error handling ([02cfc0c](https://github.com/chemodun/X4-UniverseEditor/commit/02cfc0c7199965e73ca503bcb7b19464d2dd3b06))
* **GalaxyMap:** update event handlers and improve map update logic when options is shown ([cb66435](https://github.com/chemodun/X4-UniverseEditor/commit/cb66435878f81dd5cef093a11959809be7a51096))
* **GalaxyMapWindow:** remove redundant visibility update for Hexagon ([e60f17a](https://github.com/chemodun/X4-UniverseEditor/commit/e60f17ab3f3ac976407147ad00a4d3fb8c5641fb))
* **SectorMap:** add Source property and image visibility control methods ([bc6f949](https://github.com/chemodun/X4-UniverseEditor/commit/bc6f949ea7d0c35669f48dc19db13b15cba6f9e2))


### Documentation

* **images:** Update GIF files ([0d75f94](https://github.com/chemodun/X4-UniverseEditor/commit/0d75f94bdd26373d790b1346dea09e980fa2b7d9))
* **README:** add gate rotation details and credits section ([20bcc79](https://github.com/chemodun/X4-UniverseEditor/commit/20bcc79d9ada2e3b5752f570f92d306dff2cb82e))
* **README:** clarify the changelog ([6dcce41](https://github.com/chemodun/X4-UniverseEditor/commit/6dcce4145390f0651c48cd9b628b96cf8ab5ec46))
* **README:** update for the new features of version 0.8.0 ([082d4bf](https://github.com/chemodun/X4-UniverseEditor/commit/082d4bfc5d321a2b0b1079f5caa2bd91f9de05aa))
* **README:** Update README's files ([badb722](https://github.com/chemodun/X4-UniverseEditor/commit/badb72277bceb8eb7375eed8c4c1beb9ec4246cb))
* **videos:** add several new videos ([f6aa70b](https://github.com/chemodun/X4-UniverseEditor/commit/f6aa70b1947e97418dfbb16448fc267f31ab4174))

## [0.7.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.6.0...ChemGateBuilder@v0.7.0) (2025-02-11)


### Features

* **MainWindow:** add option to load mods data in X4 data processing ([7b032f3](https://github.com/chemodun/X4-UniverseEditor/commit/7b032f3cd2b9054f9f16636d49ca4789349f6c80))


### Code Refactoring

* **GalaxyMapWindow:** update line stroke color based on source and destination status. DarkGray for inactive connected gates ([1bc1c06](https://github.com/chemodun/X4-UniverseEditor/commit/1bc1c06a29f7b587569d3354cb51a2800534695d))
* **MainWindow:** simplify ID generation for sectors and connections, enhancing readability and consistency ([1e8799d](https://github.com/chemodun/X4-UniverseEditor/commit/1e8799d443ef50f1fb5b5c19d38afc5f260afc14))
* **MainWindow:** update SaveData call to include Galaxy parameter for improved functionality ([7206ff7](https://github.com/chemodun/X4-UniverseEditor/commit/7206ff7d35f24e422fb75878bed01a96314c0034))
* **Mod:** Align mod loading to the new game structure loading principles ([f6da896](https://github.com/chemodun/X4-UniverseEditor/commit/f6da896040f00c5ae3d67374805bb3918068f98b))
* **Mod:** enhance SaveData and SaveModXMLs methods to write more detailed dependencies ([1674e72](https://github.com/chemodun/X4-UniverseEditor/commit/1674e72c247bc32c3245a921857ce4c0c99c8ffc))
* **Mod:** update connection retrieval to use FirstOrDefault for safer null handling ([a4da014](https://github.com/chemodun/X4-UniverseEditor/commit/a4da014d755a6242f0133ca28d191b4d96c1a44b))
* **X4DataExtractionWindow:** replace hardcoded paths with DataLoader constants for improved maintainability and add content.xml copying on extraction ([45b929f](https://github.com/chemodun/X4-UniverseEditor/commit/45b929fb5496cf9f83c184fb04d2b0fd5377da77))


### Documentation

* **README:** clarify extraction process and add warning for existing game files ([8d54284](https://github.com/chemodun/X4-UniverseEditor/commit/8d5428499c32554b7554bded35919dd37de163d8))
* **README:** update changelog for version 0.7.0 with new mod support and extraction requirements ([6f56ca2](https://github.com/chemodun/X4-UniverseEditor/commit/6f56ca2742b197dd5c82e09f84a8176e2b5a2b00))
* **README:** Update README's files ([17e495d](https://github.com/chemodun/X4-UniverseEditor/commit/17e495d0c2d4b7a8fb29cc022f3aa7ee8ead03e2))

## [0.6.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.5.2...ChemGateBuilder@v0.6.0) (2025-02-10)


### Features

* add X4 Data extraction functionality with UI integration ([ab0d84a](https://github.com/chemodun/X4-UniverseEditor/commit/ab0d84a2510b7fb105ab1393ded920f8c3a449d8))
* integrate X4Unpack project and enhance folder selection dialog as preparation to in-tool extraction of X4 data ([5201cf2](https://github.com/chemodun/X4-UniverseEditor/commit/5201cf202d12b07d916ed66da416b05635222ad3))
* **X4DataExtraction:** add extraction progress text binding and update reporting in background worker ([9f5feff](https://github.com/chemodun/X4-UniverseEditor/commit/9f5feffc0fa3258dfbcc88167293d3f9c4d9d448))
* **X4DataExtraction:** add file copy functionality for version data with error logging ([e26c2f4](https://github.com/chemodun/X4-UniverseEditor/commit/e26c2f4c3d1a03d8f22d228207ab97ecbfede74f))


### Bug Fixes

* **MainWindow:** apply logging changes on the fly ([2ca2dbd](https://github.com/chemodun/X4-UniverseEditor/commit/2ca2dbd9268599bf961c7b3aca2ae4a93c788f35))


### Code Refactoring

* **AboutWindow:** enhance UI layout and add version display; sort component list ([13935a9](https://github.com/chemodun/X4-UniverseEditor/commit/13935a93d055e8c427088d4adc6941671b0cb84c))
* **App:** streamline configuration loading and integrate NLog setup ([2ca2dbd](https://github.com/chemodun/X4-UniverseEditor/commit/2ca2dbd9268599bf961c7b3aca2ae4a93c788f35))
* replace array initializations with collection initializers for clarity ([a5242bc](https://github.com/chemodun/X4-UniverseEditor/commit/a5242bcf55d9720fd65db6f4d8f8f1a9eb621866))
* **X4DataExtraction:** add data extraction options for verification and overwrite settings ([229c84a](https://github.com/chemodun/X4-UniverseEditor/commit/229c84aeb3442a30638c8865cdd7173b3077788f))


### Documentation

* **ChemGateBuilder:** update README to enhance formatting and improve readability ([b213ab2](https://github.com/chemodun/X4-UniverseEditor/commit/b213ab261093a8b7d636ba91c744c276702e113c))
* **images:** Update GIF files ([c6a91a2](https://github.com/chemodun/X4-UniverseEditor/commit/c6a91a2e20477fa94e1193963308ac9b93c76198))
* **README:** Update first start instructions and add extraction options ([f2658bc](https://github.com/chemodun/X4-UniverseEditor/commit/f2658bcd9035639de17a66b5266f5a2c37920db1))
* **README:** update license information and add changelog with version history ([2a7dd86](https://github.com/chemodun/X4-UniverseEditor/commit/2a7dd869c65a7330a307aafb17a997873e0ec5fc))
* **README:** Update README's files ([4bcccf4](https://github.com/chemodun/X4-UniverseEditor/commit/4bcccf42c67e3e844c2ca238d077539b63c1c5aa))
* **README:** Update README's files ([7751dea](https://github.com/chemodun/X4-UniverseEditor/commit/7751deaff9ca42496fad34413a263dc13a2f7435))
* **videos:** added first_start_with_extraction.mp4 ([f93dac6](https://github.com/chemodun/X4-UniverseEditor/commit/f93dac6232031226a3de2af86c6c7ff187f4ced7))

## [0.5.2](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.5.1...ChemGateBuilder@v0.5.2) (2025-02-08)


### Code Refactoring

* improve code readability by using 'readonly' and simplifying variable assignments ([0eb87a0](https://github.com/chemodun/X4-UniverseEditor/commit/0eb87a0b144b7f1a5b007f308b0f07262c2df10d))


### Documentation

* **README:** enhance forum link description for user engagement ([a21e1f1](https://github.com/chemodun/X4-UniverseEditor/commit/a21e1f10845526afc8f017662032b44283d00024))
* **README:** reorganize links section for clarity and user guidance ([792ecb2](https://github.com/chemodun/X4-UniverseEditor/commit/792ecb222942b3f211c56085600548658c5d0128))
* **README:** update feature descriptions for consistency and clarity; remove obsolete image ([88d5bc3](https://github.com/chemodun/X4-UniverseEditor/commit/88d5bc330ab95c06aad11cff16dd6bb17a0150d2))
* **README:** Update README's files ([5c8272a](https://github.com/chemodun/X4-UniverseEditor/commit/5c8272ac0db93913958b731b06aa412405aac740))

## [0.5.1](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.5.0...ChemGateBuilder@v0.5.1) (2025-02-08)


### Bug Fixes

* **GalaxyMapWindow:** avoid doubling the gate connection on selection sectors from the map ([c4214fc](https://github.com/chemodun/X4-UniverseEditor/commit/c4214fc97b82a3409e46ad82c467664fd595d6a8))


### Code Refactoring

* **ChemGateBuilder:** improve code formatting and structure across multiple files ([822d8c3](https://github.com/chemodun/X4-UniverseEditor/commit/822d8c3c25da01c9709918e6b8bc162fe5bead18))
* **TextBoxExtensions:** update regex initialization for improved readability ([b548617](https://github.com/chemodun/X4-UniverseEditor/commit/b5486177b80a44262d7112afe6e71649ccc3b5ce))


### Documentation

* **images:** Update GIF files ([7ad0fb1](https://github.com/chemodun/X4-UniverseEditor/commit/7ad0fb1933a8ff06bc6c91d9e39f3f65d5451d98))
* **images:** update main_window.png for improved visuals ([124c2db](https://github.com/chemodun/X4-UniverseEditor/commit/124c2dbdefa632c243f2819a4e32b47d7897fd68))
* **README:** add Galaxy Map section with navigation details and usage instructions ([44408bb](https://github.com/chemodun/X4-UniverseEditor/commit/44408bb40a225fbcd3251cac56e0b3165f6d5d64))
* **README:** add new lines for improved readability ([69e7a8a](https://github.com/chemodun/X4-UniverseEditor/commit/69e7a8ab0ccbd88514737c9e96e5f37bbc954966))
* **README:** correct spelling in Map Objects section header ([6ce973a](https://github.com/chemodun/X4-UniverseEditor/commit/6ce973ac7ae575d61dce61c3aa7fd051cc5e8dcc))
* **README:** refine Galaxy Map section for clarity and readability ([3582da2](https://github.com/chemodun/X4-UniverseEditor/commit/3582da22d0d58834dbda6a2f0a4eb87b35db2c0d))
* **README:** Update README.html files ([134912f](https://github.com/chemodun/X4-UniverseEditor/commit/134912fa478c75b48ad9c97abbebfdbffa99977c))
* **README:** Update README.html files ([83b6a56](https://github.com/chemodun/X4-UniverseEditor/commit/83b6a56e158481e9c6bc89699709601f19af6df9))
* **README:** Update README.html files ([57c0892](https://github.com/chemodun/X4-UniverseEditor/commit/57c0892a13f20faf53faa59b7f33805a0df9125d))
* **videos:** add galaxy_map.mp4 ([bb8dece](https://github.com/chemodun/X4-UniverseEditor/commit/bb8dece9b268d9b34ad5a082f2c35938f2281760))

## [0.5.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.4.3...ChemGateBuilder@v0.5.0) (2025-02-07)


### Features

* **MainWindow:** add "Readme" button to Help tab for easy access to README.html ([bf9a4d3](https://github.com/chemodun/X4-UniverseEditor/commit/bf9a4d333fb2e6fcd006286da4c3494f90b0fd03))


### Bug Fixes

* **ChemGateBuilder:** add SetFrom methods for Coordinates and Rotation classes and fix the defaults change after SectorMapExpandedWindow usage ([fbbcaa4](https://github.com/chemodun/X4-UniverseEditor/commit/fbbcaa4273a2b03b8f725bd2f6f1c5d64c551044))
* **README:** to test workflow ([fc4994a](https://github.com/chemodun/X4-UniverseEditor/commit/fc4994a79903c144422048a66feb4a90c8f09060))


### Code Refactoring

* **docs:** update images and videos in documentation, remove obsolete files ([f7342e0](https://github.com/chemodun/X4-UniverseEditor/commit/f7342e08188518fa46a2d65eb2f51479e1a5185d))
* **docs:** update README and appropriate image and video files ([ba49527](https://github.com/chemodun/X4-UniverseEditor/commit/ba49527cfe56bea12ad6b3c6e5e1c2c45e745bcd))
* **README:** remove unnecessary question mark in README.md ([d241a3f](https://github.com/chemodun/X4-UniverseEditor/commit/d241a3fbe2dc50a10d9b01ae16cc9f155766d939))
* **README:** update feature list to use gerund form for consistency ([455ef91](https://github.com/chemodun/X4-UniverseEditor/commit/455ef91b38033e8ee4cef9c13cac23b864b7f21e))


### Miscellaneous Chores

* **docs:** Update README.html files ([7752677](https://github.com/chemodun/X4-UniverseEditor/commit/775267777c379f08abd6882bcf1cf540f78bfd6b))


### Documentation

* **README:** clarify instructions for loading the mod by specifying the selection of content.xml ([b4a38d5](https://github.com/chemodun/X4-UniverseEditor/commit/b4a38d59a2f7ed48b39be74a2d8c5332746f7dd5))
* **README:** clarify instructions for selecting the extracted game folder ([724400e](https://github.com/chemodun/X4-UniverseEditor/commit/724400e04219545f0dc71c5da466a2f648e6ee90))
* **README:** enhance clarity on loading the mod by specifying content.xml selection and confirming successful loading ([a97f083](https://github.com/chemodun/X4-UniverseEditor/commit/a97f08394991017f8f70dc9f063a624f63388786))
* **README:** update forum reference to use consistent capitalization for EGOSOFT ([8ce3bf9](https://github.com/chemodun/X4-UniverseEditor/commit/8ce3bf9df23246945656d7bb4a06418e692be560))

## [0.4.3](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.4.2...ChemGateBuilder@v0.4.3) (2025-02-07)


### Bug Fixes

* **ChemGateKeeper:** fix ModFolderPath operations and improve variable naming ([3cd0934](https://github.com/chemodun/X4-UniverseEditor/commit/3cd0934652ada1c669afe10e77826c1ff7ef753d))

## [0.4.2](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.4.1...ChemGateBuilder@v0.4.2) (2025-02-06)


### Documentation

* **ChemGateBuilder:** draft update of the README to be comply with the current tool version ([161d483](https://github.com/chemodun/X4-UniverseEditor/commit/161d483f4de6a66212599cbc0ba1fcd7a1e3d767))

## [0.4.1](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.4.0...ChemGateBuilder@v0.4.1) (2025-02-05)


### Miscellaneous Chores

* move GalaxyConnections to the ChemGateKeeperMod ([b4493ae](https://github.com/chemodun/X4-UniverseEditor/commit/b4493aeec71c2ff98d29ffc83f37118fd0af6ba6))

## [0.4.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.3.1...ChemGateBuilder@v0.4.0) (2025-02-05)


### Features

* **MainWindow:** remove separate X4 load ribbon tab with button (feat used to reach the python version) ([918f977](https://github.com/chemodun/X4-UniverseEditor/commit/918f9772b02702cf826e401d8ba84c55bab1fdfb))


### Code Refactoring

* **MainWindow:** update Options tab to Configuration and improve user prompts ([918f977](https://github.com/chemodun/X4-UniverseEditor/commit/918f9772b02702cf826e401d8ba84c55bab1fdfb))

## [0.3.1](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.3.0...ChemGateBuilder@v0.3.1) (2025-02-05)


### Bug Fixes

* **ChemGateBuilder:** simplify SaveData logic, to take a file prefix from Sector PositionSourceFile and improve version handling ([0ebdebd](https://github.com/chemodun/X4-UniverseEditor/commit/0ebdebd9495b3c133ca0af32833b7d782e8ee33b))
* **GalaxyConnectionData:** adjust gate position calculations to account for scaling factor ([5a94443](https://github.com/chemodun/X4-UniverseEditor/commit/5a9444350354f3326078bc0f56769f6d7d39fc12))
* **MainWindow:** add error handling on the mod load ([046471e](https://github.com/chemodun/X4-UniverseEditor/commit/046471eb52bc559c4c3fd5edf662190ea6b85996))
* **Mod:** update file saving logic to include right file prefix for non-vanilla paths ([1ae4900](https://github.com/chemodun/X4-UniverseEditor/commit/1ae490027e501d477dcdb9a372808f4c13863c13))


### Code Refactoring

* **GalaxyMapWindow:** remove unused FillColor variable ([ffb12e2](https://github.com/chemodun/X4-UniverseEditor/commit/ffb12e26550dc11070b033de3f12bedd4d059f90))
* **MainWindow:** remove Window_Loaded event and encapsulate validation logic in X4DataNotLoadedCheckAndWarning method ([f294347](https://github.com/chemodun/X4-UniverseEditor/commit/f294347599914413f9904190d2c9115cf8b058bd))

## [0.3.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.2.0...ChemGateBuilder@v0.3.0) (2025-02-05)


### Features

* **MainWindow:** enhance UI with tab names and data binding improvements; add window loaded event ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))


### Bug Fixes

* **DataLoader:** log warning instead of throwing exception for missing files ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))
* **LoadX4Data:** Replace mandatory data load by warning and switching the ribbon on options tab ([3a65a0a](https://github.com/chemodun/X4-UniverseEditor/commit/3a65a0af2464a26a46ec41835f71009d6b460298))

## [0.2.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.1.1...ChemGateBuilder@v0.2.0) (2025-02-04)


### Features

* **MainWindow:** enhance folder selection dialog with description and disable new folder option ([cb918ea](https://github.com/chemodun/X4-UniverseEditor/commit/cb918eaf578e67bac68713b5c0aeec3618a8ff87))


### Bug Fixes

* **AboutWindow:** add builder icon and adjust button margin ([743b054](https://github.com/chemodun/X4-UniverseEditor/commit/743b054ab522424fea64a9c83e2250f3b1e5336d))
* **GalaxyMapWindow:** comment out unused clickedSector variable ([5ec4b13](https://github.com/chemodun/X4-UniverseEditor/commit/5ec4b132097b1506c83b6892edb90ba4d80ef4e2))


### Miscellaneous Chores

* **README:** add old documentation for X4 Chem Gate Builder tool from the Python version ([f9a47c3](https://github.com/chemodun/X4-UniverseEditor/commit/f9a47c361ef625cd4cb3aa987adf156ab680f61e))

## [0.1.1](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.1.0...ChemGateBuilder@v0.1.1) (2025-02-04)


### Bug Fixes

* **AboutWindow:** add components list to About dialog ([418a5e5](https://github.com/chemodun/X4-UniverseEditor/commit/418a5e5d1b86ee2f10cd4679a809b02698e67f52))

## [0.1.0](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder@v0.0.1...ChemGateBuilder@v0.1.0) (2025-02-04)


### Features

* **AboutWindow:** Add About dialog with version and copyright information ([821c790](https://github.com/chemodun/X4-UniverseEditor/commit/821c790604d21d8af82b26d5e40bfafdb25cadc1))
* **ChemGateBuilder:** Add new map object resources for equipment dock, shipyard, trade station, and wharf ([2f04014](https://github.com/chemodun/X4-UniverseEditor/commit/2f04014fcff1db52bea045493d3a7f540b80ab74))
* **ClusterMapWindow:** Add ClusterMapWindow for visualizing cluster positions and integrate selection buttons in MainWindow ([8463771](https://github.com/chemodun/X4-UniverseEditor/commit/8463771cf1ccc320e5a2df01dc07dc54e496d930))
* **ClusterMapWindow:** Add mouse wheel support and scroll change event handling ([ce17943](https://github.com/chemodun/X4-UniverseEditor/commit/ce17943231d68aea3f9770b8684e45b5e5c20786))
* **ClusterMapWindow:** Implement sorting logic for hexagon corners in ClusterMapWindow for better UI ([6e43552](https://github.com/chemodun/X4-UniverseEditor/commit/6e43552dfc85c87bd8dc4f23bef0fb67e330bac7))
* **ClusterMapWindow:** Implemented Gates rendering with data binding and improve sector management ([7128a2b](https://github.com/chemodun/X4-UniverseEditor/commit/7128a2b81dd61a5d6e296dee2a044165ff1ebbac))
* **ColorManagement:** Integrate FactionColors into sector mapping and update color handling across relevant components ([6fed176](https://github.com/chemodun/X4-UniverseEditor/commit/6fed176bcf193c4d70cc6af8a3bb8f8ca9b1c412))
* **FactionColors:** Implement FactionColors class for managing faction color mappings and update GalaxyMapWindow to use new color handling ([caa41d3](https://github.com/chemodun/X4-UniverseEditor/commit/caa41d3f538d65ef0ea641f8f2bbaf63af463f64))
* **GalaxyConnection:** implement GalaxyConnectionData class and enhance GatesConnectionData with connection ID generation ([87e53b9](https://github.com/chemodun/X4-UniverseEditor/commit/87e53b95465c73d814cc3ebf44d86057583fbf1e))
* **GalaxyConnection:** implemented Reset to initial values, and detecting changes and readiness to save ([c31be42](https://github.com/chemodun/X4-UniverseEditor/commit/c31be421cc2925687ba9188c3e3f58c2f26030db))
* **GalaxyMap:** Add GalaxyMapGateConnection class and implement gate connection lines rendering logic with data binding ([dc59897](https://github.com/chemodun/X4-UniverseEditor/commit/dc598974f6233f04e72a36b66d835abe41f3cbdb))
* **GalaxyMapWindow:** Update hexagon opacity based on MapColorsOpacity and adjust owner color values for improved visualization ([7c985bf](https://github.com/chemodun/X4-UniverseEditor/commit/7c985bf0007326087dbb5936b0ec63afe13218dd))
* **GateData:** add GateData class with properties for coordinates, position, rotation, and active state ([f321b4a](https://github.com/chemodun/X4-UniverseEditor/commit/f321b4a40bbe19c9f4091bc843731df22c2f7c7e))
* **GateData:** add selected connection properties in table and reflect selection handling in SectorMap ([13a6dbb](https://github.com/chemodun/X4-UniverseEditor/commit/13a6dbbe3c21d062ac4ce3f5ca69d196df2bd199))
* **icon:** Add application icon and resource dictionary for image assets ([31303b0](https://github.com/chemodun/X4-UniverseEditor/commit/31303b01d4bafc5870a81b12e42bdd050ae462bf))
* **Logging:** integrate NLog for enhanced logging capabilities and configuration management ([82121e0](https://github.com/chemodun/X4-UniverseEditor/commit/82121e0dabe0b3761df7f8be39eb7027ad485e73))
* **MainWindow:** add GateDataDirect and GateDataOpposite properties with change notification ([f321b4a](https://github.com/chemodun/X4-UniverseEditor/commit/f321b4a40bbe19c9f4091bc843731df22c2f7c7e))
* **MainWindow:** Add Load button very initial functionality to load mod data and handle errors ([47a5f83](https://github.com/chemodun/X4-UniverseEditor/commit/47a5f839598fa418dd5f315951681c656b08faf5))
* **MainWindow:** add sector radius configuration and enhance UI layout for gate options ([b645bb9](https://github.com/chemodun/X4-UniverseEditor/commit/b645bb9d89a67ce56b68f4ccbab2f847cc91a66a))
* **MainWindow:** add selectable property to ComboBox items for better UI feedback ([7d69d6d](https://github.com/chemodun/X4-UniverseEditor/commit/7d69d6dc04b452c969bbd8c76d3a2530651a16d6))
* **MainWindow:** add status message binding and validation error handling ([9e7ffb9](https://github.com/chemodun/X4-UniverseEditor/commit/9e7ffb9faa8c818e94d56402cc188d9812771ee3))
* **MainWindow:** bind sector data to ComboBoxes and implement sector loading ([e5fb9fd](https://github.com/chemodun/X4-UniverseEditor/commit/e5fb9fd55485edc1eeae6272fed16453353cc5dc))
* **MainWindow:** block moving the item outside the hexagon map ([33aa1de](https://github.com/chemodun/X4-UniverseEditor/commit/33aa1ded9383fa016afb98d9bbc9b945fc873a65))
* **MainWindow:** blocking most of controls if X4 Data not loaded ([808d4cd](https://github.com/chemodun/X4-UniverseEditor/commit/808d4cdb88d81c227c67a1a842650d93daa5b9ca))
* **MainWindow:** enable two-way binding for selected connection and update selection handling in mouse events ([1c1b7dd](https://github.com/chemodun/X4-UniverseEditor/commit/1c1b7dd31efe713d317b40da6c934b78b5fb5a8e))
* **MainWindow:** expose Galaxy property for loaded data ([44edf11](https://github.com/chemodun/X4-UniverseEditor/commit/44edf1196ed2257a779b07952584ca21087ab7d8))
* **MainWindow:** GalaxyConnection creation is mostly done, except rotation ([20b6b2c](https://github.com/chemodun/X4-UniverseEditor/commit/20b6b2c7abd64b2a79da660142df85affee1df5b))
* **MainWindow:** implement gate and zone creation logic using sector data as part of future save procedure ([47cca40](https://github.com/chemodun/X4-UniverseEditor/commit/47cca400c86a3c74d6b7ebb0da08251c04f73beb))
* **MainWindow:** implement sector filtering and improve ComboBox data binding ([0be2347](https://github.com/chemodun/X4-UniverseEditor/commit/0be2347770607dd04f4e2ab0f3947759ab2f9322))
* **MainWindow:** Implement sector selection logic in ClusterMapWindow to assign selected sectors to GatesConnection ([cdb170f](https://github.com/chemodun/X4-UniverseEditor/commit/cdb170fbb8cdad76633278dc45566dce6ea222b6))
* **MainWindow:** integrate X4DataLoader ([55c5980](https://github.com/chemodun/X4-UniverseEditor/commit/55c598058b18265e7be1929541d672e47c8b8f30))
* **MainWindow:** Refactor Main window top and left get ([1040995](https://github.com/chemodun/X4-UniverseEditor/commit/104099510fe420ed24e88c3dc3ce4e9ec7c95020))
* **MainWindow:** Update game version in ChemGateKeeper on version change ([4f434fa](https://github.com/chemodun/X4-UniverseEditor/commit/4f434faea2b66c8849bf805e048b3c830f0eef93))
* **Mod:** Update mod change detection logic and improve version string formatting ([d9c78f6](https://github.com/chemodun/X4-UniverseEditor/commit/d9c78f680dec8209b3e186df066d5ac0c4dc8622))
* **SectorMap:** Add color handling for stations and implement color replacement in images ([8bf8b44](https://github.com/chemodun/X4-UniverseEditor/commit/8bf8b448285fc41728630b639244dfd47c8632c2))
* **SectorMap:** Add functionality to show expanded sector maps for direct and opposite connections ([70256c6](https://github.com/chemodun/X4-UniverseEditor/commit/70256c6c002ff2e8d25107e603e55771251c18d5))
* **SectorMap:** add SectorMap class and HexagonPointsConverter for visual representation ([ac6d621](https://github.com/chemodun/X4-UniverseEditor/commit/ac6d62123941ffb63c448d5965b60f5a77e88f86))
* **SectorMap:** enhance dragging functionality (it's working now) and update coordinate handling for SectorMap items ([70b53fc](https://github.com/chemodun/X4-UniverseEditor/commit/70b53fc507955645ae99758668d8085605d3c1c0))
* **SectorMap:** Enhance sector visualization with owner color representation for hexagons based on DominantOwner value ([0b09f82](https://github.com/chemodun/X4-UniverseEditor/commit/0b09f8216db326a61cba9d6b9df8d6567fedaace))
* **SectorMap:** Implement dynamic tooltip updates for the Map items ([aef273c](https://github.com/chemodun/X4-UniverseEditor/commit/aef273cd2f7e8d83373bc63ed7558b474e1cb63c))
* **SectorMap:** implement SectorMapItem class for representation of gates and other Sector items ([8a3c0e2](https://github.com/chemodun/X4-UniverseEditor/commit/8a3c0e2d740ac4634665ac61b6ed0a4bc9ce84cc))
* **SectorMap:** Implement station retrieval by tags and enhance connection data handling ([79ecb18](https://github.com/chemodun/X4-UniverseEditor/commit/79ecb18666017dc9273cf9335c18043e6921ac06))
* **SectorMap:** Move there handlers of mouse events for sector map items ([180e63f](https://github.com/chemodun/X4-UniverseEditor/commit/180e63f2bfc9341f1f1840f5187ceb95e7706db2))
* **SectorMap:** update size handling and item representation in SectorMap ([95c6217](https://github.com/chemodun/X4-UniverseEditor/commit/95c6217b97c1b23f476a031b2f6da4fe5b58c10c))
* **TextBoxExtensions:** add MinValue and MaxValue attached properties for range validation ([e78a8a3](https://github.com/chemodun/X4-UniverseEditor/commit/e78a8a32b0fc6e90eb21bedeab6adbafc3b21c9a))
* **TextBoxExtensions:** implement attached property for integer-only input in TextBox ([f321b4a](https://github.com/chemodun/X4-UniverseEditor/commit/f321b4a40bbe19c9f4091bc843731df22c2f7c7e))


### Bug Fixes

* **ChemGateBuilder:** Load content.xml and initiation ([f27fa83](https://github.com/chemodun/X4-UniverseEditor/commit/f27fa834d0b678606348b8037d7fcf7317c39a08))
* Correct formatting and spacing in SectorMapExpanded and GateData classes for improved readability ([e3e4bbe](https://github.com/chemodun/X4-UniverseEditor/commit/e3e4bbefda89452e48da2d6f24cea147c96b8a28))
* **GateData:** Filling the random Position ([8fe3334](https://github.com/chemodun/X4-UniverseEditor/commit/8fe3334ced4e5efe451514002fe2d9f280d0918e))
* **GateData:** fix _isDataReadyToSave condition by removing gate activity checks ([06a2be9](https://github.com/chemodun/X4-UniverseEditor/commit/06a2be959d9623c1aae7079d7cdf6f5eef018e42))
* **GateData:** restore lost lists refresh for Sector dropdowns ([e1011d2](https://github.com/chemodun/X4-UniverseEditor/commit/e1011d2f611c5f69153a53359cade920427216f3))
* **GateData:** wrong name on binding of SectorDirectConnections ([e1011d2](https://github.com/chemodun/X4-UniverseEditor/commit/e1011d2f611c5f69153a53359cade920427216f3))
* **GatesConnectionData:** Fix sector default assignment by enhancing SetDefaultsFromReference method ([fb77353](https://github.com/chemodun/X4-UniverseEditor/commit/fb77353fa80b6b9a67c1b91269573ebfb4f3d080))
* **MainWindow:** enabling /disabling ButtonGateNew and ButtonGateDelete now working properly ([5a54f16](https://github.com/chemodun/X4-UniverseEditor/commit/5a54f1686d43d7b9c4df1fddd44fa08079d02729))
* **MainWindow:** improve X4 data folder validation and user feedback ([fb4f6d5](https://github.com/chemodun/X4-UniverseEditor/commit/fb4f6d5e4d375efe5daa598843400f82b0156ca3))
* **MainWindow:** prevent selection of new items for sector connections ([f0edb54](https://github.com/chemodun/X4-UniverseEditor/commit/f0edb5468b6ecc8f95438752ea5d488fa3cbe032))
* **MainWindow:** Refresh sector views after resetting gates connection ([551ff9b](https://github.com/chemodun/X4-UniverseEditor/commit/551ff9ba36e294a1141080c9187c57d7c162b58a))
* **MainWindow:** validate X4 data folder and file existence ([80a67dc](https://github.com/chemodun/X4-UniverseEditor/commit/80a67dc57ce3ff467a0d23a4384fb0f857c73b46))
* **Mod:** fix galaxy.xml diff structure on a Mod Save ([23a6cea](https://github.com/chemodun/X4-UniverseEditor/commit/23a6cea587c7debf0f4b6734781cab56ac3cc7e6))
* **SectorMap:** Internal coordinates not updated when item is moved ([3e98da7](https://github.com/chemodun/X4-UniverseEditor/commit/3e98da7f67e84f80ba5bac99f813495a4638fdb9))
* **TextBoxExtensions:** improve validation error handling by clearing messages on valid input ([17ab0e5](https://github.com/chemodun/X4-UniverseEditor/commit/17ab0e513aa1a15c8504bce1c598c2abce6e9b41))


### Code Refactoring

* Change window startup location to CenterOwner for GalaxyMap and SectorMap ([64a96fa](https://github.com/chemodun/X4-UniverseEditor/commit/64a96fad61df48b43b4dd0936c96b57cbc0c2f64))
* **ChemGateBuilder:** Add Attributes dictionary to SectorConnectionData and implement highway point handling ([1762288](https://github.com/chemodun/X4-UniverseEditor/commit/17622889cc1a71a0bda233a25f02b9a8f3cfd3bc))
* **ChemGateBuilder:** Add GalaxyMap icon and update window title ([fa836db](https://github.com/chemodun/X4-UniverseEditor/commit/fa836dbc96bfd6caea35357bb95e615f47135850))
* **ChemGateBuilder:** Add new highway map resources and enhance image path logic for better status representation ([c188433](https://github.com/chemodun/X4-UniverseEditor/commit/c1884338829b29fecbbcb0e4f75f37125c60088b))
* **ChemGateBuilder:** Add version information ([821c790](https://github.com/chemodun/X4-UniverseEditor/commit/821c790604d21d8af82b26d5e40bfafdb25cadc1))
* **ChemGateBuilder:** Enhance null safety and improve property handling across multiple classes ([5a54f16](https://github.com/chemodun/X4-UniverseEditor/commit/5a54f1686d43d7b9c4df1fddd44fa08079d02729))
* **ChemGateBuilder:** Implement new mod creation functionality and improve sector map color handling ([ca42411](https://github.com/chemodun/X4-UniverseEditor/commit/ca424114c8c2f5e0df8c3d11d9efe25685721c30))
* **ChemGateBuilder:** Improve mod change detection logic ([8fe3334](https://github.com/chemodun/X4-UniverseEditor/commit/8fe3334ced4e5efe451514002fe2d9f280d0918e))
* **ChemGateBuilder:** Improve sector map handling and update new gate identification logic ([6669527](https://github.com/chemodun/X4-UniverseEditor/commit/6669527e003087cf2d0b4da02d227b6533657fa4))
* **ChemGateBuilder:** Rename GalaxyMapGateConnection to GalaxyMapInterConnection and implement related logic for highway connections ([5ccad37](https://github.com/chemodun/X4-UniverseEditor/commit/5ccad371c8190880975a2cab89a0daa20294c851))
* **ChemGateBuilder:** Rename SectorItem to SectorsListItem for consistency across the codebase ([0029354](https://github.com/chemodun/X4-UniverseEditor/commit/002935419e8926f1a80fb3ff4b6acf2078207af1))
* **ChemGateBuilder:** Update data flags on property change and enhance mod versioning ([f27fa83](https://github.com/chemodun/X4-UniverseEditor/commit/f27fa834d0b678606348b8037d7fcf7317c39a08))
* **ClusterMap:** Refactor to use ClusterMapCluster and ClusterMapSector classes. Avoid to recrate all staff on any change ([0025211](https://github.com/chemodun/X4-UniverseEditor/commit/00252117eb1fa8d2090233ff8727339eece19777))
* **ClusterMapWindow:** Adjust zoom step and maximal hexagon width for improved scaling behavior ([cc7fb7a](https://github.com/chemodun/X4-UniverseEditor/commit/cc7fb7a4b14486212e09132eb341a111c9ff877f))
* **ClusterMapWindow:** Implemented working with more than one sector per cluster ([ce17943](https://github.com/chemodun/X4-UniverseEditor/commit/ce17943231d68aea3f9770b8684e45b5e5c20786))
* **FactionColors:** Clear mapped colors and brushes before loading new data ([37474ed](https://github.com/chemodun/X4-UniverseEditor/commit/37474ed75110265ff147babaa2cbdf4b817a4bae))
* **GalaxyConnectionData, GateData:** streamline coordinate retrieval by using Position class and simplify calculations ([a19c03f](https://github.com/chemodun/X4-UniverseEditor/commit/a19c03ff81da9c1db9bc4c9160942f7684747653))
* **GalaxyConnectionData, GateData:** Update property accessors to use PascalCase for consistency ([10603ea](https://github.com/chemodun/X4-UniverseEditor/commit/10603ea90f7b76104bf456f9ef23b5ebfc4c075b))
* **GalaxyConnectionData:** Enhance constructor to handle null connectionData and positions and rotations from GalaxyConnection ([71e8b3f](https://github.com/chemodun/X4-UniverseEditor/commit/71e8b3fecb7a350987193ebc698214bc90c84f7c))
* **GalaxyConnectionData:** Make Reset to already stored GalaxyConnectionData working ([d3ed04c](https://github.com/chemodun/X4-UniverseEditor/commit/d3ed04c5cd1f7487a4eeb474c0f153afb3cd0212))
* **GalaxyConnectionData:** simplify gate position retrieval by directly accessing zone position properties ([a0572fe](https://github.com/chemodun/X4-UniverseEditor/commit/a0572fe58e741812acadab8a1f685c7eb6772ce3))
* **GalaxyMap:** Enhance connection method to support map mode for sector visualization ([08a7a37](https://github.com/chemodun/X4-UniverseEditor/commit/08a7a37066a4fd02cc86abd854b0833d4d51353b))
* **GalaxyMap:** Improve corner selection and draw order for sectors ([0c46613](https://github.com/chemodun/X4-UniverseEditor/commit/0c46613e6f7eac177dd0e83146e35cf5a7de6af2))
* **GalaxyMapWindow:** Enhance sector text display with TextBlock and dynamic font sizing ([6afba21](https://github.com/chemodun/X4-UniverseEditor/commit/6afba215b76d608a9ea915a9168377ea26345521))
* **GalaxyMapWindow:** Fix map width and height calculation with ScaleFactor ([b264651](https://github.com/chemodun/X4-UniverseEditor/commit/b26465162bedbb88ba89dc598e1834fde869e351))
* **GalaxyMapWindow:** Improve parameter naming for clarity and consistency in bindings ([2b6fb47](https://github.com/chemodun/X4-UniverseEditor/commit/2b6fb47b2c1a9ee15f687ee1657cb0240aa4b195))
* **GalaxyMapWindow:** Make possible to show the currently edited connection ([5fa1131](https://github.com/chemodun/X4-UniverseEditor/commit/5fa1131eaeb50f9f8ed1658c050ac303d419575d))
* **GalaxyMapWindow:** Make possible to show the Gate connections from the mod ([ccf521b](https://github.com/chemodun/X4-UniverseEditor/commit/ccf521bb2b73007a3bed7c27ccd9a60b1df6dcfc))
* **GalaxyMapWindow:** Replace ClusterMapWindow with GalaxyMapWindow and apply related items renaming ([3581000](https://github.com/chemodun/X4-UniverseEditor/commit/358100017adca268a4c1d63f00590999a847c322))
* **GalaxyMapWindow:** Update background color to LightGray and improve variable declarations ([b367d6f](https://github.com/chemodun/X4-UniverseEditor/commit/b367d6fb94c63d227688febae18229ff8552ee96))
* **GalaxyMapWindow:** Update corner assignment logic based on Cluster macro values for improved hexagon positioning ([4d7f28a](https://github.com/chemodun/X4-UniverseEditor/commit/4d7f28a18605920521cc8ef3d3b684d0a859a3da))
* **GalaxyMapWindow:** Update corner assignments for hexagons based on Cluster macro values for improved accuracy ([f79f2e4](https://github.com/chemodun/X4-UniverseEditor/commit/f79f2e4cd84c5c3061eb1fddd13caea3d6e63a66))
* **GalaxyMapWindow:** Update cursor styles during panning and mouse interactions for improved user experience ([320b14a](https://github.com/chemodun/X4-UniverseEditor/commit/320b14abd2de3555fafa5dcde09a97f251df55a9))
* **GateData, MainWindow:** Improve gate connection management and enable/disable UI elements based on connection edit status ([70857e5](https://github.com/chemodun/X4-UniverseEditor/commit/70857e506e5561a4eb01002df128a2551b9e4054))
* **GateData:** add DataTable properties for Sector Direct and Opposite connections and add headers for all DataGrid items ([a719199](https://github.com/chemodun/X4-UniverseEditor/commit/a719199ffdb6c4f330c6bd97e9073e5ebc499342))
* **GateData:** add existing connections macros for direct and opposite sectors to prevent create a doubles for the existing connections ([49a6bb6](https://github.com/chemodun/X4-UniverseEditor/commit/49a6bb6e0eb11fff722f1a6965411895181aff5a))
* **GateData:** add FromQuaternion method to convert Quaternion to Roll, Pitch, Yaw ([91090e5](https://github.com/chemodun/X4-UniverseEditor/commit/91090e5af36442ab3c16ad7a03dba9c2a2037e88))
* **GateData:** Add random position filling for gates ([f61468c](https://github.com/chemodun/X4-UniverseEditor/commit/f61468cbfb4c8838d113d47d23ecb1c94b80fa91))
* **GateData:** add ToQuaternion method for converting Euler angles to Quaternion ([786c19b](https://github.com/chemodun/X4-UniverseEditor/commit/786c19b234c955325d6e78ab45e6e5c1969dabef))
* **GateData:** Enhance gate status management and add reset functionality ([a73d55f](https://github.com/chemodun/X4-UniverseEditor/commit/a73d55f0ab32a09b04634c43eaea2cd3b66af604))
* **GateData:** enhance sector connection data with coordinates calculation ([eabfec9](https://github.com/chemodun/X4-UniverseEditor/commit/eabfec94238c7bf6086cba775b8516913f915958))
* **GateData:** Implement gate distance validation logic and ensure checks are performed on property changes ([e1eb834](https://github.com/chemodun/X4-UniverseEditor/commit/e1eb834fdea4069418a401c1d77c42e45b05c5de))
* **GateData:** implement styles for some items to make configuration more simple ([0f32f79](https://github.com/chemodun/X4-UniverseEditor/commit/0f32f79902d40c05ccd9c70af9d1215362781d62))
* **GateData:** Improve gate distance validation and provide recommendations for adjustments ([2dd91b4](https://github.com/chemodun/X4-UniverseEditor/commit/2dd91b49f8d1b936c95062e9cf6b9072bcc7cf97))
* **GateData:** Remove redundant gate distance checks on property changes ([7741be6](https://github.com/chemodun/X4-UniverseEditor/commit/7741be693ea6f2be7e0bfb8556d6a2cda93ef8da))
* **GateData:** Rename connection-related variables and methods to use ObjectInSector for improved clarity ([ac91623](https://github.com/chemodun/X4-UniverseEditor/commit/ac9162353a67649a2542def2ccfbb4929f16536a))
* **GateData:** Rename connection-related variables and methods to use ObjectInSector for improved clarity ([21716b2](https://github.com/chemodun/X4-UniverseEditor/commit/21716b29ed76fdd0eff471ffa3549593ab37ba4f))
* **GateData:** replace DataTable with ObservableCollection for SectorDirectConnections and enhance data binding in MainWindow ([9035d8a](https://github.com/chemodun/X4-UniverseEditor/commit/9035d8aa361659a4474dc11aac81d0cf8dda87a5))
* **GateData:** replace DataTable with ObservableCollection for SectorOppositeConnections and update data binding in MainWindow ([e1011d2](https://github.com/chemodun/X4-UniverseEditor/commit/e1011d2f611c5f69153a53359cade920427216f3))
* **GateData:** Simplify initialization of collections and constructors ([d386f35](https://github.com/chemodun/X4-UniverseEditor/commit/d386f35962127dace8cca56a4976797490010533))
* **GateData:** Simplify MainWindow reference checks using pattern matching ([7808678](https://github.com/chemodun/X4-UniverseEditor/commit/7808678638fd52d1a0219fe969cb8fe7ad7e72b2))
* **GateData:** simplify zone coordinate retrieval by using zone's GetCoordinates method directly ([8c659b1](https://github.com/chemodun/X4-UniverseEditor/commit/8c659b1967c2ff6a9a1a22e836525c4ebf81ff74))
* **GateData:** Update possibleValues array syntax and change Attributes initialization ([315660d](https://github.com/chemodun/X4-UniverseEditor/commit/315660de911fe295b7daddd9a37d7a51cd63e73f))
* **GatesConnection:** implement GatesConnectionData class for managing sector and gate connections at once ([defd42d](https://github.com/chemodun/X4-UniverseEditor/commit/defd42df16c22af84b15ffcd0c9470a12394796c))
* **icon:** Update application icon and add FontAwesome icons for buttons ([8f9664e](https://github.com/chemodun/X4-UniverseEditor/commit/8f9664e6acf63dcb2ef37dcb902e73c34ae59c08))
* **Layout:** fit all gate parameters in one grid and increase size of sector map canvas ([d45aa3b](https://github.com/chemodun/X4-UniverseEditor/commit/d45aa3b783f6edf4685dd141534260ea53d96528))
* **Logging:** Update logging configuration and replace logger instances with centralized logging utility ([0f596a0](https://github.com/chemodun/X4-UniverseEditor/commit/0f596a0b706b1704042cbedb7c29a18ec2276a9d))
* **MainWindow, Mod:** Enhance sector filtering logic(from current mod) and update mod version handling ([f5fd96a](https://github.com/chemodun/X4-UniverseEditor/commit/f5fd96a6f8b3248cc7fa60b9cf7762e8150bc2c3))
* **MainWindow:** Add 'Program' group box with exit button in the ribbon ([26c7773](https://github.com/chemodun/X4-UniverseEditor/commit/26c777317ab573e1671182b33e08f9ca67b8060a))
* **MainWindow:** Add load button for X4 data with confirmation dialog ([2fc774e](https://github.com/chemodun/X4-UniverseEditor/commit/2fc774e65c11a27de224937dc9eb95b24160397a))
* **MainWindow:** Add Name property and update ComboBoxItem style references ([e3a92f3](https://github.com/chemodun/X4-UniverseEditor/commit/e3a92f35af322f428b16941af0b685dfab90b66a))
* **MainWindow:** adjust button margins for better layout consistency and update status message for clarity ([53c2d08](https://github.com/chemodun/X4-UniverseEditor/commit/53c2d08a09d57ac351859c5b6fb3cb87f9c32b09))
* **MainWindow:** adjust window dimensions and move the Ribbon Edit Content to the  Gate Connections layout with new Add and Reset buttons ([04a5947](https://github.com/chemodun/X4-UniverseEditor/commit/04a59476c941ea0513406dd995b29136a73350b3))
* **MainWindow:** Center sector map expanded window and adjust size dynamically; update layout for new gate coordinates display ([5576afc](https://github.com/chemodun/X4-UniverseEditor/commit/5576afc635880b4c405f3d19a8fd3c463d2c9566))
* **MainWindow:** Center window on startup and refactor variable initializations ([eb57e3f](https://github.com/chemodun/X4-UniverseEditor/commit/eb57e3f30b5b307375edf0494e20fbc7fa003079))
* **MainWindow:** Change colors of SectorMap related elements ([8f9664e](https://github.com/chemodun/X4-UniverseEditor/commit/8f9664e6acf63dcb2ef37dcb902e73c34ae59c08))
* **MainWindow:** enhance Gate Connections layout with new buttons for adding and deleting gates ([be2cd5c](https://github.com/chemodun/X4-UniverseEditor/commit/be2cd5c8a9c589ca3f8ea01fdb269d6a541cca35))
* **MainWindow:** Enhance SetDefaultsFromReference method to accept AllSectors parameter ([d4c7e82](https://github.com/chemodun/X4-UniverseEditor/commit/d4c7e8233e2da9beefc962be74cbdb226e930c8d))
* **MainWindow:** Enhance UI for Gates settings with sliders for minimal distance and sector radius ([d29e5ce](https://github.com/chemodun/X4-UniverseEditor/commit/d29e5cea692e52696bb0cb0e2457ba3604084c6c))
* **MainWindow:** Enhance X4 Data Options layout and add versioning controls ([505b6a0](https://github.com/chemodun/X4-UniverseEditor/commit/505b6a0d40b8fcea590c500ba97259dde751a38f))
* **MainWindow:** Improve LoadData method to enhance data loading and status messaging ([6440ae4](https://github.com/chemodun/X4-UniverseEditor/commit/6440ae45551b537a71f151cd64aed35386a8ef38))
* **MainWindow:** increase minimum height and canvas sizes for better layout visibility ([9dfeddb](https://github.com/chemodun/X4-UniverseEditor/commit/9dfeddb44fe28d052cff4aefcbd79d1f7fc906a7))
* **MainWindow:** Integrate game version setting into mod initialization and update version check logic ([6d7dbf9](https://github.com/chemodun/X4-UniverseEditor/commit/6d7dbf9da8c5a035850d45e1daa7be011e949be9))
* **MainWindow:** Make ButtonGateNew and ButtonGateDelete fully functional. Navigation in DataGridGalaxyConnections is functional too ([a73d55f](https://github.com/chemodun/X4-UniverseEditor/commit/a73d55f0ab32a09b04634c43eaea2cd3b66af604))
* **MainWindow:** Make ButtonSave content changeable depend on action. ([5a54f16](https://github.com/chemodun/X4-UniverseEditor/commit/5a54f1686d43d7b9c4df1fddd44fa08079d02729))
* **MainWindow:** reorganize layout to make Gate Connections table vertically stretchable ([0d4598f](https://github.com/chemodun/X4-UniverseEditor/commit/0d4598fa8a4c78ca6c62c22193b3f7000bed7b0f))
* **MainWindow:** Reorganize methods locations in code ([684dd8c](https://github.com/chemodun/X4-UniverseEditor/commit/684dd8c87a461eb226741812b0000c5bfcc663fb))
* **MainWindow:** Reorganize properties for improved readability and maintainability ([d219aa1](https://github.com/chemodun/X4-UniverseEditor/commit/d219aa147275182a6f2cead4e4b8177871771ff9))
* **MainWindow:** Simplify null checks and improve logging for mouse events ([30478a9](https://github.com/chemodun/X4-UniverseEditor/commit/30478a91cf8bab6d38c6c92b6982218ed293aebd))
* **MainWindow:** Update App icon ([fb9d354](https://github.com/chemodun/X4-UniverseEditor/commit/fb9d354ecb5d33ca824d97f67f7dd8ef30ce887e))
* **MainWindow:** Update button bindings for mod actions and implement mod save functionality ([0dc7e25](https://github.com/chemodun/X4-UniverseEditor/commit/0dc7e25806acd74cea4ad6bf1e4dc313905feaf7))
* **MainWindow:** update gate creation to include rotation as Quaternion ([10c8cc5](https://github.com/chemodun/X4-UniverseEditor/commit/10c8cc59250d71c58a8f6bc5e6320a4324c40aa7))
* **MainWindow:** update gate labels to include units for better clarity ([f613321](https://github.com/chemodun/X4-UniverseEditor/commit/f6133212e39305b109af96196289a6bc944e82ed))
* **Mod:** Add error handling for loading sectors, zones, and galaxy files ([94f05fb](https://github.com/chemodun/X4-UniverseEditor/commit/94f05fbbcb67e50a457d5ed07c0cb477d6de7198))
* **Mod:** Add initial files gathering ([eb214fc](https://github.com/chemodun/X4-UniverseEditor/commit/eb214fc832f1013dac37e550f079554ee2a9eca1))
* **Mod:** Adjust versioning logic and improve file dialog handling ([47fd0f0](https://github.com/chemodun/X4-UniverseEditor/commit/47fd0f0fcfddbc940e439d0d5c85b94a5499c24d))
* **Mod:** Finalize LoadData method and enhance error handling for content file loading ([bc80eab](https://github.com/chemodun/X4-UniverseEditor/commit/bc80eabfd40e6b41bd075bde81453afa75f118b2))
* **Mod:** Implement GalaxyConnections loading into internal variable in Load() method ([d63b81f](https://github.com/chemodun/X4-UniverseEditor/commit/d63b81fb1e34891b76a22e3201b53cef97690256))
* **Mod:** Update fields to readonly and simplify collection initialization ([843f3f5](https://github.com/chemodun/X4-UniverseEditor/commit/843f3f5b666d26272fd402f7f6e28c7a51940861))
* **SectorMap:** Add random coordinate generation for stations without defined zones ([32386bf](https://github.com/chemodun/X4-UniverseEditor/commit/32386bf1b87b3b84c3518527e7018c31415c604a))
* **SectorMap:** Adjust size calculations and alignment for improved layout to use as much space of Canvas as possible, taking in account relation between width and height of Hexagon. I.e make Map as much bigger as possible in current layout. ([52cce49](https://github.com/chemodun/X4-UniverseEditor/commit/52cce492f62eeb0d0042641f4af6f5a5ee4f7a9d))
* **SectorMap:** Adjust visual size and item size calculations for better rendering ([72e6a5f](https://github.com/chemodun/X4-UniverseEditor/commit/72e6a5fdc6cb1f2b2fd754ed405169aab4601565))
* **SectorMap:** Bind slider minimum and maximum values to dynamic properties for internal sector size ([0fe189b](https://github.com/chemodun/X4-UniverseEditor/commit/0fe189baf7ed2d3b6f76909c11feb21ca7af1b5f))
* **SectorMap:** Correct result string assignment for station ID formatting ([5ba5741](https://github.com/chemodun/X4-UniverseEditor/commit/5ba57419f13da83d81db5346997e136d42bd3072))
* **SectorMap:** Enhance item update logic(get rid of delete/add) and change a processing of minimum item size ([186045f](https://github.com/chemodun/X4-UniverseEditor/commit/186045fc5cd60cda9f50d6d2cefb640714588899))
* **SectorMap:** Enhance sector map drawing with new 'From' property and mod connections retrieval ([5ff694c](https://github.com/chemodun/X4-UniverseEditor/commit/5ff694c5bbcb989c30a2f34e870ecf626daae78b))
* **SectorMap:** enhance SectorMap items selection functionality and propagate functionality on opposite Sector ([5e883d1](https://github.com/chemodun/X4-UniverseEditor/commit/5e883d1ea27aebdbdf7e61eb00500d303d13d570))
* **SectorMap:** Enhance tooltip formatting for connection data display ([66f1e5e](https://github.com/chemodun/X4-UniverseEditor/commit/66f1e5ee671bc779867f50a1bcb1359d6c1a065c))
* **SectorMap:** finishing migrating on bitmap images for objects ([e6e0835](https://github.com/chemodun/X4-UniverseEditor/commit/e6e0835f3d51311749360578c508487b0b5963d6))
* **SectorMap:** first try to use jumpgate image from game ([401f042](https://github.com/chemodun/X4-UniverseEditor/commit/401f0422801bd8555e817f34f0afcb23ee793405))
* **SectorMap:** Implement SetSector method to manage sector connections and streamline data handling ([2cf58fa](https://github.com/chemodun/X4-UniverseEditor/commit/2cf58fa1bdcf0393adc82aba4d93e585365de446))
* **SectorMap:** Implemented slider to manage internal sector size per sector ([ca141db](https://github.com/chemodun/X4-UniverseEditor/commit/ca141db501d179f92b9171a41dbb8a6dbd79eb92))
* **SectorMap:** simplify AddItem method, add UpdateItem  method and enhance SectorMapItem with connection data handling ([22beca0](https://github.com/chemodun/X4-UniverseEditor/commit/22beca0254b3c101b387996b1225153aaba2ce1a))
* **SectorMap:** Simplify canvas and hexagon connection logic by introducing Connect method ([0945b3e](https://github.com/chemodun/X4-UniverseEditor/commit/0945b3ebe323105b66d2df5acc9d3b9a35f83865))
* **SectorMap:** Update size change handling to use width and height parameters for improved clarity and make a Hexagon centered ([1e2134c](https://github.com/chemodun/X4-UniverseEditor/commit/1e2134c86805020e2e6cf55487179e6d37974cf8))
* **StatusMessage:** Enhance status message handling with type differentiation and automatic clearing ([f7f334d](https://github.com/chemodun/X4-UniverseEditor/commit/f7f334d63dbfcde99ddc5aabadf8823a8d94614e))
* **TextBoxExtensions:** Comment out handling for invalid input case to improve validation logic ([e17482c](https://github.com/chemodun/X4-UniverseEditor/commit/e17482c71c0590f5e13fee1de3ad50e273fe74a7))
* **TextBoxExtensions:** Update regex to allow optional leading minus sign for negative integers ([16a678b](https://github.com/chemodun/X4-UniverseEditor/commit/16a678b5dc56b33e8e2430d01cfadbd4c932d2dd))
* **UI:** Improve responsiveness of settings panel with dynamic layout adjustments ([f7700d6](https://github.com/chemodun/X4-UniverseEditor/commit/f7700d640f8658d823d7574850b6108a6d9249d4))
* **X4Data:** Update data loading logic and introduce version handling from patchactions.xml ([ce5efde](https://github.com/chemodun/X4-UniverseEditor/commit/ce5efdeba84d62b29323ddc11844a151b6827fb3))


### Miscellaneous Chores

* **App:** remove unused dependency on Microsoft.Extensions.DependencyInjection ([84711bc](https://github.com/chemodun/X4-UniverseEditor/commit/84711bcde9ac80ff6ace4648125434ffa811073a))

## [0.0.1](https://github.com/chemodun/X4-UniverseEditor/compare/ChemGateBuilder-v0.0.1...ChemGateBuilder@v0.0.1) (2025-01-19)


### Features

* enhance MainWindow with configuration options and data handling ([ba3f92a](https://github.com/chemodun/X4-UniverseEditor/commit/ba3f92a302f37988302f6aa211e03d378f4815a2))


### Miscellaneous Chores

* initialize ChemGateBuilder application with WPF structure and main window ([a6e4939](https://github.com/chemodun/X4-UniverseEditor/commit/a6e4939feb0d388fbd6943ad0a6bf16c6a973b6b))
* release 0.0.1 ([7efa89e](https://github.com/chemodun/X4-UniverseEditor/commit/7efa89e5fefe14be0435dd40d1539eaee93c5070))
