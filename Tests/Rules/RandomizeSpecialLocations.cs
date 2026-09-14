namespace Tests.Rules;

public class RandomizeSpecialLocations : OutputRuleBase
{
    private static List<string> _manorDoors =
    [
        "Level.SpawnPoint.Manor.AlineWorkshop",
        "Level.SpawnPoint.Manor.Bathroom",
        "Level.SpawnPoint.Manor.CleaBedroom",
        "Level.SpawnPoint.Manor.Entrance",
        "Level.SpawnPoint.Manor.Kitchen",
        "Level.SpawnPoint.Manor.Library",
        "Level.SpawnPoint.Manor.ParentsBedroom",
        "Level.SpawnPoint.Manor.Room01",
        "Level.SpawnPoint.Manor.Room02",
        "Level.SpawnPoint.Manor.Room03",
        "Level.SpawnPoint.Manor.VersoBedroom",
    ];

    private static List<string> _gestralBeaches =
    [
        "Level.SpawnPoint.GestralBeach.WipeOut",
        "Level.SpawnPoint.GestralBeach.VolleyBall",
        "Level.SpawnPoint.GestralBeach.Race",
        "Level.SpawnPoint.GestralBeach.OnlyUp",
        "Level.SpawnPoint.GestralBeach.Climb",
    ];

    private static List<string> _paintingWorkshops =
    [
        "Level.SpawnPoint.CleaWorkshop.Path1",
        "Level.SpawnPoint.CleaWorkshop.Path2",
        "Level.SpawnPoint.CleaWorkshop.Path3",
    ];

    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.RandomizeLocations) return true;

        var failures = new List<string>();

        if (config.Settings.RandomizeManorDoors)
            CheckGroup("ManorDoors", _manorDoors, output, failures);
        else
            CheckGroupUntouched("ManorDoors", _manorDoors, output, failures);

        if (config.Settings.RandomizeGestralBeachPortals)
            CheckGroup("GestralBeaches", _gestralBeaches, output, failures);
        else
            CheckGroupUntouched("GestralBeaches", _gestralBeaches, output, failures);

        if (config.Settings.RandomizeWorkshopEntries)
            CheckGroup("PaintingWorkshops", _paintingWorkshops, output, failures);
        else
            CheckGroupUntouched("PaintingWorkshops", _paintingWorkshops, output, failures);

        if (failures.Any())
        {
            FailureMessage += string.Join("\n\n", failures);
            return false;
        }

        return true;
    }
    
    private static void CheckGroupUntouched(string groupName, List<string> locations, Output output, List<string> failures)
    {
        var problems = new List<string>();

        foreach (var location in locations)
        {
            if (!output.DestinationChanges.TryGetValue(location, out var target)) continue;
            if (target == location) continue;

            problems.Add($"{location} -> {target} (randomization disabled for {groupName}, expected no override or identity)");
        }

        if (problems.Any())
        {
            failures.Add($"[{groupName} — disabled, must be untouched]\n" + string.Join("\n", problems));
        }
    }

    private static void CheckGroup(string groupName, List<string> locations, Output output, List<string> failures)
    {
        var problems = new List<string>();
        var targets = new List<string>();

        foreach (var location in locations)
        {
            var target = output.DestinationChanges.GetValueOrDefault(location, location);

            if (target == location)
            {
                problems.Add($"{location} was not randomized (no override in DestinationChanges)");
                continue;
            }

            if (!locations.Contains(target))
            {
                problems.Add($"{location} -> {target} (target is not part of {groupName})");
                continue;
            }

            targets.Add(target);
        }

        var duplicates = targets
            .GroupBy(t => t)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicate in duplicates)
        {
            problems.Add($"{duplicate} is used as a destination more than once");
        }

        if (problems.Any())
        {
            failures.Add($"[{groupName}]\n" + string.Join("\n", problems));
        }
    }
}
