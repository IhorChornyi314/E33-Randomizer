using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer;


public class EnemyData: ObjectData
{
    public static Dictionary<int, string> EnemyArchetypes = new()
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

    public static Dictionary<string, string> MismatchedEnemyCodeNames = new()
    {
        {"GO_Bruler_Alpha", "GO_Bruler_ALPHA"},
        {"GO_Goblu_Alpha", "GO_Goblu_ALPHA"},
        {"GO_Demineur_Alpha", "GO_Demineur_ALPHA"},
    };
    
    public StructPropertyData enemyDataStruct;
    public int Level = 1;
    public double LootChanceMultiplier = 1;
    public List<EnemyLootDrop> PossibleLoot = new ();
    public string Archetype = "Regular";
    public bool IsBoss => Archetype == "Boss" || Archetype == "Alpha";

    public EnemyData(StructPropertyData enemyData)
    {
        enemyDataStruct = enemyData;
        CodeName = enemyData.Name.ToString();
        RandomizerLogic.EnemyCustomNames.TryGetValue(CodeName, out CustomName);
        
        IsBroken = RandomizerLogic.BrokenEnemies.Contains(CodeName) || CustomName == null;
        
        Level = (enemyDataStruct.Value[2] as IntPropertyData).Value;
        LootChanceMultiplier = (enemyDataStruct.Value[17] as DoublePropertyData).Value;
        var lootDataArray = enemyDataStruct.Value[10] as ArrayPropertyData;
        PossibleLoot = lootDataArray.Value.Select(l => new EnemyLootDrop(l as StructPropertyData)).ToList();
        if (EnemyArchetypes.ContainsKey((enemyDataStruct.Value[6] as ObjectPropertyData).Value.Index))
        {
            Archetype = EnemyArchetypes[(enemyDataStruct.Value[6] as ObjectPropertyData).Value.Index];
        }
    }

    public EnemyData()
    {
        CustomName = "Place holder battle";
        CodeName = "Test_PlaceHolderBattleDude";
    }

    public void ClearDrops()
    {
        var lootDataArray = enemyDataStruct.Value[10] as ArrayPropertyData;
        lootDataArray.Value = [];
    }

    public void AddDrops(List<EnemyLootDrop> drops)
    {
        PossibleLoot.AddRange(drops);
        var lootDataArray = enemyDataStruct.Value[10] as ArrayPropertyData;
        var dropStructs = drops.Select(d => d.dataStruct).ToArray();
        lootDataArray.Value = dropStructs;
    }
}