using E33Randomizer.RandomizationLogic;

namespace Tests.Rules;

public class EnableTwoWayTeleport: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.EnableTwoWayTeleport || !config.Settings.RandomizeLocations) return true;
        var brokenPairs = new List<string>();
        
        var paths = new List<string>();
        
        foreach (var codeName in output.DestinationChanges.Keys)
        {
            var vanillaPartner = Controllers.LocationController.GetObject(codeName).PortalConnection;

            if (string.IsNullOrEmpty(vanillaPartner) || vanillaPartner == codeName) continue;
            if (Controllers.LocationController.GetObject(vanillaPartner).PortalConnection != codeName) continue;

            var forwardTarget = output.DestinationChanges.GetValueOrDefault(vanillaPartner, vanillaPartner);
            var forwardVanillaPartner = Controllers.LocationController.GetObject(forwardTarget).PortalConnection;

            var returnTarget = string.IsNullOrEmpty(forwardVanillaPartner)
                ? forwardTarget
                : output.DestinationChanges.GetValueOrDefault(forwardVanillaPartner, forwardVanillaPartner);
            
            paths.Add($"{codeName} -> {forwardTarget} -> {returnTarget}");
            
            if (returnTarget != codeName)
            {
                brokenPairs.Add($"{codeName} -> {forwardTarget} -> {returnTarget} (expected back at {codeName})");
            }
        }

        if (brokenPairs.Any())
        {
            FailureMessage += string.Join(",\n", brokenPairs);
            return false;
        }

        return true;
    }
}