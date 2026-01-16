using E33Randomizer;

namespace Tests;

public static class TestLogic
{
    public static Output OriginalData;
    public static Random Random = new Random();

    public static Output CollectState()
    {
        var result = new Output();

        result.Encounters = Controllers.EnemiesController.Encounters;
        result.Checks =
            Controllers.ItemsController.ItemsSources.SelectMany(iS => iS.SourceSections,
                (source, pair) => new Check($"{source.FileName}#{pair.Key}", pair.Value)).ToList();
        result.SkillTrees = Controllers.SkillsController.SkillGraphs.Select(sG => new SkillTree(sG)).ToList();
        
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
        return CollectState();
    }
}