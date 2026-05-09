using E33Randomizer;

namespace Tests;

public class LocationNode
{
    public string CodeName;
    public List<LocationNode> UnconditionalConnections = new();
    public Dictionary<string, List<LocationNode>> ConditionalConnections = new();
    public List<string> Keys = new();
    public bool BFSVisited = false;

    public LocationNode(LocationData data)
    {
        CodeName = data.CodeName;
        Keys = data.Keys;
    }

    public override string ToString()
    {
        return CodeName;
    }
}