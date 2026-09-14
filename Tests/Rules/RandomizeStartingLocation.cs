namespace Tests.Rules;

public class RandomizeStartingLocation : OutputRuleBase
{
    private const string VanillaStart = "Level.SpawnPoint.SpringMeadows.Entry";

    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.RandomizeLocations) return true;

        var newStart = output.DestinationChanges.GetValueOrDefault(VanillaStart, VanillaStart);

        if (newStart != VanillaStart ^ config.Settings.RandomizeStartingLocation)
        {
            FailureMessage += $"Starting location is {newStart}";
            return false;
        }

        return true;
    }
}