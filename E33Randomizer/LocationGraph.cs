using System.Windows.Controls.Primitives;

namespace E33Randomizer;

public class PathTrace
{
    public int Node { get; set; }
    public PathTrace Parent { get; set; }

    public List<int> ToList()
    {
        var path = new List<int>();
        var current = this;
        while (current != null)
        {
            path.Add(current.Node);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}

public class LocationNode
{
    public int ID;
    public string CodeName;
    public List<int> UnconditionalConnections;
    public Dictionary<string, List<int>> ConditionalConnections;
    public int OriginalPortalConnection = -1;
    public int PortalConnection = -1;

    public int Depth = Int32.MaxValue;

    //TODO: Rework to bitset
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
        OriginalPortalConnection =
            locationData.PortalConnection != "" ? nodeIndexes[locationData.PortalConnection] : -1;
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

    public Dictionary<string, string> ConstructGoldenPath(List<string> constraintStrings, out List<LocationData> criticalPath)
    {
        criticalPath = new();
        var constraints = constraintStrings.Select(c => Nodes[nodeIndexes[c]]).ToList();
        var destinationChanges = new Dictionary<string, string>();
        var currentPath = new List<int>();

        int startNode = nodeIndexes[constraints[0].CodeName];

        var queue = new Queue<(int Node, string Keys, int Distance, PathTrace Trace)>();
        var visited = new HashSet<(int, string)>();
        var visitedOrder = new Stack<(int, string, PathTrace)>();

        string initialKeys = "";
        var initialTrace = new PathTrace { Node = startNode, Parent = null };

        queue.Enqueue((startNode, initialKeys, 0, initialTrace));
        visited.Add((startNode, initialKeys));

        while (constraints.Count > 0)
        {
            while (constraints.Count > 0 && queue.Count > 0)
            {
                var (currentNode, currentKeysStr, distance, trace) = queue.Dequeue();
                var node = Nodes[currentNode];

                if (node.CodeName == constraints[0].CodeName)
                {
                    constraints.RemoveAt(0);

                    currentPath = trace.ToList();

                    if (constraints.Count == 0) break;

                    queue.Clear();
                    visited.Clear();
                    visitedOrder.Clear();

                    queue.Enqueue((currentNode, currentKeysStr, distance, trace));
                    visited.Add((currentNode, currentKeysStr));
                    continue;
                }

                node.Depth = Math.Min(node.Depth, distance);

                List<string> currentKeysList = string.IsNullOrEmpty(currentKeysStr)
                    ? new List<string>()
                    : currentKeysStr.Split(',').ToList();

                bool pickedUpNewKey = false;
                foreach (var key in node.Keys)
                {
                    if (!currentKeysList.Contains(key))
                    {
                        currentKeysList.Add(key);
                        pickedUpNewKey = true;
                    }
                }

                string nextKeysStr = currentKeysStr;
                if (pickedUpNewKey)
                {
                    currentKeysList.Sort();
                    nextKeysStr = string.Join(',', currentKeysList);
                }

                var connections = node.GetConnections(currentKeysList);

                foreach (int nextNode in connections)
                {
                    // Enforce strict order
                    if (nextNode != constraints[0].ID && constraints.Any(c => c.ID == nextNode))
                    {
                        continue;
                    }

                    var nextState = (nextNode, nextKeysStr);
                    if (!visited.Add(nextState)) continue;

                    var nextTrace = new PathTrace { Node = nextNode, Parent = trace };
                    visitedOrder.Push((nextNode, nextKeysStr, nextTrace));

                    queue.Enqueue((nextNode, nextKeysStr, distance + 1, nextTrace));
                }
            }

            if (constraints.Count == 0) break;

            while (visitedOrder.Count > 0)
            {
                var (i, keysStr, trace) = visitedOrder.Pop();
                visited.Remove((i, keysStr));
                Nodes[i].Depth = Int16.MaxValue;

                if (currentPath.Contains(i) || Nodes[i].PortalConnection == -1) continue;

                destinationChanges[Nodes[Nodes[i].PortalConnection].CodeName] = constraints[0].CodeName;
                Nodes[i].PortalConnection = constraints[0].ID;
                var nextTrace = new PathTrace { Node = constraints[0].ID, Parent = trace };
                queue.Enqueue((constraints[0].ID, keysStr, Nodes[i].Depth + 1, nextTrace));
                break;
            }

            if (visitedOrder.Count == 0)
            {
                throw new Exception(
                    "Could not construct critical path, aborting randomization. Please change generation settings if this problem persists.");
            }
        }

        criticalPath = currentPath.Select(i => Controllers.LocationController.GetObject(Nodes[i].CodeName)).ToList();
        return destinationChanges;
    }
}