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

        Utils.ShuffleList(result);
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
        HashSet<(int, string)> visited, List<int> path, out List<int> newPath, out List<string> collectedKeys)
    {
        if (path.Count > 400)
        {
            collectedKeys = new List<string>(currentKeys);
            newPath = new List<int>();
            return false;
        }
        
        path.Add(startNode);
        if (Nodes[startNode].Keys.Any(k => !currentKeys.Contains(k)))
            currentKeys.AddRange(Nodes[startNode].Keys);
        collectedKeys = new List<string>(currentKeys);

        if (startNode == endNode)
        {
            newPath =  new List<int>(path);
            return true;
        }
        
        var stateKey = (startNode, string.Join(',', currentKeys.OrderBy(k => k)));
        
        if (!visited.Add(stateKey))
        {
            newPath = new ();
            return false;
        }
        
        var backupPath = new List<int>(path);
        var backupKeys = new List<string>(currentKeys);

        var connections = Nodes[startNode].GetConnections(currentKeys);
        for (int i = connections.Count - 1; i >= 0; i--)
        {
            if (GetPath(connections[i], endNode, currentKeys, visited, path, out newPath, out collectedKeys))
                return true;

            path = new List<int>(backupPath);
            currentKeys = new List<string>(backupKeys);
        }
        newPath = new ();
        return false;
    }

    public List<int> GetRandomPath(int startingPoint, List<int> currentPath, List<string> currentKeys, out List<string> newKeys)
    {
        var toVisitQueue = new Queue<int>();
        toVisitQueue.Enqueue(startingPoint);
        List<int> suitablePoints = new();
        List<int> visited = new();
        newKeys = new List<string>();
        Dictionary<int, int> parents = new();
        while (toVisitQueue.Count > 0)
        {
            var currentPoint = toVisitQueue.Dequeue();
            
            if (Nodes[currentPoint].PortalConnection != -1 && !currentPath.Contains(currentPoint)) suitablePoints.Add(currentPoint);
            
            var connections = Nodes[currentPoint].GetConnections(currentKeys);
            connections = connections.Where(c => !visited.Contains(c)).ToList();
            foreach (var connection in connections)
            {
                if (toVisitQueue.Contains(connection) || visited.Contains(connection)) continue;
                parents[connection] = currentPoint;
                toVisitQueue.Enqueue(connection);
            }
            visited.Add(currentPoint);
        }
        if (suitablePoints.Count == 0)
        {
            return null;
        }
        var point = Utils.Pick(suitablePoints);

        List<int> path = [point];
        newKeys.AddRange(Nodes[point].Keys);
        while (point != startingPoint)
        {
            point = parents[point];
            path.Add(point);
            newKeys.AddRange(Nodes[point].Keys);
        }
        
        newKeys = newKeys.Distinct().ToList();
        path.Reverse();
        return path;
    }

    public List<int> CleanUpPath(List<int> currentPath)
    {
        HashSet<(int, string)> visited = new();
        List<int> pathWithoutRepetition = [];

        List<string> currentKeys = [];
        
        foreach (var i in currentPath)
        {
            var stateKey = (i, string.Join(',', currentKeys.OrderBy(k => k)));

            if (visited.Add(stateKey))
            {
                
                pathWithoutRepetition.Add(i);
                currentKeys.AddRange(Nodes[i].Keys);
                currentKeys = currentKeys.Distinct().ToList();
            }
        }
        
        List<int> pathWithoutUnnecessarySteps = [pathWithoutRepetition[0]];
        for (int i = 1; i < pathWithoutRepetition.Count - 1; i++)
        {
            if (
                Nodes[pathWithoutRepetition[i]].UnconditionalConnections.Contains(pathWithoutUnnecessarySteps[^1]) &&
                Nodes[pathWithoutRepetition[i + 1]].UnconditionalConnections.Contains(pathWithoutUnnecessarySteps[^1])
                )
                continue;
            pathWithoutUnnecessarySteps.Add(pathWithoutRepetition[i]);
        }
        pathWithoutUnnecessarySteps.Add(pathWithoutRepetition[^1]);
        return pathWithoutUnnecessarySteps;
    }
    
    public Dictionary<string, string> ConstructGoldenPath(List<string> constraintStrings, out List<LocationData> criticalPath)
    {
        var constraints = constraintStrings.Select(c => Nodes[nodeIndexes[c]]).ToList();
        
        var destinationChanges = new Dictionary<string, string>();
        
        List<int> currentPath = [];
        
        var visited = new HashSet<(int, string)>();
        var keys = new List<string>();

        var failedAttempts = 0;
        
        for (int i = 0; i < constraints.Count - 1; i++)
        {
            int currentConstraint = constraints[i].ID;
            int nextConstraint = constraints[i + 1].ID;
            var intermediatePath = new List<int>();
            // TODO: Investigate if this causes errors and maybe come up with a different method for random path construction
            if (!GetPath(currentConstraint, nextConstraint, keys, visited, intermediatePath, out var newPath, out var collectedKeys))
            {
                var randomPath = GetRandomPath(currentConstraint, currentPath, collectedKeys, out var newKeys);

                if (randomPath == null)
                {
                    i--;
                    failedAttempts++;
                    if (failedAttempts > 10)
                    {
                        throw new Exception("Could not critical path, aborting randomization. Please change generation settings if this problem persists.");
                    }
                    continue;
                }
                
                currentPath.AddRange(randomPath);
                keys.AddRange(newKeys);
                keys = keys.Distinct().ToList();
                destinationChanges[Nodes[Nodes[randomPath[^1]].PortalConnection].CodeName] =
                    Nodes[nextConstraint].CodeName;
                Nodes[randomPath[^1]].PortalConnection = nextConstraint;
            }
            else
            {
                keys.AddRange(collectedKeys);
                keys = keys.Distinct().ToList();
                currentPath.AddRange(newPath);
            }
            visited.Clear();
        }
        
        var cleanPath = CleanUpPath(currentPath);
        criticalPath = cleanPath.Select(i => Controllers.LocationController.GetObject(Nodes[i].CodeName)).ToList();

        return destinationChanges;
    }
}