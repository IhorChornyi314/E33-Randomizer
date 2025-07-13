namespace E33Randomizer;


public class EnemyData
{
    public string Name = "Place holder battle";
    public string CodeName = "Test_PlaceHolderBattleDude";
    public string Level = "1";
    public string Archetype = "Regular";
    public bool IsBoss => Archetype == "Boss" || Archetype == "Alpha";

    public EnemyData(string name)
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