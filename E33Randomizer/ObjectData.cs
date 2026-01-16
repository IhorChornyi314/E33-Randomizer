namespace E33Randomizer;

public abstract class ObjectData
{
    public string CustomName = "";
    public string CodeName = "";
    public bool IsBroken = false;
    public bool IsCutContent = false;
    
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