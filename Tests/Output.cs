using E33Randomizer;

namespace Tests;

public class Output
{
    public List<Encounter> Encounters;
    public List<Check> Checks;
    public List<SkillTree> SkillTrees;
    
    public List<Encounter> RandomizedEncounters;
    public List<Check> RandomizedChecks;
    public List<SkillNode> RandomizedSkillNodes;

    public Encounter GetEncounter(string encounterName)
    {
        return Encounters.FirstOrDefault(e => e.Name == encounterName);
    }

    public Check GetCheck(string checkName)
    {
        return Checks.FirstOrDefault(c => c.Name == checkName);
    }
}