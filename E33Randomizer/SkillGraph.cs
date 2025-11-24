using System.Windows.Controls;
using System.Windows.Data;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class SkillNode
{
    private StructPropertyData _structData;
    public string OriginalSkillCodeName;
    public FPackageIndex SkillPackageIndex;
    public SkillData SkillData;
    public int UnlockCost;
    public bool IsStarting;
    public string RequiredItem;
    public bool IsSecret;
    public FVector2D Position2D;
    
    public SkillNode(string rep, StructPropertyData structData)
    {
        _structData = structData;
        SkillPackageIndex = ((_structData.Value[0] as StructPropertyData).Value[0] as ObjectPropertyData).Value;
        
        var stringParts = rep.Split(':');
        OriginalSkillCodeName = stringParts[0];
        SkillData = Controllers.SkillsController.GetObject(stringParts[0]);
        UnlockCost = int.Parse(stringParts[1]);
        IsStarting = bool.Parse(stringParts[2]);
        RequiredItem = stringParts[3];
        IsSecret = bool.Parse(stringParts[4]);
        Position2D.X = int.Parse(stringParts[5]);
        Position2D.Y = int.Parse(stringParts[6]);
    }
    
    public SkillNode(StructPropertyData structData, UAsset parentAsset){
        _structData = structData;
        SkillPackageIndex = ((_structData.Value[0] as StructPropertyData).Value[0] as ObjectPropertyData).Value;
        var skillImport = parentAsset.Imports[int.Abs(SkillPackageIndex.Index) - 1];
        OriginalSkillCodeName = skillImport.ObjectName.ToString();
        SkillData = Controllers.SkillsController.GetObject(OriginalSkillCodeName);
        
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

        
        if (RequiredItem != "null")
        {
            parentAsset.AddNameReference(FString.FromString("DT_jRPG_Items_Composite"));
            parentAsset.AddNameReference(FString.FromString("/Game/jRPGTemplate/Datatables/DT_jRPG_Items_Composite"));
            var outerImport = new Import("/Script/CoreUObject", "Package", FPackageIndex.FromRawIndex(0), "/Game/jRPGTemplate/Datatables/DT_jRPG_Items_Composite", false, parentAsset);
            var outerIndex = parentAsset.AddImport(outerImport);
            var innerImport = new Import("/Script/Engine", "CompositeDataTable", outerIndex, "DT_jRPG_Items_Composite", false, parentAsset);
            var itemDataTableIndex = parentAsset.AddImport(innerImport);
            parentAsset.AddNameReference(FString.FromString(RequiredItem));
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData).Value = FName.FromString(parentAsset, RequiredItem);
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[0] as
                ObjectPropertyData).Value = itemDataTableIndex;
        }
        else
        {
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData).Value = null;
        }
        
        IsSecret = ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value;

        ((_structData.Value[1] as StructPropertyData).Value[0] as Vector2DPropertyData).Value = Position2D;
        
        return _structData;
    }

    public string EncodeTxt()
    {
        return $"{SkillData.CodeName}:{UnlockCost}:{IsStarting}:{RequiredItem}:{IsSecret}:{(int)Position2D.X}:{(int)Position2D.Y}";
    }

    public override string ToString()
    {
        return SkillData.ToString();
    }
}

public class SkillGraph
{
    private UAsset _asset;

    private StructPropertyData _dummyStructData;
    private int _totalSkillCost = 0;
    public List<SkillNode> Nodes = new();
    // Edges in the uasset connect objects, so duplicate skills copy connections
    public List<Tuple<int, int>> Edges = new();
    public string CharacterName;

    public SkillGraph(UAsset asset)
    {
        _asset = asset;
        CharacterName = _asset.FolderName.Value.Split('_')[^1];
        CharacterName = CharacterName == "Noah" ? "Gustave" : CharacterName;
        var nodesArrayData = (_asset.Exports[0] as NormalExport).Data[0] as ArrayPropertyData;
        if (nodesArrayData.Value.Length > 0)
        {
            _dummyStructData = nodesArrayData.Value[0].Clone() as StructPropertyData;
            _dummyStructData.Value[0] = _dummyStructData.Value[0].Clone() as StructPropertyData;
            _dummyStructData.Value[1] = _dummyStructData.Value[1].Clone() as StructPropertyData;
        }
        foreach (StructPropertyData nodeStruct in nodesArrayData.Value)
        {
            Nodes.Add(new SkillNode(nodeStruct, _asset));
            _totalSkillCost += Nodes.Last().UnlockCost;
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
            var newSkillName = RandomizerLogic.CustomSkillPlacement.Replace(node.OriginalSkillCodeName);
            node.SkillData = Controllers.SkillsController.GetObject(newSkillName);
            
            SpecialRules.ApplySpecialRulesToSkillNode(node);
        }
        
        if (RandomizerLogic.Settings.RandomizeSkillUnlockCosts)
        {
            for (int i = 0; i < Nodes.Count * 6; i++)
            {
                var firstNodeIndex = RandomizerLogic.rand.Next(0, Nodes.Count - 1);
                var secondNodeIndex = firstNodeIndex + 1;

                if (Nodes[firstNodeIndex].IsStarting || Nodes[secondNodeIndex].IsStarting)
                {
                    continue;
                }
                    
                if (Nodes[firstNodeIndex].UnlockCost > 0 && Nodes[firstNodeIndex].UnlockCost < 9 && 
                    Nodes[secondNodeIndex].UnlockCost > 1 && Nodes[secondNodeIndex].UnlockCost < 10)
                {
                    Nodes[firstNodeIndex].UnlockCost += 1;
                    Nodes[secondNodeIndex].UnlockCost -= 1;
                }
            }
        }
    }

    // I'm not gonna add RemoveNode cause it will be a hell of a headache to properly manage
    // Honestly I don't know if I even should have AddNode
    public void AddNode(SkillData skillData, int unlockCost, bool isStarting = true, bool isSecret = false, List<int> connectedNodeIndexes = null, Tuple<double, double> position2D = null)
    {
        var newDummyStructData = _dummyStructData.Clone() as StructPropertyData;
        newDummyStructData.Value[0] = newDummyStructData.Value[0].Clone() as StructPropertyData;
        newDummyStructData.Value[1] = newDummyStructData.Value[1].Clone() as StructPropertyData;
        var newNode = new SkillNode(newDummyStructData, _asset);
        newNode.SkillData = skillData;
        newNode.UnlockCost = unlockCost;
        newNode.IsStarting = isStarting;
        newNode.IsSecret = isSecret;
        if (position2D != null)
        {
            newNode.Position2D.X = position2D.Item1;
            newNode.Position2D.Y = position2D.Item2;
        }
        else
        {
            newNode.Position2D.X = RandomizerLogic.rand.Next(-500, 500);
            newNode.Position2D.Y = RandomizerLogic.rand.Next(-500, 500);
        }

        if (connectedNodeIndexes != null)
        {
            foreach (var connectedNodeIndex in connectedNodeIndexes)
            {
                Edges.Add(new Tuple<int, int>(Nodes.Count, connectedNodeIndex));
            }
        }
        
        Nodes.Add(newNode);
    }

    public void SetNode(int i, SkillData skillData, int unlockCost = -1, bool isStarting = true, bool isSecret = false,
        Tuple<double, double> position2D = null)
    {
        Nodes[i].SkillData = skillData;
        Nodes[i].UnlockCost = unlockCost == -1 ? Nodes[i].UnlockCost : unlockCost;
        Nodes[i].IsStarting = isStarting;
        Nodes[i].IsSecret = isSecret;
        if (position2D == null) return;
        Nodes[i].Position2D.X = position2D.Item1;
        Nodes[i].Position2D.Y = position2D.Item2;
    }

    public void SetNode(int i, SkillData skillData)
    {
        Nodes[i].SkillData = skillData;
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

    public string EncodeTxt()
    {
        var result = $"{CharacterName}|";
        result += string.Join(',', Nodes.Select(n => n.EncodeTxt()));
        result += "|" + string.Join(',', Edges.Select(e => $"{e.Item1}:{e.Item2}"));
        return result;
    }

    public void DecodeTxt(string rep)
    {
        var stringParts = rep.Split('|');
        CharacterName = stringParts[0];
        // This is so scuffed but whatever 
        Nodes.Clear();
        foreach (var nodeRep in stringParts[1].Split(','))
        {
            var newDummyStructData = _dummyStructData.Clone() as StructPropertyData;
            newDummyStructData.Value[0] = newDummyStructData.Value[0].Clone() as StructPropertyData;
            newDummyStructData.Value[1] = newDummyStructData.Value[1].Clone() as StructPropertyData;
            Nodes.Add(new SkillNode(nodeRep, newDummyStructData));
        }

        if (Edges.Count == 0)
            return;
        
        Edges.Clear();
        foreach (var edgeRep in stringParts[2].Split(','))
        {
            Edges.Add(new Tuple<int, int>(int.Parse(edgeRep.Split(':')[0]), int.Parse(edgeRep.Split(':')[1])));
        }
    }

    public override string ToString()
    {
        return $"{CharacterName}'s Skills";
    }
}