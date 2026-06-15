namespace E33Randomizer;

public partial class RandomizeSkillsTab : TabBase
{

    public RandomizeSkillsTab()
    {
        InitializeComponent();
    }
}

// Design Time Settings DataContext
public static class DesignSkillsSettingsViewModel
{
    public static SettingsViewModel SettingsViewModel => new()
    {
        RandomizeSkills = true,
        ReduceSkillRepetition = true,
        RandomizeSkillUnlockCosts = true,
        UnlockGustaveSkills = true,
        GuaranteeGustaveOvercharge = true,
        MakeSkillsIntoItems = true,
        RandomizeTreeEdges = true,
        FullyRandomEdges = true,
        MinTreeEdges = 2,
        MaxTreeEdges = 2,
        RandomEdgeChancePercent = 60
    };
}