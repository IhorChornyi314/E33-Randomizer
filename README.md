# E33-Randomizer

A randomizer mod for Clair Obscur: Expedition 33 that gives users complete control over every enemy placement in the game, as well as a lot of different tools to customize them to their heart's desire.

## Installation

- Download the latest release
- If you don't have it, install the .Net 10 Runtime:
  - [Windows](https://builds.dotnet.microsoft.com/dotnet/Runtime/10.0.8/dotnet-runtime-10.0.8-win-x64.exe)
  - Linux installation methods vary across distributions, see the [Linux Package Manager](https://learn.microsoft.com/en-us/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website) article for many of them.  For Arch Linux, see [.Net docs](https://wiki.archlinux.org/title/.NET)

- Unpack the `zip` file in a non-admin folder.
  - For Linux this is a `tar.gz`
- Run `E33Randomizer.exe`
  - For Linux this it's just `E33Randomizer`

## Running the randomizer

- Start the E33Randomizer.exe
- Configure the mod as you see fit
- Click "Generate and pack mod" button in the main window or "Generate mod files from current"
- Put the generated `.pak`, `.utoc`, and `.ucas` files into `Expedition 33\Sandfall\Content\Paks\~mods` folder, creating `~mods` directory if necessary
  - For Linux, it is most likely under `~/.steam/steam/steamapps/common/Expedition 33/Sandfall/Content/Paks/~mods/`.  However if your steam install is somewhere else, then replace the `~/.steam/` with that path. 
- Start the game and enjoy the chaos

## For Other Modders

Enemy rando overrides `DT_jRPG_Encounters`, `DT_jRPG_Encounters_CleaTower`, `DT_Encounters_Composite`, and `DT_WorldMap_Encounters` files, as well as `DT_jRPG_Enemies` if the option "Tie loot drops to encounters instead of enemies" is on. For the full list of files that get overridden by the item rando, look in the Data/ItemsData directory.

## Building Locally

* Building the application locally is as simple as cloning this repo, opening up the `E33Randomizer.sln`, and building.  The external tools are automatically downloaded from their respective sources with known valid versions.
* [Optional] The [Avalonia Extensions](https://docs.avaloniaui.net/tools/ide/) for your IDE so that Control/Window previews work without running the app.
* [Optional] The [AvaloniaUI Developer Tools](https://docs.avaloniaui.net/tools/developer-tools/installation) are also useful to be able to inspect the UI during runtime.

### Developer Guidlines

* All user displayed strings (other than the presets and item/skill/character/location data) are found in the `Resources.resx` file.  While this application is not currently localized, due to the international love of the game, this keeps that door open for the future.  Please do not embed user displayed strings directly into the code or axaml files. 
* All the Json handling is done via `System.Text.Json` within the main `E33Randomizer` project, do not use Newtonsoft.Json.  If UAssetAPI ever moves to being AotCompatible, It'd e nice to do that here too and NewtonsoftJson is never going to be AoT Compatible.
* When possible avoid locally styling single items and rely on setting the styles in [Styles.axaml](E33Randomizer/Styles.axaml) or [UIControls](E33Randomizer/UIControls).  This ensures that the app remains visually consistent.
* In general, prefer to use ViewModels for binding rather than binding interactions in the code behinds for the `Windows` or `UserControls`
* 

## Publishing a Build Locally

* Open the terminal
* Navigate to the [ScriptsAndHelpers](./ScriptsAndHelpers/) folder
* Run `./publish.bat versionNumber` (or `./publish.sh versionNumber` when on linux)
  * This will create both the Windows (.zip) and Linux (tar.gz) archives for the release in the repository root folder.  It will publish both regardless of if you're building in Windows or Linux.
  * Note that the `versionNumber` must be a validate .Net version such as `1.2.3` or `1.2.3-alpha`.  `1.2.3a` is not valid. 

## Other Notices

This project uses external tools (namely [repak](https://github.com/trumank/repak), [retoc](https://github.com/trumank/retoc), and [uesave](https://github.com/trumank/uesave)). If built from source code, please include the .exes in the project root folder. The release already contains them for ease of use. By using E33 Randomizer, users must also adhere to the licenses of [repak](https://github.com/trumank/repak/blob/master/LICENSE-MIT), [retoc](https://github.com/trumank/retoc/blob/master/LICENSE), and [uesave](https://github.com/trumank/uesave/blob/master/LICENSE), as well as [UAssetAPI](https://github.com/atenfyr/UAssetAPI/blob/master/LICENSE), in addition to [E33 Randomizers' own](./LICENSE).

## Credits & Special Thanks

- truman: For writing repak, retoc, and uesave
- atenfyr: For writing UAssetAPI
- TheNaeem: For hosting the Expedition 33 .usmap file
- Thefifthmatt: For the inspiration behind the UX of the mod
- Sandfall Interactive and Kepler Interactive: For creating one of my favourite games of all time and making it easy to mod
