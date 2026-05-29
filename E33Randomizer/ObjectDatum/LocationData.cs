namespace E33Randomizer.ObjectDatum;

public class LocationData: ObjectData
{
    public string LevelAsset { get; set; }
    public int LevelScaling { get; set; }
    public List<string> UnconditionalConnections { get; set; }
    public Dictionary<string, List<string>> ConditionalConnections { get; set; }
    public string PortalConnection { get; set; }
    public List<string> Keys { get; set; }
    
    public List<string> AllConditionalConnections
    {
        get { return ConditionalConnections.SelectMany(kv => kv.Value).ToList(); }
    }
}