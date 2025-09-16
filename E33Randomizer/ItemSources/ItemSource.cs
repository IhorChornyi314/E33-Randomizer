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

    public static ItemSourceParticle Clone(ItemSourceParticle other)
    {
        var newParticle = new ItemSourceParticle(other.Item, other.Quantity, other.LootDropChance, other.IsLootTableChest, other.MerchantInventoryLocked);
        return newParticle;
    }
    
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
    public Dictionary<string, List<ItemSourceParticle>> SourceSections = new();
    public string FolderName;
    public string FileName;
    public List<CheckData> Checks = new();
    public bool HasItemQuantities;
    protected UAsset _asset;
    protected int _minNumberOfItems = -1;
    protected int _maxNumberOfItems = -1;
    protected bool _changeNumberOfItems;
    
    public virtual void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FolderName = asset.FolderName.ToString();
        FileName = FolderName.Split('/').Last();
        SourceSections.Clear();
        Checks.Clear();
        SourceSections.Clear();
    }
    public abstract UAsset SaveToAsset();
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
    }
    public void RemoveItem(string key, int index)
    {
        SourceSections[key].RemoveAt(index);
    }

    public void SetItem(string key, int index, ItemData item)
    {
        SourceSections[key][index].Item = item;
    }

    public void RandomizeNumberOfItems(int min, int max)
    {
        foreach (var sourceSection in SourceSections)
        {
            if (!RandomizerLogic.Settings.ChangeSizesOfNonRandomizedChecks && sourceSection.Value.Count > 0)
            {
                var encounterRandomized = sourceSection.Value.Any(e => !RandomizerLogic.CustomItemPlacement.NotRandomizedCodeNames.Contains(e.Item.CodeName));
                if (!encounterRandomized) continue;
            }
            
            var newSize = Utils.Between(min, max);
            var oldSize = sourceSection.Value.Count;
            if (oldSize == newSize) continue;
            if (oldSize > newSize)
            {
                sourceSection.Value.RemoveRange(newSize, oldSize - newSize);
                continue;
            }

            if (oldSize == 0)
            {
                for (int i = 0; i < newSize; i++)
                    sourceSection.Value.Add(new ItemSourceParticle(ItemsController.GetRandomItem(), 1));
                continue;
            }
            
            for (int i = 0; i < newSize - oldSize; i++)
            {
                sourceSection.Value.Add(ItemSourceParticle.Clone(sourceSection.Value[i]));
            }
        }
    }

    public virtual void Randomize()
    {
        if (_changeNumberOfItems) RandomizeNumberOfItems(_minNumberOfItems, _maxNumberOfItems);
        foreach (var rewardData in SourceSections)
        {
            foreach (var item in rewardData.Value)
            {
                var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.Item.CodeName);
                item.Item = ItemsController.GetItemData(newItemName);
                item.Quantity = item.Item.HasQuantities ? item.Quantity : 1;
                if (HasItemQuantities && RandomizerLogic.Settings.ChangeItemQuantity && item.Item.HasQuantities)
                {
                    item.Quantity = Utils.Between(RandomizerLogic.Settings.ItemQuantityMin, RandomizerLogic.Settings.ItemQuantityMax);
                }
            }
        }
    }
    
    public override string ToString()
    {
        return FileName;
    }
}