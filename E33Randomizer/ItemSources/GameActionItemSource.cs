using System.Net;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

public class GameActionItemSource: ItemSource
{
    private static Dictionary<string, string> _customNames = new()
    { 
        {"DA_GA_SQT_BossMirrorRenoir", "The Monolith: Gustave's Renoir Outfit"},
        {"DA_GA_SQT_BossSimon", "The Abyss: Simon's Rewards"},
        {"DA_GA_SQT_GradientTutorial", "Monoco's Station: First Gradient Unlocks"},
        {"DA_GA_SQT_RedAndWhiteTree", "Lumiere Act III: Maelle's Real Me Outfits"},
        {"DA_GA_SQT_CampAfterSecondAxonEntrance", "The Camp: Barrier Breaker"},
        {"DA_GA_SQT_CampAfterTheFirstAxonP2", "The Camp: Lettre a Maelle"},
        {"DA_GA_SQT_TheGommage", "Lumiere Act I: The Gommage Sequence Items"},
        {"SA_GA_SQT_EpilogueWithMaelle", "Heart of the Canvas: A Life to Paint Outfits"},
        {"SA_GA_SQT_EpilogueWithVerso", "Heart of the Canvas: A Life to Love Outfits"}
    };
    
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
                CustomName = _customNames.GetValueOrDefault(FileName, FileName),
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
        _minNumberOfItems = RandomizerLogic.Settings.ActionRewardsNumberMin;
        _maxNumberOfItems = RandomizerLogic.Settings.ActionRewardsNumberMax;
        _changeNumberOfItems = RandomizerLogic.Settings.ChangeNumberOfActionRewards;
        base.Randomize();
    }
}