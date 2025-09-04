# E33-Randomizer
A randomizer mod for Clair Obscur: Expedition 33 that gives users complete control over every enemy placement in the game, as well as a lot of different tools to customize them to their heart's desire.

Installation:
- Download the latest release
- If you don't have it, install [.NET 9.0 Desktop Runtime](https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/9.0.7/windowsdesktop-runtime-9.0.7-win-x64.exe)
- Unpack the zip file in a non-admin folder
- Run E33Randomizer.exe
<br>

Running the randomizer:
- Start the E33Randomizer.exe
- Configure the mod as you see fit
- Click "Generate and pack mod" button in the main window or "Generate mod files from current"
- Put the generated .pak, .utoc, and .ucas files into **Expedition 33\Sandfall\Content\Paks\\~mods** folder, creating ~mods directory if necessary
- Start the game and enjoy the chaos


<br>
For other modders - enemy rando overrides DT_jRPG_Encounters, DT_jRPG_Encounters_CleaTower, DT_Encounters_Composite, and DT_WorldMap_Encounters files, as well as DT_jRPG_Enemies if the option "Tie loot drops to encounters instead of enemies" is on. For the full list of files that get overridden by the item rando, look in the Data/ItemsData directory.
<br>
<br>
<br>
This project uses external tools (namely repak, retoc, and uesave). If built from source code, please include the .exes in the project root folder. The release already contains them for ease of use. By using E33 Randomizer, users must also adhere to the licenses of repak, retoc, and uesave, as well as UAssetAPI, in addition to E33 Randomizers' own.
<br>
<br>
<br>

Credits & Special Thanks:

- truman: For writing repak, retoc, and uesave
- atenfyr: For writing UAssetAPI
- TheNaeem: For hosting the Expedition 33 .usmap file
- Thefifthmatt: For the inspiration behind the UX of the mod
- Sandfall Interactive and Kepler Interactive: For creating one of my favourite games of all time and making it easy to mod
