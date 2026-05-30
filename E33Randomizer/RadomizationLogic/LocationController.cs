using System.Collections.ObjectModel;
using System.Text.Json;
using E33Randomizer.ObjectDatum;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.RadomizationLogic;

public class LocationController: Controller<LocationData>
{
    private LocationGraph _locationGraph = new LocationGraph();
    private UAsset _levelScalingTableAsset;
    public Dictionary<string, string> DestinationChanges = new();
    public Dictionary<string, (int, int)> LevelScaling = new();
    
    private List<string> _currentConstraints = new();
    private List<LocationData> criticalPath = new();
    private UAsset _stringTableAsset;
    
    public override void Initialize()
    {
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/location_data.json");
        var portalDestinations = ObjectsData.Where(o => o.PortalConnection != "").Select(o => o.PortalConnection);
        DestinationChanges = portalDestinations.Distinct().ToDictionary(pD => pD);
        _locationGraph.Init();
        ViewModel.ContainerName = "Original Destination";
        ViewModel.ObjectName = "New Destination";
        ReadConstraintFile();
        _levelScalingTableAsset = new UAsset($"{RandomizerLogic.DataDirectory}/LocationData/DT_LevelData.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _cleanSnapshot = ConvertToTxt();
        UpdateViewModel();
        ResetRandomObjectPool();
    }

    public override void Randomize()
    {
        _locationGraph.Reset();
        RandomizerLogic.CustomLocationPlacement.Update();
        foreach (var originalLocation in DestinationChanges.Keys)
        { 
            DestinationChanges[originalLocation] = RandomizerLogic.CustomLocationPlacement.Replace(originalLocation);
        }

        if (RandomizerLogic.Settings.RandomizeStartingLocation)
        {
            _currentConstraints[0] =  RandomizerLogic.CustomLocationPlacement.Replace("Level.SpawnPoint.LumiereAct01.Entry");
        }
        
        _locationGraph.ApplyDestinationChanges(DestinationChanges);
        var criticalPathChanges = _locationGraph.ConstructGoldenPath(_currentConstraints, out criticalPath);
        _locationGraph.ConstructDepths(_currentConstraints[0]);
        ConstructLevelScaling();
        foreach (var (originalDestination, newDestination) in DestinationChanges)
        {
            if (criticalPathChanges.TryGetValue(newDestination, out var criticalPathChange))
            {
                DestinationChanges[originalDestination] = criticalPathChange;
            }
        }
        UpdateViewModel();
    }

    public void ReadConstraintFile()
    {
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/LocationData/critical_path.json"))
        {
            string json = r.ReadToEnd();
            _currentConstraints = JsonSerializer.Deserialize<List<string>>(json, JsonSourceGenerationContext.Default.ListString) ?? [];
        }
    }

    public void ConstructLevelScaling()
    {
        var levelScalingTable = (_levelScalingTableAsset.Exports[0] as DataTableExport).Table;

        var scalingModifier = (float)RandomizerLogic.Settings.ScaleModifierPercentage / 100 * 0.9f;

        var criticalLevels = criticalPath.Select(n => n.LevelAsset).Distinct();
        
        foreach (StructPropertyData levelData in levelScalingTable.Data)
        {
            var levelName = (levelData.Value[0] as NamePropertyData).ToString();
            if (levelName is "Level_WorldMap_Main_V2" or "Level_Camp_Main" or "Manor_Main_WP")  continue;
            var locationNodes = ObjectsData.Where(n => n.LevelAsset == levelName).Select(n => n.CodeName);
            
            if (!locationNodes.Any()) continue;
            
            var minDepth = _locationGraph.Nodes.Where(n => locationNodes.Contains(n.CodeName)).Min(n => n.Depth);
            var maxDepth = _locationGraph.Nodes.Where(n => locationNodes.Contains(n.CodeName)).Max(n => n.Depth);
            
            minDepth = (int)(minDepth * scalingModifier);
            maxDepth = (int)(maxDepth * scalingModifier);

            if (levelName == "Level_Lumiere_Main_V2") minDepth = maxDepth;
            
            minDepth = Math.Min(minDepth, 99);
            maxDepth = Math.Min(maxDepth, 99);
            maxDepth = Math.Min(maxDepth, minDepth + 3);

            var areaOptional = !criticalLevels.Contains(levelName);

            if (areaOptional && !RandomizerLogic.Settings.ScaleOptionalAreas)
            {
                LevelScaling[levelName] = (
                    (levelData.Value[8] as IntPropertyData).Value,
                    (levelData.Value[9] as IntPropertyData).Value
                    );
                continue;
            }

            LevelScaling[levelName] = (minDepth, maxDepth);
        }
        Utils.WriteAsset(_levelScalingTableAsset);
    }
    
    public void WriteScalingTable()
    {
        var levelScalingTable = (_levelScalingTableAsset.Exports[0] as DataTableExport).Table;
        
        foreach (StructPropertyData levelData in levelScalingTable.Data)
        {
            var levelName = (levelData.Value[0] as NamePropertyData).ToString();
            if(!LevelScaling.ContainsKey(levelName)) continue;
            
            (levelData.Value[8] as IntPropertyData).Value = LevelScaling[levelName].Item1;
            (levelData.Value[9] as IntPropertyData).Value = LevelScaling[levelName].Item2;
        }
        Utils.WriteAsset(_levelScalingTableAsset);
    }

    public void ConstructReplacementStringTableAsset()
    {
        //In reality this is ST_UI_ModalPopup
        _stringTableAsset = new UAsset($"{RandomizerLogic.DataDirectory}/LocationData/ST_LocationRandomizerData.uasset",EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        foreach (var destinationChange in DestinationChanges)
        {
            if (destinationChange.Key == destinationChange.Value) continue;
            var originalData = GetObject(destinationChange.Key);
            var original = $"{originalData.LevelAsset}:{originalData.CodeName}";
            
            var changedData = GetObject(destinationChange.Value);
            var changed = $"{changedData.LevelAsset}:{changedData.CodeName}";
            
            (_stringTableAsset.Exports[0] as StringTableExport).Table.Add(FString.FromString(original), FString.FromString(changed));
        }
    }

    public override void AddObjectToContainer(string objectCodeName, string containerCodeName)
    {
        throw new NotSupportedException("A location can only have one corresponding replacement.");
    }

    public override void RemoveObjectFromContainer(int objectIndex, string containerCodeName)
    {
        throw new NotSupportedException("A location can only have one corresponding replacement.");
    }

    public override void InitFromTxt(string text)
    {
        text = text.ReplaceLineEndings("\n");

        var criticalPathLine = text.Split('\n').FirstOrDefault(line => line.StartsWith("CRITICAL PATH"), "");
        var destinationLines = text.Split("SCALING:\n")[0].Split('\n').Where(line => !line.StartsWith("CRITICAL PATH"));
        var scalingLines = text.Split("SCALING:\n")[1].Split('\n');
        
        DestinationChanges = destinationLines
            .Select(l => (l.Split('|')[0], l.Split('|')[1])).ToDictionary();
        LevelScaling = scalingLines.Select(l => (
            l.Split('|')[0], 
            (
                int.Parse(l.Split('|')[1].Split('-')[0]), 
                int.Parse(l.Split('|')[1].Split('-')[1])))
        ).ToDictionary();

        if (criticalPathLine != "")
            criticalPath = criticalPathLine.Replace("~", "").Split(" > ").Select(cN => ObjectsData.Find(lD => lD.CustomName == cN)).ToList();
        UpdateViewModel();
    }

    public override void ApplyViewModel()
    {
        foreach (var categoryViewModel in ViewModel.Categories)
        {
            foreach (var originalNodeContainer in categoryViewModel.Containers)
            {
                var originalNodeCodeName = originalNodeContainer.CodeName;
                var newNodeCodeName = originalNodeContainer.Objects[0].CodeName;
                DestinationChanges[originalNodeCodeName] = originalNodeContainer.Objects[0].BoolProperty ? newNodeCodeName : originalNodeCodeName;

                _locationGraph.GetNode(newNodeCodeName).Depth = originalNodeContainer.Objects[0].IntProperty;
            }
        }
    }

    public override void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
        if (ViewModel.AllObjects.Count == 0)
        {
            ViewModel.AllObjects = new ObservableCollection<ObjectViewModel>(ObjectsData.Select(i => new ObjectViewModel(i)).OrderBy(ovm => ovm.Name));
            foreach (var objectViewModel in ViewModel.AllObjects)
            {
                objectViewModel.BoolProperty = true;
                objectViewModel.IntProperty = GetObject(objectViewModel.CodeName).LevelScaling;
            }
        }
        
        Dictionary<string, CategoryViewModel> categoryViewModels = new();

        var changes = DestinationChanges.OrderBy(c => GetObject(c.Key).CustomName);
        
        foreach (var (originalDestination, newDestination) in changes)
        {
            var originalNode = GetObject(originalDestination);
            var newNode = GetObject(newDestination);
            var newNodeDepth = _locationGraph.GetNode(newDestination).Depth;
            
            var categoryName = originalNode.CustomName.Split(" - ")[0];
            if (!categoryViewModels.ContainsKey(categoryName))
            {
                categoryViewModels[categoryName] = new CategoryViewModel
                {
                    CategoryName = originalNode.CustomName.Split(" - ")[0],
                    Containers = []
                };
                ViewModel.Categories.Add(categoryViewModels[categoryName]);
            }
            
            var scalingLevel = newNodeDepth == Int16.MaxValue
                ? newNode.LevelScaling : (int)Math.Round(newNodeDepth * 0.9);
            
            var newContainer = new ContainerViewModel(originalNode.CodeName, originalNode.CustomName)
            {
                Objects =
                [
                    new ObjectViewModel(newNode)
                    {
                        CanDelete = false,
                        Index = 0,
                        // Whether the change is active
                        BoolProperty = true,
                        HasBoolPropertyControl = true,
                        IntProperty = scalingLevel
                    }
                ],
                CanAddObjects = false
            };

            categoryViewModels[categoryName].Containers.Add(newContainer);
            if (ViewModel.CurrentContainer != null && originalNode.CodeName == ViewModel.CurrentContainer.CodeName)
            { 
                ViewModel.CurrentContainer = newContainer;
                ViewModel.UpdateDisplayedObjects();
            }
        }
        ViewModel.UpdateFilteredCategories();
    }

    public override string ConvertToTxt()
    {
        var result = "";
        if (criticalPath.Count > 0)
        {
            var criticalPathString = "CRITICAL PATH:\t";
            for (int i = 0; i < criticalPath.Count - 1; i++)
            {
                var transition = criticalPath[i].UnconditionalConnections.Contains(criticalPath[i + 1].CodeName) ? " > " : " ~> ";
                transition = criticalPath[i].AllConditionalConnections.Contains(criticalPath[i + 1].CodeName) ? " > " : transition;
                criticalPathString += criticalPath[i].CustomName + transition;
            }
            criticalPathString += criticalPath[^1].CustomName;
            result += criticalPathString + '\n';
        }
        
        result += string.Join('\n', DestinationChanges.Select(kvp => $"{kvp.Key}|{kvp.Value}"));
        result += "SCALING:\n";
        result += string.Join('\n', LevelScaling.Select(kvp => $"{kvp.Key}|{kvp.Value.Item1}-{kvp.Value.Item2}"));

        return result;
    }

    public override void Reset()
    {
        InitFromTxt(_cleanSnapshot);
    }

    public override void WriteAssets()
    {
        ConstructReplacementStringTableAsset();
        WriteScalingTable();
        Utils.WriteAsset(_stringTableAsset);
    }
}