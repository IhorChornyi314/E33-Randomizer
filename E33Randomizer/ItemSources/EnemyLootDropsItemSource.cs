using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

class EnemyDrop(ItemData itemData, int quantity = 1, double dropChance = 100)
{
    public ItemData Item = itemData;
    public int Quantity = quantity;
    public double DropChance = dropChance;
}

public class EnemyLootDropsItemSource: ItemSource
{
    private Dictionary<string, List<EnemyDrop>> _dropsData = new();
    private Dictionary<string, int> _enemyLevelOffsets = new();
    public override void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FileName = asset.FolderName.ToString();
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var enemyData in tableData)
        {
            var enemyName = enemyData.Name.ToString();

            List<EnemyDrop> drops = new();
            foreach (StructPropertyData itemStruct in (enemyData.Value[10] as ArrayPropertyData).Value)
            {
                var itemData = ItemController.GetItemData(((itemStruct.Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString());
                var quantity = (itemStruct.Value[1] as IntPropertyData).Value;
                var dropChance = (itemStruct.Value[2] as DoublePropertyData).Value;
                
                drops.Add(new EnemyDrop(itemData, quantity, dropChance));
                Items.Add(itemData);
                _enemyLevelOffsets[enemyName] = (itemStruct.Value[3] as IntPropertyData).Value;
            }
            _dropsData[enemyName] = drops;
        }
    }

    public override UAsset SaveToAsset()
    {
        var tableData = (_asset.Exports[0] as DataTableExport).Table.Data;
        StructPropertyData dummyDropStruct = null;
        ObjectPropertyData compositeTableReference = null;
        foreach (var enemyData in tableData)
        {
            if ((enemyData.Value[10] as ArrayPropertyData).Value.Length > 0)
            {
                dummyDropStruct = (enemyData.Value[10] as ArrayPropertyData).Value[0].Clone() as StructPropertyData;
                compositeTableReference = (dummyDropStruct.Value[0] as StructPropertyData).Value[0].Clone() as ObjectPropertyData;
            }

            if (dummyDropStruct != null)
            {
                break;
            } 
        }
        
        foreach (var enemyData in tableData)
        {
            var enemyName = enemyData.Name.ToString();
            var enemyDrops = _dropsData[enemyName];
            List<PropertyData> newDrops = [];

            foreach (var drop in enemyDrops)
            {
                _asset.AddNameReference(FString.FromString(drop.Item.CodeName));
                var newDropStruct = dummyDropStruct.Clone() as StructPropertyData;
                var newItemStruct = newDropStruct.Value[0].Clone() as StructPropertyData;

                newItemStruct.Value[0] = compositeTableReference;
                (newItemStruct.Value[1] as NamePropertyData).Value = FName.FromString(_asset, drop.Item.CodeName);
                
                newDropStruct.Value[0] = newItemStruct;
                (newDropStruct.Value[1] as IntPropertyData).Value = drop.Quantity;
                (newDropStruct.Value[2] as DoublePropertyData).Value = drop.DropChance;
                (newDropStruct.Value[3] as IntPropertyData).Value = _enemyLevelOffsets.GetValueOrDefault(enemyName, 0);
                newDrops.Add(newDropStruct);
            }
            (enemyData.Value[10] as ArrayPropertyData).Value = newDrops.ToArray();
        }
        return _asset;
    }
    
    public override void Randomize()
    {
        foreach (var dropsData in _dropsData)
        {
            foreach (var item in dropsData.Value)
            {
                item.Item = ItemController.GetRandomItem();
                item.Quantity = RandomizerLogic.rand.Next(3);
            }
        }
    }
}