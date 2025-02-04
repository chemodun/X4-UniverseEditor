# Changelog

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
