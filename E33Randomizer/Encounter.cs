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
    public bool IsBossEncounter;
    
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
            var enemyData = RandomizerLogic.GetEnemyData(enemyName.Value.Value.Value);
            Enemies.Add(enemyData);
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
    }

    public Encounter(string encounterName, List<string> encounterEnemies)
    {
        Name = encounterName;
        Enemies = RandomizerLogic.GetEnemyDataList(encounterEnemies);
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
            enemyName.Value.Value = FString.FromString(Enemies[i].CodeName);
            enemiesField.Value.Add(dummyEnemyKey, dummyEnemy);
        }

        fleeImpossibleField.Value = fleeImpossible;
        levelOverrideField.Value = levelOverride;
        disableCameraEndMovementField.Value = disableCameraEndMovement;
        disableReactionBattleLinesField.Value = disableReactionBattleLines;
        isNarrativeBattleField.Value = isNarrativeBattle;
    }

    public override string ToString()
    {
        var rep = Name + "|";
        foreach (var enemy in Enemies)
        {
            rep += $",{enemy.CodeName}";
        }
        // This is a hack but I'm too tired to write something better
        return rep.Replace("|,", "|");
    }
}