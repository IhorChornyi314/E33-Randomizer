using E33Randomizer;

namespace Tests.RuleTests;

public class NoSimonP2BeforeLune: OutputRuleBase
{
    private static List<string> _noP2Encounters =
    [
        "SM_Lancelier*1",
        "SM_FirstLancelier*1",
        "SM_FirstLancelierNoTuto*1",
        "SM_FirstPortier*1",
        "SM_FirstPortier_NoTuto*1"
    ];

    public override bool IsSatisfied(Output output, Config config)
    {
        if (!config.Settings.NoSimonP2BeforeLune) return true;
        
        foreach (var encounterName in _noP2Encounters)
        {
            var encounter = output.GetEncounter(encounterName);
            foreach (var enemyData in encounter.Enemies)
            {
                if (enemyData.CodeName == "Boss_Simon_Phase2" || enemyData.CodeName == "Boss_Simon_ALPHA")
                {
                    FailureMessage += $"Encounter {encounter.Name} contains {enemyData.CodeName}";
                    return false;
                }
            }
        }
        return true;
    }
}