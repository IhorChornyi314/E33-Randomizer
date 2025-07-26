using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer;


public class EnemyData
{
    public StructPropertyData enemyDataStruct;
    public string CustomName = "Place holder battle";
    public string CodeName = "Test_PlaceHolderBattleDude";
    public int Level = 1;
    public double LootChanceMultiplier = 1;
    public List<EnemyLootDrop> PossibleLoot = new ();
    public string Archetype = "Regular";
    public bool IsBoss => Archetype == "Boss" || Archetype == "Alpha";
    public bool IsBroken = true;

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
        if (EnemiesController.enemyArchetypes.ContainsKey((enemyDataStruct.Value[6] as ObjectPropertyData).Value.Index))
        {
            Archetype = EnemiesController.enemyArchetypes[(enemyDataStruct.Value[6] as ObjectPropertyData).Value.Index];
        }
    }
    
    public EnemyData(){}

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

    public override bool Equals(object? obj)
    {
        return obj != null && (obj as EnemyData).CodeName == CodeName;
    }

    public override string ToString()
    {
        return CodeName;
    }
}