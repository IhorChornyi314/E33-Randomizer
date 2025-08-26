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
        FileName = asset.FolderName.ToString();
        foreach (var export in asset.Exports)
        {
            if (!export.ObjectName.Value.Value.Contains("AddItemToInventory"))
            {
                continue;
            }
            _actionsData[export.ObjectName.ToString()] = new List<ItemData>();
            foreach (StructPropertyData itemPropertyData in ((export as NormalExport).Data[0] as ArrayPropertyData).Value)
            {
                var itemName = (((itemPropertyData.Value[0] as StructPropertyData).Value[0] as StructPropertyData).Value[1] as NamePropertyData).ToString();
                var newItemData = ItemController.GetItemData(itemName);
                _actionsData[export.ObjectName.ToString()].Add(newItemData);
                Items.Add(newItemData);
            }
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
        foreach (var action in _actionsData)
        {
            var numberOfItems = action.Value.Count;
            action.Value.Clear();
            for (int i = 0; i < numberOfItems; i++)
            {
                action.Value.Add(ItemController.GetRandomItem());
            }
        }
    }
}