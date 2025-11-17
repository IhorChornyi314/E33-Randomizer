namespace E33Randomizer;

public static class Controllers
{
    public static SkillsController SkillsController = new();
    public static ItemsController ItemsController = new();
    public static EnemiesController EnemiesController = new();

    public static BaseController GetController(string objectType)
    {
        return objectType switch
        {
            "Enemy" => EnemiesController,
            "Item" => ItemsController,
            "Skill" => SkillsController,
            _ => null
        };
    }
    
    public static void InitControllers()
    {
        EnemiesController.Initialize();
        SkillsController.Initialize();
        ItemsController.Initialize();
    }

    public static void WriteAssets()
    {
        if (RandomizerLogic.Settings.RandomizeSkills)
        {
            SkillsController.WriteAssets();
        }
        if (RandomizerLogic.Settings.RandomizeItems)
        {
            ItemsController.WriteAssets();
        }
        if (RandomizerLogic.Settings.RandomizeEnemies)
        {
            EnemiesController.WriteAssets();
        }
    }
}