using UAssetAPI;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer;

public class Node
{
    private StructPropertyData _structData;
    public string SkillCodeName;
    public int UnlockCost;
    public bool IsStarting;
    public string RequiredItem;
    public bool IsSecret;
    public Tuple<double, double> Position2D;
    
    public Node(StructPropertyData structData){
        _structData = structData;
    }

    public StructPropertyData ToStruct()
    {
        return _structData;
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
    }

    public UAsset ToAsset()
    {
        return _asset;
    }
}