using UAssetAPI;

namespace E33Randomizer.Checks;

public abstract class Check
{
    public UAsset Asset;
    public string Name;
    public string ItemID;
    public ItemCategory Category;
    public bool SingleFile;
    
    public abstract void SaveToAsset();
    public abstract void LoadFromAsset();
}