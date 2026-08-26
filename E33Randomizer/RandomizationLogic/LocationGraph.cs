using AvaloniaEdit.Utils;
using E33Randomizer.ObjectDatum;

namespace E33Randomizer.RandomizationLogic;

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

public class ConditionalConnection
{
    public UInt128 RequiredKeyBit;
    public bool IsEsquieAbility;
    public List<int> Connections;
}

public class LocationNodeInternal
{
    public int ID;
    public string CodeName;
    public List<int> UnconditionalConnections;
    public List<ConditionalConnection> ConditionalConnections;
    public int OriginalPortalConnection = -1;
    public int PortalConnection = -1;

    public int Depth = Int16.MaxValue;
    public int DepthIncrement = 1;

    public UInt128 KeysMask;

    public List<int> GetConnections(UInt128 unlockedKeys, UInt128 esquieCarBit, bool includeTeleport=true)
    {
        var result = new List<int>(UnconditionalConnections);
        if (includeTeleport && PortalConnection != -1)
            result.Add(PortalConnection);
        foreach (var cond in ConditionalConnections)
        {
            if (cond.IsEsquieAbility && (unlockedKeys & esquieCarBit) == 0) continue;
            if ((unlockedKeys & cond.RequiredKeyBit) != 0) result.AddRange(cond.Connections);
        }

        Utils.ShuffleList(result);
        return result;
    }

    public LocationNodeInternal(LocationData locationData, Dictionary<string, int> nodeIndexes, Dictionary<string, int> keyIndexes)
    {
        CodeName = locationData.CodeName;
        ID = nodeIndexes[CodeName];
        UnconditionalConnections = locationData.UnconditionalConnections.Select(c => nodeIndexes[c]).ToList();
        ConditionalConnections = locationData.ConditionalConnections
            .Select(kvp => new ConditionalConnection
            {
                RequiredKeyBit = (UInt128)1 << keyIndexes[kvp.Key],
                IsEsquieAbility = kvp.Key.Contains("EsquieAbility"),
                Connections = kvp.Value.Select(c => nodeIndexes[c]).ToList()
            }).ToList();
        PortalConnection = locationData.PortalConnection != "" ? nodeIndexes[locationData.PortalConnection] : -1;
        OriginalPortalConnection =
            locationData.PortalConnection != "" ? nodeIndexes[locationData.PortalConnection] : -1;
        KeysMask = locationData.Keys.Aggregate((UInt128)0, (mask, key) => mask | ((UInt128)1 << keyIndexes[key]));
        DepthIncrement = CodeName.Contains("Special") || CodeName.Contains("World") ? 0 : 1;
        DepthIncrement = locationData.LevelScaling == 1 ? 0 : DepthIncrement;
    }

    public override string ToString()
    {
        return $"{ID}: {CodeName}";
    }
}

public class LocationGraph
{
    private UInt128 MASTER_KEY = UInt128.MaxValue;
    public List<LocationNodeInternal> Nodes = [];
    public Dictionary<int, List<int>> ConstraintPortals = new();
    public Dictionary<string, int> nodeIndexes = new();
    public Dictionary<string, int> keyIndexes = new();
    public Dictionary<int, List<int>> ConnectedPortals = new();
    public HashSet<int> UnclaimedPortals = new();
    public Dictionary<int, List<int>> PortalDestinationPools = new();
    private HashSet<int> NotRandomizedDestinations = new();
    public List<List<int>> PortalGroups = [];

    public UInt128 EsquieCarBit;

    public void Init()
    {
        if (Nodes.Count > 0) return;

        var nodeData = Controllers.LocationController.ObjectsData;
        nodeIndexes = nodeData.Select((n, i) => new KeyValuePair<string, int>(n.CodeName, i)).ToDictionary();

        keyIndexes = new Dictionary<string, int>();
        foreach (var n in nodeData)
        {
            foreach (var key in n.Keys)
                if (!keyIndexes.ContainsKey(key)) keyIndexes[key] = keyIndexes.Count;
            foreach (var key in n.ConditionalConnections.Keys)
                if (!keyIndexes.ContainsKey(key)) keyIndexes[key] = keyIndexes.Count;
        }

        if (keyIndexes.Count > 128)
            throw new InvalidOperationException(ResourceHelper.GetString(nameof(Assets.Resources.LocationController_TooManyKeys_Exception)));

        Nodes = nodeData.Select(n => new LocationNodeInternal(n, nodeIndexes, keyIndexes)).ToList();
        
        EsquieCarBit = keyIndexes.TryGetValue("EsquieAbility.Car", out var carIdx)
            ? (UInt128)1 << carIdx
            : (UInt128)0;

        var allNodes = Nodes.Select(n => n.ID).ToHashSet();
        
        foreach (var node in Nodes)
        {
            var reachableStates = GetReachableStates((node.ID, 0), allNodes);
            ConnectedPortals[node.ID] = reachableStates.Select(s => s.Node).Where(IsPortal).ToList();
            if (!PortalGroups.Any(pg => pg.Contains(node.ID)))
            {
                var newGroup = new List<int>(ConnectedPortals[node.ID]);
                newGroup.Add(node.ID);
                PortalGroups.Add(newGroup);
            }
        }
    }

    public LocationNodeInternal GetNode(string codeName)
    {
        return Nodes[nodeIndexes[codeName]];
    }

    public void Reset()
    {
        foreach (var node in Nodes)
        {
            node.PortalConnection = node.OriginalPortalConnection;
            node.Depth = Int16.MaxValue;
        }
    }

    public void ApplyDestinationChanges(Dictionary<string, string> destinationChanges)
    {
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

    public HashSet<(int Node, UInt128 Keys)> GetReachableStates((int Node, UInt128 Keys) startingState, HashSet<int> bannedPortals=null)
    {
        bannedPortals ??= new HashSet<int>();
        var queue = new Queue<(int Node, UInt128 Keys)>();
        var visited = new Dictionary<int, List<UInt128>>();
        
        queue.Enqueue(startingState);
        visited[startingState.Node] = [startingState.Keys];
        
        while (queue.Count > 0)
        {
            var (currentNode, currentKeysMask) = queue.Dequeue();
            var node = Nodes[currentNode];

            var nextKeysMask = currentKeysMask | node.KeysMask;

            var connections = node.GetConnections(nextKeysMask, EsquieCarBit, !bannedPortals.Contains(node.ID));

            foreach (int nextNode in connections)
            {
                var nextState = (nextNode, nextKeysMask);
                if (!visited.ContainsKey(nextNode))
                {
                    visited[nextNode] = [nextKeysMask];
                    queue.Enqueue(nextState);
                    continue;
                }

                var alreadyVisited = false;
                
                foreach (var keyset in visited[nextNode])
                {
                    if ((keyset | nextKeysMask) == keyset)
                    {
                        alreadyVisited = true;
                        break;
                    }
                }

                if (!alreadyVisited)
                {
                    visited[nextNode] = visited[nextNode].Where(k => (k | nextKeysMask) != nextKeysMask).ToList();
                    visited[nextNode].Add(nextKeysMask);
                    queue.Enqueue(nextState);
                }
            }
        }
        var result = new HashSet<(int, UInt128)>();
        foreach (var (node, keys) in visited)
        {
            result.AddRange(keys.Select(k => (node, k)));
        }
        return result;
    }

    public IEnumerable<int> GetNodesFromStates(IEnumerable<(int Node, UInt128)> states)
    {
        return states.Select(s => s.Node).Distinct();
    }
    
    public List<int> GetPortalsFromStates(IEnumerable<(int Node, UInt128)> states)
    {
        return states.Select(s => s.Node).Distinct().Where(n => Nodes[n].OriginalPortalConnection != -1 && Nodes[Nodes[n].OriginalPortalConnection].OriginalPortalConnection == Nodes[n].ID).ToList();
    }
    
    public bool CheckPortalMutation(int portalIndex, int lastReachableConstraint, List<int> constraintNodes)
    {
        var oldVal = Nodes[portalIndex].PortalConnection;
        Nodes[portalIndex].PortalConnection = constraintNodes[lastReachableConstraint + 1];
        
        var success = true;
        (int, UInt128) currentConstraintState = (constraintNodes[0], 0);

        for (int i = 0; i < lastReachableConstraint + 1; i++)
        {
            var reachableStates = GetReachableStates(currentConstraintState);
            if (GetNodesFromStates(reachableStates).Contains(constraintNodes[i + 1]))
            {
                currentConstraintState = reachableStates.First(s => s.Node == constraintNodes[i + 1]);
                continue;
            }
            success = false;
            break;
        }

        if (success) return true;

        Nodes[portalIndex].PortalConnection = oldVal;

        return false;
    }

    public List<LocationData> GetPathAndDepths(int startNode, int endNode)
    {
        var currentPath = new List<int>();

        var queue = new Queue<(int Node, UInt128 Keys, int Distance, PathTrace Trace)>();
        var visited = new HashSet<(int, UInt128)>();

        UInt128 initialKeys = 0;
        var initialTrace = new PathTrace { Node = startNode, Parent = null };


        queue.Enqueue((startNode, initialKeys, 1, initialTrace));
        visited.Add((startNode, initialKeys));

        while (queue.Count > 0)
        {
            var (currentNode, currentKeysMask, distance, trace) = queue.Dequeue();
            var node = Nodes[currentNode];

            node.Depth = Math.Min(node.Depth, distance);

            if (currentNode == endNode)
            {
                currentPath = trace.ToList();
            }

            var nextKeysMask = currentKeysMask | node.KeysMask;

            var connections = node.GetConnections(nextKeysMask, EsquieCarBit);

            foreach (int nextNode in connections)
            {
                var nextState = (nextNode, nextKeysMask);
                if (!visited.Add(nextState)) continue;

                var nextTrace = new PathTrace { Node = nextNode, Parent = trace };

                queue.Enqueue((nextNode, nextKeysMask, distance + node.DepthIncrement, nextTrace));
            }
        }

        var criticalPath = currentPath.Select(i => Controllers.LocationController.GetObject(Nodes[i].CodeName)).ToList();
        return criticalPath;
    }
    
    public bool ConstructGoldenPathMinimal(List<string> constraintStrings, out List<LocationData> criticalPath,
        out Dictionary<string, string> destinationChanges)
    {
        criticalPath = new();
        var constraintNodes = constraintStrings.Select(c => nodeIndexes[c]).ToList();
        var constraints = constraintNodes.Select(c => Nodes[c]).ToList();
        
        ConstraintPortals.Clear();
        foreach (var c in constraints)
        {
            ConstraintPortals[c.ID] = c.GetConnections(MASTER_KEY, EsquieCarBit, false).Where(IsPortal).ToList();
        }
        
        destinationChanges = new Dictionary<string, string>();
        int startNode = constraintNodes[0];

        UInt128 initialKeys = 0;


        int currentConstraint = 0;
        int nextConstraint = 1;
        (int, UInt128) currentConstraintState = (startNode, initialKeys);
        Stack<(int Node, UInt128 Keys)> constraintStates = [];
        constraintStates.Push(currentConstraintState);
        List<(List<int> Portals, int lastChecked)> portalsReachableFromState = [];
        var iterations = 0;
        while (nextConstraint < constraints.Count && iterations < 500)
        {
            iterations++;
            var reachableStates = GetReachableStates(currentConstraintState);
            portalsReachableFromState.Add((Utils.ShuffleList(GetPortalsFromStates(reachableStates)), 0));
            if (reachableStates.Any(s => s.Node == constraintNodes[nextConstraint]))
            {
                currentConstraintState = reachableStates.First(s => s.Node == constraintNodes[nextConstraint]);
                constraintStates.Push(currentConstraintState);
                currentConstraint++;
                nextConstraint++;
                continue;
            }

            if (nextConstraint == constraints.Count) break;

            var portalsReachableFromConstraint = portalsReachableFromState[currentConstraint];
            var lastChecked = portalsReachableFromConstraint.lastChecked;
            var portals = portalsReachableFromConstraint.Portals[lastChecked..];
            var foundPortal = false;
            foreach (var portal in portals)
            {
                lastChecked++;
                if (CheckPortalMutation(portal, currentConstraint, constraintNodes))
                {
                    foundPortal = true;
                    break;
                }
            }
            
            portalsReachableFromState[currentConstraint] = (portalsReachableFromConstraint.Portals, lastChecked);
            
            if (foundPortal)
            {
                continue;
            }

            if (currentConstraint == -1) return false;
            
            currentConstraintState = constraintStates.Pop();
            portalsReachableFromState = portalsReachableFromState[..currentConstraint];
            currentConstraint--;
            nextConstraint--;
        }

        if (nextConstraint < constraints.Count) return false;
        
        foreach (var node in Nodes)
        {
            if (node.PortalConnection == -1) continue;
            destinationChanges[Nodes[node.OriginalPortalConnection].CodeName] = Nodes[node.PortalConnection].CodeName;
        }

        criticalPath = GetPathAndDepths(constraintNodes[0], constraintNodes[^1]);
        return true;
    }

    public bool IsPortal(int node)
    {
        return Nodes[node].OriginalPortalConnection != -1 && Nodes[Nodes[node].OriginalPortalConnection].OriginalPortalConnection == node;
    }

    public bool IsPortal(LocationNodeInternal node)
    {
        return node.OriginalPortalConnection != -1 && Nodes[node.OriginalPortalConnection].OriginalPortalConnection == node.ID;
    }

    public List<int> SortPortalsByPreference(int startingPortal)
    {
        return Utils.ShuffleList(UnclaimedPortals).OrderBy(p => NotRandomizedDestinations.Contains(p) && p == Nodes[startingPortal].OriginalPortalConnection ? -1 :
            PortalDestinationPools[startingPortal].Contains(p) ? 0 : 1).ToList();
    }

    public List<(int Node, UInt128 Keys)> GetReachableUnclaimedPortals((int Node, UInt128 Keys) startingState)
    {
        return GetReachableStates(startingState, UnclaimedPortals).
            Where(s => IsPortal(s.Node)).Where(s => UnclaimedPortals.Contains(s.Node)).ToList();
    }

    public void CheckForParity()
    {
        var portals = Nodes.Where(IsPortal).ToList();
        var nonPairedPortals = portals.Where(p => Nodes[p.PortalConnection].PortalConnection != p.ID).ToList();
        if (nonPairedPortals.Any())
        {
            Console.WriteLine($"Parity broken by {nonPairedPortals.Count} portals");
        }
    }
    
    public bool RemakeWholeGraph(List<string> constraintStrings, out List<LocationData> criticalPath,
        out Dictionary<string, string> destinationChanges)
    {
        var clp = RandomizerLogic.CustomLocationPlacement.GetCategory;
        NotRandomizedDestinations = Nodes.Where(n => RandomizerLogic.CustomLocationPlacement.NotRandomizedCodeNames.Contains(n.CodeName)).Select(n => n.ID).ToHashSet();
        foreach (var node in Nodes)
        {
            node.PortalConnection = node.OriginalPortalConnection;
            if (!IsPortal(node)) continue;
            var nodeDestinationCategory = clp(Nodes[node.OriginalPortalConnection].CodeName);
            PortalDestinationPools[node.ID] = Nodes.Where(n =>
                IsPortal(n) && clp(n.CodeName) == nodeDestinationCategory && n != node).Select(n => n.ID).ToList();
        }
        criticalPath = [];
        destinationChanges = new();
        UnclaimedPortals = Nodes.Where(n => n.OriginalPortalConnection != -1 && Nodes[n.OriginalPortalConnection].OriginalPortalConnection == n.ID).Select(n => n.ID).ToHashSet();
        var constraintNodes = constraintStrings.Select(c => nodeIndexes[c]).ToList();
        int startNode = constraintNodes[0];

        UInt128 initialKeys = 0;

        (int, UInt128) startingState = (startNode, initialKeys);

        List<(int Node, UInt128 Keys)> reachableUnclaimedPortals = [];

        var iterationsLeft = UnclaimedPortals.Count;
        CheckForParity();
        
        while (UnclaimedPortals.Count > 0 && iterationsLeft > 0)
        {
            iterationsLeft--;
            reachableUnclaimedPortals = GetReachableStates(startingState, UnclaimedPortals).
                Where(s => IsPortal(s.Node)).Where(s => UnclaimedPortals.Contains(s.Node)).ToList();
            Console.WriteLine($"Reachable portals: {reachableUnclaimedPortals.Count}");
            var foundValidPortal = false;
            foreach (var candidatePortalState in reachableUnclaimedPortals)
            {
                Console.WriteLine($"Candidate: {Nodes[candidatePortalState.Node].CodeName}");
                UnclaimedPortals.Remove(candidatePortalState.Node);
                var preferredPortals = SortPortalsByPreference(candidatePortalState.Node);
                foreach (var portal in preferredPortals)
                {
                    if (RandomizerLogic.Settings.EnableTwoWayTeleport && ConnectedPortals[portal].Any(p => !UnclaimedPortals.Contains(p)))
                    {
                        Console.WriteLine($"Skipping {Nodes[portal].CodeName} due to reverse binding");
                        continue;
                    }
                    
                    Nodes[candidatePortalState.Node].PortalConnection = portal;
                    if (RandomizerLogic.Settings.EnableTwoWayTeleport)
                    {
                        Nodes[Nodes[candidatePortalState.Node].PortalConnection].PortalConnection = candidatePortalState.Node;
                        UnclaimedPortals.Remove(Nodes[candidatePortalState.Node].PortalConnection);
                    }
                    
                    Console.WriteLine($"Trying to connect to: {Nodes[portal].CodeName}...");

                    if (UnclaimedPortals.Count == 0) break;
                    
                    var newReachablePortals = GetReachableUnclaimedPortals(startingState);
                    if (newReachablePortals.Count > 0)
                    {
                        Console.WriteLine($"Success! Reachable portals: {newReachablePortals.Count}");
                        foundValidPortal = true;
                        break;
                    }
                    Console.WriteLine("Failure!");
                    if (RandomizerLogic.Settings.EnableTwoWayTeleport)
                    {
                        Nodes[Nodes[candidatePortalState.Node].PortalConnection].PortalConnection = Nodes[Nodes[candidatePortalState.Node].PortalConnection].OriginalPortalConnection;
                        UnclaimedPortals.Add(Nodes[candidatePortalState.Node].PortalConnection);
                    }
                    Nodes[candidatePortalState.Node].PortalConnection = Nodes[candidatePortalState.Node].OriginalPortalConnection;
                }

                if (foundValidPortal || UnclaimedPortals.Count == 0)
                {
                    break;
                }
                UnclaimedPortals.Add(candidatePortalState.Node);
            }

            if (!foundValidPortal) break;
        }
        CheckForParity();

        
        if (UnclaimedPortals.Any()) return false;

        foreach (var node in Nodes.Where(IsPortal))
        {
            destinationChanges[Nodes[node.OriginalPortalConnection].CodeName] = Nodes[node.PortalConnection].CodeName;
        }
        
        criticalPath = GetPathAndDepths(constraintNodes[0], constraintNodes[^1]);
        return true;
    }
}