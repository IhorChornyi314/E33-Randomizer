using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

class ChestContent(ItemData itemData, int quantity = 1, bool isLootTable = false)
{
    public ItemData ItemData = itemData;
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
        FileName = asset.FolderName.ToString();
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var chestData in tableData)
        {
            var chestName = chestData.Name.ToString();
            if ((chestData.Value[0] as ArrayPropertyData).Value.Length > 0)
            {
                _chestsData[chestName] = [new ChestContent(new ItemData(), 1, true)];
                continue;
            }

            List<ChestContent> items = new();
            foreach (var itemStruct in (((chestData.Value[1] as ArrayPropertyData).Value[0] as StructPropertyData).Value[3] as ArrayPropertyData).Value)
            {
                var itemData = ItemController.GetItemData(((itemStruct as StructPropertyData).Value[0] as NamePropertyData).Value.ToString());
                items.Add(new ChestContent(itemData, ((itemStruct as StructPropertyData).Value[2] as IntPropertyData).Value));
                Items.Add(itemData);
            }
            _chestsData[chestName] = items;
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
                    
                    (newLootEntryStruct.Value[0] as NamePropertyData).Value = FName.FromString(_asset, item.ItemData.CodeName);
                    (newLootEntryStruct.Value[2] as IntPropertyData).Value = item.Quantity;
                    (newLootStruct.Value[3] as ArrayPropertyData).Value[0] = newLootEntryStruct;
                    newLootStructs.Add(newLootStruct);
                    _asset.AddNameReference(FString.FromString(item.ItemData.CodeName));
                }
            }
            
            (chestData.Value[0] as ArrayPropertyData).Value = newLootTableStructs.ToArray();
            (chestData.Value[1] as ArrayPropertyData).Value = newLootStructs.ToArray();
        }

        return _asset;
    }

    public override void Randomize()
    {
        foreach (var chestData in _chestsData)
        {
            foreach (var item in chestData.Value)
            {
                item.ItemData = ItemController.GetRandomItem();
                item.IsLootTable = RandomizerLogic.rand.Next(0, 50) == 0;
                item.Quantity = RandomizerLogic.rand.Next(3);
            }
        }
    }
}