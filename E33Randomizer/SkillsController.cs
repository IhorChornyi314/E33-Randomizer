using System.IO;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class SkillsController: Controller<SkillData>
{
    public List<SkillGraph> SkillGraphs = new();

    public override void Initialize()
    {
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/skill_data.json");
        ReadAssets($"{RandomizerLogic.DataDirectory}/SkillsData");
        CustomPlacement = new CustomSkillPlacement();
        CustomPlacement.Init();
    }

    public override void InitFromTxt(string text)
    {
        throw new NotImplementedException();
    }

    public override void ApplyViewModel()
    {
        throw new NotImplementedException();
    }

    public override void UpdateViewModel()
    {
        throw new NotImplementedException();
    }

    public override string ConvertToTxt()
    {
        throw new NotImplementedException();
    }

    public override void Reset()
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
        throw new NotImplementedException();
    }

    public override void RemoveObjectFromContainer(int objectIndex, string containerCodeName)
    {
        throw new NotImplementedException();
    }

    public void WriteAssets()
    {
        foreach (var skillGraph in SkillGraphs)
        {
            Utils.WriteAsset(skillGraph.ToAsset());
        }
    }
}