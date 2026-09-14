using E33Randomizer.ObjectDatum;
using E33Randomizer.RandomizationLogic;

namespace Tests;

public class ReachabilityResult
{
    public HashSet<string> ReachableLocations { get; } = new();
    public HashSet<string> CollectedKeys { get; } = new();
}

public class LocationGraphTraversal
{
    private readonly Output _output;

    public LocationGraphTraversal(Output output)
    {
        _output = output;
    }

    private List<string> GetAvailableTransitions(LocationData location, HashSet<string> keys)
    {
        var transitions = new List<string>();

        if (location.UnconditionalConnections != null)
            transitions.AddRange(location.UnconditionalConnections);

        if (!string.IsNullOrEmpty(location.PortalConnection))
            transitions.Add(_output.DestinationChanges.GetValueOrDefault(location.PortalConnection, location.PortalConnection));

        if (location.ConditionalConnections != null)
        {
            foreach (var kv in location.ConditionalConnections)
            {
                if (keys.Contains(kv.Key))
                    transitions.AddRange(kv.Value);
            }
        }

        return transitions;
    }

    public string Fingerprint(HashSet<string> keys)
    {
        return string.Join("|", keys.OrderBy(k => k));
    }

    public ReachabilityResult GetReachableLocations(string startCodeName)
    {
        var result = new ReachabilityResult();
        var visitedStates = new HashSet<(string Location, string Keys)>();

        var startKeys = new HashSet<string>();

        result.ReachableLocations.Add(startCodeName);

        var queue = new Queue<(string Location, HashSet<string> Keys)>();
        visitedStates.Add((startCodeName, Fingerprint(startKeys)));
        queue.Enqueue((startCodeName, startKeys));

        while (queue.Count > 0)
        {
            var (current, keys) = queue.Dequeue();

            var data = Controllers.LocationController.GetObject(current);

            foreach (var transition in GetAvailableTransitions(data, keys))
            {
                var fingerprint = (transition, Fingerprint(keys));
                if (visitedStates.Contains(fingerprint)) continue;

                visitedStates.Add(fingerprint);
                result.ReachableLocations.Add(transition);
                queue.Enqueue((transition, new HashSet<string>(keys)));
            }

            if (data.Keys.Any(k => !keys.Contains(k)))
            {
                var enriched = new HashSet<string>(keys);
                foreach (var key in data.Keys)
                {
                    enriched.Add(key);
                    result.CollectedKeys.Add(key);
                }

                var fingerprint = (current, Fingerprint(enriched));
                if (visitedStates.Add(fingerprint))
                {
                    queue.Enqueue((current, enriched));
                }
            }
        }

        return result;
    }

}
