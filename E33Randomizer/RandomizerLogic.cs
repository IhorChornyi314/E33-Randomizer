using System.IO;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;
namespace E33Randomizer;

public class GenerationReport
{
    public int Seed;
    public List<Encounter> DT_jRPG_Encounters = [];
    public List<Encounter> DT_jRPG_Encounters_CleaTower = [];
    public List<Encounter> DT_Encounters_Composite = [];
    public List<Encounter> DT_WorldMap_Encounters = [];

    public string GenerateString()
    {
        var result = Seed + "\nDT_jRPG_Encounters\n";
        foreach (var encounter in DT_jRPG_Encounters)
        {
            result += encounter + "\n";
        }
        
        result += "DT_jRPG_Encounters_CleaTower\n";
        foreach (var encounter in DT_jRPG_Encounters_CleaTower)
        {
            result += encounter + "\n";
        }
        
        result += "DT_Encounters_Composite\n";
        foreach (var encounter in DT_Encounters_Composite)
        {
            result += encounter + "\n";
        }
        
        result += "DT_WorldMap_Encounters\n";
        foreach (var encounter in DT_WorldMap_Encounters)
        {
            result += encounter + "\n";
        }
        
        return result;
    }
}

public static class RandomizerLogic
{
    public static readonly List<string> BrokenEnemies =
    [
        "QUEST_WeaponlessChalier",
        "Boss_Simon_ALPHA",
        "FB_Dualliste_Phase1"
    ];
    public static Usmap mappings;
    public static List<EnemyData> allEnemies;
    public static Random rand;
    public static int usedSeed;
    public static readonly List<Encounter> ProcessedEncounters = [];
    public static GenerationReport Report;
    public static Dictionary<string, Dictionary<string, float>> EnemyFrequenciesWithinArchetype = new();
    public static Dictionary<string, float> TotalEnemyFrequencies;
    public static readonly string PresetName = "";

    public static readonly List<string> Archetypes =
        ["Regular", "Weak", "Strong", "Elite", "Boss", "Alpha", "Elusive", "Petank"];

    public static void Init()
    {
        usedSeed = Settings.Seed != -1 ? Settings.Seed : Environment.TickCount; 
        rand = new Random(usedSeed);
    
        mappings = new Usmap("Data/Mappings.usmap");
        using (StreamReader r = new StreamReader("Data/enemy_data.json"))
        {
            string json = r.ReadToEnd();
            allEnemies = JsonConvert.DeserializeObject<List<EnemyData>>(json);
        }

        allEnemies = allEnemies.Where(e => !BrokenEnemies.Contains(e.CodeName)).ToList();
        ConstructTotalEnemyFrequencies();
        ConstructEnemyFrequenciesWithinArchetype();
        CustomEnemyPlacement.InitPlacementOptions();
        SpecialRules.Reset();
    }

    public static void ConstructTotalEnemyFrequencies()
    {
        TotalEnemyFrequencies = new Dictionary<string, float>();
        foreach (var enemyData in allEnemies)
        {
            TotalEnemyFrequencies[enemyData.CodeName] = 1;
        }
    }

    public static void ConstructEnemyFrequenciesWithinArchetype()
    {
        EnemyFrequenciesWithinArchetype = new Dictionary<string, Dictionary<string, float>>();
        foreach (var archetype in Archetypes)
        {
            EnemyFrequenciesWithinArchetype[archetype] = new Dictionary<string, float>();
            foreach (var enemyData in allEnemies.FindAll(e => e.Archetype == archetype))
            {
                EnemyFrequenciesWithinArchetype[archetype][enemyData.CodeName] = 1;
            }
        }
    }

    private static void PackAndConvertData()
    {
        var presetName = PresetName.Length == 0 ? usedSeed.ToString() : PresetName;
        var exportPath = $"rand_{presetName}/";
        Directory.CreateDirectory(exportPath);
        WriteReport(exportPath);
        var repackArgs = $"pack randomizer \"{exportPath}randomizer_P.pak\"";
        var retocArgs = $"to-zen --version UE5_4 \"{exportPath}randomizer_P.pak\" \"{exportPath}randomizer_P.utoc\"";

        Process.Start("repak.exe", repackArgs).WaitForExit();
        Process.Start("retoc.exe", retocArgs);
    }

    public static EnemyData GetEnemyData(string enemyCodeName)
    {
        return allEnemies.Find(e => e.CodeName == enemyCodeName) ?? new EnemyData();
    }

    public static EnemyData GetEnemyDataByName(string enemyName)
    {
        return allEnemies.Find(e => e.Name == enemyName) ?? new EnemyData();
    }
    
    public static List<EnemyData> GetEnemyDataList(List<string> enemyCodeNames)
    {
        return allEnemies.FindAll(e => enemyCodeNames.Contains(e.CodeName));
    }
    

    public static List<EnemyData> GetAllByArchetype(string archetype)
    {
        return allEnemies.Where(enemy => enemy.Archetype == archetype).ToList();
    }

    public static EnemyData GetRandomByArchetype(string archetype)
    {
        return GetEnemyData(Utils.GetRandomWeighted(EnemyFrequenciesWithinArchetype[archetype]));
    }

    public static EnemyData GetRandomEnemy()
    {
        var bannedEnemies = CustomEnemyPlacement.TranslatePlacementOptions(CustomEnemyPlacement.Excluded);
        var bannedEnemyNames = bannedEnemies.Select(e => e.CodeName);

        return GetEnemyData(Utils.GetRandomWeighted(TotalEnemyFrequencies, bannedEnemyNames.ToList()));
    }

    public static void ModifyEncounter(Encounter encounter)
    {
        if (!SpecialRules.Randomizable(encounter))
        {
            return;
        }
        
        var newEncounterSize = !Settings.RandomizeEncounterSizes || Settings.PossibleEncounterSizes.Count == 0 ? encounter.Enemies.Count :
                Utils.Pick(Settings.PossibleEncounterSizes);

        if (Settings.EnableEnemyOnslaught)
        {
            newEncounterSize += Settings.EnemyOnslaughtAdditionalEnemies;
            newEncounterSize = int.Min(newEncounterSize, Settings.EnemyOnslaughtEnemyCap);
        }

        if (newEncounterSize < encounter.Size)
        {
            encounter.RemoveEnemies(encounter.Size - newEncounterSize);
        }

        for (int i = 0; i < newEncounterSize; i++)
        {
            if (i < encounter.Size)
            {
                EnemyData newEnemy = CustomEnemyPlacement.Replace(encounter.Enemies[i]);
                encounter.SetEnemy(i, newEnemy);
            }
            else
            {
                encounter.AddEnemy(CustomEnemyPlacement.Replace(GetRandomEnemy()));
            }
        }
        
        SpecialRules.ApplySpecialRules(encounter);
    }

    public static void WriteReport(string exportPath)
    {
        using StreamWriter reportFile = new StreamWriter(exportPath + "enemies_report.txt");
        reportFile.Write(Report.GenerateString());
    }
    
    public static void ProcessEncounters(string assetPath, List<Encounter> fromList=null)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, mappings);

        var dataTable = asset.Exports[0] as DataTableExport;
        var encounters = dataTable.Table.Data;

        foreach (var encounterStruct in encounters)
        {
            var encounter = new Encounter(encounterStruct, asset);
            if (fromList != null)
            {
                encounter.SetEnemies(fromList.Find(e => e.Name == encounter.Name)?.Enemies);
            }
            else
            {
                ModifyEncounter(encounter);
            }
            ProcessedEncounters.Add(encounter);
        }

        var path = "randomizer\\Sandfall\\Content\\jRPGTemplate\\Datatables";
        Directory.CreateDirectory(path);   
        Directory.CreateDirectory(path + "\\Encounters_Datatables");   
        asset.Write(path + assetPath.Replace("Data/Originals", ""));
    }

    public static void Randomize()
    {
        usedSeed = Settings.Seed != -1 ? Settings.Seed : Environment.TickCount; 
        rand = new Random(usedSeed);
        
        Report = new GenerationReport
        {
            Seed = usedSeed
        };

        CustomEnemyPlacement.UpdateFinalEnemyReplacementFrequencies();
        ProcessEncounters("Data/Originals/DT_jRPG_Encounters.uasset");
        Report.DT_jRPG_Encounters = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();
        
        ProcessEncounters("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        Report.DT_jRPG_Encounters_CleaTower = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();
        
        ProcessEncounters("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        Report.DT_Encounters_Composite = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();
        
        ProcessEncounters("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
        Report.DT_WorldMap_Encounters = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();
        
        PackAndConvertData();
    }
    
    public static GenerationReport ReadReport(string pathToFile)
    {
        var currentAssetName = "";
        var report = new GenerationReport();
        foreach (var line in File.ReadLines(pathToFile, Encoding.UTF8))
        {
            if (report.Seed == 0)
            {
                report.Seed = int.Parse(line);
                continue;
            }
            if (!line.Contains("|"))
            {
                currentAssetName = line;
                continue;
            }

            var newEncounter = new Encounter(line.Split('|')[0], line.Split('|')[1].Split(',').ToList());
            switch (currentAssetName)
            {
                case "DT_jRPG_Encounters":
                    report.DT_jRPG_Encounters.Add(newEncounter);
                    break;
                case "DT_jRPG_Encounters_CleaTower":
                    report.DT_jRPG_Encounters_CleaTower.Add(newEncounter);
                    break;
                case "DT_Encounters_Composite":
                    report.DT_Encounters_Composite.Add(newEncounter);
                    break;
                case "DT_WorldMap_Encounters":
                    report.DT_WorldMap_Encounters.Add(newEncounter);
                    break;
            }
        }

        return report;
    }

    public static void GenerateFromReport(string pathToFile)
    {
        Report = new GenerationReport();
        var report = ReadReport(pathToFile);
        usedSeed = report.Seed;
        Report.Seed = usedSeed;
        ProcessEncounters("Data/Originals/DT_jRPG_Encounters.uasset", report.DT_jRPG_Encounters);
        Report.DT_jRPG_Encounters = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();
        
        ProcessEncounters("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset", report.DT_jRPG_Encounters_CleaTower);
        Report.DT_jRPG_Encounters_CleaTower = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();
        
        ProcessEncounters("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset", report.DT_Encounters_Composite);
        Report.DT_Encounters_Composite = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();
        
        ProcessEncounters("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset", report.DT_WorldMap_Encounters);
        Report.DT_WorldMap_Encounters = new List<Encounter>(ProcessedEncounters);
        ProcessedEncounters.Clear();

        PackAndConvertData();
    }

    public static void CheckBrokenEnemies()
    {
        var enemiesToTest = new List<string>()
        {
            "MM_Gargant_ALPHA",
            "SL_Sapling_CrushingWall",
            "GDC_Dualliste",
            "FB_Dualliste_Phase2",
            "CAMP_PunchingBall",
            
            
            "YF_GaultA",
            "AS_Gestral_Dragoon",
            "AS_GestralBully_A",
            
            "AS_GestralBully_B",
            "AS_GestralBully_C",
            "SC_FearLight",
            
            "SC_SapNevronBoss",
            "SC_Gestral_Sonnyso",
            "FB_DuallisteR",
            
            "FB_DuallisteL", 
            "FB_Dualliste_Phase1",  // BROKEN
            "Test_PlaceHolderBattleDude",
            
            "SM_Lancelier_AlternatifA",
            "SM_Lancelier_AlternatifB",
            "SM_Lancelier_AlternatifC",
            
            "SM_Lancelier_AlternatifD",
            "TestMichel_Lancelier",
            "YF_Gault_AlternativA",
            
            "YF_Gault_AlternativB",
            "YF_Gault_AlternativC",
            "YF_Gault_AlternativD",
            
            "YF_Gault_AlternativE",
            "SM_Portier_AlternativA",
            "SM_Portier_AlternativB",
            
            "SM_Portier_AlternativC",
            "SM_Portier_AlternativD",
            "SM_Portier_AlternativE",
            
            "SM_Volester_AlternativA",
            "SM_Volester_AlternativB",
            "SM_Volester_AlternativC",
            
            "SM_Volester_AlternativD",
            "SM_Volester_AlternativE",
            "YF_Potier_AlternativeA",
            
            "YF_Potier_AlternativeB",
            "YF_Potier_AlternativeC",
            "YF_Potier_AlternativeD",
            
            "YF_Potier_AlternativeE",
            "YF_Sapling_AlternativeA",
            "YF_Sapling_AlternativeB",
            
            "YF_Sapling_AlternativeC",
            "YF_Jar_AlternativeA",
            "YF_Jar_AlternativeB",
            
            "YF_Jar_AlternativeC",
            "CZ_ChromaMaelle",
            "CZ_ChromaVerso",
            
            "CZ_ChromaLune",
            "SI_Glissando_Alpha", //Always summons Simon???
            "CZ_ChromaSciel",
            
            "QUEST_DemineurWithoutMine",
            "ML_PaintressIntro", //Killable with gradient attacks
            "OL_MirrorRenoir_HealingSource",
            
            "WM_Sprong",
            "L_MaelleTutorial_Civilian",
            "CZ_ChromaMonoco",
        };
        var asset = new UAsset("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset", EngineVersion.VER_UE5_4, mappings);

        var dataTable = asset.Exports[0] as DataTableExport;
        var encounters = dataTable.Table.Data;
        int total = 0;
        foreach (var encounterStruct in encounters)
        {
            if (total >= enemiesToTest.Count)
            {
                break;
            }
            var encounter = new Encounter(encounterStruct, asset);
            for (int i = 0; i < encounter.Size; i++)
            {
                encounter.SetEnemy(i, GetEnemyData(enemiesToTest[total]));
                total += 1;
            }
            ProcessedEncounters.Add(encounter);
        }

        asset.Write("randomizer\\Sandfall\\Content\\jRPGTemplate\\DataTables\\DT_jRPG_Encounters_CleaTower.uasset");
    }
}