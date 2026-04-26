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
        var result = new List<int>(UnconditionalConnections);
        if (PortalConnection != -1)
            result.Add(PortalConnection);
        foreach (var (key, connection) in ConditionalConnections)
        {
            if (unlockedKeys.Contains(key)) result.AddRange(connection);
        }
        return result;
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
    
    public bool GetPath(int startNode, int endNode, List<string> currentKeys,
        HashSet<(int, string)> visited, List<int> path, out List<int> randomPath, out List<string> collectedKeys)
    {
        path.Add(startNode);
        randomPath = new List<int>(path);
        if (Nodes[startNode].Keys.Any(k => !currentKeys.Contains(k)))
            currentKeys.AddRange(Nodes[startNode].Keys);
        collectedKeys = new List<string>(currentKeys);

        if (startNode == endNode)
        {
            return true;
        }
        
        var stateKey = (startNode, string.Join(',', currentKeys.OrderBy(k => k)));
        if (!visited.Add(stateKey))
        {
            randomPath = new List<int>(path);
            return false;
        }
        
        var backupPath = new List<int>(path);
        var backupKeys = new List<string>(currentKeys);

        var connections = Nodes[startNode].GetConnections(currentKeys);
        for (int i = connections.Count - 1; i >= 0; i--)
        {
            if (GetPath(connections[i], endNode, currentKeys, visited, path, out randomPath, out collectedKeys))
                return true;

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
        
        var visited = new HashSet<(int, string)>();
        var keys = new List<string>();
        
        for (int i = 0; i < constraints.Count - 1; i++)
        {
            int currentConstraint = constraints[i].ID;
            int nextConstraint = constraints[i + 1].ID;
            var intermediatePath = new List<int>();
            // TODO: Investigate if this causes errors and maybe come up with a different method for random path construction
            if (!GetPath(currentConstraint, nextConstraint, keys, visited, intermediatePath, out var randomPath, out var collectedKeys))
            {
                List<int> suitablePoints = [];
                for (int j = 0; j < randomPath.Count; j++)
                {
                    if (!currentPath.Contains(randomPath[j]) && Nodes[randomPath[j]].PortalConnection != -1)
                    {
                        suitablePoints.Add(j);
                    }
                }
                if (suitablePoints.Count == 0)
                {
                    throw new Exception("Location randomization failed, aborting randomizer. Please change the seed or alter the settings if the error persists.");
                }
                var point = Utils.Pick(suitablePoints);
                for (int j = 0; j < point; j++)
                {
                    currentPath.Add(randomPath[j]);
                    keys.AddRange(Nodes[randomPath[j]].Keys);
                }
                keys = keys.Distinct().ToList();
                currentPath.AddRange(randomPath[..(point + 1)]);
                destinationChanges[Nodes[Nodes[randomPath[point]].PortalConnection].CodeName] =
                    Nodes[nextConstraint].CodeName;
                Nodes[randomPath[point]].PortalConnection = nextConstraint;
            }
            else
            {
                keys.AddRange(collectedKeys);
                keys = keys.Distinct().ToList();
                currentPath.AddRange(randomPath);
            }
        }
        return destinationChanges;
    }
}