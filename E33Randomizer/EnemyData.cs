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
    
    public string Archetype = "Regular";
    // For balance reasons Osquio is considered a Strong enemy
    public bool IsBoss => Archetype == "Boss" || Archetype == "Alpha" || CodeName == "VD_Osquio";

    public EnemyData()
    {
        CustomName = "Place holder battle";
        CodeName = "Test_PlaceHolderBattleDude";
    }
}