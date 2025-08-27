using UAssetAPI;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class GenericItemSource: ItemSource
{
    private static List<string> _questRequirementItems = ["Quest_Resin", "Quest_WoodBoards", "LostGestral", "Quest_Mine", "FestivalToken", "Quest_UniformForSon", "Quest_HexgaRock"];
    private List<string> _originalItems = [];
    public override void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FolderName = asset.FolderName.ToString();
        foreach (var name in asset.GetNameMapIndexList())
        {
            if (_questRequirementItems.Contains(name.ToString()))
            {
                continue;
            }
            if (ItemsController.IsItem(name.ToString()))
            {
                Items.Add(ItemsController.GetItemData(name.ToString()));
                _originalItems.Add(name.ToString());
            }
        }
        var check = new CheckData
        {
            CodeName = FileName,
            CustomName = FileName,
            IsBroken = false,
            IsPartialCheck = false,
            IsFixedSize = true,
            ItemSource = this
        };
        Checks.Add(check);
    }

    public override UAsset SaveToAsset()
    {
        var itemReplacements = new Dictionary<string, string>();
        for (int i = 0; i < _originalItems.Count; i++)
        {
            if (i >= Items.Count)
            {
                break;
            }
            itemReplacements[_originalItems[i]] = Items[i].CodeName;
        }
        
        for (int i = 0; i < _asset.GetNameMapIndexList().Count; i++)
        {
            if (itemReplacements.ContainsKey(_asset.GetNameReference(i).ToString()))
            {
                _asset.SetNameReference(i, FString.FromString(itemReplacements[_asset.GetNameReference(i).ToString()]));
            }
        }
        return _asset;
    }
    
    public override void Randomize()
    {
        var newItems = new List<ItemData>();
        foreach (var item in Items)
        {
            var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.CodeName);
            var newItem = ItemsController.GetItemData(newItemName);
            newItems.Add(newItem);
        }

        Items = newItems;
    }
    
    public override List<ItemData> GetCheckItems(string key)
    {
        return Items;
    }
    
    public override void AddItem(string key, ItemData item)
    {
        Items.Add(item);
    }

    public override void RemoveItem(string key, ItemData item)
    {
        Items.Remove(item);
    }
}