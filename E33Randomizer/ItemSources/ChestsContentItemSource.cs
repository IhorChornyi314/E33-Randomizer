using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

class ChestContent(ItemData item, int quantity = 1, bool isLootTable = false)
{
    public ItemData Item = item;
    public int Quantity = quantity;
    public bool IsLootTable = isLootTable; // All chests use DT_LootTable_UpgradeItems_Exploration
}

public class ChestsContentItemSource: ItemSource
{
    private Dictionary<string, List<ChestContent>> _chestsData = new();
    private StructPropertyData _lootTableStruct;
    public override void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FolderName = asset.FolderName.ToString();
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var chestData in tableData)
        {
            var chestName = chestData.Name.ToString();
            if ((chestData.Value[0] as ArrayPropertyData).Value.Length > 0)
            {
                var lootTableItemData = ItemsController.GetItemData("UpgradeMaterial_Level1");
                _chestsData[chestName] = [new ChestContent(lootTableItemData, 1, true)];
                Items.Add(lootTableItemData);
                continue;
            }

            List<ChestContent> items = new();
            foreach (var itemStruct in (((chestData.Value[1] as ArrayPropertyData).Value[0] as StructPropertyData).Value[3] as ArrayPropertyData).Value)
            {
                var itemData = ItemsController.GetItemData(((itemStruct as StructPropertyData).Value[0] as NamePropertyData).Value.ToString());
                items.Add(new ChestContent(itemData, ((itemStruct as StructPropertyData).Value[2] as IntPropertyData).Value));
                Items.Add(itemData);
            }
            _chestsData[chestName] = items;

            var areaName = chestName.Split('_')[^2];
            
            var check = new CheckData
            {
                CodeName = chestName,
                CustomName = $"{areaName}: {items[0].Item.CodeName}",
                IsBroken = false,
                IsPartialCheck = true,
                ItemSource = this,
                Key = chestName
            };
            Checks.Add(check);
        }
    }

    public override UAsset SaveToAsset()
    {
        var tableData = (_asset.Exports[0] as DataTableExport).Table.Data;
        StructPropertyData dummyLootStruct = null;
        StructPropertyData dummyLootTableStruct = null;

        foreach (var chestData in tableData)
        {
            if ((chestData.Value[0] as ArrayPropertyData).Value.Length > 0)
            {
                dummyLootTableStruct = (chestData.Value[0] as ArrayPropertyData).Value[0] as StructPropertyData;
            }
            else
            {
                dummyLootStruct = (chestData.Value[1] as ArrayPropertyData).Value[0] as StructPropertyData;
            }

            if (dummyLootStruct != null && dummyLootTableStruct != null)
            {
                break;
            }
        }
        
        foreach (var chestData in tableData)
        {
            var chestName = chestData.Name.ToString();
            
            var chestContent = _chestsData[chestName];

            List<PropertyData> newLootTableStructs = [];
            List<PropertyData> newLootStructs = [];
            
            foreach (var item in chestContent)
            {
                if (item.IsLootTable)
                {
                    var newLootTableStruct = dummyLootTableStruct.Clone() as StructPropertyData;
                    newLootTableStructs.Add(newLootTableStruct);
                }
                else
                {
                    var newLootStruct = dummyLootStruct.Clone() as StructPropertyData;
                    var newLootEntryStruct = (newLootStruct.Value[3] as ArrayPropertyData).Value[0].Clone() as StructPropertyData;
                    
                    (newLootEntryStruct.Value[0] as NamePropertyData).Value = FName.FromString(_asset, item.Item.CodeName);
                    (newLootEntryStruct.Value[2] as IntPropertyData).Value = item.Quantity;
                    (newLootStruct.Value[3] as ArrayPropertyData).Value[0] = newLootEntryStruct;
                    newLootStructs.Add(newLootStruct);
                    _asset.AddNameReference(FString.FromString(item.Item.CodeName));
                }
            }
            
            (chestData.Value[0] as ArrayPropertyData).Value = newLootTableStructs.ToArray();
            (chestData.Value[1] as ArrayPropertyData).Value = newLootStructs.ToArray();
        }

        return _asset;
    }

    public override void Randomize()
    {
        Items.Clear();
        foreach (var chestData in _chestsData)
        {
            foreach (var item in chestData.Value)
            {
                var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.Item.CodeName);
                item.Item = ItemsController.GetItemData(newItemName);
                item.Quantity = RandomizerLogic.rand.Next(3);
                item.IsLootTable = newItemName.StartsWith("UpgradeMaterial_Level") && !newItemName.EndsWith('5');
                Items.Add(item.Item);
            }
        }
    }
    
    public override List<ItemData> GetCheckItems(string key)
    {
        return _chestsData[key].Select(c => c.Item).ToList();
    }
    
    public override void AddItem(string key, ItemData item)
    {
        _chestsData[key].Add(new ChestContent(item));
    }

    public override void RemoveItem(string key, ItemData item)
    {
        _chestsData[key].RemoveAll(tr => tr.Item.CodeName == item.CodeName);
    }
}