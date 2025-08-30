using System.Collections.ObjectModel;
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
    public static EditIndividualObjectsWindowViewModel ViewModel = new();
    
    public static void ConstructEncountersByLocation()
    {
        var encounterLocations = new Dictionary<string, List<int>>();
        var uncategorizedEncounters = Enumerable.Range(0, Encounters.Count).ToList();
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/encounter_locations.json"))
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
        UpdateViewModel();
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
        ViewModel.ContainerName = "Encounter";
        ViewModel.ObjectName = "Enemy";
        ReadEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/DT_jRPG_Encounters.uasset");
        ReadEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        ReadEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        ReadEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
        UpdateViewModel();
    }

    public static void WriteEncounterAsset(string assetPath)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        PackEncounters(asset, Encounters);
        Utils.WriteAsset(asset);
    }

    public static void WriteEncounterAssets()
    {
        Directory.CreateDirectory("randomizer/Sandfall/Content/jRPGTemplate/Datatables");
        Directory.CreateDirectory("randomizer/Sandfall/Content/jRPGTemplate/Datatables/Encounters_Datatables");
        WriteEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/DT_jRPG_Encounters.uasset");
        WriteEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        WriteEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        WriteEncounterAsset($"{RandomizerLogic.DataDirectory}/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
    }
    
    public static void ReadEncountersTxt(string fileName)
    {
        Encounters.Clear();
        foreach (var line in File.ReadLines(fileName, Encoding.UTF8))
        {
            var newEncounter = new Encounter(line.Split('|')[0], line.Split('|')[1].Split(',').ToList());
            Encounters.Add(newEncounter);
        }
        UpdateViewModel();
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
        SpecialRules.Reset();
        ReadEncounterAssets();
        RandomizerLogic.CustomEnemyPlacement.Update();
        Encounters.ForEach(ModifyEncounter);
        UpdateViewModel();
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

    public static void ModifyEncounter(Encounter encounter)
    {
        if (!SpecialRules.Randomizable(encounter))
        {
            return;
        }
        
        var oldEncounterSize = encounter.Size;
        
        var possibleEncounterSizes = new List<int>();
        if (RandomizerLogic.Settings.EncounterSizeOne) possibleEncounterSizes.Add(1);
        if (RandomizerLogic.Settings.EncounterSizeTwo) possibleEncounterSizes.Add(2);
        if (RandomizerLogic.Settings.EncounterSizeThree) possibleEncounterSizes.Add(3);
        
        var newEncounterSize = !RandomizerLogic.Settings.RandomizeEncounterSizes || possibleEncounterSizes.Count == 0 ? oldEncounterSize :
                Utils.Pick(possibleEncounterSizes);

        if (!RandomizerLogic.Settings.ChangeSizeOfNonRandomizedEncounters)
        {
            var encounterRandomized = encounter.Enemies.Any(e => !RandomizerLogic.CustomEnemyPlacement.NotRandomizedCodeNames.Contains(e.CodeName));
            newEncounterSize = encounterRandomized ? newEncounterSize : oldEncounterSize;
        }
        
        if (RandomizerLogic.Settings.EnableEnemyOnslaught)
        {
            newEncounterSize += RandomizerLogic.Settings.EnemyOnslaughtAdditionalEnemies;
            newEncounterSize = int.Min(newEncounterSize, RandomizerLogic.Settings.EnemyOnslaughtEnemyCap);
        }

        if (newEncounterSize < encounter.Size)
        {
            encounter.Enemies.RemoveRange(0, encounter.Size - newEncounterSize);
        }

        
        for (int i = 0; i < newEncounterSize; i++)
        {
            if (i < encounter.Size)
            {
                var newEnemyCodeName = RandomizerLogic.CustomEnemyPlacement.Replace(encounter.Enemies[i].CodeName);
                encounter.Enemies[i] = EnemiesController.GetEnemyData(newEnemyCodeName);
            }
            else
            {
                if (i == 0 || RandomizerLogic.Settings.RandomizeAddedEnemies || !RandomizerLogic.Settings.EnableEnemyOnslaught)
                {
                    var newBaseEnemy = oldEncounterSize == 0 ? RandomizerLogic.GetRandomEnemy() : encounter.Enemies[i - int.Max(oldEncounterSize, 1)];
                    var newEnemyCodeName = RandomizerLogic.CustomEnemyPlacement.Replace(newBaseEnemy.CodeName);
                    encounter.Enemies.Add(EnemiesController.GetEnemyData(newEnemyCodeName));
                }
                else
                {
                    encounter.Enemies.Add(encounter.Enemies[i - int.Max(oldEncounterSize, 1)]);
                }
            }
        }
        
        SpecialRules.ApplySpecialRulesToEncounter(encounter);
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
        UpdateViewModel();
    }
    
    public static void RemoveEnemyFromEncounter(int enemyIndex, string encounterCodeName)
    {
        Encounters.FindAll(e => e.Name == encounterCodeName).ForEach(e => e.Enemies.RemoveAt(enemyIndex));
        UpdateViewModel();
    }
    
    public static void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
    
        if (ViewModel.AllObjects.Count == 0)
        {
            ViewModel.AllObjects = new ObservableCollection<ObjectViewModel>(EnemiesController.enemies.Select(e => new ObjectViewModel(e)));
        }
        
        var encountersByLocation = EncounterIndexesByLocation;
        foreach (var locationEncounterPair in encountersByLocation)
        {
            var newLocationViewModel = new CategoryViewModel();
            newLocationViewModel.CategoryName = locationEncounterPair.Key;
            newLocationViewModel.Containers = new ObservableCollection<ContainerViewModel>();
            foreach (var encounterIndex in locationEncounterPair.Value)
            {
                var encounterData = Encounters[encounterIndex];
                var newContainer = new ContainerViewModel(encounterData.Name);
                newContainer.Objects = new ObservableCollection<ObjectViewModel>(encounterData.Enemies.Select(e => new ObjectViewModel(e)));
                newLocationViewModel.Containers.Add(newContainer);
                if (ViewModel.CurrentContainer != null && encounterData.Name == ViewModel.CurrentContainer.Name)
                {
                    ViewModel.CurrentContainer = newContainer;
                    ViewModel.UpdateDisplayedObjects();
                }
            }

            if (newLocationViewModel.Containers.Count > 0)
            {
                ViewModel.Categories.Add(newLocationViewModel);
            }
        }

        ViewModel.UpdateFilteredCategories();
    }
}