using UAssetAPI;

namespace E33Randomizer.ItemSources;

public abstract class ItemSource
{
    public List<ItemData> Items = new();
    public string FileName;
    protected UAsset _asset;
    
    public abstract void LoadFromAsset(UAsset asset);
    public abstract UAsset SaveToAsset();
    public abstract void Randomize();
    
    public override string ToString()
    {
        return FileName;
    }
}