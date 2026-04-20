using System.Windows.Controls.Primitives;

namespace E33Randomizer;

public class LocationNode
{
    public int ID;
    public string CodeName;
    public List<int> UnconditionalConnections;
    public Dictionary<string, List<int>> ConditionalConnections;
    public int OriginalPortalConnection = -1;
    public int PortalConnection = -1;
    public List<string> Keys;

    public List<int> GetConnections(List<string> unlockedKeys)
    {
        var result = UnconditionalConnections;
        if (PortalConnection != -1)
            result.Add(PortalConnection);
        foreach (var (key, connection) in ConditionalConnections)
        {
            if (unlockedKeys.Contains(key)) result.AddRange(connection);
        }
        return Utils.ShuffleList(result);
    }

    public LocationNode(LocationData locationData, Dictionary<string, int> nodeIndexes)
    {
        CodeName = locationData.CodeName;
        ID = nodeIndexes[CodeName];
        UnconditionalConnections = locationData.UnconditionalConnections.Select(c => nodeIndexes[c]).ToList();
        ConditionalConnections = locationData.ConditionalConnections
            .Select(kvp => new KeyValuePair<string, List<int>>(kvp.Key, kvp.Value.Select(c => nodeIndexes[c]).ToList()
                )).ToDictionary();
        PortalConnection = locationData.PortalConnection != "" ? nodeIndexes[locationData.PortalConnection] : -1;
        OriginalPortalConnection = locationData.PortalConnection != "" ? nodeIndexes[locationData.PortalConnection] : -1;
        Keys = new List<string>(locationData.Keys);
    }

    public override string ToString()
    {
        return $"{ID}: {CodeName}";
    }
}

public class LocationGraph
{
    public List<LocationNode> Nodes = [];
    public Dictionary<string, int> nodeIndexes = new();

    public void Init()
    {
        var nodeData = Controllers.LocationController.ObjectsData;
        nodeIndexes = nodeData.Select((n, i) => new KeyValuePair<string, int>(n.CodeName, i)).ToDictionary();
        Nodes = nodeData.Select(n => new LocationNode(n, nodeIndexes)).ToList();
    }

    public void Reset()
    {
        foreach (var node in Nodes)
        {
            node.PortalConnection = node.OriginalPortalConnection;
        }
    }

    public void ApplyDestinationChanges(Dictionary<string, string> destinationChanges)
    {
        Reset();
        foreach (var node in Nodes)
        {
            if (node.PortalConnection == -1) continue;
            var originalDestination = Nodes[node.PortalConnection].CodeName;
            if (destinationChanges.TryGetValue(originalDestination, out var change))
            {
                node.PortalConnection = nodeIndexes[change];
            }
        }
    }
    
    public bool GetPath(int startNode, int endNode, List<string> currentKeys, bool[] visited, List<int> path, out List<int> randomPath)
    {
        path.Add(startNode);
        
        Console.WriteLine(String.Join('-', path.Select(n => Nodes[n])));
        
        visited[startNode] = true;
        randomPath = new List<int>(path);
        if (startNode == endNode)
        {
            return true;
        }

        if (Nodes[startNode].Keys.Count != 0)
        {
            currentKeys.AddRange(Nodes[startNode].Keys);
        }
        
        var backupPath = new List<int>(path);
        var backupKeys = new List<string>(currentKeys);

        foreach (var connection in Nodes[startNode].GetConnections(currentKeys))
        {
            if (visited[connection]) continue;
            if (GetPath(connection, endNode, currentKeys, visited, path, out randomPath))
            {
                return true;
            }
            
            path = new List<int>(backupPath);
            currentKeys = new List<string>(backupKeys);
        }
        return false;
    }
    
    public Dictionary<string, string> ConstructGoldenPath(List<string> constraintStrings)
    {
        var constraints = constraintStrings.Select(c => Nodes[nodeIndexes[c]]).ToList();
        
        var destinationChanges = new Dictionary<string, string>();
        
        List<int> currentPath = [];
        
        var visited = new bool[Nodes.Count];
        var keys = new List<string>();
        
        for (int i = 0; i < constraints.Count - 1; i++)
        {
            int currentConstraint = constraints[i].ID;
            int nextConstraint = constraints[i + 1].ID;
            var intermediatePath = new List<int>();
            if (!GetPath(currentConstraint, nextConstraint, keys, visited, intermediatePath, out var randomPath))
            {
                int randomDepth = RandomizerLogic.rand.Next(randomPath.Count / 2, randomPath.Count);
                int j = randomDepth;
                // Iterate from the end of a random point in the slice back until we can alter the teleport point without changing the golden path
                while (j > 0 && (currentPath.Contains(randomPath[j]) && Nodes[randomPath[j]].PortalConnection == -1))
                {
                    j--;
                }
                if (j == 0)
                {
                    throw new Exception("Location randomization failed, aborting randomizer. Please change the seed or alter the settings if the error persists.");
                }
                currentPath.AddRange(randomPath[..j]);
                destinationChanges[Nodes[Nodes[randomPath[j]].PortalConnection].CodeName] =
                    Nodes[nextConstraint].CodeName;
                Nodes[randomPath[j]].PortalConnection = nextConstraint;
            }
            else
            {
                currentPath.AddRange(intermediatePath);
            }
        }
        return destinationChanges;
    }
}