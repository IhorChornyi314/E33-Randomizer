namespace E33Randomizer;

public static class Controllers
{
    public static SkillsController SkillsController = new();
    public static ItemsController ItemsController = new();
    public static EnemiesController EnemiesController = new();
    public static CharacterController CharacterController = new();

    public static BaseController GetController(string objectType)
    {
        return objectType switch
        {
            "Enemy" => EnemiesController,
            "Item" => ItemsController,
            "Skill" => SkillsController,
            "Character" => CharacterController,
            _ => null
        };
    }
    
    public static void InitControllers()
    {
        EnemiesController.Initialize();
        // It is important to init items before skills for skill unlock items
        ItemsController.Initialize();
        SkillsController.Initialize();
        CharacterController.Initialize();
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
        if (RandomizerLogic.Settings.RandomizeCharacters)
        {
            CharacterController.WriteAssets();
        }
    }
}