namespace E33Randomizer;

public class LocationData: ObjectData
{
    public string LevelAsset;
    public int LevelScaling;
    public List<string> UnconditionalConnections;
    public Dictionary<string, List<string>> ConditionalConnections;
    public string PortalConnection;
    public List<string> Keys;

    public List<string> AllConditionalConnections
    {
        get { return ConditionalConnections.SelectMany(kv => kv.Value).ToList(); }
    }
}