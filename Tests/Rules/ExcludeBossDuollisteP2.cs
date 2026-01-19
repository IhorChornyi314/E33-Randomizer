namespace Tests.Rules;

public class ExcludeBossDuollisteP2: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        return !output.Encounters.Where(e => e.Name != "Boss_Duolliste_P2").Any(e => e.Enemies.Any(i => i.CodeName == "Duolliste_P2"));
    }
}