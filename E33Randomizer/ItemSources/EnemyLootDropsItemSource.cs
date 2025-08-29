using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class EnemyLootDropsItemSource: ItemSource
{
    private Dictionary<string, int> _enemyLevelOffsets = new();
    public override void LoadFromAsset(UAsset asset)
    {
        HasItemQuantities = true;
        base.LoadFromAsset(asset);
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var enemyData in tableData)
        {
            var enemyName = enemyData.Name.ToString();

            List<ItemSourceParticle> drops = new();
            foreach (StructPropertyData itemStruct in (enemyData.Value[10] as ArrayPropertyData).Value)
            {
                var itemData = ItemsController.GetItemData(((itemStruct.Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString());
                var quantity = (itemStruct.Value[1] as IntPropertyData).Value;
                var dropChance = (itemStruct.Value[2] as DoublePropertyData).Value;
                
                drops.Add(new ItemSourceParticle(itemData, quantity, dropChance));
                Items.Add(itemData);
                _enemyLevelOffsets[enemyName] = (itemStruct.Value[3] as IntPropertyData).Value;
            }
            SourceSections[enemyName] = drops;
            
            var check = new CheckData
            {
                CodeName = enemyName,
                CustomName = $"{EnemiesController.GetEnemyData(enemyName).CustomName}",
                IsBroken = false,
                IsPartialCheck = true,
                ItemSource = this,
                Key = enemyName,
            };
            Checks.Add(check);
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
            var enemyDrops = SourceSections[enemyName];
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
                (newDropStruct.Value[2] as DoublePropertyData).Value = drop.LootDropChance;
                (newDropStruct.Value[3] as IntPropertyData).Value = _enemyLevelOffsets.GetValueOrDefault(enemyName, 0);
                newDrops.Add(newDropStruct);
            }
            (enemyData.Value[10] as ArrayPropertyData).Value = newDrops.ToArray();
        }
        return _asset;
    }
    
    public override void Randomize()
    {
        Items.Clear();
        foreach (var dropsData in SourceSections)
        {
            foreach (var item in dropsData.Value)
            {
                var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.Item.CodeName);
                item.Item = ItemsController.GetItemData(newItemName);
                Items.Add(item.Item);
            }
        }
    }
}