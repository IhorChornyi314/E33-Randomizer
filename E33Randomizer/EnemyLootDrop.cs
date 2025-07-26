using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer;

public class EnemyLootDrop
{
    public StructPropertyData dataStruct;
    public string Name;
    public double Chance;
    public int Quantity;
    public int LevelOffset;

    public EnemyLootDrop(StructPropertyData lootDataStruct)
    {
        dataStruct = lootDataStruct;
        Name = ((lootDataStruct.Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString();
        Quantity = (lootDataStruct.Value[1] as IntPropertyData).Value;
        Chance = (lootDataStruct.Value[2] as DoublePropertyData).Value;
        LevelOffset = (lootDataStruct.Value[3] as IntPropertyData).Value;
    }

    public override bool Equals(object? obj)
    {
        return (obj as EnemyLootDrop).Name == Name;
    }
}