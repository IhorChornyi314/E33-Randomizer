using E33Randomizer.RandomizationLogic;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer.ObjectDatum;

public class ItemData: ObjectData
{
    public string Type { get; set; } = "Invalid";
    public bool HasQuantities { get; set; } = false;
    
    public ItemData()
    {
        CodeName = "PlaceHolderItem";
        CustomName = "PlaceHolderItem (Cut Content)";
    }

    public ItemData(StructPropertyData compositeTableEntryStruct)
    {
        CodeName = compositeTableEntryStruct.Name.ToString();
        CustomName = RandomizerLogic.ItemCustomNames.GetValueOrDefault(CodeName, CodeName);
        Type = CustomName.Split('(')[1].Split(')')[0];
        IsBroken = RandomizerLogic.BrokenItems.Contains(CodeName);
        HasQuantities = Controllers.ItemsController.ItemsWithQuantities.Contains(CodeName);
    }

    public override string ToString()
    {
        return CustomName;
    }
}