using E33Randomizer;

namespace Tests;

public abstract class RuleTest
{
    public abstract bool RunTest(Output output, SettingsViewModel settings);
}