namespace E33Randomizer;

public abstract class ObjectData
{
    public string CustomName;
    public string CodeName;
    public bool IsBroken;
    
    public override bool Equals(object? obj)
    {
        return obj != null && (obj as ObjectData).CodeName == CodeName;
    }

    public override string ToString()
    {
        return CodeName;
    }
}