using System.IO;
using System.Text;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace E33Randomizer;

public static class EncountersController
{
    public static Dictionary<String, List<Encounter>> Encounters = new();
    private static List<string> encounterAssets = new(){"DT_jRPG_Encounters", "DT_jRPG_Encounters_CleaTower", "DT_Encounters_Composite", "DT_WorldMap_Encounters"};

    public static List<Encounter> AllEncounters => Encounters.Values.SelectMany(e => e).ToList();
    
    public static void ReadEncounterAsset(string assetPath)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        var dataTable = asset.Exports[0] as DataTableExport;
        var encountersTable = dataTable.Table.Data;

        var encounters = encountersTable.Select(encounterStruct => new Encounter(encounterStruct, asset)).ToList();
        Encounters[asset.FolderName.Value.Split("/").Last()] = encounters;
    }
    
    public static void ReadEncounterAssets()
    {
        ReadEncounterAsset("Data/Originals/DT_jRPG_Encounters.uasset");
        ReadEncounterAsset("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        ReadEncounterAsset("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        ReadEncounterAsset("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
    }

    public static void WriteEncounterAsset(string assetPath)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        var assetName = asset.FolderName.Value.Split("/").Last();
        var assetFolder = asset.FolderName.Value.Replace("/Game", "randomizer/Sandfall/Content");
        PackEncounters(asset, Encounters[assetName]);
        asset.Write($"{assetFolder}.uasset");
    }

    public static void WriteEncounterAssets()
    {
        WriteEncounterAsset("Data/Originals/DT_jRPG_Encounters.uasset");
        WriteEncounterAsset("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        WriteEncounterAsset("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        WriteEncounterAsset("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
    }
    
    public static void ReadEncountersTxt(string fileName)
    {
        foreach (var line in File.ReadLines(fileName, Encoding.UTF8))
        {
            var currentAssetName = "";
            if (!line.Contains("|"))
            {
                currentAssetName = line;
                continue;
            }
            
            var newEncounter = new Encounter(line.Split('|')[0], line.Split('|')[1].Split(',').ToList());
            
            if (!Encounters.ContainsKey(currentAssetName))
            {
                Encounters.Add(currentAssetName, new List<Encounter>());
            }
            
            Encounters[currentAssetName].Add(newEncounter);
        }
    }

    public static void WriteEncountersTxt(string fileName)
    {
        var result = "";
        foreach (var encounterTable in Encounters)
        {
            result += encounterTable.Key + "\n";
            foreach (var encounter in encounterTable.Value)
            {
                result += encounter + "\n";
            }
        }
        File.WriteAllText(fileName, result, Encoding.UTF8);
    }

    public static void GenerateNewEncounters()
    {
        AllEncounters.ForEach(e => ModifyEncounter(e));
    }

    public static Encounter ModifyEncounter(Encounter encounter)
    {
        if (!SpecialRules.Randomizable(encounter))
        {
            return encounter;
        }
        
        var newEncounterSize = !Settings.RandomizeEncounterSizes || Settings.PossibleEncounterSizes.Count == 0 ? encounter.Enemies.Count :
                Utils.Pick(Settings.PossibleEncounterSizes);

        if (Settings.EnableEnemyOnslaught)
        {
            newEncounterSize += Settings.EnemyOnslaughtAdditionalEnemies;
            newEncounterSize = int.Min(newEncounterSize, Settings.EnemyOnslaughtEnemyCap);
        }

        if (newEncounterSize < encounter.Size)
        {
            encounter.Enemies.RemoveRange(0, encounter.Size - newEncounterSize);
        }

        for (int i = 0; i < newEncounterSize; i++)
        {
            if (i < encounter.Size)
            {
                EnemyData newEnemy = CustomEnemyPlacement.Replace(encounter.Enemies[i]);
                encounter.Enemies[i] = newEnemy;
            }
            else
            {
                encounter.Enemies.Add(CustomEnemyPlacement.Replace(RandomizerLogic.GetRandomEnemy()));
            }
        }
        
        SpecialRules.ApplySpecialRules(encounter);
        return encounter;
    }

    public static void PackEncounters(UAsset asset, List<Encounter> encounters)
    {
        var dataTable = asset.Exports[0] as DataTableExport;
        var encountersTable = dataTable.Table.Data;

        foreach (var encounterStruct in encountersTable)
        {
            var originalEncounter = new Encounter(encounterStruct, asset);
            var newEncounter = encounters.Find(e => e.Name == originalEncounter.Name);
            if (newEncounter != null)
            {
                newEncounter.SaveToStruct(encounterStruct);
            }
        }
    }
}