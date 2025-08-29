using System.Net;
using UAssetAPI;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class GenericItemSource: ItemSource
{
    private static List<string> _questRequirementItems = ["Quest_Resin", "Quest_WoodBoards", "LostGestral", "Quest_Mine", "FestivalToken", "Quest_UniformForSon", "Quest_HexgaRock"];
    private List<int> _originalNameReferenceIndexes = [];
    
    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);
        _originalNameReferenceIndexes.Clear();
        SourceSections[FileName] = [];

        for (int i = 0; i < _asset.GetNameMapIndexList().Count; i++)
        {
            var name = _asset.GetNameMapIndexList()[i];
            if (_questRequirementItems.Contains(name.ToString()))
            {
                continue;
            }
            if (ItemsController.IsItem(name.ToString()))
            {
                var newItem = ItemsController.GetItemData(name.ToString());
                Items.Add(newItem);
                _originalNameReferenceIndexes.Add(i);
                SourceSections[FileName].Add(new ItemSourceParticle(newItem));
            }
        }
        var check = new CheckData
        {
            CodeName = FileName,
            CustomName = FileName,
            IsBroken = false,
            IsPartialCheck = false,
            IsFixedSize = true,
            ItemSource = this,
            Key = FileName
        };
        Checks.Add(check);
    }

    public override UAsset SaveToAsset()
    {
        var newItems = SourceSections[FileName].Select(i => i.Item.CodeName).ToList();
        for (int i = 0; i < Math.Min(_originalNameReferenceIndexes.Count, newItems.Count); i++)
        {
            _asset.SetNameReference(_originalNameReferenceIndexes[i], FString.FromString(newItems[i]));
        }
        return _asset;
    }
    
    public override void Randomize()
    {
        Items.Clear();
        foreach (var itemSourceParticle in SourceSections[FileName])
        {
            var oldItem = itemSourceParticle.Item;
            var newItem = ItemsController.GetItemData(RandomizerLogic.CustomItemPlacement.Replace(oldItem.CodeName));
            itemSourceParticle.Item = newItem;
            Items.Add(newItem);
        }
    }
}