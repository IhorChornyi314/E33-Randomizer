namespace Tests.Rules;

public class MakeSkillsIntoItems: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        var skillItemNames = output.SkillTrees.SelectMany(sT => sT.Skills).Select(s => s.CodeName).ToList();
        var numberOfSkillItems = new Dictionary<string, int>();
        foreach (var skillItem in skillItemNames)
        {
            numberOfSkillItems.TryAdd(skillItem, 0);
        }
        
        var totalNumberOfSkillItems = 0;
        foreach (var check in output.Checks)
        {
            foreach (var item in check.Items)
            {
                if (numberOfSkillItems.TryGetValue(item.Item.CodeName, out int value))
                {
                    numberOfSkillItems[item.Item.CodeName] = ++value;
                    totalNumberOfSkillItems++;
                }
            }
        }

        if (!config.Settings.MakeSkillsIntoItems && totalNumberOfSkillItems != 0)
        {
            FailureMessage += $"Expected 0 skill items, got {totalNumberOfSkillItems}";
            return false;
        }
        
        var unrepresentedSkills = skillItemNames.Where(s => numberOfSkillItems[s] < 2).ToList();

        if (config.Settings.MakeSkillsIntoItems && unrepresentedSkills.Count > 0)
        {
            FailureMessage +=
                $"Expected {skillItemNames.Count} skill items, got {unrepresentedSkills.Count - skillItemNames.Count}";
            return false;
        }
        
        return true;
    }
}