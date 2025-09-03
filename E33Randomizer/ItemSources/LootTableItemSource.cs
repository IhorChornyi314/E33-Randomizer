using System.Runtime.InteropServices;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;



public class LootTableItemSource: ItemSource
{
    private static Dictionary<string, string> _lootTableNames = new()
    {
        {"DT_LootTable_UpgradeItems_Alpha", "Chromatic bosses drops"},
        {"DT_LootTable_UpgradeItems_Elite", "Mini-bosses drops"},
        {"DT_LootTable_UpgradeItems_Exploration", "Chroma catalysts"},
        {"DT_LootTable_UpgradeItems_Petank", "Petank drops"},
        {"DT_LootTable_UpgradeItems_Regular", "Regular enemy drops"}
    };

    private static Dictionary<string, string> _entryCustomNames = new()
    {
        {"Respec", "Recoat"},
        {"WeaponUpgradeLvl5", "Perfect Chroma Catalyst"},
        {"Lumina", "Color Lumina"},
    };
    
    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);
        HasItemQuantities = true;
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var entryData in tableData)
        {
            var entryName = entryData.Name.ToString();

            List<ItemSourceParticle> items = new();
            foreach (StructPropertyData itemStruct in (entryData.Value[3] as ArrayPropertyData).Value)
            {
                var itemData = ItemsController.GetItemData((itemStruct.Value[0] as NamePropertyData).ToString());
                var quantity = (itemStruct.Value[2] as IntPropertyData).Value;
                
                items.Add(new ItemSourceParticle(itemData, quantity));
            }

            var customString = entryName.Contains('_') ? entryName.Split('_')[1] : entryName;
            
            customString = _entryCustomNames.GetValueOrDefault(customString, customString);
            
            if (customString.Contains('-')) customString = "levels " + customString;
            
            var check = new CheckData
            {
                CodeName = entryName,
                CustomName = $"{_lootTableNames[FileName]} table entry for {customString}",
                IsBroken = false,
                IsPartialCheck = true,
                ItemSource = this,
                Key = entryName
            };
            Checks.Add(check);
            SourceSections[entryName] = items;
        }
    }

    public override UAsset SaveToAsset()
    {
        var tableData = (_asset.Exports[0] as DataTableExport).Table.Data;
        StructPropertyData dummyEntryStruct = null;
        foreach (var entryData in tableData)
        {
            if ((entryData.Value[3] as ArrayPropertyData).Value.Length > 0)
            {
                dummyEntryStruct = (entryData.Value[3] as ArrayPropertyData).Value[0].Clone() as StructPropertyData;
            }

            if (dummyEntryStruct != null)
            {
                break;
            } 
        }
        
        foreach (var entryData in tableData)
        {
            var entryName = entryData.Name.ToString();
            var entryItems = SourceSections[entryName];
            List<PropertyData> newEntries = [];

            foreach (var entry in entryItems)
            {
                _asset.AddNameReference(FString.FromString(entry.Item.CodeName));
                var newEntryStruct = dummyEntryStruct.Clone() as StructPropertyData;

                (newEntryStruct.Value[0] as NamePropertyData).Value = FName.FromString(_asset, entry.Item.CodeName);
                
                (newEntryStruct.Value[2] as IntPropertyData).Value = entry.Quantity;
                newEntries.Add(newEntryStruct);
            }
            (entryData.Value[3] as ArrayPropertyData).Value = newEntries.ToArray();
        }
        return _asset;
    }

    public override void Randomize()
    {
        _minNumberOfItems = FileName.Contains("Exploration") ? RandomizerLogic.Settings.ChestContentsNumberMin : RandomizerLogic.Settings.TowerRewardsNumberMin;
        _maxNumberOfItems = FileName.Contains("Exploration") ? RandomizerLogic.Settings.ChestContentsNumberMax : RandomizerLogic.Settings.TowerRewardsNumberMax;
        _changeNumberOfItems = FileName.Contains("Exploration") ? RandomizerLogic.Settings.ChangeNumberOfChestContents : RandomizerLogic.Settings.ChangeNumberOfTowerRewards;
        base.Randomize();
    }
}