namespace E33Randomizer;

public static class Controllers
{
    public static SkillsController SkillsController = new();
    public static void InitControllers()
    {
        SkillsController.Initialize();
    }
}