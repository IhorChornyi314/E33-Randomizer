using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class SkillsController: Controller<SkillData>
{
    private string _cleanSnapshot;
    public List<SkillGraph> SkillGraphs = new();

    public override void Initialize()
    {
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/skill_data.json");
        ReadAssets($"{RandomizerLogic.DataDirectory}/SkillsData");
        CustomPlacement = new CustomSkillPlacement();
        CustomPlacement.Init();
        _cleanSnapshot = ConvertToTxt();
    }

    public override void InitFromTxt(string text)
    {
        var graphLines = text.Split('\n');
        foreach (var line in graphLines)
        {
            var characterName = line.Split('|')[0];
            var skillGraph = SkillGraphs.Find(sG => sG.CharacterName == characterName);
            skillGraph?.DecodeTxt(line);
        }
    }

    public override string ConvertToTxt()
    {
        return string.Join('\n', SkillGraphs.Select(sG => sG.EncodeTxt()));
    }
    
    public override void Reset()
    {
        InitFromTxt(_cleanSnapshot);
    }
    
    public override void ApplyViewModel()
    {
        foreach (var categoryViewModel in ViewModel.Categories)
        {
            foreach (var skillNodeViewModel in categoryViewModel.Containers)
            {
                var codeName = skillNodeViewModel.CodeName;
                var characterName = codeName.Split("#")[0];
                var skillCodeName = codeName.Split("#")[1];
                var skillGraph = SkillGraphs.FirstOrDefault(sG => sG.CharacterName == characterName);
                var node = skillGraph.Nodes.FirstOrDefault(c => c.OriginalSkillCodeName == skillCodeName);
                var skillViewModel = skillNodeViewModel.Objects[0];
                node.SkillData = GetObject(skillViewModel.CodeName);
                node.UnlockCost = skillViewModel.IntProperty;
                node.IsStarting = skillViewModel.BoolProperty;
            }
        }
    }

    public override void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
        
        if (ViewModel.AllObjects.Count == 0)
        {
            ViewModel.AllObjects = new ObservableCollection<ObjectViewModel>(ObjectsData.Select(i => new ObjectViewModel(i)));
        }

        foreach (var characterGraph in SkillGraphs)
        {
            var newTypeViewModel = new CategoryViewModel();
            newTypeViewModel.CategoryName = characterGraph.CharacterName;
            newTypeViewModel.Containers = new ObservableCollection<ContainerViewModel>();

            foreach (var node in characterGraph.Nodes)
            {
                var newContainer = new ContainerViewModel($"{characterGraph.CharacterName}#{node.OriginalSkillCodeName}", node.SkillData.CustomName);
                newContainer.Objects = new ObservableCollection<ObjectViewModel>([new ObjectViewModel(node.SkillData)]);
                newContainer.Objects[0].CanDelete = false;
                newContainer.Objects[0].Index = 0;
                
                newContainer.Objects[0].IntProperty = node.UnlockCost;
                newContainer.Objects[0].BoolProperty = node.IsStarting;
                newContainer.Objects[0].HasBoolPropertyControl = true;
                
                newContainer.CanAddObjects = false;
                
                newTypeViewModel.Containers.Add(newContainer);
                if (ViewModel.CurrentContainer != null && $"{node.OriginalSkillCodeName}" == ViewModel.CurrentContainer.CodeName)
                { 
                    ViewModel.CurrentContainer = newContainer;
                    ViewModel.UpdateDisplayedObjects();
                }
            }
            
            if (newTypeViewModel.Containers.Count > 0)
            {
                ViewModel.Categories.Add(newTypeViewModel);
            }
        }

        ViewModel.UpdateFilteredCategories();
    }

    public void ReadAssets(string filesDirectory)
    {
        var fileEntries = new List<string> (Directory.GetFiles(filesDirectory));
        foreach (var fileEntry in fileEntries.Where(f => f.Contains("DA_SkillGraph") && f.EndsWith(".uasset")))
        {
            var graphAsset = new UAsset(fileEntry, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
            var skillGraph = new SkillGraph(graphAsset);
            SkillGraphs.Add(skillGraph);
        }
    }

    public override void Randomize()
    {
        foreach (var skillGraph in SkillGraphs)
        {
            skillGraph.Randomize();
        }
    }

    public override void AddObjectToContainer(string objectCodeName, string containerCodeName)
    {
        throw new NotSupportedException("Skill nodes must have exactly one skill in them.");

    }

    public override void RemoveObjectFromContainer(int objectIndex, string containerCodeName)
    {
        throw new NotSupportedException("Skill nodes must have exactly one skill in them.");
    }

    public override void WriteAssets()
    {
        foreach (var skillGraph in SkillGraphs)
        {
            Utils.WriteAsset(skillGraph.ToAsset());
        }
    }
}