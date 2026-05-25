using E33Randomizer.RadomizationLogic;

namespace Tests;

public class Output
{
    public List<Encounter> Encounters;
    public List<Check> Checks;
    public List<SkillTree> SkillTrees;
    public List<LocationNode> LocationNodes;
    
    public List<Encounter> RandomizedEncounters;
    public List<Check> RandomizedChecks;
    public List<SkillNode> RandomizedSkillNodes;
    public List<LocationNode> RandomizedLocationNodes;

    public Encounter GetEncounter(string encounterName)
    {
        return Encounters.FirstOrDefault(e => e.Name == encounterName);
    }

    public Check GetCheck(string checkName)
    {
        return Checks.FirstOrDefault(c => c.Name == checkName);
    }

    public LocationNode GetLocationNode(string locationName)
    {
        return LocationNodes.FirstOrDefault(n => n.CodeName == locationName);
    }
}