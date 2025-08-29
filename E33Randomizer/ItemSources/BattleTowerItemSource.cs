using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;



public class BattleTowerItemSource: ItemSource
{
    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);
        HasItemQuantities = true;
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var stageData in tableData)
        {
            var stageName = stageData.Name.ToString();

            List<ItemSourceParticle> items = new();
            foreach (StructPropertyData itemStruct in (stageData.Value[4] as ArrayPropertyData).Value)
            {
                var itemData = ItemsController.GetItemData(((itemStruct.Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString());
                var quantity = (itemStruct.Value[1] as IntPropertyData).Value;
                
                items.Add(new ItemSourceParticle(itemData, quantity));
                Items.Add(itemData);
            }

            var check = new CheckData
            {
                CodeName = stageName,
                CustomName = $"Stage {stageName.Split('_')[0]} Trial {stageName.Split('_')[1]}",
                IsBroken = false,
                IsPartialCheck = true,
                ItemSource = this,
                Key = stageName
            };
            Checks.Add(check);
            SourceSections[stageName] = items;
        }
    }

    public override UAsset SaveToAsset()
    {
        var tableData = (_asset.Exports[0] as DataTableExport).Table.Data;
        StructPropertyData dummyRewardStruct = null;
        ObjectPropertyData compositeTableReference = null;
        foreach (var stageData in tableData)
        {
            if ((stageData.Value[4] as ArrayPropertyData).Value.Length > 0)
            {
                dummyRewardStruct = (stageData.Value[4] as ArrayPropertyData).Value[0].Clone() as StructPropertyData;
                compositeTableReference = (dummyRewardStruct.Value[0] as StructPropertyData).Value[0].Clone() as ObjectPropertyData;
                compositeTableReference.Value = FPackageIndex.FromImport(1);
            }

            if (dummyRewardStruct != null)
            {
                break;
            } 
        }
        
        foreach (var stageData in tableData)
        {
            var stageName = stageData.Name.ToString();
            var stageRewards = SourceSections[stageName];
            List<PropertyData> newRewards = [];

            foreach (var reward in stageRewards)
            {
                _asset.AddNameReference(FString.FromString(reward.Item.CodeName));
                var newRewardStruct = dummyRewardStruct.Clone() as StructPropertyData;
                var newItemStruct = newRewardStruct.Value[0].Clone() as StructPropertyData;

                newItemStruct.Value[0] = compositeTableReference;
                (newItemStruct.Value[1] as NamePropertyData).Value = FName.FromString(_asset, reward.Item.CodeName);
                
                newRewardStruct.Value[0] = newItemStruct;
                (newRewardStruct.Value[1] as IntPropertyData).Value = reward.Quantity;
                newRewards.Add(newRewardStruct);
            }
            (stageData.Value[4] as ArrayPropertyData).Value = newRewards.ToArray();
        }
        return _asset;
    }

    public override void Randomize()
    {
        Items.Clear();
        foreach (var rewardData in SourceSections)
        {
            foreach (var item in rewardData.Value)
            {
                var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.Item.CodeName);
                item.Item = ItemsController.GetItemData(newItemName);
                Items.Add(item.Item);
            }
        }
    }
}