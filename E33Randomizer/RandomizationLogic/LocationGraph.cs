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
    private Stack<(int From, int To, int PrevFrom, int PrevTo)> ModificationHistory = new();

    public List<LocationNodeInternal> Nodes = [];
    public int TotalReachableNodes = 0;
    public List<LocationNodeInternal> Portals = [];
    public Dictionary<int, List<int>> ConstraintPortals = new();
    public Dictionary<string, int> nodeIndexes = new();
    public Dictionary<string, int> keyIndexes = new();
    public Dictionary<int, List<int>> ConnectedPortals = new();
    public HashSet<int> UnclaimedPortals = new();
    public Dictionary<int, List<int>> PortalDestinationPools = new();
    private HashSet<int> NotRandomizedDestinations = new();
    public List<List<int>> PortalGroups = [];

    public int TotalIterations = 0;

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
        Portals = Nodes.Where(IsPortal).ToList();
        TotalIterations = 0;
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

        TotalIterations = 0;
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

    public bool HasFullReachability(int startNode)
    {
        return GetReachableStates((startNode, 0)).Select(s => s.Node).Distinct().Count() == TotalReachableNodes;
    }
    
    public bool FixUnclaimedLeaves(int startNode)
    {
        if (!RandomizerLogic.Settings.EnableTwoWayTeleport) return false;
        
        var numberOfReachableNodes = GetReachableStates((startNode, 0), UnclaimedPortals).Select(s => s.Node).Distinct().Count();
        
        foreach (var portalNode in Portals)
        {
            if (ConnectedPortals[portalNode.ID].Count < 2) continue;
            var firstUnclaimed = UnclaimedPortals.First();
            var secondUnclaimed = UnclaimedPortals.Last();
            for (int i = 0; i < ConnectedPortals[portalNode.ID].Count; i++)
            {
                var claimed = ConnectedPortals[portalNode.ID][i];
                if (UnclaimedPortals.Contains(claimed)) continue;
            
                var originalClaimedDestination = Nodes[claimed].PortalConnection;
            
                Nodes[Nodes[claimed].PortalConnection].PortalConnection = firstUnclaimed;
                Nodes[firstUnclaimed].PortalConnection = Nodes[claimed].PortalConnection;
            
                Nodes[claimed].PortalConnection = secondUnclaimed;
                Nodes[secondUnclaimed].PortalConnection = claimed;
                var newNumberOfReachableNodes = GetReachableStates((startNode, 0), UnclaimedPortals).Select(s => s.Node).Distinct().Count();
                if (newNumberOfReachableNodes < numberOfReachableNodes + 2 || (UnclaimedPortals.Count == 2 && newNumberOfReachableNodes < TotalReachableNodes))
                {
                    Nodes[originalClaimedDestination].PortalConnection = claimed;
                    Nodes[claimed].PortalConnection = originalClaimedDestination;
                    Nodes[firstUnclaimed].PortalConnection = Nodes[firstUnclaimed].OriginalPortalConnection;
                    Nodes[secondUnclaimed].PortalConnection = Nodes[secondUnclaimed].OriginalPortalConnection;
                    continue;
                }
                numberOfReachableNodes = newNumberOfReachableNodes;
                UnclaimedPortals.Remove(firstUnclaimed);
                UnclaimedPortals.Remove(secondUnclaimed);
                if (UnclaimedPortals.Count == 0) break;
                
                firstUnclaimed = UnclaimedPortals.First();
                secondUnclaimed = UnclaimedPortals.Last();
            }
            if (UnclaimedPortals.Count == 0) break;
        }

        var reachableNodes = GetReachableStates((startNode, 0), UnclaimedPortals).Select(s => s.Node).Distinct();
        
        var unreachableNodes = Nodes.Where(n => !reachableNodes.Contains(n.ID)).ToList();
        
        return HasFullReachability(startNode) && UnclaimedPortals.Count == 0;
    }
    
    private void ApplyPortalLink(int fromPortal, int toPortal)
    {
        var prevFrom = Nodes[fromPortal].PortalConnection;
        var prevTo = Nodes[toPortal].PortalConnection;

        Nodes[fromPortal].PortalConnection = toPortal;
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
            Nodes[toPortal].PortalConnection = fromPortal;

        ModificationHistory.Push((fromPortal, toPortal, prevFrom, prevTo));
        UnclaimedPortals.Remove(fromPortal);
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
            UnclaimedPortals.Remove(toPortal);
    }

    private void UndoPortalLink()
    {
        var (from, to, prevFrom, prevTo) = ModificationHistory.Pop();
        Nodes[from].PortalConnection = prevFrom;
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
            Nodes[to].PortalConnection = prevTo;
        UnclaimedPortals.Add(from);
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
            UnclaimedPortals.Add(to);
    }
    
    private static Dictionary<int, List<UInt128>> CloneVisited(Dictionary<int, List<UInt128>> visited)
    {
        var clone = new Dictionary<int, List<UInt128>>(visited.Count);
        foreach (var (nodeId, keysList) in visited)
        {
            clone[nodeId] = new List<UInt128>(keysList);
        }
        return clone;
    }

    private void TryEnqueue(
        Dictionary<int, List<UInt128>> visited,
        Queue<(int Node, UInt128 Keys)> queue,
        int targetNode,
        UInt128 incomingKeys)
    {
        if (!visited.TryGetValue(targetNode, out var keysList))
        {
            visited[targetNode] = [incomingKeys];
            queue.Enqueue((targetNode, incomingKeys));
            return;
        }

        foreach (var keyset in keysList)
        {
            if ((keyset | incomingKeys) == keyset)
                return;
        }

        keysList.RemoveAll(k => (k | incomingKeys) == incomingKeys);
        keysList.Add(incomingKeys);
        queue.Enqueue((targetNode, incomingKeys));
    }

    private void RunBfs(Dictionary<int, List<UInt128>> visited, Queue<(int Node, UInt128 Keys)> queue)
    {
        while (queue.Count > 0)
        {
            var (currentNode, currentKeysMask) = queue.Dequeue();
            var node = Nodes[currentNode];

            var nextKeysMask = currentKeysMask | node.KeysMask;
            var connections = node.GetConnections(nextKeysMask, EsquieCarBit, !UnclaimedPortals.Contains(node.ID));

            foreach (int nextNode in connections)
            {
                TryEnqueue(visited, queue, nextNode, nextKeysMask);
            }
        }
    }

    private HashSet<(int Node, UInt128 Keys)> GetReachableUnclaimedPortalsFromVisited(
        Dictionary<int, List<UInt128>> visited)
    {
        var result = new HashSet<(int Node, UInt128 Keys)>();
        foreach (var (nodeId, keysList) in visited)
        {
            if (UnclaimedPortals.Contains(nodeId) && IsPortal(nodeId))
            {
                foreach (var k in keysList)
                {
                    result.Add((nodeId, k));
                }
            }
        }
        return result;
    }

    private HashSet<(int Node, UInt128 Keys)> ExtendReachability(
        Dictionary<int, List<UInt128>> visited,
        int candidate,
        int dest)
    {
        var queue = new Queue<(int Node, UInt128 Keys)>();

        if (visited.TryGetValue(candidate, out var candidateKeys))
        {
            var candidateNode = Nodes[candidate];
            foreach (var k in candidateKeys.ToList())
            {
                TryEnqueue(visited, queue, dest, k | candidateNode.KeysMask);
            }
        }

        if (RandomizerLogic.Settings.EnableTwoWayTeleport && visited.TryGetValue(dest, out var destKeys))
        {
            var destNode = Nodes[dest];
            foreach (var k in destKeys.ToList())
            {
                TryEnqueue(visited, queue, candidate, k | destNode.KeysMask);
            }
        }

        RunBfs(visited, queue);

        return GetReachableUnclaimedPortalsFromVisited(visited);
    }

    private bool CompleteGraphPlacement(int startNode, (int Node, UInt128 Keys) startingState, int depth)
    {
        var visited = new Dictionary<int, List<UInt128>>();
        var queue = new Queue<(int Node, UInt128 Keys)>();

        visited[startingState.Node] = [startingState.Keys];
        queue.Enqueue(startingState);

        RunBfs(visited, queue);
        var frontier = GetReachableUnclaimedPortalsFromVisited(visited);

        return CompleteGraphPlacement(startNode, visited, frontier, depth);
    }

    private bool CompleteGraphPlacement(
        int startNode,
        Dictionary<int, List<UInt128>> visited,
        HashSet<(int Node, UInt128 Keys)> frontier,
        int depth)
    {
        TotalIterations++;
        
        if (UnclaimedPortals.Count == 0)
            return HasFullReachability(startNode);
        
        if (depth > 500 || TotalIterations > 500 || frontier.Count == 0) return false;

        var candidate = Utils.ShuffleList(frontier.Select(s => s.Node).Distinct().ToList())
            .Where(n => UnclaimedPortals.Contains(n))
            .OrderBy(n => PortalDestinationPools[n].Count(d => UnclaimedPortals.Contains(d)))
            .FirstOrDefault(-1);

        if (candidate == -1) return false;

        foreach (var dest in SortPortalsByPreference(candidate))
        {
            if (dest == candidate) continue;
            if (!UnclaimedPortals.Contains(dest)) continue;

            ApplyPortalLink(candidate, dest);

            var childVisited = CloneVisited(visited);
            var childFrontier = ExtendReachability(childVisited, candidate, dest);

            if (UnclaimedPortals.Count > 0 && childFrontier.Count == 0)
            {
                UndoPortalLink();
                continue;
            }

            if (CompleteGraphPlacement(startNode, childVisited, childFrontier, depth + 1))
                return true;

            UndoPortalLink();
        }

        return false;
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
        UnclaimedPortals = Portals.Select(n => n.ID).ToHashSet();
        var constraintNodes = constraintStrings.Select(c => nodeIndexes[c]).ToList();
        int startNode = constraintNodes[0];
        
        TotalReachableNodes = GetReachableStates((startNode, 0)).Select(s => s.Node).Distinct().Count();

        UInt128 initialKeys = 0;

        (int, UInt128) startingState = (startNode, initialKeys);


        var iterationsLeft = 10;
        CheckForParity();
        
        while (UnclaimedPortals.Count > 0 && iterationsLeft > 0)
        {
            iterationsLeft--;
            TotalIterations = 0;
            ModificationHistory.Clear();

            if (CompleteGraphPlacement(startNode, startingState, 0))
                break;

            while (ModificationHistory.Count > 0)
                UndoPortalLink();
        }
        
        CheckForParity();

        if (UnclaimedPortals.Any())
        {
            if (!FixUnclaimedLeaves(startNode)) return false;
        }

        foreach (var node in Portals)
        {
            destinationChanges[Nodes[node.OriginalPortalConnection].CodeName] = Nodes[node.PortalConnection].CodeName;
        }
        
        criticalPath = GetPathAndDepths(constraintNodes[0], constraintNodes[^1]);
        return true;
    }
}