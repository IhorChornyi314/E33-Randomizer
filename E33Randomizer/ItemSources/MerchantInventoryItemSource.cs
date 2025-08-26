using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

class MerchantInventoryItem(ItemData item, int quantity, bool locked)
{
    public ItemData Item = item;
    public int Quantity = quantity;
    public bool Locked = locked;
}

public class MerchantInventoryItemSource: ItemSource
{
    private List<MerchantInventoryItem> _inventory = new();
     
    public override void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FileName = asset.FolderName.ToString();
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        _inventory = new();
        foreach (var soldItemData in tableData)
        {
            var itemName = soldItemData.Name.ToString();
            var itemData = ItemController.GetItemData(itemName);
            var itemQuantity = (soldItemData.Value[3] as IntPropertyData).Value;
            var itemLocked = (soldItemData.Value[4] as ObjectPropertyData).Value.Index != 0;
            
            _inventory.Add(new MerchantInventoryItem(itemData, itemQuantity, itemLocked));
            Items.Add(itemData);
        }
    }

    public override UAsset SaveToAsset()
    {
        var tableData = (_asset.Exports[0] as DataTableExport).Table.Data;

        ObjectPropertyData dummyConditionStructLocked = null;
        ObjectPropertyData dummyConditionStructUnlocked = null;
        
        foreach (var itemData in tableData)
        {
            if ((itemData.Value[4] as ObjectPropertyData).Value.Index != 0)
            {
                dummyConditionStructLocked = itemData.Value[4] as ObjectPropertyData;
            }
            else
            {
                dummyConditionStructUnlocked = itemData.Value[4] as ObjectPropertyData;
            }

            if (dummyConditionStructLocked != null && dummyConditionStructUnlocked != null)
            {
                break;
            }
        }
        
        var dummyItemStruct = tableData[0].Clone() as StructPropertyData;
        tableData.Clear();
        
        foreach (var inventoryItem in _inventory)
        {
            _asset.AddNameReference(FString.FromString(inventoryItem.Item.CodeName));
            var newItemStruct = dummyItemStruct.Clone() as StructPropertyData;
            newItemStruct.Name = FName.FromString(_asset, inventoryItem.Item.CodeName);
            (newItemStruct.Value[0] as NamePropertyData).Value = FName.FromString(_asset, inventoryItem.Item.CodeName);
            (newItemStruct.Value[3] as IntPropertyData).Value = inventoryItem.Quantity;
            if (dummyConditionStructLocked != null && inventoryItem.Locked || dummyConditionStructUnlocked == null)
            {
                newItemStruct.Value[4] = dummyConditionStructLocked.Clone() as ObjectPropertyData;
            }
            else
            {
                newItemStruct.Value[4] = dummyConditionStructUnlocked;
            }
            
            tableData.Add(newItemStruct);
        }

        return _asset;
    }
    
    public override void Randomize()
    {
        foreach (var item in _inventory)
        {
            item.Item = ItemController.GetRandomItem();
            item.Quantity = RandomizerLogic.rand.Next(10);
            item.Locked = RandomizerLogic.rand.Next(7) == 0;
        }
    }
}