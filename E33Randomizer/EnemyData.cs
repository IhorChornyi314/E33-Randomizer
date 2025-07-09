namespace E33Randomizer;


public class EnemyData
{
    public String Name = "Place holder battle";
    public String CodeName = "Test_PlaceHolderBattleDude";
    public String Level = "1";
    public String Archetype = "Regular";
    public bool IsBoss => Archetype == "Boss" || Archetype == "Alpha";

    public EnemyData(String name)
    {
        var existingData = RandomizerLogic.allEnemies.Find(enemy => enemy.CodeName == name);
        if (existingData != null)
        {
            Name = existingData.Name;
            CodeName = existingData.CodeName;
            Level = existingData.Level;
            Archetype = existingData.Archetype;
        }
    }
    
    public EnemyData(){}

    public override string ToString()
    {
        return CodeName;
    }
}