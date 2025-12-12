using UAssetAPI;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class Encounter
{
    private StructPropertyData _encounterData;
    private UAsset _asset;
    public string Name;
    public List<EnemyData> Enemies;
    public List<EnemyLootDrop> PossibleLootDrops = new();
    public bool IsBossEncounter = false;
    public bool IsBroken;
    public EnemyData LootEnemy;
    
    private bool fleeImpossible;
    private int levelOverride;
    private bool disableCameraEndMovement;
    private bool disableReactionBattleLines;
    private bool isNarrativeBattle;
    
    public int Size => Enemies.Count;

    public Encounter(StructPropertyData encounterData, UAsset asset)
    {
        _encounterData = encounterData;
        _asset = asset;
        
        Name = _encounterData.Name.ToString();
        Enemies = [];
        var enemyArchetypes = new List<string>();
        var enemiesData = _encounterData.Value[0] as MapPropertyData;
        foreach (StructPropertyData enemy in enemiesData.Value.Values)
        {
            var enemyName = enemy.Value[1] as NamePropertyData;
            var enemyCodeName = enemyName.Value.Value.Value;
            if (EnemyData.MismatchedEnemyCodeNames.ContainsKey(enemyCodeName))
            {
                enemyCodeName = EnemyData.MismatchedEnemyCodeNames[enemyCodeName];
            }
            var enemyData = Controllers.EnemiesController.GetObject(enemyCodeName);
            if (enemyData.IsBroken)
            {
                IsBroken = true;
            }
            Enemies.Add(enemyData);
            if (enemyData.Archetype == "Boss" || enemyData.Archetype == "Alpha")
            {
                IsBossEncounter = true;
            }
            enemyArchetypes.Add(enemyData.Archetype);
        }

        fleeImpossible = (_encounterData.Value[1] as BoolPropertyData).Value;
        levelOverride = (_encounterData.Value[2] as IntPropertyData).Value;
        disableCameraEndMovement = (_encounterData.Value[3] as BoolPropertyData).Value;
        disableReactionBattleLines = (_encounterData.Value[4] as BoolPropertyData).Value;
        isNarrativeBattle = (_encounterData.Value[5] as BoolPropertyData).Value;
        PossibleLootDrops =  PossibleLootDrops.Distinct().ToList();
    }

    public void SaveToStruct(StructPropertyData encounterStruct)
    {
        var enemiesField = encounterStruct.Value[0] as MapPropertyData;
        var fleeImpossibleField  = encounterStruct.Value[1] as BoolPropertyData;
        var levelOverrideField = encounterStruct.Value[2] as IntPropertyData;
        var disableCameraEndMovementField  = encounterStruct.Value[3] as BoolPropertyData;
        var disableReactionBattleLinesField  = encounterStruct.Value[4] as BoolPropertyData;
        var isNarrativeBattleField  = encounterStruct.Value[5] as BoolPropertyData;
        
        var dummyEnemyStruct = (enemiesField.Clone() as MapPropertyData).Value.First();
        
        enemiesField.Value.Clear();
        for (int i = 0; i < Size; i++)
        {
            var dummyEnemyKey = dummyEnemyStruct.Key.Clone() as IntPropertyData;
            dummyEnemyKey.Value = i;
        
            var dummyEnemy = dummyEnemyStruct.Value.Clone() as StructPropertyData;
            var enemyName = dummyEnemy.Value[1] as NamePropertyData;
            var enemyCodeName = Enemies[i].CodeName;
            if (i == 0 && LootEnemy != null)
            {
                enemyCodeName = LootEnemy.CodeName;
            }
            enemyName.Value.Value = FString.FromString(enemyCodeName);
            enemiesField.Value.Add(dummyEnemyKey, dummyEnemy);
        }

        fleeImpossibleField.Value = fleeImpossible;
        levelOverrideField.Value = levelOverride;
        disableCameraEndMovementField.Value = disableCameraEndMovement;
        disableReactionBattleLinesField.Value = disableReactionBattleLines;
        isNarrativeBattleField.Value = isNarrativeBattle;
    }
    
    public override bool Equals(object? obj)
    {
        return obj != null && (obj as Encounter).Name == Name;
    }

    public override string ToString()
    {
        return $"{Name}|" + String.Join(",", Enemies.Select(e => e.CodeName));
    }
}