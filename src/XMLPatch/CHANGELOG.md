# Changelog

## [0.1.9](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.8...XMLPatch@v0.1.9) (2025-11-25)


### Code Refactoring

* **XMLPatch:** improve XPath element selection methods ([086f917](https://github.com/chemodun/X4-UniverseEditor/commit/086f9171a6a0617b5884e29657fb675069b128da))

## [0.1.8](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.7...XMLPatch@v0.1.8) (2025-03-04)


### Code Refactoring

* **XMLPatch:** enhance element logging format and ensure correct element insertion flow ([26b6e8f](https://github.com/chemodun/X4-UniverseEditor/commit/26b6e8fb4e838ef3d8c64573d76beae58644226b))
* **XMLPatch:** improve attribute comparison logic for cloned elements to prevent doubling ([02819d3](https://github.com/chemodun/X4-UniverseEditor/commit/02819d3c3cd2ef4ace53e3a18a8c988e14bf0317))
* **XMLPatch:** streamline element insertion logic and improve handling of duplicate elements and right order on multiple items ([8371e82](https://github.com/chemodun/X4-UniverseEditor/commit/8371e82f6efed1f929b033b042daaf1a5d869844))

## [0.1.7](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.6...XMLPatch@v0.1.7) (2025-02-26)


### Code Refactoring

* **XMLPatch:** standardize parameter naming in ApplyPatch method ([4298871](https://github.com/chemodun/X4-UniverseEditor/commit/42988710cf90d4de9c101d385e213de9a2cb0fef))

## [0.1.6](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.5...XMLPatch@v0.1.6) (2025-02-25)


### Bug Fixes

* **XMLPatch:** ensure _source attribute is added or updated correctly ([1e24cf7](https://github.com/chemodun/X4-UniverseEditor/commit/1e24cf7af4c58144d9915e15aa12f524afa0fc3f))

## [0.1.5](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.4...XMLPatch@v0.1.5) (2025-02-25)


### Bug Fixes

* **XMLPatch:** diff will be always applied, despite errors during the processing ([24f474b](https://github.com/chemodun/X4-UniverseEditor/commit/24f474bf609217b50677519cde9292ac7649ccc9))
* **XMLPatch:** enhance logging for node selection failures and add LastApplicableNode method ([d21d971](https://github.com/chemodun/X4-UniverseEditor/commit/d21d9711b5444d62bfbf1e1366affdba980cae47))
* **XMLPatch:** prevent duplicate elements from being added in various positions ([9439182](https://github.com/chemodun/X4-UniverseEditor/commit/9439182a8358717028063c3da2a8f9736de2c16f))

## [0.1.4](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.3...XMLPatch@v0.1.4) (2025-02-24)


### Bug Fixes

* **XMLPatch:** improve element replacement logic and logging ([779a906](https://github.com/chemodun/X4-UniverseEditor/commit/779a906f4e4e93c4e67b24e3338a142256083ba2))

## [0.1.3](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.2...XMLPatch@v0.1.3) (2025-02-24)


### Code Refactoring

* **XMLPatch:** enhance logging details and add utility methods for element and attribute info ([36558cd](https://github.com/chemodun/X4-UniverseEditor/commit/36558cd302f6314c6bc5866ec37fef25f6aea4b3))

## [0.1.2](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.1...XMLPatch@v0.1.2) (2025-02-14)


### Code Refactoring

* **XMLPatch:** change ApplyPatch method to return patched XElement instead of bool ([7894bdb](https://github.com/chemodun/X4-UniverseEditor/commit/7894bdb31b8a8ec19cf3ca2990f9e1c3018d9d2a))

## [0.1.1](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch@v0.1.0...XMLPatch@v0.1.1) (2025-02-13)


### Code Refactoring

* **XMLPatch:** enhance XML patching methods to return success status ([6449d2e](https://github.com/chemodun/X4-UniverseEditor/commit/6449d2e8b4d4b96d769a322cb348d2520a060144))

## [0.1.0](https://github.com/chemodun/X4-UniverseEditor/compare/XMLPatch-v0.1.0...XMLPatch@v0.1.0) (2025-02-13)


### Features

* add project reference to Logger in XMLPatch project ([37ce753](https://github.com/chemodun/X4-UniverseEditor/commit/37ce75360b3717815e5386495f32a3b07d5c5674))
* implement XMLPatch functionality with add, replace, and remove operations ([abdda2f](https://github.com/chemodun/X4-UniverseEditor/commit/abdda2f71d301f6b6796ee96633dec4da207aa63))
