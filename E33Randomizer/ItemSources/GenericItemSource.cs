using UAssetAPI;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class GenericItemSource: ItemSource
{
    private static List<string> _questRequirementItems = ["Quest_Resin", "Quest_WoodBoards", "LostGestral", "Quest_Mine", "FestivalToken", "Quest_UniformForSon", "Quest_HexgaRock"];
    private List<string> _originalItems = [];
    public override void LoadFromAsset(UAsset asset)
    {
        _asset = asset;
        FileName = asset.FolderName.ToString();
        foreach (var name in asset.GetNameMapIndexList())
        {
            if (_questRequirementItems.Contains(name.ToString()))
            {
                continue;
            }
            if (ItemController.IsItem(name.ToString()))
            {
                Items.Add(ItemController.GetItemData(name.ToString()));
                _originalItems.Add(name.ToString());
            }
        }
    }

    public override UAsset SaveToAsset()
    {
        var itemReplacements = new Dictionary<string, string>();
        for (int i = 0; i < _originalItems.Count; i++)
        {
            if (i >= Items.Count)
            {
                break;
            }
            itemReplacements[_originalItems[i]] = Items[i].CodeName;
        }
        
        for (int i = 0; i < _asset.GetNameMapIndexList().Count; i++)
        {
            if (itemReplacements.ContainsKey(_asset.GetNameReference(i).ToString()))
            {
                _asset.SetNameReference(i, FString.FromString(itemReplacements[_asset.GetNameReference(i).ToString()]));
            }
        }
        return _asset;
    }
    
    public override void Randomize()
    {
        var numberOfItems = Items.Count;
        Items.Clear();
        for (int i = 0; i < numberOfItems; i++)
        {
            Items.Add(ItemController.GetRandomItem());
        }
    }
}