using E33Randomizer.RandomizationLogic;

namespace Tests.Rules;

public class EnsureFullConnectivity : OutputRuleBase
{
    private const string _startLocation = "Level.SpawnPoint.LumiereAct01.Entry";
    
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.EnsureFullConnectivity || !config.Settings.RandomizeLocations) return true;

        var vanillaOutput = new Output { DestinationChanges = new Dictionary<string, string>() };
        var vanillaTraversal = new LocationGraphTraversal(vanillaOutput);

        var vanilla = vanillaTraversal.GetReachableLocations(_startLocation);
        var randomized = new LocationGraphTraversal(output).GetReachableLocations(_startLocation);

        var missing = vanilla.ReachableLocations.Except(randomized.ReachableLocations).ToList();
        var gained = randomized.ReachableLocations.Except(vanilla.ReachableLocations).ToList();

        var problems = new List<string>();

        if (missing.Any())
        {
            problems.Add("Locations reachable in vanilla but NOT in randomized output:\n" +
                         string.Join(",\n", missing));
        }

        if (gained.Any())
        {
            problems.Add("Locations NOT reachable in vanilla but reachable in randomized output:\n" +
                         string.Join(",\n", gained));
        }

        if (problems.Any())
        {
            FailureMessage += string.Join("\n\n", problems);
            return false;
        }

        return true;
    }
}
