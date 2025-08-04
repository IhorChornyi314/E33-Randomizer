using System.IO;
using System.Text;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace E33Randomizer;

public static class EncountersController
{
    public static List<Encounter> Encounters = new();
    public static Dictionary<string, List<int>> EncounterIndexesByLocation = new();
    
    
    public static void ConstructEncountersByLocation()
    {
        var encounterLocations = new Dictionary<string, List<int>>();
        var uncategorizedEncounters = Enumerable.Range(0, Encounters.Count).ToList();
        using (StreamReader r = new StreamReader("Data/encounter_locations.json"))
        {
            string json = r.ReadToEnd();
            var locationsEncounters = JsonConvert.DeserializeObject<Dictionary<string, List<string> >>(json);
            foreach (var locationEncounters in locationsEncounters)
            {
                encounterLocations[locationEncounters.Key] = locationEncounters.Value.Select(eStr => Encounters.FindIndex(e => e.Name == eStr)).ToList();
                uncategorizedEncounters.RemoveAll(e => encounterLocations[locationEncounters.Key].Contains(e));
            }
        }
        
        encounterLocations["Uncategorized / Cut Content"] = uncategorizedEncounters;
        EncounterIndexesByLocation = encounterLocations;
    }
    
    public static void ReadEncounterAsset(string assetPath)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        var dataTable = asset.Exports[0] as DataTableExport;
        var encountersTable = dataTable.Table.Data;

        var encounters = encountersTable.Select(encounterStruct => new Encounter(encounterStruct, asset)).ToList();
        encounters = encounters.FindAll(e => !Encounters.Contains(e));
        Encounters.AddRange(encounters);
    }
    
    public static void ReadEncounterAssets()
    {
        Encounters.Clear();
        ReadEncounterAsset("Data/Originals/DT_jRPG_Encounters.uasset");
        ReadEncounterAsset("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        ReadEncounterAsset("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        ReadEncounterAsset("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
    }

    public static void WriteEncounterAsset(string assetPath)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        var assetFolder = asset.FolderName.Value.Replace("/Game", "randomizer/Sandfall/Content");
        PackEncounters(asset, Encounters);
        asset.Write($"{assetFolder}.uasset");
    }

    public static void WriteEncounterAssets()
    {
        Directory.CreateDirectory("randomizer/Sandfall/Content/jRPGTemplate/Datatables");
        Directory.CreateDirectory("randomizer/Sandfall/Content/jRPGTemplate/Datatables/Encounters_Datatables");
        WriteEncounterAsset("Data/Originals/DT_jRPG_Encounters.uasset");
        WriteEncounterAsset("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        WriteEncounterAsset("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        WriteEncounterAsset("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
    }
    
    public static void ReadEncountersTxt(string fileName)
    {
        Encounters.Clear();
        foreach (var line in File.ReadLines(fileName, Encoding.UTF8))
        {
            var newEncounter = new Encounter(line.Split('|')[0], line.Split('|')[1].Split(',').ToList());
            Encounters.Add(newEncounter);
        }
    }

    public static void WriteEncountersTxt(string fileName)
    {
        var result = "";
        foreach (var encounter in Encounters)
        {
            result += encounter + "\n";
        }
        File.WriteAllText(fileName, result, Encoding.UTF8);
    }

    public static void GenerateNewEncounters()
    {
        ReadEncounterAssets();
        CustomEnemyPlacement.Update();
        Encounters.ForEach(e => ModifyEncounter(e));
    }

    public static void Reset()
    {
        foreach (var encounter in Encounters)
        {
            encounter.LootEnemy = null;
        }
    }

    public static void HandleLoot()
    {
        foreach (var encounter in Encounters)
        {
            encounter.HandleEncounterLoot();
        }
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

        var oldEncounterSize = encounter.Size;
        
        for (int i = 0; i < newEncounterSize; i++)
        {
            if (i < encounter.Size)
            {
                EnemyData newEnemy = CustomEnemyPlacement.Replace(encounter.Enemies[i]);
                encounter.Enemies[i] = newEnemy;
            }
            else
            {
                if (i == 0 || Settings.RandomizeAddedEnemies)
                {
                    var newBaseEnemy = oldEncounterSize == 0 ? RandomizerLogic.GetRandomEnemy() : encounter.Enemies[i - int.Max(oldEncounterSize, 1)];
                    encounter.Enemies.Add(CustomEnemyPlacement.Replace(newBaseEnemy));
                }
                else
                {
                    encounter.Enemies.Add(encounter.Enemies[i - int.Max(oldEncounterSize, 1)]);
                }
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

    public static void AddEnemyToEncounter(string enemyCodeName, string encounterCodeName)
    {
        var enemyData = EnemiesController.GetEnemyData(enemyCodeName);
        Encounters.FindAll(e => e.Name == encounterCodeName).ForEach(e => e.Enemies.Add(enemyData));
    }
    
    public static void RemoveEnemyFromEncounter(string enemyCodeName, string encounterCodeName)
    {
        var enemyData = EnemiesController.GetEnemyData(enemyCodeName);
        Encounters.FindAll(e => e.Name == encounterCodeName).ForEach(e => e.Enemies.Remove(enemyData));
    }
}