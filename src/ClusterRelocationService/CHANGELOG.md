# Changelog

## [1.2.0](https://github.com/chemodun/X4-UniverseEditor/compare/ClusterRelocationService@v1.1.0...ClusterRelocationService@v1.2.0) (2025-10-05)


### Features

* **content:** update version and description for relocated clusters ([eaaf7d7](https://github.com/chemodun/X4-UniverseEditor/commit/eaaf7d7569a561cdac3c60043c0b4ca3724d1290))
* **GalaxyMapClusterForClusterRelocation:** add IsOverlapping property and update status handling ([7fb8250](https://github.com/chemodun/X4-UniverseEditor/commit/7fb825020bed05ec450370ab2d665c51fe68b7fb))
* **GalaxyMapSectorForClusterRelocation:** add IsOverlapped property and update status handling ([7fb8250](https://github.com/chemodun/X4-UniverseEditor/commit/7fb825020bed05ec450370ab2d665c51fe68b7fb))
* **GalaxyMapViewerForClusterRelocation:** implement overlapping cluster checks ([7fb8250](https://github.com/chemodun/X4-UniverseEditor/commit/7fb825020bed05ec450370ab2d665c51fe68b7fb))
* **README:** enhance documentation for overlaid clusters handling ([d86ffce](https://github.com/chemodun/X4-UniverseEditor/commit/d86ffce6e90eddff6b4e96233c0356028a39d3bf))


### Bug Fixes

* **MainWindow:** update context menu for overlapping clusters ([7fb8250](https://github.com/chemodun/X4-UniverseEditor/commit/7fb825020bed05ec450370ab2d665c51fe68b7fb))
* **MainWindow:** update relocation logic for overlaid clusters ([71956f7](https://github.com/chemodun/X4-UniverseEditor/commit/71956f78e1f81d0d44b0cdfeca811f52ccd342b3))
* **README:** correct context menu description for overlaid clusters ([d86ffce](https://github.com/chemodun/X4-UniverseEditor/commit/d86ffce6e90eddff6b4e96233c0356028a39d3bf))
* **RelocatedCluster:** enhance cluster name formatting for multiple sectors ([83a82ae](https://github.com/chemodun/X4-UniverseEditor/commit/83a82ae30b14925f167caaf0657bce75da411063))


### Code Refactoring

* **GalaxyMapClusterForClusterRelocation:** rename IsOverlapping to IsCovers ([71956f7](https://github.com/chemodun/X4-UniverseEditor/commit/71956f78e1f81d0d44b0cdfeca811f52ccd342b3))


### Documentation

* **ClusterRelocationService:** more images ([a2061c0](https://github.com/chemodun/X4-UniverseEditor/commit/a2061c05aecd06b6a02af99563d7174661502c47))
* **README:** improve clarity of galaxy relocation feature ([7c0bb78](https://github.com/chemodun/X4-UniverseEditor/commit/7c0bb7804059b957dfe5ee0c634188851b4ae32f))
* **README:** Update README's files ([a158917](https://github.com/chemodun/X4-UniverseEditor/commit/a158917d4e4a0815f48f1f858e675d727ab43dbf))
* **README:** Update README's files ([f759f78](https://github.com/chemodun/X4-UniverseEditor/commit/f759f780a157ec08b7113ad4f4cf6febd2a8cc64))

## [1.1.0](https://github.com/chemodun/X4-UniverseEditor/compare/ClusterRelocationService@v1.0.0...ClusterRelocationService@v1.1.0) (2025-09-28)


### Features

* **ClusterRelocationService:** Initial pre-release version 1.0.0 ([ea30a1b](https://github.com/chemodun/X4-UniverseEditor/commit/ea30a1b8517d2dc3d77be68db712f61cc49650ff))


### Bug Fixes

* **GalaxyMapViewerForClusterRelocation:** correct relocation logic ([48f3931](https://github.com/chemodun/X4-UniverseEditor/commit/48f393169089989a3d4dab2e13509fd9ced766a0))
* **MainWindow.xaml.cs:** update data version and mod loading behavior ([7f51893](https://github.com/chemodun/X4-UniverseEditor/commit/7f51893f1f31f8f4fd9e36a526c66d9bb2d274cd))
* **MainWindow.xaml:** increase minimum width for better layout ([7f51893](https://github.com/chemodun/X4-UniverseEditor/commit/7f51893f1f31f8f4fd9e36a526c66d9bb2d274cd))
* **RelocatedClustersMod:** add offset handling for cluster positions if not present (fix saving central cluster) ([8f0f5d5](https://github.com/chemodun/X4-UniverseEditor/commit/8f0f5d5e889dab12788ea241b438f43fa3b87bb0))


### Code Refactoring

* **MainWindow:** add confirmation dialog for unsaved changes ([8dadc8f](https://github.com/chemodun/X4-UniverseEditor/commit/8dadc8f8321a465a96d974e1eea573ecc2635770))
* **MainWindow:** add update check options ([edf7ab2](https://github.com/chemodun/X4-UniverseEditor/commit/edf7ab250211d70ca362492201ed969629295b0a))
* **MainWindow:** clear relocation marker on mod load ([15f08c5](https://github.com/chemodun/X4-UniverseEditor/commit/15f08c5d51a5bb79e4e56d9a9ba0974b80e60cdd))
* **MainWindow:** enhance mod loading error handling ([24ca8ef](https://github.com/chemodun/X4-UniverseEditor/commit/24ca8ef8d89d0b3873e77dca58c1d7aa08afdf16))
* **MainWindow:** rename ResetRelocation to ResetRelocations ([8dadc8f](https://github.com/chemodun/X4-UniverseEditor/commit/8dadc8f8321a465a96d974e1eea573ecc2635770))
* **MainWindow:** update X4 data version ([8dadc8f](https://github.com/chemodun/X4-UniverseEditor/commit/8dadc8f8321a465a96d974e1eea573ecc2635770))
* **Mod:** update mod description for clarity ([3257afa](https://github.com/chemodun/X4-UniverseEditor/commit/3257afa8c48d131dbb41a77cd88ad35d95b8e63e))
* **Mod:** update mod identifiers and descriptions ([24ca8ef](https://github.com/chemodun/X4-UniverseEditor/commit/24ca8ef8d89d0b3873e77dca58c1d7aa08afdf16))
* **Mod:** update ModId for consistency ([c5626d8](https://github.com/chemodun/X4-UniverseEditor/commit/c5626d8c2b51fe9b60fdcb47c40f55cd07de4c47))
* **Mod:** update RelocatedCluster constructor parameters ([a410ca5](https://github.com/chemodun/X4-UniverseEditor/commit/a410ca58af9f23b051221780532a0237e3279a54))
* **RelocatedCluster:** enhance target position handling ([48f3931](https://github.com/chemodun/X4-UniverseEditor/commit/48f393169089989a3d4dab2e13509fd9ced766a0))


### Miscellaneous Chores

* **ClusterRelocationService.csproj:** add assembly metadata ([edf7ab2](https://github.com/chemodun/X4-UniverseEditor/commit/edf7ab250211d70ca362492201ed969629295b0a))
* **ClusterRelocationService:** remove configuration file from a git ([bacfdc4](https://github.com/chemodun/X4-UniverseEditor/commit/bacfdc4c4dbfaa2e9c085611353153286598853a))
* **content:** add content.xml for Relocated Clusters extension ([fe9b014](https://github.com/chemodun/X4-UniverseEditor/commit/fe9b01418da428f6734fd202b9a4080e8d8be64d))
* **videos:** remove obsolete video files ([aa726b2](https://github.com/chemodun/X4-UniverseEditor/commit/aa726b2ba0f97c6176a9af8c62a2699cf2eeadbd))


### Documentation

* **ClusterRelocationService:** update EGOSOFT forum URL in metadata ([18778cc](https://github.com/chemodun/X4-UniverseEditor/commit/18778cc0e1c4ead4337d3cf5a71edbb2eb1dbad7))
* **ClusterRelocationService:** update README.md and upload some images ([83bf2c3](https://github.com/chemodun/X4-UniverseEditor/commit/83bf2c386de22deec5f709e6f2aaaca453b9a784))
* **images:** add configuration and main window images ([473745c](https://github.com/chemodun/X4-UniverseEditor/commit/473745c5fd62ba408df199791ae114f48ed3b66b))
* **images:** Update GIF files ([6152cac](https://github.com/chemodun/X4-UniverseEditor/commit/6152cac5f8be55c11eb06d3db015b8a3fb0339c1))
* **images:** Update GIF files ([b75a1b5](https://github.com/chemodun/X4-UniverseEditor/commit/b75a1b5e59546604cceedafdf091c1e866fc08dd))
* **README:** enhance documentation with new features and video links ([473745c](https://github.com/chemodun/X4-UniverseEditor/commit/473745c5fd62ba408df199791ae114f48ed3b66b))
* **README:** update image links and changelog date ([bf3e5a7](https://github.com/chemodun/X4-UniverseEditor/commit/bf3e5a7ed2a4f8aecb35c0650545fd6d99b59bf3))
* **README:** Update README's files ([2f79295](https://github.com/chemodun/X4-UniverseEditor/commit/2f7929562d80caff44e4de689580e594a7edc756))
* **README:** Update README's files ([e322471](https://github.com/chemodun/X4-UniverseEditor/commit/e322471659d600a1a65ea0ca9ad005afc11bfea3))
* **README:** Update README's files ([44d67e5](https://github.com/chemodun/X4-UniverseEditor/commit/44d67e59b78e0967f16e2cdb66d2f29512ccccf4))
* **README:** update version in changelog to 1.1.0 ([093e3bd](https://github.com/chemodun/X4-UniverseEditor/commit/093e3bdf6e3c40dcb22e4df15ebdaecb1b1a1e38))
* **videos:** add demonstration videos ([473745c](https://github.com/chemodun/X4-UniverseEditor/commit/473745c5fd62ba408df199791ae114f48ed3b66b))
* **videos:** add first start video tutorial ([72e8850](https://github.com/chemodun/X4-UniverseEditor/commit/72e8850a376d9362376c5a6beaaca51512eb8574))
* **videos:** add more video tutorials ([fe8bc9d](https://github.com/chemodun/X4-UniverseEditor/commit/fe8bc9d28a71bf88d0033b0bfa8009a15e1b36ae))

## Changelog
