using System.Net;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

public class GameActionItemSource: ItemSource
{
    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);
        HasItemQuantities = true;
        foreach (var export in asset.Exports)
        {
            if (!export.ObjectName.Value.Value.Contains("AddItemToInventory"))
            {
                continue;
            }

            var actionName = export.ObjectName.ToString();
            SourceSections[actionName] = new List<ItemSourceParticle>();
            foreach (StructPropertyData itemPropertyData in ((export as NormalExport).Data[0] as ArrayPropertyData).Value)
            {
                var itemName = (((itemPropertyData.Value[0] as StructPropertyData).Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString();
                var newItemData = ItemsController.GetItemData(itemName);
                var itemQuantity = (itemPropertyData.Value[1] as IntPropertyData).Value;
                SourceSections[actionName].Add(new ItemSourceParticle(newItemData, itemQuantity));
            }
            
            var check = new CheckData
            {
                CodeName = actionName,
                CustomName = $"{FileName}: {SourceSections[actionName][0].Item.CustomName}",
                IsBroken = false,
                IsPartialCheck = true,
                ItemSource = this,
                Key = actionName
            };
            Checks.Add(check);
        }
    }

    public override UAsset SaveToAsset()
    {
        foreach (var export in _asset.Exports)
        {
            if (!export.ObjectName.Value.Value.Contains("AddItemToInventory"))
            {
                continue;
            }

            var dummyItemStruct = ((export as NormalExport).Data[0] as ArrayPropertyData).Value[0] as StructPropertyData;
            var tableImportObject =
                ((dummyItemStruct.Value[0] as StructPropertyData).Value[0] as StructPropertyData).Value[0] as
                ObjectPropertyData;
            if (_asset.Imports[Math.Abs(tableImportObject.Value.Index) - 1].ObjectName.ToString() !=
                "DT_jRPG_Items_Composite")
            {
                throw new Exception("Game action asset is not using DT_jRPG_Items_Composite!");
            }
            
            var items = SourceSections[export.ObjectName.ToString()];
            
            List<PropertyData> newItemStructs = [];
            foreach (var item in items)
            {
                _asset.AddNameReference(FString.FromString(item.Item.CodeName));
                var newItemStruct = dummyItemStruct.Clone() as StructPropertyData;
                var newItemTableEntryStruct = (newItemStruct.Value[0] as StructPropertyData).Value[0].Clone() as StructPropertyData;
                (newItemTableEntryStruct.Value[1] as NamePropertyData).Value = new FName(_asset, item.Item.CodeName);
                (newItemStruct.Value[1] as IntPropertyData).Value = Math.Max(item.Quantity, 1);
                (newItemStruct.Value[0] as StructPropertyData).Value[0] = newItemTableEntryStruct;
                newItemStructs.Add(newItemStruct);
            }
            
            ((export as NormalExport).Data[0] as ArrayPropertyData).Value = newItemStructs.ToArray();
        }
        return _asset;
    }
    
    public override void Randomize()
    {
        if (RandomizerLogic.Settings.ChangeNumberOfActionRewards) RandomizeNumberOfItems(RandomizerLogic.Settings.ActionRewardsNumberMin, RandomizerLogic.Settings.ActionRewardsNumberMax);
        foreach (var action in SourceSections)
        {
            foreach (var item in action.Value)
            {
                var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.Item.CodeName);
                item.Item = ItemsController.GetItemData(newItemName);
                if (RandomizerLogic.Settings.ChangeQuantityOfActionRewards)
                {
                    item.Quantity = RandomizerLogic.rand.Next(RandomizerLogic.Settings.ActionRewardsQuantityMin, RandomizerLogic.Settings.ActionRewardsQuantityMax + 1);
                }
            }
        }
    }
}