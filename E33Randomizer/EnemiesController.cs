using System.IO;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public static class EnemiesController
{
    private static UDataTable enemiesTable;
    private static UAsset asset;

    public static List<EnemyData> enemies;
    public static Dictionary<string, EnemyData> EnemiesByName;

    public static Dictionary<int, string> enemyArchetypes = new()
    {
        {-3, "Alpha"},
        {-4, "Boss"},
        {-5, "Boss"},
        {-6, "Elite"},
        {-7, "Elusive"},
        {-8, "Boss"},
        {-9, "Petank"},
        {-10, "Regular"},
        {-11, "Strong"},
        {-12, "Weak"},
        {-15, "Alpha"},
        {-16, "Alpha"},
        {-17, "Alpha"},
        {-18, "Boss"},
        {-19, "Elite"},
        {-20, "Elusive"},
        {-21, "Regular"},
        {-22, "Strong"},
        {-23, "Weak"},
    };

    public static Dictionary<string, string> mismatchedEnemyCodeNames = new()
    {
        {"GO_Bruler_Alpha", "GO_Bruler_ALPHA"},
        {"GO_Goblu_Alpha", "GO_Goblu_ALPHA"},
        {"GO_Demineur_Alpha", "GO_Demineur_ALPHA"},
    };
    
    public static void Init()
    {
        
    }

    public static void Reset()
    {
        ReadAsset($"{RandomizerLogic.DataDirectory}/Originals/DT_jRPG_Enemies.uasset");
    }
    
    public static void ReadAsset(string assetPath)
    {
        asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        enemiesTable = (asset.Exports[0] as DataTableExport).Table;
        enemies = enemiesTable.Data.Select(e => new EnemyData(e)).ToList();
        enemies = enemies.Where(e => !e.IsBroken).ToList();
        enemies = enemies.OrderBy(e => e.CustomName).ToList();
        EnemiesByName = enemies.Select(e => new KeyValuePair<string, EnemyData>(e.CodeName, e)).ToDictionary();
    }

    public static void WriteAsset()
    {
        var assetFolder = asset.FolderName.Value.Replace("/Game", "randomizer/Sandfall/Content").Replace("/DT_jRPG_Enemies", "");
        Directory.CreateDirectory(assetFolder);
        asset.Write($"{assetFolder}/DT_jRPG_Enemies.uasset");
    }

    public static EnemyData GetEnemyData(string enemyCodeName)
    {
        return EnemiesByName.TryGetValue(enemyCodeName, out EnemyData value) ? value : new EnemyData();
    }
    
    public static List<EnemyData> GetEnemyDataList(List<string> enemyCodeNames)
    {
        return enemies.FindAll(e => enemyCodeNames.Contains(e.CodeName));
    }

    public static List<EnemyData> GetAllByArchetype(string archetype)
    {
        return enemies.Where(enemy => enemy.Archetype == archetype).ToList();
    }

    public static void ClearEnemyDrops()
    {
        foreach (var enemy in enemies)
        {
            enemy.ClearDrops();
        }
    }

    public static EnemyData AddEnemyClone(EnemyData original, string newName)
    {
        if (original.enemyDataStruct == null)
        {
            return null;
        }
        var cloneStruct = original.enemyDataStruct.Clone() as StructPropertyData;
        asset.AddNameReference(FString.FromString(newName));
        cloneStruct.Name = FName.FromString(asset, newName);
        enemiesTable.Data.Add(cloneStruct);
        var result = new EnemyData(cloneStruct);
        
        return result;
    }
}