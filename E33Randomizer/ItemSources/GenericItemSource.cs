using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class GenericItemSource: ItemSource
{
    private static Dictionary<string, string> _checkNames = new()
    {
        { "BP_Dialogue_Lumiere_ExpFestival_Quizz_Antoine", "Lumiere Act I: Antoine's Quiz Reward" },
    };
    
    
    private List<int> _originalNameReferenceIndexes = [];
    
    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);
        _originalNameReferenceIndexes.Clear();
        SourceSections[FileName] = [];

        
        
        for (int i = 0; i < _asset.GetNameMapIndexList().Count; i++)
        {
            var name = _asset.GetNameMapIndexList()[i].ToString();
            if (Controllers.ItemsController.IsItem(name))
            {
                var newItem = Controllers.ItemsController.GetObject(name);
                _originalNameReferenceIndexes.Add(i);
                SourceSections[FileName].Add(new ItemSourceParticle(newItem));
            }
        }
        var check = new CheckData
        {
            CodeName = FileName,
            CustomName = _checkNames.GetValueOrDefault(FileName, FileName),
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
        foreach (var itemSourceParticle in SourceSections[FileName])
        {
            var oldItem = itemSourceParticle.Item;
            var newItem = Controllers.ItemsController.GetObject(RandomizerLogic.CustomItemPlacement.Replace(oldItem.CodeName));
            itemSourceParticle.Item = newItem;
        }
    }
}