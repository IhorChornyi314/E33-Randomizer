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
    public ArchetypeGroup Archetypes;
    public List<EnemyLootDrop> PossibleLootDrops = new();
    public bool IsBossEncounter;
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
            if (EnemiesController.mismatchedEnemyCodeNames.ContainsKey(enemyCodeName))
            {
                enemyCodeName = EnemiesController.mismatchedEnemyCodeNames[enemyCodeName];
            }
            var enemyData = EnemiesController.GetEnemyData(enemyCodeName);
            if (enemyData.IsBroken)
            {
                IsBroken = true;
            }
            Enemies.Add(enemyData);
            PossibleLootDrops.AddRange(enemyData.PossibleLoot);
            if (enemyData.Archetype == "Boss" || enemyData.Archetype == "Alpha")
            {
                IsBossEncounter = true;
            }
            enemyArchetypes.Add(enemyData.Archetype);
        }

        Archetypes = new ArchetypeGroup(enemyArchetypes);
        fleeImpossible = (_encounterData.Value[1] as BoolPropertyData).Value;
        levelOverride = (_encounterData.Value[2] as IntPropertyData).Value;
        disableCameraEndMovement = (_encounterData.Value[3] as BoolPropertyData).Value;
        disableReactionBattleLines = (_encounterData.Value[4] as BoolPropertyData).Value;
        isNarrativeBattle = (_encounterData.Value[5] as BoolPropertyData).Value;
        PossibleLootDrops =  PossibleLootDrops.Distinct().ToList();
    }

    public Encounter(string encounterName, List<string> encounterEnemies)
    {
        Name = encounterName;
        Enemies = EnemiesController.GetEnemyDataList(encounterEnemies);
        foreach (var enemyData in Enemies)
        {
            PossibleLootDrops.AddRange(enemyData.PossibleLoot);
        }
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

    public void HandleEncounterLoot()
    {
        if (Size == 0 || PossibleLootDrops.Count == 0)
        {
            return;
        }

        var result = EnemiesController.AddEnemyClone(Enemies[0], $"{Name}_{Enemies[0].CodeName}");
        if (result == null)
        {
            return;
        }
        
        result.AddDrops(PossibleLootDrops);
        LootEnemy = result;
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