using E33Randomizer;

namespace Tests;

public class Config(SettingsViewModel s, CustomEnemyPlacement cep, CustomItemPlacement cip, CustomSkillPlacement csp)
{
    public SettingsViewModel Settings = s;
    public CustomEnemyPlacement CustomEnemyPlacement = cep;
    public CustomItemPlacement CustomItemPlacement = cip;
    public CustomSkillPlacement CustomSkillPlacement = csp;
}