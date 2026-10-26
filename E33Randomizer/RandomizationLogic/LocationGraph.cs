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
    public int[] ConnectionsArray;
}

public class LocationNodeInternal
{
    public int ID;
    public string CodeName;
    public List<int> UnconditionalConnections;
    public List<ConditionalConnection> ConditionalConnections;
    public int[] UnconditionalArray;
    public ConditionalConnection[] ConditionalArray;
    public int OriginalPortalConnection = -1;
    public int PortalConnection = -1;

    public int Depth = Int16.MaxValue;
    public int DepthIncrement = 1;

    public UInt128 KeysMask;

    public List<int> GetConnections(UInt128 unlockedKeys, UInt128 esquieCarBit, bool includeTeleport = true)
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

    public LocationNodeInternal(LocationData locationData, Dictionary<string, int> nodeIndexes,
        Dictionary<string, int> keyIndexes)
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
        UnconditionalArray = UnconditionalConnections.ToArray();
        ConditionalArray = ConditionalConnections.ToArray();
        foreach (var cc in ConditionalArray)
            cc.ConnectionsArray = cc.Connections.ToArray();
    }

    public override string ToString()
    {
        return $"{ID}: {CodeName}";
    }
}

public class LocationGraph
{
    private Stack<(int From, int To, int PrevFrom, int PrevTo)> ModificationHistory = new();
    private UInt128[][] _visited;
    private readonly List<(int Node, UInt128[] Old)> _trail = new();

    public List<LocationNodeInternal> Nodes = [];
    public int TotalReachableNodes = 0;
    public List<LocationNodeInternal> Portals = [];
    public Dictionary<string, int> nodeIndexes = new();
    public Dictionary<string, int> keyIndexes = new();
    public Dictionary<int, List<int>> ConnectedPortals = new();
    public HashSet<int> AvailablePortals = new();
    public HashSet<int> AvailableDestinations = new();
    public Dictionary<int, List<int>> PortalDestinationPools = new();
    public Dictionary<int, HashSet<int>> PortalDestinationPoolSets = new();
    public Dictionary<int, int> PinnedDestination = new();
    public HashSet<int> NotRandomizedDestinations = new();
    public List<List<int>> PortalGroups = [];

    public int MaxDfsDepth = 500; 
    public int MaxTotalIterations = 500;
    public int MaxAttempts = 5000;

    public int[] RegionId;
    private int _regionCount;
    private int[] _availableRegions;
    private bool[] _reachedRegions;
    public int FrontierGuard = 1;
    public int FrontierCount;

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
                if (!keyIndexes.ContainsKey(key))
                    keyIndexes[key] = keyIndexes.Count;
            foreach (var key in n.ConditionalConnections.Keys)
                if (!keyIndexes.ContainsKey(key))
                    keyIndexes[key] = keyIndexes.Count;
        }

        if (keyIndexes.Count > 128)
            throw new InvalidOperationException(
                ResourceHelper.GetString(nameof(Assets.Resources.LocationController_TooManyKeys_Exception)));

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

        var parent = Enumerable.Range(0, Nodes.Count).ToArray();

        int Find(int x)
        {
            while (parent[x] != x) x = parent[x] = parent[parent[x]];
            return x;
        }

        foreach (var (n, list) in ConnectedPortals)
        foreach (var p in list)
            parent[Find(p)] = Find(n);

        RegionId = new int[Nodes.Count];
        var ids = new Dictionary<int, int>();
        for (var i = 0; i < Nodes.Count; i++)
            RegionId[i] = ids.TryGetValue(Find(i), out var id) ? id : (ids[Find(i)] = ids.Count);
        _regionCount = ids.Count;
        _availableRegions = new int[_regionCount];
        _reachedRegions = new bool[_regionCount];

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

    public HashSet<(int Node, UInt128 Keys)> GetReachableStates((int Node, UInt128 Keys) startingState,
        HashSet<int> bannedPortals = null)
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

            if (currentNode == endNode && currentPath.Count == 0)
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

        var criticalPath = currentPath.Select(i => Controllers.LocationController.GetObject(Nodes[i].CodeName))
            .ToList();
        return criticalPath;
    }

    public bool IsPortal(int node)
    {
        return Nodes[node].OriginalPortalConnection != -1 &&
               Nodes[Nodes[node].OriginalPortalConnection].OriginalPortalConnection == node;
    }

    public bool IsPortal(LocationNodeInternal node)
    {
        return node.OriginalPortalConnection != -1 &&
               Nodes[node.OriginalPortalConnection].OriginalPortalConnection == node.ID;
    }


    public IEnumerable<int> SortPortalsByPreference(int startingPortal, int frontierCount, bool[] reachedRegions)
    {
        var pool = PortalDestinationPoolSets[startingPortal];
        var pinned = PinnedDestination[startingPortal];
        var guard = frontierCount <= FrontierGuard;

        var hasPinned = false;
        var unreachedPool = new List<int>();
        var unreachedRest = new List<int>();
        var reachablePool = new List<int>();
        var reachableRest = new List<int>();

        foreach (var p in AvailableDestinations)
        {
            if (p == startingPortal) continue;
            if (reachedRegions != null && reachedRegions[RegionId[p]]) continue;

            if (p == pinned)
            {
                hasPinned = true;
                continue;
            }

            var inPool = pool.Contains(p);
            if (guard && _visited[p] == null) (inPool ? unreachedPool : unreachedRest).Add(p);
            else (inPool ? reachablePool : reachableRest).Add(p);
        }

        if (hasPinned) yield return pinned;

        foreach (var tier in new[] { unreachedPool, unreachedRest, reachablePool, reachableRest })
        foreach (var p in Utils.LazyShuffle(tier))
            yield return p;
    }

    public bool HasFullReachability(int startNode)
    {
        return GetReachableStates((startNode, 0)).Select(s => s.Node).Distinct().Count() == TotalReachableNodes;
    }

    private void ApplyPortalLink(int fromPortal, int toPortal)
    {
        var prevFrom = Nodes[fromPortal].PortalConnection;
        var prevTo = Nodes[toPortal].PortalConnection;

        Nodes[fromPortal].PortalConnection = toPortal;
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
            Nodes[toPortal].PortalConnection = fromPortal;

        ModificationHistory.Push((fromPortal, toPortal, prevFrom, prevTo));
        AvailablePortals.Remove(fromPortal);
        AvailableDestinations.Remove(toPortal);
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
        {
            AvailableDestinations.Remove(fromPortal);
            AvailablePortals.Remove(toPortal);
        }
    }

    private void UndoPortalLink()
    {
        var (from, to, prevFrom, prevTo) = ModificationHistory.Pop();
        Nodes[from].PortalConnection = prevFrom;
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
            Nodes[to].PortalConnection = prevTo;
        AvailablePortals.Add(from);
        AvailableDestinations.Add(to);
        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
        {
            AvailableDestinations.Add(from);
            AvailablePortals.Add(to);
        }
    }

    private void SetVisited(int node, UInt128[] value)
    {
        _trail.Add((node, _visited[node]));
        _visited[node] = value;
    }

    private void RollbackTo(int mark)
    {
        for (var i = _trail.Count - 1; i >= mark; i--)
            _visited[_trail[i].Node] = _trail[i].Old;
        _trail.RemoveRange(mark, _trail.Count - mark);
    }

    private void TryEnqueue(Queue<(int Node, UInt128 Keys)> queue, int targetNode, UInt128 incoming)
    {
        var existing = _visited[targetNode];
        if (existing == null)
        {
            SetVisited(targetNode, [incoming]);
            queue.Enqueue((targetNode, incoming));
            return;
        }

        var keep = 0;
        foreach (var k in existing)
        {
            if ((k | incoming) == k) return;
            if ((k | incoming) != incoming) keep++;
        }

        var updated = new UInt128[keep + 1];
        var idx = 0;
        foreach (var k in existing)
            if ((k | incoming) != incoming)
                updated[idx++] = k;
        updated[idx] = incoming;

        SetVisited(targetNode, updated);
        queue.Enqueue((targetNode, incoming));
    }

    private void RunBfs(Queue<(int Node, UInt128 Keys)> queue)
    {
        while (queue.Count > 0)
        {
            var (currentNode, currentKeysMask) = queue.Dequeue();
            var node = Nodes[currentNode];
            var nextKeys = currentKeysMask | node.KeysMask;

            var unc = node.UnconditionalArray;
            for (var i = 0; i < unc.Length; i++)
                TryEnqueue(queue, unc[i], nextKeys);

            if (node.PortalConnection != -1 && !AvailablePortals.Contains(node.ID))
                TryEnqueue(queue, node.PortalConnection, nextKeys);

            var conds = node.ConditionalArray;
            for (var i = 0; i < conds.Length; i++)
            {
                var cond = conds[i];
                if ((nextKeys & cond.RequiredKeyBit) == 0) continue;
                if (cond.IsEsquieAbility && (nextKeys & EsquieCarBit) == 0) continue;

                var targets = cond.ConnectionsArray;
                for (var j = 0; j < targets.Length; j++)
                    TryEnqueue(queue, targets[j], nextKeys);
            }
        }
    }

    private void ExtendReachability(int candidate, int dest)
    {
        var queue = new Queue<(int Node, UInt128 Keys)>();

        var candidateKeys = _visited[candidate];
        if (candidateKeys != null)
        {
            var mask = Nodes[candidate].KeysMask;
            foreach (var k in candidateKeys)
                TryEnqueue(queue, dest, k | mask);
        }

        if (RandomizerLogic.Settings.EnableTwoWayTeleport)
        {
            var destKeys = _visited[dest];
            if (destKeys != null)
            {
                var mask = Nodes[dest].KeysMask;
                foreach (var k in destKeys)
                    TryEnqueue(queue, candidate, k | mask);
            }
        }

        RunBfs(queue);
    }

    private bool CompleteGraphPlacement(int startNode, (int Node, UInt128 Keys) startingState, int depth)
    {
        _visited = new UInt128[Nodes.Count][];
        _trail.Clear();

        var queue = new Queue<(int Node, UInt128 Keys)>();
        SetVisited(startingState.Node, [startingState.Keys]);
        queue.Enqueue(startingState);

        RunBfs(queue);
        return CompleteGraphPlacement(startNode, depth);
    }

    private int ComputeSlack()
    {
        Array.Clear(_availableRegions);
        Array.Clear(_reachedRegions);

        foreach (var p in AvailablePortals)
        {
            var r = RegionId[p];
            _availableRegions[r]++;
            if (_visited[p] != null) _reachedRegions[r] = true;
        }

        var slack = 0;
        for (var r = 0; r < _regionCount; r++)
        {
            if (_availableRegions[r] == 0) continue;
            slack += _reachedRegions[r]
                ? _availableRegions[r]
                : _availableRegions[r] - 2;
        }

        return slack;
    }

    private int PickCandidate()
    {
        var best = -1;
        var bestScore = int.MaxValue;
        var ties = 0;
        FrontierCount = 0;
        foreach (var n in AvailablePortals)
        {
            if (_visited[n] == null) continue;
            FrontierCount++;
            var score = 0;
            foreach (var d in PortalDestinationPools[n])
                if (AvailablePortals.Contains(d))
                    score++;

            if (score < bestScore)
            {
                best = n;
                bestScore = score;
                ties = 1;
            }
            else if (score == bestScore && RandomizerLogic.rand.Next(++ties) == 0)
            {
                best = n;
            }
        }

        return best;
    }

    private bool CompleteGraphPlacement(int startNode, int depth)
    {
        TotalIterations++;

        if (AvailablePortals.Count == 0) return HasFullReachability(startNode);

        if (depth > MaxDfsDepth || TotalIterations > MaxTotalIterations) return false;

        var slack = ComputeSlack();
        var twoWay = RandomizerLogic.Settings.EnableTwoWayTeleport;
        if (twoWay && slack < 0) return false;

        var reachedRegions = twoWay && slack < 2 ? (bool[])_reachedRegions.Clone() : null;

        var candidate = PickCandidate();
        if (candidate == -1) return false;

        foreach (var dest in SortPortalsByPreference(candidate, FrontierCount, reachedRegions))
        {
            ApplyPortalLink(candidate, dest);

            var mark = _trail.Count;
            ExtendReachability(candidate, dest);

            if (CompleteGraphPlacement(startNode, depth + 1))
                return true;

            RollbackTo(mark);
            UndoPortalLink();

            if (TotalIterations > MaxTotalIterations) return false;
        }

        return false;
    }

    public bool RemakeWholeGraph(string startNodeCodeName, string endingNodeCodeName,
        List<string> possibleLandingLocationCodeNames, out List<LocationData> criticalPath,
        out Dictionary<string, string> destinationChanges)
    {
        var clp = RandomizerLogic.CustomLocationPlacement.GetCategory;
        NotRandomizedDestinations =
            Nodes.Where(n => RandomizerLogic.CustomLocationPlacement.NotRandomizedCodeNames.Contains(n.CodeName))
                .Select(n => n.ID).ToHashSet();
        var categories = Nodes.ToDictionary(n => n.ID, n => clp(n.CodeName));

        foreach (var node in Nodes)
        {
            if (!IsPortal(node)) continue;
            node.PortalConnection = node.OriginalPortalConnection;

            var destCategory = categories[node.OriginalPortalConnection];
            var pool = Nodes.Where(n => IsPortal(n) && n != node && categories[n.ID] == destCategory)
                .Select(n => n.ID).ToList();

            PortalDestinationPools[node.ID] = pool;
            PortalDestinationPoolSets[node.ID] = pool.ToHashSet();
            PinnedDestination[node.ID] =
                NotRandomizedDestinations.Contains(node.OriginalPortalConnection)
                    ? node.OriginalPortalConnection
                    : -1;
        }

        criticalPath = [];
        destinationChanges = new();
        AvailablePortals = Portals.Select(n => n.ID).ToHashSet();
        AvailableDestinations = Portals.Select(n => n.ID).ToHashSet();

        int startNode = nodeIndexes[startNodeCodeName];
        int endNode = nodeIndexes[endingNodeCodeName];
        List<int> possibleLandingLocations = possibleLandingLocationCodeNames.Select(cN => nodeIndexes[cN]).ToList();
        TotalReachableNodes = GetReachableStates((startNode, 0)).Select(s => s.Node).Distinct().Count();

        UInt128 initialKeys = 0;

        (int, UInt128) startingState = (startNode, initialKeys);

        foreach (var possibleLandingLocation in possibleLandingLocations)
        {
            var iterationsLeft = MaxAttempts;
            var foundValid = false;
            Nodes[95].PortalConnection = possibleLandingLocation;
            while (AvailablePortals.Count > 0 && iterationsLeft > 0)
            {
                iterationsLeft--;
                TotalIterations = 0;
                ModificationHistory.Clear();

                if (CompleteGraphPlacement(startNode, startingState, 0))
                {
                    foundValid = true;
                    break;
                }

                while (ModificationHistory.Count > 0)
                    UndoPortalLink();
            }

            if (foundValid)
                break;
        }

        if (AvailablePortals.Any())
        {
            return false;
        }

        foreach (var node in Portals)
        {
            destinationChanges[Nodes[node.OriginalPortalConnection].CodeName] = Nodes[node.PortalConnection].CodeName;
        }

        destinationChanges["Level.SpawnPoint.SpringMeadows.Entry"] = Nodes[Nodes[95].PortalConnection].CodeName;

        criticalPath = GetPathAndDepths(startNode, endNode);
        return true;
    }
}