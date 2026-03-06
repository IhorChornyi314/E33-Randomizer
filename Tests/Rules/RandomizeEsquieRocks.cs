using E33Randomizer;
using Newtonsoft.Json;

namespace Tests.Rules;

public class RandomizeEsquieRocks: OutputRuleBase
{
    public override bool IsSatisfied(Output output, Config config)
    {
        List<string> rocks = ["Florrie", "Dorrie", "Soarrie"];
        Dictionary<string, List<string>> rocksChests;
        using (StreamReader r = new StreamReader($"{RandomizerLogic.DataDirectory}/rock_chests.json"))
        {
            string json = r.ReadToEnd();
            rocksChests = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        }

        Dictionary<string, int> rocksPresent = new()
        {
            {"Florrie", 0},
            {"Dorrie", 0},
            {"Soarrie", 0},
        };
        
        foreach (var rock in rocks)
        {
            var locations = config.Settings.LimitEsquieRandomization
                ? $"{rock}LimitedLocations"
                : $"{rock}AllLocations";
            var rockChests =  rocksChests[locations];
            var checks = output.Checks.Where(c => rockChests.Contains(c.Name.Split("#")[1]));
            var nonRockChecks = output.Checks.Where(c => !checks.Contains(c));
            rocksPresent[rock] = checks.Sum(c => c.Items.Count(iS => iS.Item.CodeName.Contains(rock)));
            if (nonRockChecks.Any(c => c.Items.Any(iS => iS.Item.CodeName.Contains(rock))))
            {
                FailureMessage += $"{rock} was found in a non-rock check\n";
                return false;
            }
        }

        var failed = false;
        
        foreach (var (rock, count) in rocksPresent)
        {
            if (config.Settings.RandomizeEsquieRocks)
            {
                if (count != 1)
                {
                    FailureMessage += $"Expected 1 {rock}, got {count}\n";
                    failed = true;
                }
            }
            else
            {
                if (count != 0)
                {
                    FailureMessage += $"Expected 0 {rock}s, got {count}\n";
                    failed = true;
                }
            }
        }
        
        return !failed;
    }
}