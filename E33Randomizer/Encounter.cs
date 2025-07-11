using UAssetAPI;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class Encounter
{
    private StructPropertyData encounterData;
    private UAsset asset;
    public String Name;
    public List<EnemyData> Enemies;
    public ArchetypeGroup Archetypes;
    public int LevelOverride;
    public bool IsBossEncounter;

    public int Size => Enemies.Count;

    public Encounter(StructPropertyData encounterData, UAsset asset)
    {
        this.encounterData = encounterData;
        this.asset = asset;
        
        Name = encounterData.Name.ToString();
        Enemies = new List<EnemyData>();
        var enemyArchetypes = new List<String>();
        var enemiesData = encounterData.Value[0] as MapPropertyData;
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
    }

    public Encounter(String encounterName, List<String> encounterEnemies)
    {
        Name = encounterName;
        Enemies = RandomizerLogic.GetEnemyDataList(encounterEnemies);
    }
    
    public void SetEnemy(int index, EnemyData newEnemy)
    {
        if (index >= Enemies.Count())
        {
            return;
        }
        var enemiesData = encounterData.Value[0] as MapPropertyData;
        var enemies = enemiesData.Value.Values.ToList();
        var enemyName = (enemies[index] as StructPropertyData).Value[1] as NamePropertyData;
        enemyName.Value = FName.FromString(asset, newEnemy.CodeName);
        Enemies[index] = newEnemy;
        Archetypes.PossibleArchetypes[index] = newEnemy.Archetype;
    }

    public void SetOverrideLevel(int newLevel)
    {
        LevelOverride = newLevel;
        var levelData = encounterData.Value[2];
    }

    public void RemoveRandomEnemy()
    {
        int enemyIndex = RandomizerLogic.rand.Next(Size);
        
        var enemiesData = encounterData.Value[0] as MapPropertyData;
        enemiesData.Value.RemoveAt(enemyIndex);
        Enemies.RemoveAt(enemyIndex);
        Archetypes.PossibleArchetypes.RemoveAt(enemyIndex);

        for (int i = 0; i < Size; i++)
        {
            var dummyEnemyKey = enemiesData.Value.Keys.ElementAt(i) as IntPropertyData;
            dummyEnemyKey.Value = i;
        }
    }
    
    public void RemoveEnemies(int number)
    {
        while (Enemies.Count > 1 && number > 0)
        {
            RemoveRandomEnemy();
            number -= 1;
        }
    }

    public void AddEnemy(EnemyData enemy)
    {
        var enemiesData = encounterData.Value[0] as MapPropertyData;
        var dummyEnemyKey = enemiesData.Value.Keys.ElementAt(0).Clone() as IntPropertyData;
        dummyEnemyKey.Value = Enemies.Count;
        
        var dummyEnemy = enemiesData.Value.Values.ElementAt(0).Clone() as StructPropertyData;
        var enemyName = dummyEnemy.Value[1] as NamePropertyData;
        enemyName.Value = FName.FromString(asset, enemy.CodeName);
        enemiesData.Value.Add(dummyEnemyKey, dummyEnemy);
        Enemies.Add(enemy);
        Archetypes.PossibleArchetypes.Add(enemy.Archetype);
    }

    public void SetEnemies(List<EnemyData> enemies)
    {
        if (enemies == null)
        {
            return;
        }
        if (enemies.Count < Size)
        {
            RemoveEnemies(Size - enemies.Count);
        }
        
        for (int i = 0; i < enemies.Count; i++)
        {
            if (i < Size)
            {
                SetEnemy(i, enemies[i]);
            }
            else
            {
                AddEnemy(enemies[i]);
            }
        }
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