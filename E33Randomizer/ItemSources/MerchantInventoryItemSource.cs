using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class MerchantInventoryItemSource: ItemSource
{
    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);
        HasItemQuantities = true;
        SourceSections[""] = [];
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var soldItemData in tableData)
        {
            var itemName = soldItemData.Name.ToString();
            var itemData = ItemsController.GetItemData(itemName);
            var itemQuantity = (soldItemData.Value[3] as IntPropertyData).Value;
            var itemLocked = (soldItemData.Value[4] as ObjectPropertyData).Value.Index != 0;
            
            SourceSections[""].Add(new ItemSourceParticle(itemData, itemQuantity, locked: itemLocked));
            Items.Add(itemData);
        }
        
        var check = new CheckData
        {
            CodeName = FileName,
            CustomName = FileName.Replace("DT_Merchant_", ""),
            IsBroken = false,
            IsPartialCheck = false,
            ItemSource = this,
            Key = ""
        };
        Checks.Add(check);
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
        
        foreach (var inventoryItem in SourceSections[""])
        {
            _asset.AddNameReference(FString.FromString(inventoryItem.Item.CodeName));
            var newItemStruct = dummyItemStruct.Clone() as StructPropertyData;
            newItemStruct.Name = FName.FromString(_asset, inventoryItem.Item.CodeName);
            (newItemStruct.Value[0] as NamePropertyData).Value = FName.FromString(_asset, inventoryItem.Item.CodeName);
            (newItemStruct.Value[3] as IntPropertyData).Value = inventoryItem.Quantity;
            if (dummyConditionStructLocked != null && inventoryItem.MerchantInventoryLocked || dummyConditionStructUnlocked == null)
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
        Items.Clear();
        foreach (var item in SourceSections[""])
        {
            var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.Item.CodeName);
            item.Item = ItemsController.GetItemData(newItemName);
            Items.Add(item.Item);
        }
    }
}