using E33Randomizer;
using Tests.Rules;

namespace Tests;

public static class TestLogic
{
    public static Output OriginalData;
    public static Random Random = new Random();

    public static List<string> SpecialCaseEncounterNames = [
        "Boss_Duolliste_P2", 
        "MM_DanseuseAlphaSummon", 
        "MM_DanseuseClone*1",
        "QUEST_Danseuse_DanceClass_Clone*1"
    ];

    public static Output CollectState(Config config)
    {
        var result = new Output();

        result.Encounters = new List<Encounter>(Controllers.EnemiesController.Encounters); 
        result.Encounters = result.Encounters
            .Where(e => !RemoveBrokenObjects.BrokenEncounters.Contains(e.Name)).ToList();
        result.Checks =
            Controllers.ItemsController.ItemsSources.SelectMany(iS => iS.SourceSections,
                (source, pair) => new Check($"{source.FileName}#{pair.Key}", pair.Value)).ToList();
        result.SkillTrees = Controllers.SkillsController.SkillGraphs.Select(sG => new SkillTree(sG)).ToList();
        
        if (config == null) return result;
        
        var randomizedEncounterNames =
            OriginalData.Encounters.Where(e => e.Enemies.Any(i => config.CustomEnemyPlacement.IsRandomized(i.CodeName))
            ).Select(e => e.Name);

        randomizedEncounterNames = randomizedEncounterNames.Where(n => !SpecialCaseEncounterNames.Contains(n));
        if (!config.Settings.RandomizeMerchantFights)
        {
            randomizedEncounterNames = randomizedEncounterNames.Where(n => !n.Contains("Merchant"));
        }
        
        result.RandomizedEncounters = result.Encounters.Where(e => randomizedEncounterNames.Contains(e.Name)).ToList();
        
        var randomizedCheckNames =
            OriginalData.Checks.Where(c => c.Items.Any(i => config.CustomItemPlacement.IsRandomized(i.Item.CodeName))
            ).Select(c => c.Name);

        result.RandomizedChecks = result.Checks.Where(c => randomizedCheckNames.Contains(c.Name)).ToList();

        result.RandomizedSkillNodes = result.SkillTrees.SelectMany(
                sT => sT.SkillNodes, (_, data) => data).
            Where(sN => config.CustomSkillPlacement.IsRandomized(sN.OriginalSkillCodeName)
            ).ToList();
        
        return result;
    }
    
    public static Output RunRandomizer(Config config)
    {
        RandomizerLogic.Init();
        RandomizerLogic.Settings = config.Settings;
        RandomizerLogic.CustomEnemyPlacement = config.CustomEnemyPlacement;
        RandomizerLogic.CustomItemPlacement = config.CustomItemPlacement;
        RandomizerLogic.CustomSkillPlacement = config.CustomSkillPlacement;
        RandomizerLogic.Randomize();
        return CollectState(config);
    }
}