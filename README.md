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

Building the application locally is as simple as cloning this repo, opening up the `E33Randomizer.sln`, and building.  The external tools are automatically downloaded from their respective sources with known valid versions.

## Other Notices

This project uses external tools (namely [repak](https://github.com/trumank/repak), [retoc](https://github.com/trumank/retoc), and [uesave](https://github.com/trumank/uesave)). If built from source code, please include the .exes in the project root folder. The release already contains them for ease of use. By using E33 Randomizer, users must also adhere to the licenses of [repak](https://github.com/trumank/repak/blob/master/LICENSE-MIT), [retoc](https://github.com/trumank/retoc/blob/master/LICENSE), and [uesave](https://github.com/trumank/uesave/blob/master/LICENSE), as well as [UAssetAPI](https://github.com/atenfyr/UAssetAPI/blob/master/LICENSE), in addition to [E33 Randomizers' own](./LICENSE).

## Credits & Special Thanks

- truman: For writing repak, retoc, and uesave
- atenfyr: For writing UAssetAPI
- TheNaeem: For hosting the Expedition 33 .usmap file
- Thefifthmatt: For the inspiration behind the UX of the mod
- Sandfall Interactive and Kepler Interactive: For creating one of my favourite games of all time and making it easy to mod
