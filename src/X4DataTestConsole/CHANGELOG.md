# Changelog

## [0.2.6](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.2.5...X4DataTestConsole@v0.2.6) (2025-02-17)


### Miscellaneous Chores

* **MainWindow:** add processing order for X4 data loading ([e73e5ed](https://github.com/chemodun/X4-UniverseEditor/commit/e73e5edcfbf50c87378999c9d714eedc72042e70))

## [0.2.5](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.2.4...X4DataTestConsole@v0.2.5) (2025-02-17)


### Code Refactoring

* **LoadData:** define game files structure locally ([777dff4](https://github.com/chemodun/X4-UniverseEditor/commit/777dff44fbd99dd9fa1f83db479f819a864b66a3))
* **X4DataTest:** update data loading mechanism to use DataLoader for improved clarity and structure ([147e16b](https://github.com/chemodun/X4-UniverseEditor/commit/147e16b11de922dfe9ee99ff6419a75f41c7db0b))

## [0.2.4](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.2.3...X4DataTestConsole@v0.2.4) (2025-02-08)


### Code Refactoring

* **X4DataTestConsole:** improve code formatting and structure in Main method ([4851ddc](https://github.com/chemodun/X4-UniverseEditor/commit/4851ddca142deb1eeafc9cccfb971a44295f8951))

## [0.2.3](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.2.2...X4DataTestConsole@v0.2.3) (2025-02-05)


### Bug Fixes

* **X4DataTest:** replace Log.Info with Console.WriteLine for improved output ([88b88f2](https://github.com/chemodun/X4-UniverseEditor/commit/88b88f223bcec507ecef95244909f6f9c1a393d6))

## [0.2.2](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.2.1...X4DataTestConsole@v0.2.2) (2025-02-05)


### Bug Fixes

* **X4DataTest:** remove unused namespaces for cleaner code ([941a088](https://github.com/chemodun/X4-UniverseEditor/commit/941a0886320568cb43b5e92622e9491d28323264))

## [0.2.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.2.0...X4DataTestConsole@v0.2.1) (2025-02-05)


### Bug Fixes

* **X4DataTest:** enhance sector output and remove owner station calculations ([6b6f494](https://github.com/chemodun/X4-UniverseEditor/commit/6b6f49431dc948ad5cd875b94b83e47f208ce15c))

## [0.2.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.1.1...X4DataTestConsole@v0.2.0) (2025-02-04)


### Features

* **X4DataTest:** Add station ownership analysis to calculate and display dominant owner in sector ([b0d3b40](https://github.com/chemodun/X4-UniverseEditor/commit/b0d3b4003e6511b149995d3cf708a782cc735f9e))


### Code Refactoring

* **Logging:** Update logging configuration and replace logger instances with centralized logging utility ([0f596a0](https://github.com/chemodun/X4-UniverseEditor/commit/0f596a0b706b1704042cbedb7c29a18ec2276a9d))
* **X4DataTest:** Add detailed logging for Highway Points in test output ([8c16bdf](https://github.com/chemodun/X4-UniverseEditor/commit/8c16bdf12f4cb5cb4049a3bb3daec7654e772669))
* **X4DataTest:** Add PrepareClusterMap method to generate cluster position CSV ([bf67642](https://github.com/chemodun/X4-UniverseEditor/commit/bf676420286a8d8930e014682bd68ec5a7a4beca))
* **X4DataTestConsole:** Integrate Logging for enhanced logging functionality ([5ccf2ef](https://github.com/chemodun/X4-UniverseEditor/commit/5ccf2efcf77d55d9a13365bb67159375bc082b70))
* **X4DataTest:** Include ConnectedSector information in Highway Point output for better context ([5418174](https://github.com/chemodun/X4-UniverseEditor/commit/54181745885b4c7b8e9eba57c072e633e7694cf7))
* **X4DataTest:** Remove PrepareClusterMap method to streamline codebase ([3868be0](https://github.com/chemodun/X4-UniverseEditor/commit/3868be0f11e1e9840d605f50202d7f0489d70e73))
* **X4DataTest:** Update owner references to use OwnerId for consistency in station data output ([65807ea](https://github.com/chemodun/X4-UniverseEditor/commit/65807eacb31764988f122d410ed4d6cf4f74adcf))

## [0.1.1](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.1.0...X4DataTestConsole@v0.1.1) (2025-01-19)


### Code Refactoring

* simplify data loading by using X4Galaxy.LoadData method ([1703a03](https://github.com/chemodun/X4-UniverseEditor/commit/1703a030e0d96cc23cf3bf4fbf4062fabaf6f4e9))

## [0.1.0](https://github.com/chemodun/X4-UniverseEditor/compare/X4DataTestConsole@v0.0.1...X4DataTestConsole@v0.1.0) (2025-01-18)


### Features

* enhance output logging to include source and filename for clusters, sectors, zones, and connections ([c37e32d](https://github.com/chemodun/X4-UniverseEditor/commit/c37e32d53a5a9f6fdf2f6dc37f1b7847ef2920ac))
