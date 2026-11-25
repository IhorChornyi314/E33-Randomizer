# E33-Randomizer

A randomizer mod for Clair Obscur: Expedition 33 that gives users complete control over every enemy, item, skill, and location in the game, as well as tools to customize their placement.

## Installation

- Download the latest release
- If you don't have it, install the .Net 10 Runtime:
  - [Windows](https://builds.dotnet.microsoft.com/dotnet/Runtime/10.0.8/dotnet-runtime-10.0.8-win-x64.exe)
  - Linux installation methods vary across distributions; see the [Linux Package Manager](https://learn.microsoft.com/en-us/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website) article for many of them.  For Arch Linux, see [.Net docs](https://wiki.archlinux.org/title/.NET)

- Unpack the `zip` file in a non-admin folder.
- Run `E33Randomizer.exe`
  - For Linux, this is just `E33Randomizer.`

## Running the randomizer

- Start E33Randomizer.exe on Windows or E33Randomizer on Linux.
- Configure the mod as you see fit.
- Click the "Generate and pack mod" button in the main window or the "Generate mod files from current" button in the Edit Individual Objects windows.
- Put the generated `.pak`, `.utoc`, and `.ucas` files into the `Expedition 33\Sandfall\Content\Paks\~mods` folder, creating the `~mods` directory if necessary.
  - For Linux, it is most likely under `~/.steam/steam/steamapps/common/Expedition 33/Sandfall/Content/Paks/~mods/`.  However, if your Steam install is somewhere else, then replace the `~/.steam/` with that path.
- For the location randomizer:
  - Install UE4SS, either from the copy on [NexusMods](https://www.nexusmods.com/clairobscurexpedition33/mods/630) (don't forget to clean up the unnecessary mods from there for performance), or use the version provided in the latest release of this repo. 
  - Put the LocationRandomizer folder into UE4SS's Mods folder and enable it in the mods.txt and mods.json files.
- Start the game and enjoy the chaos.
- If you get stuck or need to find a specific enemy/item/etc., check out the config/spoilers.txt file in the generated folder.

## For Other Modders

For the full list of files that get overridden by the item rando, look in the Data directory. Note that the majority of those get overridden by the item randomizer. You can check which randomizer part needs the file by the folder it's located in. Feel free to contact me on [NexusMods](https://www.nexusmods.com/profile/Hastur1325) for more info.

## Building Locally

- Building the application locally is as simple as cloning this repo, opening up the `E33Randomizer.sln`, and building.  The external tools are automatically downloaded from their respective sources with known valid versions.
- [Optional] The [Avalonia Extensions](https://docs.avaloniaui.net/tools/ide/) for your IDE so that Control/Window previews work without running the app.
- [Optional] The [AvaloniaUI Developer Tools](https://docs.avaloniaui.net/tools/developer-tools/installation) are also useful to be able to inspect the UI during runtime.

### Developer Guidelines

- All user-displayed strings (other than the presets and item/skill/character/location data) are found in the `Resources.resx` file.  While this application is not currently localized, due to the international love of the game, this keeps that door open for the future.  Please do not embed user-displayed strings directly into the code or axaml files.
- All the JSON handling is done via `System.Text.Json` within the main `E33Randomizer` project; do not use `Newtonsoft.Json`.  If UAssetAPI ever moves to being AoT-compatible, It'd be nice to do that here too, and `Newtonsoft.Json` is never going to be AoT-compatible.
- When possible, avoid locally styling single items and rely on setting the styles in [Styles.axaml](E33Randomizer/Styles.axaml) or [UIControls](E33Randomizer/UIControls).  This ensures that the app remains visually consistent.
- In general, prefer to use ViewModels for binding rather than binding interactions in the code-behinds for the `Windows` or `UserControls`
- If the version of uesave or retoc needs to be updated, update the `external-assembly-version.json`.  You need to set the version and also grab the checksums from the release.

## Publishing a Build Locally

- Open the terminal
- Navigate to the [ScriptsAndHelpers](./ScriptsAndHelpers/) folder
- Run `./publish.bat versionNumber` (or `./publish.sh versionNumber` when on linux)
  - This will create both the Windows (.zip) and Linux (tar.gz) archives for the release in the repository root folder.  It will publish both regardless of whether you're building in Windows or Linux.
  - Note that the `versionNumber` must be a valid .Net version such as `1.2.3` or `1.2.3-alpha`.  `1.2.3a` is not valid. 

## Other Notices

This project uses external tools ([retoc](https://github.com/trumank/retoc), [ue4ss](https://github.com/UE4SS-RE/RE-UE4SS), and [uesave](https://github.com/trumank/uesave)). If built from source code, please include the executables in the project root folder. The release already contains them for ease of use. By using E33 Randomizer, users must also adhere to the licenses of [retoc](https://github.com/trumank/retoc/blob/master/LICENSE), [ue4ss](https://github.com/UE4SS-RE/RE-UE4SS/blob/main/LICENSE), and [uesave](https://github.com/trumank/uesave/blob/master/LICENSE), as well as [UAssetAPI](https://github.com/atenfyr/UAssetAPI/blob/master/LICENSE), in addition to [E33 Randomizers' own](./LICENSE).

## Credits & Special Thanks

- truman: For writing repak, retoc, and uesave
- atenfyr: For writing UAssetAPI
- UE4SS Team: For creating and maintaining UE4SS
- TheNaeem: For hosting the Expedition 33 .usmap file
- Demorck: For noting down the locations of every chest in the game and letting me use that information
- Thefifthmatt: For the inspiration behind the UX of the mod
- Sandfall Interactive and Kepler Interactive: For creating one of my favourite games of all time and making it easy to mod
