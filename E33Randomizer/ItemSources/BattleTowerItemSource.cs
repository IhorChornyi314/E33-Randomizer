using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

class TowerReward(ItemData itemData, int quantity = 1)
{
    public ItemData Item = itemData;
    public int Quantity = quantity;
}

public class BattleTowerItemSource: ItemSource
{
    private Dictionary<string, List<TowerReward>> _rewardsData = new();
    
    public override void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FileName = asset.FolderName.ToString();
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var stageData in tableData)
        {
            var stageName = stageData.Name.ToString();

            List<TowerReward> items = new();
            foreach (StructPropertyData itemStruct in (stageData.Value[4] as ArrayPropertyData).Value)
            {
                var itemData = ItemController.GetItemData(((itemStruct.Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString());
                var quantity = (itemStruct.Value[1] as IntPropertyData).Value;
                
                items.Add(new TowerReward(itemData, quantity));
                Items.Add(itemData);
            }
            _rewardsData[stageName] = items;
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
            var stageRewards = _rewardsData[stageName];
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
        foreach (var rewardData in _rewardsData)
        {
            foreach (var item in rewardData.Value)
            {
                item.Item = ItemController.GetRandomItem();
                item.Quantity = RandomizerLogic.rand.Next(3);
            }
        }
    }
}