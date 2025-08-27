using UAssetAPI;

namespace E33Randomizer.ItemSources;

public abstract class ItemSource
{
    public List<ItemData> Items = new();
    public string FolderName;
    public string FileName => FolderName.Split('/').Last();
    public List<CheckData> Checks = new();
    protected UAsset _asset;
    
    public abstract void LoadFromAsset(UAsset asset);
    public abstract UAsset SaveToAsset();
    public abstract void Randomize();
    public abstract List<ItemData> GetCheckItems(string key);
    public abstract void AddItem(string key, ItemData item);
    public abstract void RemoveItem(string key, ItemData item);
    
    public override string ToString()
    {
        return FileName;
    }
}