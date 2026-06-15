using System.Text.Json.Serialization;

namespace E33Randomizer.ObjectDatum;

[JsonDerivedType(typeof(CharacterData))]
[JsonDerivedType(typeof(CheckData))]
[JsonDerivedType(typeof(EnemyData))]
[JsonDerivedType(typeof(ItemData))]
[JsonDerivedType(typeof(LocationData))]
[JsonDerivedType(typeof(SkillData))]
[JsonDerivedType(typeof(SpawnPointData))]
public abstract class ObjectData
{
    public string CustomName { get; set; } = string.Empty;
    public string CodeName { get; set; } = string.Empty;
    public bool IsBroken { get; set; }
    public bool IsCutContent { get; set; }
    
    public override bool Equals(object? obj)
    {
        var data = obj as ObjectData;
        return obj != null && data != null && data.CodeName == CodeName;
    }
    
    public override string ToString()
    {
        return CodeName;
    }
}