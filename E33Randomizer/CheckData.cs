using E33Randomizer.ItemSources;

namespace E33Randomizer;

public class CheckData: ObjectData
{
    public ItemSource ItemSource;
    public string Key;
    public bool IsPartialCheck;
    public bool IsFixedSize;
}