using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;
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
    public bool IsUnlockedByDefault;
    
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
        IsUnlockedByDefault = SkillGraph.StartingSkillNames.SelectMany(s => s.Value).Contains(OriginalSkillCodeName);
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
        IsSecret = ((_structData.Value[0] as StructPropertyData).Value[4] as BoolPropertyData).Value;

        var positionDataArray = (_structData.Value[1] as StructPropertyData).Value[0] as Vector2DPropertyData;
        Position2D = positionDataArray.Value;

        IsUnlockedByDefault = UnlockCost == 0 && !IsSecret || OriginalSkillCodeName == "DA_Skill_Maelle_NEW18_Spark";
    }

    public StructPropertyData ToStruct(UAsset parentAsset)
    {
        var importIndex = parentAsset.SearchForImport(FName.FromString(parentAsset, SkillData.CodeName));

        parentAsset.AddNameReference(FString.FromString(SkillData.ClassPath));
        parentAsset.AddNameReference(FString.FromString(SkillData.CodeName));
        
        if (importIndex == 0)
        {
            SkillPackageIndex = Utils.AddImportToUAsset(parentAsset, "BP_DataAsset_Skill_C", SkillData.ClassPath, SkillData.ClassName,
                "/Game/Gameplay/SkillTree/BP_DataAsset_Skill");
            importIndex = SkillPackageIndex.Index;
        }
        SkillPackageIndex = FPackageIndex.FromRawIndex(importIndex);
        ((_structData.Value[0] as StructPropertyData).Value[0] as ObjectPropertyData).Value = FPackageIndex.FromRawIndex(importIndex);
        
        ((_structData.Value[0] as StructPropertyData).Value[1] as IntPropertyData).Value = UnlockCost;
        ((_structData.Value[0] as StructPropertyData).Value[2] as BoolPropertyData).Value = IsStarting;

        
        if (RequiredItem != "null")
        {
            Utils.AddImportToUAsset(parentAsset, "CompositeDataTable", "/Game/jRPGTemplate/Datatables/DT_jRPG_Items_Composite");
            var itemDataTableIndex = Utils.AddImportToUAsset(parentAsset, "DataTable",
                "/Game/Gameplay/SkillTree/Content/DT_Items_GradientAttackUnlocks");
            parentAsset.AddNameReference(FString.FromString(RequiredItem), true);
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData).Value = FName.FromString(parentAsset, RequiredItem);
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[0] as
                ObjectPropertyData).Value = itemDataTableIndex;
        }
        else
        {
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[1] as NamePropertyData).Value = FName.FromString(parentAsset, "None");
            (((_structData.Value[0] as StructPropertyData).Value[3] as StructPropertyData).Value[0] as
                ObjectPropertyData).Value = null;
        }
        
        ((_structData.Value[0] as StructPropertyData).Value[4] as BoolPropertyData).Value = IsSecret;

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

    private StructPropertyData _dummyNodeStructData;
    private StructPropertyData _dummyEdgeStructData;
    private int _totalSkillCost = 0;

    private static Dictionary<string, List<int>> _startingSkillIndexes = new()
    {
        {"Julie", []},
        {"Gustave", [1, 7]},
        {"Lune", [3, 7]},
        {"Maelle", [0, 1, 11]},
        {"Sciel", [0, 11]},
        {"Verso", [0, 7]},
        {"Monoco", [8, 31]},
    };
    public static Dictionary<string, List<string>> StartingSkillNames = new()
    {
        {"Julie", []},
        {"Gustave", ["DA_Skill_Gustave_UnleashCharge", "DA_Skill_Gustave_Combo1"]},
        {"Lune", ["DA_Skill_Lune_Immolation", "DA_Skill_Lune_IceGust"]},
        {"Maelle", ["DA_Skill_Maelle_OffensiveSwitch", "DA_Skill_Maelle_NEW18_Spark", "DA_Skill_Maelle_NEW14_Percee"]},
        {"Sciel", ["DA_Skill_Sciel_new2_Foretelling2", "DA_Skill_Sciel_new3_GrimPrediction"]},
        {"Verso", ["DA_Skill_Combo1", "DA_Skill_Verso_NEW_FromFire"]},
        {"Monoco", ["DA_Skill_Transfo_StalactMeleeCombo", "DA_Skill_Transfo_ChalierRelentlessSword"]},
    };
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
            _dummyNodeStructData = nodesArrayData.Value[0].Clone() as StructPropertyData;
            _dummyNodeStructData.Value[0] = _dummyNodeStructData.Value[0].Clone() as StructPropertyData;
            _dummyNodeStructData.Value[1] = _dummyNodeStructData.Value[1].Clone() as StructPropertyData;
        }
        foreach (StructPropertyData nodeStruct in nodesArrayData.Value)
        {
            Nodes.Add(new SkillNode(nodeStruct, _asset));
            _totalSkillCost += Nodes.Last().UnlockCost;
        }
        var edgesArrayData = (_asset.Exports[0] as NormalExport).Data[1] as ArrayPropertyData;
        if (edgesArrayData.Value.Length > 1)
        {
            _dummyEdgeStructData = edgesArrayData.Value[0].Clone() as StructPropertyData;
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
        if (CharacterName == "Julie")
        {
            return;
        }
        
        foreach (var node in Nodes)
        {
            var newSkillName = RandomizerLogic.CustomSkillPlacement.Replace(node.OriginalSkillCodeName);
            node.SkillData = Controllers.SkillsController.GetObject(newSkillName);
            
            SpecialRules.ApplySpecialRulesToSkillNode(node);
        }

        if (RandomizerLogic.Settings.GuaranteeGustaveOvercharge && CharacterName == "Gustave")
        {
            // Overcharge is 7
            var replacedSkill = Nodes[7].SkillData;
            var overchargeNode = Nodes.Find(n => n.SkillData.CodeName == "DA_Skill_Gustave_UnleashCharge");
            if (overchargeNode != null) overchargeNode.SkillData = replacedSkill;
            Nodes[7].SkillData = Controllers.SkillsController.GetObject("DA_Skill_Gustave_UnleashCharge");
        }

        
        var newStartingSkills = _startingSkillIndexes[CharacterName].Select(i => Nodes[i].SkillData).ToList();
        CharacterStartingStateManager.SetStartingSkills(CharacterName, newStartingSkills);
        
        if (RandomizerLogic.Settings.RandomizeSkillUnlockCosts)
        {
            for (int i = 0; i < Nodes.Count * 6; i++)
            {
                var firstNodeIndex = RandomizerLogic.rand.Next(0, Nodes.Count);
                var secondNodeIndex = (firstNodeIndex + 1) % Nodes.Count;

                if (Nodes[firstNodeIndex].IsStarting || Nodes[secondNodeIndex].IsStarting)
                {
                    continue;
                }

                // Spark is unlocked by default despite not being a starting skill
                if (Nodes[firstNodeIndex].OriginalSkillCodeName == "DA_Skill_Maelle_NEW18_Spark" ||
                    Nodes[secondNodeIndex].OriginalSkillCodeName == "DA_Skill_Maelle_NEW18_Spark")
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
        
        if (RandomizerLogic.Settings.RandomizeTreeEdges && (CharacterName != "Monoco" || RandomizerLogic.Settings.GiveMonocoTreeEdges))
        {
            var minEdges = Math.Min(RandomizerLogic.Settings.MinTreeEdges, RandomizerLogic.Settings.MaxTreeEdges);
            var maxEdges = Math.Max(RandomizerLogic.Settings.MinTreeEdges, RandomizerLogic.Settings.MaxTreeEdges);
            
            RandomizeEdges(minEdges, maxEdges, RandomizerLogic.Settings.FullyRandomEdges);
        }

        if (RandomizerLogic.Settings.MakeSkillsIntoItems)
        {
            Edges.Clear();
        }
    }

    public List<int> GetLinkedNodes()
    {
        if (CharacterName == "Gustave" && !RandomizerLogic.Settings.UnlockGustaveSkills)
        {
            return [1, 7, 0, 4, 3, 2, 5, 6];
        }

        if (CharacterName == "Monoco")
        {
            // 36, 45, and 46 are gradients
            return
            [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27,
                28, 29, 30, 31, 32, 33, 34, 35, 37, 38, 39, 40, 41, 42, 43, 44, 47, 48
            ];
        }
        
        var linkedNodes = new List<int>();
        foreach (var edge in Edges)
        {
            if (!linkedNodes.Contains(edge.Item1))
            {
                linkedNodes.Add(edge.Item1);
            }
            if (!linkedNodes.Contains(edge.Item2))
            {
                linkedNodes.Add(edge.Item2);
            }
        }
        return linkedNodes;
    }

    public Dictionary<int, List<int>> GetPossibleEdges(bool fullyRandom)
    {
        if (fullyRandom || CharacterName == "Julie")
        {
            var linkedNodes = GetLinkedNodes();
            return linkedNodes.Select(n => new KeyValuePair<int, List<int>> (n, [..linkedNodes.Where(i => i != n)])).ToDictionary();
        }
        
        var fileName = CharacterName == "Gustave" && RandomizerLogic.Settings.UnlockGustaveSkills ? "GustaveFull" : CharacterName;
        
        using var r = new StreamReader($"{RandomizerLogic.DataDirectory}/SkillsData/CustomGraphs/{fileName}.json");
        var json = r.ReadToEnd();
        return JsonConvert.DeserializeObject<Dictionary<int, List<int>>>(json);
    }

    public int PickNextTreeNode(List<int> currentTreeNodes, int minEdges, int maxEdges,
        Dictionary<int, int> currentNodeDegrees, Dictionary<int, int> availableNodeDegrees)
    {
        currentTreeNodes = Utils.ShuffleList(currentTreeNodes);
        var lastAvailableNode = -1;
        foreach (var node in currentTreeNodes)
        {
            if (availableNodeDegrees[node] > 0)
            {
                lastAvailableNode = node;
            }
            if (currentNodeDegrees[node] >= maxEdges || availableNodeDegrees[node] == 0) continue;
            if (availableNodeDegrees[node] + currentNodeDegrees[node] < minEdges) continue;
            return node;
        }
        return lastAvailableNode;
    }
    
    public void RandomizeEdges(int minEdges, int maxEdges, bool fullyRandom)
    {
        var currentAdjacencyList = GetPossibleEdges(fullyRandom);
        var possibleEdges = GetPossibleEdges(fullyRandom);
        var linkedNodes = GetLinkedNodes();
        
        var treeAdjacencyList = new Dictionary<int, List<int>>();
        var currentNodeDegrees = linkedNodes.Select(n => new KeyValuePair<int, int>(n, 0)).ToDictionary();
        var availableNodeDegrees = possibleEdges.Select(kvp => new KeyValuePair<int, int>(kvp.Key, kvp.Value.Count)).ToDictionary();
        
        var currentTreeNodes = new List<int> ([Utils.Pick(linkedNodes)]);
        
        foreach (var availableNode in currentAdjacencyList[currentTreeNodes[0]])
        {
            if (currentAdjacencyList[availableNode].Remove(currentTreeNodes[0]))
                availableNodeDegrees[availableNode] -= 1;
        }
        
        for (int i = 0; i < linkedNodes.Count - 1; i++)
        {
            var nextNode = PickNextTreeNode(currentTreeNodes, minEdges, maxEdges, currentNodeDegrees, availableNodeDegrees);

            if (nextNode == -1 || currentAdjacencyList[nextNode].Count == 0)
            {
                Console.WriteLine("Edge randomization fucked up, breaking one of the constraints");
                nextNode = Utils.Pick(currentTreeNodes);
            }
            
            var newEdge = Utils.Pick(currentAdjacencyList[nextNode]);
            if (!treeAdjacencyList.ContainsKey(nextNode))
            {
                treeAdjacencyList[nextNode] = [];
            }
            treeAdjacencyList[nextNode].Add(newEdge);
            currentNodeDegrees[nextNode] += 1;
            currentNodeDegrees[newEdge] += 1;
            
            
            foreach (var availableNode in possibleEdges[newEdge])
            {
                if (currentAdjacencyList[availableNode].Remove(newEdge))
                    availableNodeDegrees[availableNode] -= 1;
            }
            
            currentTreeNodes.Add(newEdge);
        }
        
        var newAdjacencyList = new Dictionary<int, List<int>>(treeAdjacencyList);
        
        foreach (var currentNode in linkedNodes)
        {
            if (!newAdjacencyList.ContainsKey(currentNode))
            {
                newAdjacencyList[currentNode] = [];
            }
            foreach (var possibleEdge in possibleEdges[currentNode])
            {
                if (newAdjacencyList.TryGetValue(currentNode, out List<int>? edges) && edges.Contains(possibleEdge))
                {
                    continue;
                }
                
                if (currentNodeDegrees[currentNode] >= maxEdges || currentNodeDegrees[possibleEdge] >= maxEdges) continue;
                
                var canAdd = currentNodeDegrees[currentNode] < minEdges || currentNodeDegrees[possibleEdge] < minEdges || RandomizerLogic.rand.Next(100) < RandomizerLogic.Settings.RandomEdgeChancePercent;
                if (!canAdd)
                {
                    continue;
                }
                
                newAdjacencyList[currentNode].Add(possibleEdge);
                currentNodeDegrees[currentNode] += 1;
                currentNodeDegrees[possibleEdge] += 1;
            }
        }
    
        Edges.Clear();
            
        foreach (var edgesList in newAdjacencyList)
        {
            foreach (var edge in edgesList.Value)
            {
                if (Edges.Contains(new Tuple<int, int>(edgesList.Key, edge)) ||
                    Edges.Contains(new Tuple<int, int>(edge, edgesList.Key)))
                {
                    continue;
                }
                Edges.Add(new Tuple<int, int>(edgesList.Key, edge));
            }
        }
    }

    // I'm not gonna add RemoveNode cause it will be a hell of a headache to properly manage
    // Honestly I don't know if I even should have AddNode
    public void AddNode(SkillData skillData, int unlockCost, bool isStarting = true, bool isSecret = false, List<int> connectedNodeIndexes = null, Tuple<double, double> position2D = null)
    {
        var newDummyStructData = _dummyNodeStructData.Clone() as StructPropertyData;
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

        if (CharacterName == "Monoco")
        {
            return _asset;
        }
        
        var edgesArrayData = (_asset.Exports[0] as NormalExport).Data[1] as ArrayPropertyData;
        edgesArrayData.Value = [];

        List<StructPropertyData> newEdges = new();
        foreach (var edge in Edges)
        {
            var firstPackageIndex = edge.Item1 < 0 ? FPackageIndex.FromRawIndex(edge.Item1) : Nodes[edge.Item1].SkillPackageIndex;
            var secondPackageIndex = edge.Item2 < 0 ? FPackageIndex.FromRawIndex(edge.Item2) : Nodes[edge.Item2].SkillPackageIndex;
            
            var edgeStruct = _dummyEdgeStructData.Clone() as StructPropertyData;
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
            var newDummyStructData = _dummyNodeStructData.Clone() as StructPropertyData;
            newDummyStructData.Value[0] = newDummyStructData.Value[0].Clone() as StructPropertyData;
            newDummyStructData.Value[1] = newDummyStructData.Value[1].Clone() as StructPropertyData;
            Nodes.Add(new SkillNode(nodeRep, newDummyStructData));
            if (CharacterName == "Gustave" && StartingSkillNames["Verso"].Contains(Nodes[^1].OriginalSkillCodeName))
            {
                Nodes[^1].IsUnlockedByDefault = false;
            }
        }

        if (CharacterName == "Monoco")
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