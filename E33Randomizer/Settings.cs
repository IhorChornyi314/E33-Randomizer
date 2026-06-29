using E33Randomizer.CustomPlacements;

namespace E33Randomizer;

public class Settings
{
    public required SettingsViewModel GeneralSettings { get; set; }
    public CustomPlacementPreset? CustomLocationPlacementSettings { get; set; }
    public CustomPlacementPreset? CustomEnemyPlacementSettings { get; set; }
    public CustomPlacementPreset? CustomItemPlacementSettings { get; set; }
    public CustomPlacementPreset? CustomSkillPlacementSettings { get; set; }
}