namespace Tests.Rules;

public class RandomizeLocations : OutputRuleBase
{
    private const string VanillaStart = "Level.SpawnPoint.LumiereAct01.Entry";
    private const string EndLocation = "Level.SpawnPoint.LumiereAct03.Entry";

    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.RandomizeLocations) return true;

        var startLocation = output.DestinationChanges.GetValueOrDefault(VanillaStart, VanillaStart);

        var traversal = new LocationGraphTraversal(output);
        var reachable = traversal.GetReachableLocations(startLocation);

        if (!reachable.ReachableLocations.Contains(EndLocation))
        {
            FailureMessage += $"End location {EndLocation} is not reachable from starting " +
                              $"location {startLocation}.\n" +
                              $"Reachable locations: {reachable.ReachableLocations.Count}, " +
                              $"keys collected: {string.Join(", ", reachable.CollectedKeys)}";
            return false;
        }

        return true;
    }
}