using System.IO;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;
namespace E33Randomizer;


public static class RandomizerLogic
{
    public static List<string> BrokenEnemies =
    [
        "QUEST_WeaponlessChalier",
        "Boss_Simon_ALPHA",
        "FB_Dualliste_Phase1",
        "YF_Jar_AlternativeA",
        "YF_Jar_AlternativeB",
        "SM_Volester_AlternativA",
        "SM_Volester_AlternativB",
        "SM_Volester_AlternativC",
        "Petank_Parent"
    ];
    public static List<string> BrokenItems =
    [
        
    ];
    public static Usmap mappings;
    public static Dictionary<string, string> EnemyCustomNames = new ();
    public static Dictionary<string, string> ItemCustomNames = new ();
    public static CustomEnemyPlacement CustomEnemyPlacement = new ();
    public static CustomItemPlacement CustomItemPlacement = new ();
    
    public static Random rand;
    public static int usedSeed;
    public static Dictionary<string, Dictionary<string, float>> EnemyFrequenciesWithinArchetype = new();
    public static Dictionary<string, float> TotalEnemyFrequencies;
    public static string PresetName = "";
    public static string DataDirectory = "Data";

    public static List<string> Archetypes =
        ["Regular", "Weak", "Strong", "Elite", "Boss", "Alpha", "Elusive", "Petank"];

    public static void Init(string dataDirectory)
    {
        DataDirectory = dataDirectory;
        usedSeed = Settings.Seed != -1 ? Settings.Seed : Environment.TickCount & Int32.MaxValue; 
        rand = new Random(usedSeed);
    
        mappings = new Usmap($"{DataDirectory}/Mappings.usmap");
        EnemiesController.Init();
        using (StreamReader r = new StreamReader($"{DataDirectory}/enemy_data.json"))
        {
            string json = r.ReadToEnd();
            var enemyList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
            foreach (var enemyCustomData in enemyList)
            {
                EnemyCustomNames[enemyCustomData["CodeName"]] = enemyCustomData["Name"];
            }
        }
        EnemiesController.ReadAsset($"{DataDirectory}/Originals/DT_jRPG_Enemies.uasset");
        
        using (StreamReader r = new StreamReader($"{DataDirectory}/item_data.json"))
        {
            string json = r.ReadToEnd();
            var itemList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
            foreach (var itemCustomData in itemList)
            {
                ItemCustomNames[itemCustomData["CodeName"]] = itemCustomData["CustomName"];
            }
        }
        ItemsController.Init();
        
        ConstructTotalEnemyFrequencies();
        ConstructEnemyFrequenciesWithinArchetype();
        CustomEnemyPlacement.InitPlainNames();
        CustomItemPlacement.InitPlainNames();
        SpecialRules.Reset();
        EncountersController.ReadEncounterAssets();
        EncountersController.ConstructEncountersByLocation();
    }

    public static void ConstructTotalEnemyFrequencies()
    {
        TotalEnemyFrequencies = new Dictionary<string, float>();
        foreach (var enemyData in EnemiesController.enemies)
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
            foreach (var enemyData in EnemiesController.enemies.FindAll(e => e.Archetype == archetype))
            {
                EnemyFrequenciesWithinArchetype[archetype][enemyData.CodeName] = 1;
            }
        }
    }

    public static void PackAndConvertData(bool writeTxt=true)
    {
        var presetName = PresetName.Length == 0 ? usedSeed.ToString() : PresetName;
        var exportPath = $"rand_{presetName}/";
        if (Directory.Exists("randomizer"))
        {
            Directory.Delete("randomizer", true);
        }
        Directory.CreateDirectory(exportPath);
        
        if (writeTxt)
        {
            EncountersController.WriteEncountersTxt(exportPath + "encounters.txt");
        }

        if (Settings.TieDropsToEncounters)
        {
            EnemiesController.ClearEnemyDrops();
            EncountersController.HandleLoot();
        }
        EncountersController.WriteEncounterAssets();
        if (Settings.TieDropsToEncounters && !Settings.RandomizeItems)
        {
            EnemiesController.WriteAsset();
        }

        if (Settings.RandomizeItems)
        {
            ItemsController.WriteItemAssets();
            ItemsController.WriteTableAsset();
        }
        
        var repackArgs = $"pack randomizer \"{exportPath}randomizer_P.pak\"";
        var retocArgs = $"to-zen --version UE5_4 \"{exportPath}randomizer_P.pak\" \"{exportPath}randomizer_P.utoc\"";

        Process.Start("repak.exe", repackArgs).WaitForExit();
        Process.Start("retoc.exe", retocArgs);
        EnemiesController.Reset();
        EncountersController.Reset();
    }

    public static EnemyData GetRandomByArchetype(string archetype)
    {
        return EnemiesController.GetEnemyData(Utils.GetRandomWeighted(EnemyFrequenciesWithinArchetype[archetype]));
    }

    public static EnemyData GetRandomEnemy()
    {
        return EnemiesController.GetEnemyData(Utils.GetRandomWeighted(CustomEnemyPlacement.DefaultFrequencies, CustomEnemyPlacement.ExcludedCodeNames));
    }
    
    public static ItemData GetRandomItem()
    {
        return ItemsController.GetItemData(Utils.GetRandomWeighted(CustomItemPlacement.DefaultFrequencies, CustomItemPlacement.ExcludedCodeNames));
    }

    public static void Randomize(bool saveData = true)
    {
        usedSeed = Settings.Seed != -1 ? Settings.Seed : Environment.TickCount; 
        rand = new Random(usedSeed);
        EncountersController.GenerateNewEncounters();
        ItemsController.GenerateNewItemChecks();
        if (saveData)
            PackAndConvertData();
    }

    public static void GenerateConditionCheckerFile(string questName)
    {
        var asset = new UAsset($"{DataDirectory}/Originals/DA_ConditionChecker_Merchant_GrandisStation.uasset", EngineVersion.VER_UE5_4, mappings);

        var newConditionalName = $"DA_ConditionChecker_Merchant_{questName}";
        
        asset.SetNameReference(2, FString.FromString(questName.Split("999")[0]));
        asset.SetNameReference(20, FString.FromString(questName.Split("999")[1]));
        asset.SetNameReference(5, FString.FromString(newConditionalName));
        asset.SetNameReference(6, FString.FromString($"/Game/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker/{newConditionalName}"));

        var e = asset.Exports[1] as NormalExport;
        (e.Data[0] as TextPropertyData).Value = FString.FromString("ST_GM_MERCHANT_CONDITION_REACH_A_POINT");
        (e.Data[1] as TextPropertyData).Value = FString.FromString("ST_GM_MERCHANT_CONDITION_REACH_A_POINT_DESC");
        
        
        asset.FolderName = FString.FromString($"/Game/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker/{newConditionalName}");
        
        Directory.CreateDirectory("randomizer/Sandfall/Content/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker");
        asset.Write($"randomizer/Sandfall/Content/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker/{newConditionalName}.uasset");
    }

    public static void AddMerchantWaresToAsset(UAsset asset, string newWareName, int newWareQuantity = 1, string conditionChecker = "")
    {
        var e = asset.Exports[0] as DataTableExport;
        
        var ccPath = $"/Game/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker/{conditionChecker}";
        
        var dummyEntry = e.Table.Data[0].Clone() as StructPropertyData;
        
        (dummyEntry.Value[0] as NamePropertyData).FromString(new string[]{newWareName}, asset);
        (dummyEntry.Value[3] as IntPropertyData).Value = newWareQuantity;
        dummyEntry.Name = FName.FromString(asset, newWareName);
        
        if (conditionChecker != "")
        {
            var outerImport = new Import("/Script/CoreUObject", "Package", FPackageIndex.FromRawIndex(0), ccPath, false, asset);

            var existingOuterImportIndex = FPackageIndex.FromRawIndex(asset.SearchForImport(outerImport.ObjectName));
            
            var index2 = existingOuterImportIndex.Index != 0 ? existingOuterImportIndex : asset.AddImport(outerImport);
            var innerImport = new Import(asset.Imports[2].ClassPackage, asset.Imports[2].ClassName, index2, FName.FromString(asset, conditionChecker), asset.Imports[2].bImportOptional);
            
            var existingInnerImportIndex = FPackageIndex.FromRawIndex(asset.SearchForImport(innerImport.ObjectName));

            var conditionImportIndex = existingInnerImportIndex.Index != 0 ? existingInnerImportIndex : asset.AddImport(innerImport);
            (dummyEntry.Value[4] as ObjectPropertyData).Value = conditionImportIndex;
        }
         
        e.Table.Data.Add(dummyEntry);
    }

    public static void AddJujubreeWares(UAsset asset)
    {
        // ConsumableUpgradeMaterial_Revive - Lampmaster?
        var quest = "Main_GoldenPath99914_DefeatthePaintress";
        GenerateConditionCheckerFile(quest);
        AddMerchantWaresToAsset(asset, "OverPowered", conditionChecker: $"DA_ConditionChecker_Merchant_{quest}");
        //AddMerchantWaresToAsset(asset, "Quest_MaellePainterSkillsUnlock", conditionChecker: $"DA_ConditionChecker_Merchant_{quest}");
    }

    public static void ProcessKeyItems()
    {
        var asset = new UAsset($"{DataDirectory}/Originals/DT_Merchant_GestralVillage1.uasset", EngineVersion.VER_UE5_4, mappings);
        // if (Settings.EnableJujubreeToSellKeyItems)
        // {
        //     AddJujubreeWares(asset);
        // }
        Directory.CreateDirectory("randomizer/Sandfall/Content/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker");
        Directory.CreateDirectory("randomizer/Sandfall/Content/Gameplay/Inventory/Merchant/Merchants_Content_DT");
        asset.Write("randomizer/Sandfall/Content/Gameplay/Inventory/Merchant/Merchants_Content_DT/DT_Merchant_GestralVillage1.uasset");
    }

    public static void Test()
    {
        string json = File.ReadAllText($"{DataDirectory}/Temp/DA_ConditionChecker_Merchant_GrandisStation.json");
        UAsset asset = UAsset.DeserializeJson(json);
        asset.Mappings = mappings;
        asset.Write("test.uasset");
        Console.WriteLine("!");
    }
}