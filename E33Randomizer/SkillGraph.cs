using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer;

public class Node
{
    private StructPropertyData _structData;
    public SkillData skillData;
    public int UnlockCost;
    public bool IsStarting;
    public string RequiredItem;
    public bool IsSecret;
    public Tuple<double, double> Position2D;
    
    public Node(StructPropertyData structData, UAsset parentAsset){
        _structData = structData;
        var importIndex = ((_structData.Value[0] as StructPropertyData).Value[0] as ObjectPropertyData).Value;
        var skillImport = parentAsset.Imports[int.Abs(importIndex.Index) - 1];
        var skillCodeName = skillImport.ObjectName.ToString();
        skillData = Controllers.SkillsController.GetObject(skillCodeName);
        
        UnlockCost = ((_structData.Value[0] as StructPropertyData).Value[1] as IntPropertyData).Value;
        IsStarting = ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value;

        var requiredItemNameProperty =
            ((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData;
        RequiredItem ??= requiredItemNameProperty.ToString();
        IsSecret = ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value;

        var positionDataArray = (_structData.Value[1] as StructPropertyData).Value[0] as Vector2DPropertyData;
        Position2D = new Tuple<double, double>(positionDataArray.Value.X, positionDataArray.Value.Y);
    }

    public StructPropertyData ToStruct()
    {
        return _structData;
    }

    public override string ToString()
    {
        return skillData.ToString();
    }
}

public class SkillGraph
{
    private UAsset _asset;
    public List<Node> Nodes = new();
    public List<Tuple<int, int>> Edges = new();
    public string CharacterName;

    public SkillGraph(UAsset asset)
    {
        _asset = asset;
        CharacterName = _asset.FolderName.Value.Split('_')[^1];
        CharacterName = CharacterName == "Noah" ? "Gustave" : CharacterName;
        var nodesArrayData = (_asset.Exports[0] as NormalExport).Data[0] as ArrayPropertyData;
        foreach (StructPropertyData nodeStruct in nodesArrayData.Value)
        {
            Nodes.Add(new Node(nodeStruct, _asset));
        }
        var edgesArrayData = (_asset.Exports[0] as NormalExport).Data[1] as ArrayPropertyData;
        if (edgesArrayData.Value.Length > 1)
        {
            foreach (StructPropertyData edgeStruct in edgesArrayData.Value)
            {
                var firstNodeImportIndex = (edgeStruct.Value[0] as ObjectPropertyData).Value.Index;
                var secondNodeImportIndex = (edgeStruct.Value[1] as ObjectPropertyData).Value.Index;
                var firstNodeClassName = _asset.Imports[int.Abs(firstNodeImportIndex) - 1].ClassName.ToString();
                var secondNodeClassName = _asset.Imports[int.Abs(secondNodeImportIndex) - 1].ClassName.ToString();
                var firstNodeIndex = Nodes.FindIndex(n => n.skillData.CodeName == firstNodeClassName);
                var secondNodeIndex = Nodes.FindIndex(n => n.skillData.CodeName == secondNodeClassName);
                
                Edges.Add(new Tuple<int, int>(firstNodeIndex, secondNodeIndex));
            }
        }
    }

    public UAsset ToAsset()
    {
        return _asset;
    }

    public override string ToString()
    {
        return $"{CharacterName}'s Skills";
    }
}