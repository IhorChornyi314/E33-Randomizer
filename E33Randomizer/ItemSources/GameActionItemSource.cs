using System.Net;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

public class GameActionItemSource: ItemSource
{
    private Dictionary<string, List<ItemData>> _actionsData = new();
    public override void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FolderName = asset.FolderName.ToString();
        foreach (var export in asset.Exports)
        {
            if (!export.ObjectName.Value.Value.Contains("AddItemToInventory"))
            {
                continue;
            }

            var actionName = export.ObjectName.ToString();
            _actionsData[actionName] = new List<ItemData>();
            foreach (StructPropertyData itemPropertyData in ((export as NormalExport).Data[0] as ArrayPropertyData).Value)
            {
                var itemName = (((itemPropertyData.Value[0] as StructPropertyData).Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString();
                var newItemData = ItemsController.GetItemData(itemName);
                _actionsData[actionName].Add(newItemData);
                Items.Add(newItemData);
            }
            
            var check = new CheckData
            {
                CodeName = actionName,
                CustomName = $"{FileName}: {_actionsData[actionName][0].CustomName}",
                IsBroken = false,
                IsPartialCheck = true,
                ItemSource = this
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
            
            var items = _actionsData[export.ObjectName.ToString()];
            
            List<PropertyData> newItemStructs = [];
            foreach (var item in items)
            {
                _asset.AddNameReference(FString.FromString(item.CodeName));
                var newItemStruct = dummyItemStruct.Clone() as StructPropertyData;
                var newItemTableEntryStruct = (newItemStruct.Value[0] as StructPropertyData).Value[0].Clone() as StructPropertyData;
                (newItemTableEntryStruct.Value[1] as NamePropertyData).Value = new FName(_asset, item.CodeName);
                newItemStructs.Add(newItemStruct);
            }
            
            ((export as NormalExport).Data[0] as ArrayPropertyData).Value = newItemStructs.ToArray();
        }
        return _asset;
    }
    
    public override void Randomize()
    {
        Items.Clear();
        foreach (var action in _actionsData)
        {
            var newItems = new List<ItemData>();
            foreach (var item in action.Value)
            {
                var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.CodeName);
                var newItem = ItemsController.GetItemData(newItemName);
                newItems.Add(newItem);
                Items.Add(newItem);
            }

            _actionsData[action.Key] = newItems;
        }
    }
    
    public override List<ItemData> GetCheckItems(string key)
    {
        return _actionsData[key];
    }
    
    
    public override void AddItem(string key, ItemData item)
    {
        _actionsData[key].Add(item);
    }

    public override void RemoveItem(string key, ItemData item)
    {
        _actionsData[key].RemoveAll(i => i.CodeName == item.CodeName);
    }
}