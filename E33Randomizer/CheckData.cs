using E33Randomizer.ItemSources;

namespace E33Randomizer;

public class CheckData: ObjectData
{
    public ItemSource ItemSource  { get; set; }
    public string Key  { get; set; }
    public bool IsPartialCheck  { get; set; }
    public bool IsFixedSize  { get; set; }
}