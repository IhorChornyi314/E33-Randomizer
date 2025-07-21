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
        "YF_Jar_AlternativeB"
    ];
    public static Usmap mappings;
    public static List<EnemyData> allEnemies;
    public static Random rand;
    public static int usedSeed;
    public static Dictionary<string, Dictionary<string, float>> EnemyFrequenciesWithinArchetype = new();
    public static Dictionary<string, float> TotalEnemyFrequencies;
    public static string PresetName = "";

    public static List<string> Archetypes =
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
        EncountersController.ReadEncounterAssets();
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

    public static void PackAndConvertData(bool writeTxt=true)
    {
        var presetName = PresetName.Length == 0 ? usedSeed.ToString() : PresetName;
        var exportPath = $"rand_{presetName}/";
        Directory.CreateDirectory(exportPath);
        if (writeTxt)
        {
            EncountersController.WriteEncountersTxt(exportPath + "encounters.txt");
        }
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

    public static EnemyData GetRandomByPlainName(string plainName)
    {
        var translatedEnemies = CustomEnemyPlacement.TranslatePlacementOption(plainName);
        var weights = new Dictionary<string, float>();
        var translatedFrequencies = CustomEnemyPlacement.TranslatedFrequencyAdjustments;
        foreach (var enemy in translatedEnemies)
        {
            weights[enemy.CodeName] = translatedFrequencies.ContainsKey(enemy.CodeName)
                ? translatedFrequencies[enemy.CodeName]
                : 1;
        }
        return GetEnemyData(Utils.GetRandomWeighted(weights, CustomEnemyPlacement.BannedEnemyNames));
    }

    public static EnemyData GetRandomEnemy()
    {
        return GetEnemyData(Utils.GetRandomWeighted(TotalEnemyFrequencies, CustomEnemyPlacement.BannedEnemyNames));
    }

    public static void Randomize()
    {
        usedSeed = Settings.Seed != -1 ? Settings.Seed : Environment.TickCount; 
        rand = new Random(usedSeed);
        EncountersController.GenerateNewEncounters();
        EncountersController.WriteEncounterAssets();
        PackAndConvertData();
    }

    public static void GenerateConditionCheckerFile(string questName)
    {
        var asset = new UAsset("Data/Originals/DA_ConditionChecker_Merchant_GrandisStation.uasset", EngineVersion.VER_UE5_4, mappings);

        var newConditionalName = $"DA_ConditionChecker_Merchant_{questName}";
        
        asset.SetNameReference(2, FString.FromString(questName));
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

            var index2 = asset.AddImport(outerImport);
            var i1 = new Import(asset.Imports[2].ClassPackage, asset.Imports[2].ClassName, index2, FName.FromString(asset, conditionChecker), asset.Imports[2].bImportOptional);
            var conditionImportIndex = asset.AddImport(i1);
            (dummyEntry.Value[4] as ObjectPropertyData).Value = conditionImportIndex;
        }
         
        e.Table.Data.Add(dummyEntry);
    }

    public static void AddJujubreeWares(UAsset asset)
    {
        // ConsumableUpgradeMaterial_Revive - Lampmaster?
        var quest = "Main_ManorInterlude";
        GenerateConditionCheckerFile(quest);
        AddMerchantWaresToAsset(asset, "OverPowered", conditionChecker: $"DA_ConditionChecker_Merchant_{quest}");
        AddMerchantWaresToAsset(asset, "Quest_MaellePainterSkillsUnlock", conditionChecker: $"DA_ConditionChecker_Merchant_{quest}");
    }

    public static void ProcessKeyItems()
    {
        var asset = new UAsset("Data/Originals/DT_Merchant_GestralVillage1.uasset", EngineVersion.VER_UE5_4, mappings);
        if (Settings.EnableJujubreeToSellKeyItems)
        {
            AddJujubreeWares(asset);
        }
        Directory.CreateDirectory("randomizer/Sandfall/Content/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker");
        asset.Write("randomizer/Sandfall/Content/Gameplay/Inventory/Merchant/Merchants_Content_DT/DT_Merchant_GestralVillage1.uasset");
    }
}