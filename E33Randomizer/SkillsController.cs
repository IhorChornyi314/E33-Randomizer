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
        throw new NotImplementedException();
    }

    public override void UpdateViewModel()
    {
        throw new NotImplementedException();
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