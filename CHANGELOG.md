# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-10-04

### Added
- **Rebrand to ThemeExtrasNG**:
  - Maintained, modernized, and fixed fork of `felixkmh/ThemeExtras-for-Playnite` by gOOvER.
  - Dual registration support for both `ThemeExtras` and `ThemeExtrasNG` source names across custom elements, settings, and value converters to guarantee complete drop-in compatibility for existing themes.
  - Automatic settings migration (`config.json`) and fallback asset loading (banners, custom link icons) from legacy `felixkmh_Extras_Plugin` directory.
- **Automated CI/CD Workflows**:
  - GitHub Actions for automated build and packaging (`.github/workflows/build.yml`).
  - Automated GitHub release pipeline with changelog extraction and SHA256 checksums (`.github/workflows/release.yml`).
- **One-Click Installation Scripts**:
  - Added `Install.cmd` and `install.ps1` for rapid 1-click building, packaging, and installing directly into Playnite.

### Fixed
- **Thread Affinity & Collection Concurrency Crashes in `Links.xaml.cs` (Issues #14, #15, #16)**:
  - Fixed cross-thread access violation (`InvalidOperationException: The calling thread cannot access this object because a different thread owns it`) when `Game_PropertyChanged` or `Links_CollectionChanged` fires on background threads.
  - Safely dispatched all `ObservableCollection` modifications (`links.Clear()`, `links.Add()`) to the UI Dispatcher.
  - Replaced unsafe `GameContext` property lookups from background threads with `sender as Game`.
  - Added try-catch guards around `async void` event handlers to prevent unhandled background exceptions from crashing Playnite.
- **NullReferenceException and Division by Zero in `BannerData.xaml.cs`**:
  - Prevented crash when `bannerCache.GetBanner(game)` returns `null` before reading `.Height` or `.Width`.
  - Added `Width > 0` validation before calculating `Ratio` to prevent `NaN` / division by zero.
  - Added missing `nameof(Game.SourceId)` change detection so banners properly update when the game source is modified.
  - Ensured `Ratio` is updated whenever `BannerSource` changes.
- **NullReferenceException & Missing Source Change in `Banner.xaml.cs`**:
  - Added missing `nameof(Game.SourceId)` change handling to trigger banner updates when a game's source changes.
  - Resolved `sender as Game ?? Tag as Game ?? GameContext` consistently across UI and background thread dispatches.
- **Division by Zero & NRE in `UserRating.xaml.cs`**:
  - Prevented `NaN` calculation when `progressBar.ActualWidth <= 0`.
  - Added null check on `GameContext` before calling `Playnite.SDK.API.Instance.Database.Games.Update(GameContext)`.
- **InvalidOperationException in `ThemeExtras.cs`**:
  - Removed dangling `Game selectedGame = args.Games.First();` in `GetGameMenuItems` which threw an unhandled exception when invoked with an empty game selection.
  - Batched database updates via `Playnite.SDK.API.Instance.Database.Games.Update(games)` instead of invoking `Update(game)` repeatedly in a loop for each item.
- **Unassigned Property in `GamePropertyViewModel.cs`**:
  - Fixed bug where `typeof(Game).GetProperty(propertyName)` was evaluated without assigning the result to the local `property` variable, causing `NullReferenceException` when custom `getValue`/`setValue` were omitted.
  - Guarded `Playnite.SDK.API.Instance.MainView.SelectedGames.ToList()` with a null-safe fallback when no games are selected.
- **NullReferenceException in `EditableTags.xaml.cs`**:
  - Added null-coalescing fallback `(GameContext.TagIds ?? Enumerable.Empty<Guid>())` to prevent crashing when adding or removing tags on games that have no initial tags.
- **File Stream Leak in `LinkExt.cs`**:
  - Wrapped `File.Create(iconPath)` in a `using` block in both synchronous `GetIcon` and asynchronous `GetIconAsync` methods to eliminate file descriptor leaks and file lock errors.
- **Concurrent Dictionary Access in `LinkExt.cs`**:
  - Synchronized static `iconCache` and `fileIconCache` access to prevent corruption and 100% CPU lockups under concurrent reads/writes.
- **Disappearing Icons for Client Links (Issue #18)**:
  - Added `NormalizeDomain` to handle client application URI protocols (e.g. `steam://`, `goggalaxy://`, `origin://`, `ea://`, `battlenet://`) so that link icons like Steam load reliably even when converted to client launch URLs.
- **EA App & Domain Support (Issue #8)**:
  - Added `ea.com` and EA client protocol matching to resolve missing icons for EA App games.
- **Memory Leaks in UI Controls**:
  - Added proper event unhooking on `Unloaded` in `SmoothedValue.cs` and `Links.xaml.cs`.

### Security
- **Insecure HTTP Favicon Downloads**:
  - Changed Google Favicon service URL from insecure plain HTTP (`http://www.google.com/s2/favicons`) to HTTPS (`https://www.google.com/s2/favicons`) to prevent eavesdropping and MITM tampering.
- **Unsanitized Process Launch**:
  - Sanitized `LinkExt.OpenLinkCommand` to validate URL schemes (`http`, `https`) and configured `UseShellExecute = true` to protect against command execution vectors.

### Changed
- **Modern SDK Project Format**:
  - Migrated `Extras.csproj` and `PlayniteCommon.csproj` to modern SDK-style format (`Microsoft.NET.Sdk` with `<UseWpf>true</UseWpf>`), enabling clean command-line builds with `dotnet build`.
  - Replaced brittle compile-time COM TLB import for `Shell32` with dynamic shell shortcut handling (`WScript.Shell`).
- **Dependency Modernization**:
  - Updated `PlayniteSDK` reference from `6.4.0` to `6.18.0` with `<ExcludeAssets>runtime</ExcludeAssets>`.
- **Registered Missing Converters**:
  - Registered `IntToRatingBrushConverter` in `AddConvertersSupport` and ensured it can return both a frozen `SolidColorBrush` and `Color`.

---

## [Legacy Versions]

> All releases below are legacy versions originally published under `felixkmh/ThemeExtras-for-Playnite`.

### [1.4.4] - 2023-09-14
- Removed Test Notification.

### [1.4.3] - 2023-09-09
- Fixed crash on launch.

### [1.4.2] - 2023-01-20
- Completion status options are sorted alphabetically.
- Prefer library banner if platforms contain PC (Windows).
- Fixed error when trying to open non-existent backup folder.
- Fixed order of evaluation of banner providers.

### [1.4.1] - 2023-01-13
- Fixed exception when certain properties are updated from a background thread.

### [1.4.0] - 2023-01-03
- Added main menu functions to set icons of default Playnite shortcuts to the icon of the theme.
- Added option to automatically update icons of default Playnite shortcuts to the theme icon if the theme was changed (disabled by default).
- Added option for users to provide theme independent banners (directory can be opened via add-on settings).

### [1.3.1] - 2022-12-22
- Fixed `BannerData` control not returning the correct image source.

### [1.3.0] - 2022-12-22
- Added additional converters.
- Added `BannerData` control.

### [1.2.0] - 2022-11-11
- Banner source game can be overridden by setting the Tag of the containing `ContentControl` to a `Game` object.

### [1.1.1] - 2022-11-04
- Minor fixes.

### [1.1.0] - 2022-09-27
- Added support for smooth progress bars.
- Tweaked navigation.
- Limit number of concurrent icon fetches.

### [1.0.1] - 2022-09-18
- Fixed `CanExecute` on `BackCommand` and `ForwardsCommand` not updating correctly.

### [1.0.0] - 2022-09-18
- Playnite SDK 6.3.0 update (Playnite 10 only).
- Added `UrlToAsyncIconConverter`.
- Added common `PluginConverters`.
- Added view switching commands.
- Added delay when adding navigation events.

### [0.2.1] - 2022-09-17
- Added navigation using browse back and forwards shortcuts or custom UI elements.
- Fixed platform banners by platform name not being found.

### [0.2.0] - 2022-08-21
- Added `DiscardNotificationCommand` to Commands.

### [0.1.3] - 2022-08-20
- Link icons are now cached on disk.

### [0.1.2] - 2022-08-16
- Fixed error when switching games.

### [0.1.1] - 2022-08-14
- Fixed crash when adding/removing links.

### [0.1.0] - 2022-08-14
- Initial release.
