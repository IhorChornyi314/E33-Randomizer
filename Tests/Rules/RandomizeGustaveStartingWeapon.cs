namespace Tests.Rules;

public class RandomizeGustaveStartingWeapon: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.RandomizeStartingWeapons) return true;

        var check = output.Checks.First(c => c.Name.Contains("Chest_Generic_Chroma"));
        if (check.Items.All(i => !i.Item.CustomName.Contains("Gustave Weapon")))
        {
            FailureMessage += "Chest_Generic_Chroma check did not contain a Gustave's Weapon";
            return false;
        }
        return true;
    }
}