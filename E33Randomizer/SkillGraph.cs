using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class Node
{
    private StructPropertyData _structData;
    public FPackageIndex SkillPackageIndex;
    public SkillData SkillData;
    public int UnlockCost;
    public bool IsStarting;
    public string RequiredItem;
    public bool IsSecret;
    public FVector2D Position2D;
    
    public Node(StructPropertyData structData, UAsset parentAsset){
        _structData = structData;
        SkillPackageIndex = ((_structData.Value[0] as StructPropertyData).Value[0] as ObjectPropertyData).Value;
        var skillImport = parentAsset.Imports[int.Abs(SkillPackageIndex.Index) - 1];
        var skillCodeName = skillImport.ObjectName.ToString();
        SkillData = Controllers.SkillsController.GetObject(skillCodeName);
        
        UnlockCost = ((_structData.Value[0] as StructPropertyData).Value[1] as IntPropertyData).Value;
        IsStarting = ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value;

        var requiredItemNameProperty =
            ((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData;
        RequiredItem ??= requiredItemNameProperty.ToString();
        IsSecret = ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value;

        var positionDataArray = (_structData.Value[1] as StructPropertyData).Value[0] as Vector2DPropertyData;
        Position2D = positionDataArray.Value;
    }

    public StructPropertyData ToStruct(UAsset parentAsset)
    {
        var importIndex = parentAsset.SearchForImport(FName.FromString(parentAsset, SkillData.CodeName));
        
        if (importIndex == 0)
        {
            parentAsset.AddNameReference(FString.FromString(SkillData.ClassPath));
            parentAsset.AddNameReference(FString.FromString(SkillData.ClassName));
            var outerImport = new Import("/Script/CoreUObject", "Package", FPackageIndex.FromRawIndex(0), SkillData.ClassPath, false, parentAsset);
            var outerIndex = parentAsset.AddImport(outerImport);
            var innerImport = new Import("/Game/Gameplay/SkillTree/BP_DataAsset_Skill", "BP_DataAsset_Skill_C", outerIndex, SkillData.ClassName, false, parentAsset);
            SkillPackageIndex = parentAsset.AddImport(innerImport);
            importIndex = SkillPackageIndex.Index;
        }
        SkillPackageIndex = FPackageIndex.FromRawIndex(importIndex);
        ((_structData.Value[0] as StructPropertyData).Value[0] as ObjectPropertyData).Value = FPackageIndex.FromRawIndex(importIndex);
        
        ((_structData.Value[0] as StructPropertyData).Value[1] as IntPropertyData).Value = UnlockCost;
        ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value = IsStarting;

        
        // TODO: Add Item Table Import Reference
        if (RequiredItem != "null")
        {
            parentAsset.AddNameReference(FString.FromString(RequiredItem));
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData).Value = FName.FromString(parentAsset, RequiredItem);
        }
        else
        {
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData).Value = null;
        }
        
        IsSecret = ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value;

        ((_structData.Value[1] as StructPropertyData).Value[0] as Vector2DPropertyData).Value = Position2D;
        
        return _structData;
    }

    public override string ToString()
    {
        return SkillData.ToString();
    }
}

public class SkillGraph
{
    private UAsset _asset;
    public List<Node> Nodes = new();
    // Edges in the uasset connect objects, so duplicate skills copy connections
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
                var firstNodeClassName = _asset.Imports[int.Abs(firstNodeImportIndex) - 1].ObjectName.ToString();
                var secondNodeClassName = _asset.Imports[int.Abs(secondNodeImportIndex) - 1].ObjectName.ToString();
                
                var firstNodeIndex = Nodes.FindIndex(n => n.SkillData.CodeName == firstNodeClassName);
                firstNodeIndex = firstNodeIndex == -1 ? firstNodeImportIndex : firstNodeIndex;
                var secondNodeIndex = Nodes.FindIndex(n => n.SkillData.CodeName == secondNodeClassName);
                secondNodeIndex = secondNodeIndex == -1 ? secondNodeImportIndex : secondNodeIndex;

                Edges.Add(new Tuple<int, int>(firstNodeIndex, secondNodeIndex));
            }
        }
    }

    public void Randomize()
    {
        foreach (var node in Nodes)
        {
            node.SkillData = Controllers.SkillsController.GetRandomObject();
            node.UnlockCost = RandomizerLogic.rand.Next(10);
            node.IsSecret = RandomizerLogic.rand.Next(10) > 5;
            node.Position2D.X += RandomizerLogic.rand.Next(10) - 5;
            node.Position2D.Y += RandomizerLogic.rand.Next(10) - 5;
        }

        if (Edges.Count > 0)
        {
            Edges.Add(new  Tuple<int, int>(0, 5));
        }
    }

    public UAsset ToAsset()
    {
        var nodesArrayData = (_asset.Exports[0] as NormalExport).Data[0] as ArrayPropertyData;
        nodesArrayData.Value = Nodes.Select(n => n.ToStruct(_asset)).ToArray();

        if (Edges.Count == 0)
        {
            return _asset;
        }
        
        var edgesArrayData = (_asset.Exports[0] as NormalExport).Data[1] as ArrayPropertyData;
        var edgeStructDummy = edgesArrayData.Value[0].Clone() as StructPropertyData;
        edgesArrayData.Value = [];

        List<StructPropertyData> newEdges = new();
        foreach (var edge in Edges)
        {
            var firstPackageIndex = edge.Item1 < 0 ? FPackageIndex.FromRawIndex(edge.Item1) : Nodes[edge.Item1].SkillPackageIndex;
            var secondPackageIndex = edge.Item2 < 0 ? FPackageIndex.FromRawIndex(edge.Item2) : Nodes[edge.Item2].SkillPackageIndex;
            
            var edgeStruct = edgeStructDummy.Clone() as StructPropertyData;
            (edgeStruct.Value[0] as ObjectPropertyData).Value = firstPackageIndex;
            (edgeStruct.Value[1] as ObjectPropertyData).Value = secondPackageIndex;
            newEdges.Add(edgeStruct);
        }
        edgesArrayData.Value = newEdges.ToArray();
        return _asset;
    }

    public override string ToString()
    {
        return $"{CharacterName}'s Skills";
    }
}