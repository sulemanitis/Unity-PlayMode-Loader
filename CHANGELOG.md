# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-07-07

### Fixed
- Startup scene selection no longer resets after an editor restart. Settings were previously stored using a `ScriptableSingleton`, which could silently fail to resolve the saved asset after the package moved into its own assembly, resulting in a blank selection on every restart. Settings are now persisted through a plain JSON file at `ProjectSettings/PlayModeLoaderSettings.json`, read and written directly, with no dependency on Unity's internal type resolution.

### Changed
- Startup scene is now referenced by its GUID instead of a Build Settings index. Reordering, renaming, or moving the scene no longer breaks the reference (previously, reordering the Scenes In Build list could silently point the loader at the wrong scene).
- Repo restructured into a proper Unity Package Manager package with a `package.json` at the root, so it can be added directly via git URL without a `?path=` query parameter.
- Added an `.asmdef` for the editor scripts so the package compiles into its own assembly instead of the implicit `Assembly-CSharp-Editor`.

### Added
- `.meta` files for every asset in the package, required for git packages since Unity treats `Packages/` as read-only and won't auto-generate missing metas the way it does for `Assets/`.

### Migration notes
- If upgrading from `1.0.0`, delete the old `ProjectSettings/PlayModeLoaderSettings.asset` file. It's no longer used and its presence will cause a `missing the class attribute 'ExtensionOfNativeClass'` error on load.
- You'll need to reselect your startup scene once in Project Settings > Play Mode Loader after upgrading, since the old and new settings formats aren't compatible with each other.

## [1.0.0] - 2026-07-07

### Added
- Initial release.
- Automatically loads a designated startup scene when entering Play Mode.
- Restores previously opened scenes when exiting Play Mode.
- Optional "Save Before Play".
- Project Settings UI for configuring the loader.
