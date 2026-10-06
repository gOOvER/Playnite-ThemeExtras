# ThemeExtrasNG for Playnite

[![CI Build](https://github.com/gOOvER/Playnite-ThemeExtras/actions/workflows/build.yml/badge.svg)](https://github.com/gOOvER/Playnite-ThemeExtras/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/v/release/gOOvER/Playnite-ThemeExtras?include_prereleases&style=flat-square)](https://github.com/gOOvER/Playnite-ThemeExtras/releases)
[![Playnite 10+](https://img.shields.io/badge/Playnite-10%2B-blue?style=flat-square)](https://playnite.link)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)
[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support-orange?style=flat-square&logo=kofi)](https://ko-fi.com/goover)

**ThemeExtrasNG** (*Next Generation*) is an enhanced, highly stable, and modern fork of the popular [ThemeExtras](https://github.com/felixkmh/ThemeExtras-for-Playnite) extension for [Playnite](https://playnite.link).

It provides themes with powerful custom controls, store/platform banners, interactive star ratings, link favicons, direct two-way property editing, math/visual converters, and context menus.

---

## 🌟 Why ThemeExtrasNG?

While the original ThemeExtras laid the groundwork for modern Playnite themes, several unresolved threading issues, crashes, and outdated dependencies led to unexpected crashes or missing assets in newer Playnite installations.

**Key improvements in ThemeExtrasNG 1.0.0:**
* 🛡️ **Thread-Affinity & Collection Crash Fixes (Issues #14, #15, #16)**: Dispatches all link collection updates, ratings, and banner property changes directly to the UI Dispatcher with error guards, preventing background thread crashes.
* 🌐 **Protocol & App Link Normalization (Issues #8, #18)**: Native client URI schemes (e.g. `steam://`, `goggalaxy://`, `origin://`, `ea://`, `battlenet://`) now resolve correct icons automatically. Added full support for EA App (`ea.com`).
* ➗ **Division-by-Zero & NaN Guards**: Fixed crashes in `BannerData` and `UserRating` when dimensions are 0 or unmeasured.
* 🔄 **100% Backwards Compatible**: Supports both `SourceName="ThemeExtras"` and `SourceName="ThemeExtrasNG"` for all custom elements, converters, and settings markup extensions. Existing themes work without modification!
* 📦 **Automatic Migration**: Automatically migrates settings (`config.json`), custom link icons, and banners from legacy `felixkmh_Extras_Plugin` folders.
* 🔒 **Security Upgrades**: Replaced plain HTTP favicon scraping with HTTPS and sanitized external link execution.
* ⚙️ **Modern SDK-Style Project**: Built with .NET SDK project files and updated to PlayniteSDK 6.18.0.

---

## 📋 Table of Contents

- [Custom UI Elements](#-custom-ui-elements)
  - [Star Ratings](#star-ratings)
  - [Platform & Store Banners](#platform--store-banners)
  - [Links with Favicons](#links-with-favicons)
  - [Settable Game Properties](#settable-game-properties)
  - [Drop-in Completion Status ComboBox](#drop-in-completion-status-combobox)
  - [Editable Tags](#editable-tags)
- [Plugin Value Converters](#-plugin-value-converters)
- [Markup Extensions & Commands (`PluginSettings`)](#-markup-extensions--commands-pluginsettings)
  - [Game Properties](#game-properties)
  - [Commands](#commands)
  - [Custom Context Menus](#custom-context-menus)
- [Theme Manifest Configuration (`themeExtras.yaml`)](#-theme-manifest-configuration-themeextrasyaml)
- [Custom Banners & Link Icons](#-custom-banners--link-icons)
- [Installation](#-installation)
- [Building from Source](#-building-from-source)
- [Credits & License](#-credits--license)

---

## 🧩 Custom UI Elements

All custom elements are implemented as Playnite `PluginControl` components.

> [!TIP]
> You can check whether the plugin is installed using Playnite's `PluginStatus` markup extension:
> ```xml
> Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}"
> ```
> *(Or use `Plugin=felixkmh_Extras_Plugin` for backward compatibility).*

### Star Ratings

Interactive and display star ratings for User Score, Critic Score, and Community Score.

* `ThemeExtras_UserRating` (Interactive: hover and click to set rating)
* `ThemeExtras_CriticRating` (Display only)
* `ThemeExtras_CommunityRating` (Display only)

#### Required Resources
Define the fill and empty star brushes in your control resources or theme dictionary:

```xml
<StackPanel Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}">
    <StackPanel.Resources>
        <SolidColorBrush x:Key="Extras_FilledStarBrush" Color="#FFD700"/>
        <SolidColorBrush x:Key="Extras_EmptyStarBrush" Color="#FFFFFF" Opacity="0.25"/>
    </StackPanel.Resources>

    <!-- User Rating (Interactive) -->
    <ContentControl x:Name="ThemeExtras_UserRating" 
                    Visibility="{Binding UserScoreVisibility}" 
                    Height="24"/>

    <!-- Critic Score -->
    <ContentControl x:Name="ThemeExtras_CriticRating" 
                    Visibility="{Binding CriticScoreVisibility}" 
                    Height="20"/>

    <!-- Community Score -->
    <ContentControl x:Name="ThemeExtras_CommunityRating" 
                    Visibility="{Binding CommunityScoreVisibility}" 
                    Height="20"/>
</StackPanel>
```

---

### Platform & Store Banners

Displays platform or store banners based on a resolution hierarchy (Platform Specification ID -> Platform Name -> Plugin ID -> Source Name -> Default Banner).

* `ThemeExtras_Banner`: Displays the resolved banner image.
* `ThemeExtras_BannerData`: Exposes banner metadata (e.g. aspect ratio, dimensions) for custom layouts.

#### Banner Example:
```xml
<ContentControl x:Name="ThemeExtras_Banner"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}"
                RenderOptions.BitmapScalingMode="Fant"
                Height="40"
                HorizontalAlignment="Left"/>
```

#### BannerData Example (Aspect Ratio Handling):
```xml
<ContentControl x:Name="ThemeExtras_BannerData" x:Key="BannerData"/>
<!-- Ratio property can be bound via: Binding Source={StaticResource BannerData}, Path=Content.Ratio -->
```

---

### Links with Favicons

`ThemeExtras_Links` provides an `ItemsControl` displaying game web links, complete with cached website favicons and client brand icons (Steam, GOG, Epic Games, EA, Ubisoft, Discord, YouTube, etc.).

Each item binds to a `LinkExt` object extending `Playnite.SDK.Models.Link` with:
- `Icon`: Resolved favicon (`Image` or font glyph `TextBlock`).
- `OpenLinkCommand`: Command to launch the link safely in the default browser.

#### Example:
```xml
<ContentControl x:Name="ThemeExtras_Links"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}">
    <ContentControl.Resources>
        <Style TargetType="ItemsControl">
            <Setter Property="ItemTemplate">
                <Setter.Value>
                    <DataTemplate>
                        <DockPanel Margin="4">
                            <Viewbox DockPanel.Dock="Left" Width="18" Height="18" Margin="0,0,6,0">
                                <ContentControl Content="{Binding Icon}">
                                    <ContentControl.TargetNullValue>
                                        <TextBlock Text="&#xEF71;" FontFamily="{StaticResource FontIcoFont}"/>
                                    </ContentControl.TargetNullValue>
                                </ContentControl>
                            </Viewbox>
                            <TextBlock VerticalAlignment="Center" TextTrimming="CharacterEllipsis">
                                <Hyperlink Command="{Binding OpenLinkCommand}" ToolTip="{Binding Url}">
                                    <Run Text="{Binding Name}"/>
                                </Hyperlink>
                            </TextBlock>
                        </DockPanel>
                    </DataTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </ContentControl.Resources>
</ContentControl>
```

---

### Settable Game Properties

Settable controls provide an empty container whose `DataContext` exposes a `Value` property with `TwoWay` binding support. Changes made to `Value` immediately update and persist to Playnite's database for the selected game(s).

#### 1. Favorite Toggle (`ThemeExtras_SettableFavorite`)
```xml
<ContentControl x:Name="ThemeExtras_SettableFavorite"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}">
    <ContentControl.Resources>
        <Style TargetType="UserControl">
            <Setter Property="Content">
                <Setter.Value>
                    <CheckBox IsChecked="{Binding Value}">
                        <CheckBox.Template>
                            <ControlTemplate TargetType="CheckBox">
                                <TextBlock Text="&#xF000;" 
                                           FontFamily="{StaticResource FontIcoFont}" 
                                           Opacity="{TemplateBinding IsChecked, Converter={StaticResource OpacityBoolConverter}}"/>
                            </ControlTemplate>
                        </CheckBox.Template>
                    </CheckBox>
                </Setter.Value>
            </Setter>
        </Style>
    </ContentControl.Resources>
</ContentControl>
```

#### 2. Hidden Toggle (`ThemeExtras_SettableHidden`)
```xml
<ContentControl x:Name="ThemeExtras_SettableHidden"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}">
    <ContentControl.Resources>
        <Style TargetType="UserControl">
            <Setter Property="Content">
                <Setter.Value>
                    <CheckBox IsChecked="{Binding Value}">
                        <CheckBox.Template>
                            <ControlTemplate TargetType="CheckBox">
                                <Grid>
                                    <TextBlock Text="&#xEF24;" FontFamily="{StaticResource FontIcoFont}" 
                                               Visibility="{TemplateBinding IsChecked, Converter={StaticResource InvertedBooleanToVisibilityConverter}}"/>
                                    <TextBlock Text="&#xEF22;" FontFamily="{StaticResource FontIcoFont}" 
                                               Visibility="{TemplateBinding IsChecked, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                                </Grid>
                            </ControlTemplate>
                        </CheckBox.Template>
                    </CheckBox>
                </Setter.Value>
            </Setter>
        </Style>
    </ContentControl.Resources>
</ContentControl>
```

#### 3. Settable User Score (`ThemeExtras_SettableUserScore`)
```xml
<ContentControl x:Name="ThemeExtras_SettableUserScore"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}">
    <ContentControl.Resources>
        <Style TargetType="{x:Type UserControl}">
            <Setter Property="Content">
                <Setter.Value>
                    <TextBox Width="40" 
                             TextAlignment="Center" 
                             Text="{Binding Value, UpdateSourceTrigger=LostFocus}"/>
                </Setter.Value>
            </Setter>
        </Style>
    </ContentControl.Resources>
</ContentControl>
```

#### 4. Settable Completion Status (`ThemeExtras_SettableCompletionStatus`)
The `DataContext` also provides `CompletionStatusOptions` (`ObservableCollection<CompletionStatus>`).
```xml
<ContentControl x:Name="ThemeExtras_SettableCompletionStatus"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}">
    <ContentControl.Resources>
        <Style TargetType="UserControl">
            <Setter Property="Content">
                <Setter.Value>
                    <ComboBox ItemsSource="{Binding CompletionStatusOptions}" 
                              SelectedItem="{Binding Value}">
                        <ComboBox.ItemTemplate>
                            <DataTemplate DataType="{x:Type CompletionStatus}">
                                <TextBlock Text="{Binding Name}"/>
                            </DataTemplate>
                        </ComboBox.ItemTemplate>
                    </ComboBox>
                </Setter.Value>
            </Setter>
        </Style>
    </ContentControl.Resources>
</ContentControl>
```

---

### Drop-in Completion Status ComboBox

A pre-styled, animated `ComboBox` with built-in visual states for fast theme drop-in integration.

```xml
<ContentControl x:Name="ThemeExtras_CompletionStatusComboBox"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}"/>
```

---

### Editable Tags

Allows viewing and assigning/removing tags directly on the selected game view.

```xml
<ContentControl x:Name="ThemeExtras_EditableTags"
                Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Status=Installed}"/>
```

---

## 🧮 Plugin Value Converters

ThemeExtrasNG registers value converters with Playnite. You can invoke them using the `PluginConverter` markup extension with either `Plugin=ThemeExtrasNG` or `Plugin=ThemeExtras`:

```xaml
<TextBlock Text="{Binding Height, Converter={PluginConverter Plugin=ThemeExtrasNG, Converter=MultiplyConverter}, ConverterParameter=1.5}" />
```

| Converter | Input Type | Converter Parameter | Output Type | Description |
| :--- | :--- | :--- | :--- | :--- |
| `UrlToAsyncIconConverter` | `string` / `Uri` | *(None)* | `AsyncValue<object>` | Resolves a URL to a Favicon or brand font glyph asynchronously with disk caching. |
| `MultiplyConverter` | Number | Multiplier (Number) | `double` | Multiplies input: `Value * Parameter`. |
| `DivideConverter` | Number | Divisor (Number) | `double` | Divides input: `Value / Parameter`. |
| `PowConverter` | Number (Base) | Exponent (Number) | `double` | Calculates `Base ^ Exponent`. |
| `MultiplicativeInverseConverter` | Number (`!= 0`) | *(None)* | `double` | Calculates `1 / Value`. |
| `DoubleToSmoothedValueConverter` | `double` | *(None)* | `SmoothedValue` | Creates an interpolated animated value container. |
| `DoubleToCornerRadiusConverter` | `double` | `string` (sides) | `CornerRadius` | Converts a double to uniform or selective `CornerRadius`. |
| `IntToRatingBrushConverter` | `int` (Score) | *(None)* | `SolidColorBrush` | Maps numeric score (0-100) to rating gradient color (Red -> Yellow -> Green). |

---

## 🛠️ Markup Extensions & Commands (`PluginSettings`)

### Game Properties
Direct two-way binding to properties of the currently selected game(s):

```xml
<!-- Favorite Toggle -->
<CheckBox IsChecked="{PluginSettings Plugin=ThemeExtrasNG, Path=Game.Favorite, Mode=TwoWay}" />

<!-- Hidden Toggle -->
<CheckBox IsChecked="{PluginSettings Plugin=ThemeExtrasNG, Path=Game.Hidden, Mode=TwoWay}" />

<!-- Game Notes -->
<TextBox Text="{PluginSettings Plugin=ThemeExtrasNG, Path=Game.Notes, Mode=TwoWay}" />
```

### Commands

Bind commands using `PluginSettings`:
```xml
<Button Command="{PluginSettings Plugin=ThemeExtrasNG, Path=Commands.OpenGameAssetFolderCommand}" 
        Content="Open Asset Folder" />
```

| Command | Parameter | Description |
| :--- | :--- | :--- |
| `UpdateGamesCommand` | *(None)* | Updates metadata for selected games. |
| `ResetScoreCommand` | `"User"` \| `"Community"` \| `"Critic"` | Resets score for selected games. |
| `OpenGameAssetFolderCommand` | `Game` *(Optional)* | Opens the media asset directory for the specified or selected game. |
| `OpenPlayniteSettings` | *(None)* | Opens Playnite settings dialog. |
| `OpenAddonWindowCommand` | *(None)* | Opens Playnite Add-on browser. |
| `SwitchModeCommand` | *(None)* | Switches between Desktop and Fullscreen modes. |
| `OpenPluginSettingsCommand` | `Guid` (Plugin ID) | Opens settings window for the given plugin. |
| `OpenPluginConfigDirCommand` | `Guid` (Plugin ID) | Opens configuration directory for the given plugin. |
| `OpenPlayniteLogCommand` | *(None)* | Opens `playnite.log` in default text editor. |
| `OpenExtensionsLogCommand` | *(None)* | Opens `extensions.log` in default text editor. |
| `OpenUrlCommand` | `string` / `Uri` | Opens URI in the default system browser. |
| `BackCommand` / `ForwardCommand` | *(None)* | Navigates back / forward across game view selection history. |
| `SwitchToDetailsViewCommand` | `Game` *(Optional)* | Switches Playnite to Details View. |
| `SwitchToGridViewCommand` | `Game` *(Optional)* | Switches Playnite to Grid View. |
| `SwitchToListViewCommand` | `Game` *(Optional)* | Switches Playnite to List View. |

### Custom Context Menus

Quickly expose `ExtraMetadataLoader` and `BackgroundChanger` operations in a background image context menu without lag:

```xml
<ContextMenu Visibility="{PluginStatus Plugin=goover_ThemeExtrasNG_Plugin, Path=IsInstalled}"
             IsOpen="{PluginSettings Plugin=ThemeExtrasNG, Path=Menus.IsOpen, Mode=OneWayToSource}">
    <MenuItem Header="ExtraMetadataLoader"
              Visibility="{PluginStatus Plugin=ExtraMetadataLoader_705fdbca-e1fc-4004-b839-1d040b8b4429, Path=IsInstalled}"
              ItemsSource="{PluginSettings Plugin=ThemeExtrasNG, Path=Menus.EMLGameMenuItems}"/>
    <MenuItem Header="BackgroundChanger"
              Visibility="{PluginStatus Plugin=playnite-backgroundchanger-plugin, Path=IsInstalled}"
              ItemsSource="{PluginSettings Plugin=ThemeExtrasNG, Path=Menus.BackgroundChangerGameMenuItems}"/>
    <Separator/>
    <MenuItem Header="Open Asset Folder"
              Command="{PluginSettings Plugin=ThemeExtrasNG, Path=Commands.OpenGameAssetFolderCommand}"/>
</ContextMenu>
```

---

## 📄 Theme Manifest Configuration (`themeExtras.yaml`)

Themes can bundle an optional `themeExtras.yaml` file in their root folder (next to `theme.yaml`) to define plugin recommendations, banner assets, and custom website icons:

```yaml
ThemeId: MyCustomTheme_Desktop
Recommendations:
  - AddonName: DuplicateHiderNG
    AddonId: goover_DuplicateHiderNG_Plugin
    Features:
      - Floating Source Selector
      - Details View Integration
PersistentPaths:
  - Images/Banners
  - Images/Icons
BannersBySpecIdPath: Images/Banners/PlatformSpecId
BannersByPlatformNamePath: Images/Banners/PlatformName
BannersByPluginIdPath: Images/Banners/PluginId
BannersBySourceNamePath: Images/Banners/SourceName
DefaultBannerPath: Images/Banners/DefaultBanner.png
BannerDecodeHeight: 75
WebsiteIconsPath: Images/WebsiteIcons
```

---

## 📁 Custom Banners & Link Icons

Users can supply their own custom banners and icons by placing images directly into the ThemeExtrasNG user data directory:
`%APPDATA%\Playnite\ExtensionsData\goover_ThemeExtrasNG_Plugin\`

### Directory Structure:
```text
%APPDATA%\Playnite\ExtensionsData\goover_ThemeExtrasNG_Plugin\
├── Banners\
│   ├── Default.png
│   ├── ByPlatformSpecId\     <-- {SpecificationId}.png (e.g. pc_windows.png, sony_playstation5.png)
│   ├── ByPlatformName\       <-- {PlatformName}.png (e.g. PC (Windows).png)
│   ├── BySourceName\         <-- {SourceName}.png (e.g. Steam.png, GOG.png)
│   └── ByPluginId\           <-- {PluginGuid}.png
└── LinkIcons\                <-- {domain.tld}.png / .ico (e.g. nexusmods.com.png)
```

> [!NOTE]
> If you are upgrading from legacy ThemeExtras, ThemeExtrasNG automatically looks inside `%APPDATA%\Playnite\ExtensionsData\felixkmh_Extras_Plugin\` as a seamless fallback.

---

## 📥 Installation

### Method 1: Playnite Extension Package (`.pext`)
1. Download the latest `ThemeExtrasNG_1.0.0.pext` from [GitHub Releases](https://github.com/gOOvER/Playnite-ThemeExtras/releases).
2. Double-click the `.pext` file or drag and drop it into Playnite.
3. Restart Playnite when prompted.

### Method 2: 1-Click Local Installer
If you clone this repository locally:
1. Run `Install.cmd` (or `install.ps1`).
2. The script will automatically compile the project, clean up any conflicting legacy plugins, deploy `ThemeExtrasNG` to your Playnite extensions folder, and relaunch Playnite.

---

## 🔨 Building from Source

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download) (or .NET Framework 4.6.2 Developer Pack)
* Windows 10/11 with PowerShell

### Build Command:
```powershell
dotnet build source/Extras.sln -c Release
```

The compiled binaries will be output to `source/bin/Release/net462/`.

---

## 👏 Credits & License

* **ThemeExtrasNG Maintainer**: [gOOvER](https://github.com/gOOvER)
* **Original ThemeExtras Author**: [felixkmh](https://github.com/felixkmh)
* **CompletionStatusComboBox**: Contributed by [Jhanlon95](https://github.com/Jhanlon95)
* **Icons**: [IcoFont](https://icofont.com/) & [Font Awesome](https://fontawesome.com/)

Licensed under the [MIT License](LICENSE).
