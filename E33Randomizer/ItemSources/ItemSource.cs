using UAssetAPI;
using UAssetAPI.PropertyTypes.Objects;

namespace E33Randomizer.ItemSources;

public class ItemSourceParticle(ItemData item, int quantity = -1, double chance = 100, bool isLootTable = false, bool locked = false)
{
    public ItemData Item = item;
    public int Quantity = quantity;
    public double LootDropChance = chance;
    public bool IsLootTableChest = isLootTable;
    public bool MerchantInventoryLocked = locked;

    public static ItemSourceParticle FromString(string rep)
    {
        var stringParts = rep.Split(':');
        var newParticle = new ItemSourceParticle(ItemsController.GetItemData(stringParts[0]));
        newParticle.Quantity = int.Parse(stringParts[1]);
        newParticle.LootDropChance = double.Parse(stringParts[2]);
        newParticle.IsLootTableChest = bool.Parse(stringParts[3]);
        newParticle.MerchantInventoryLocked = bool.Parse(stringParts[4]);
        return newParticle;
    }
    
    public override string ToString()
    {
        return $"{Item.CodeName}:{Quantity}:{(int)LootDropChance}:{IsLootTableChest}:{MerchantInventoryLocked}";
    }
}

public abstract class ItemSource
{
    public List<ItemData> Items = new();
    public Dictionary<string, List<ItemSourceParticle>> SourceSections = new();
    public string FolderName;
    public string FileName;
    public List<CheckData> Checks = new();
    public bool HasItemQuantities;
    protected UAsset _asset;
    
    public virtual void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FolderName = asset.FolderName.ToString();
        FileName = FolderName.Split('/').Last();
        SourceSections.Clear();
        Items.Clear();
        Checks.Clear();
        SourceSections.Clear();
    }
    public abstract UAsset SaveToAsset();
    public abstract void Randomize();
    public List<ItemData> GetCheckItems(string key)
    {
        return SourceSections[key].Select(s => s.Item).ToList();
    }
    public int GetItemQuantity(string key, int itemIndex)
    {
        return SourceSections[key][itemIndex].Quantity;
    }

    public void AddItem(string key, ItemData item)
    {
        SourceSections[key].Add(new ItemSourceParticle(item, HasItemQuantities ? 1 : -1));
        Items.Add(item);
    }
    public void RemoveItem(string key, int index)
    {
        var item = SourceSections[key][index].Item;
        Items.Remove(item);
        SourceSections[key].RemoveAt(index);
    }

    public void SetItem(string key, int index, ItemData item)
    {
        SourceSections[key][index].Item = item;
        Items[Items.IndexOf(item)] = item;
    }
    
    public override string ToString()
    {
        return FileName;
    }
}