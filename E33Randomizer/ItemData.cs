using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer;

public class ItemData: ObjectData
{
    public string Type = "Invalid";
    public bool HasQuantities = false;

    public ItemData()
    {
        CodeName = "PlaceHolderItem";
        CustomName = "PlaceHolderItem";
        IsBroken = true;
    }

    public ItemData(StructPropertyData compositeTableEntryStruct)
    {
        CodeName = compositeTableEntryStruct.Name.ToString();
        CustomName = RandomizerLogic.ItemCustomNames.GetValueOrDefault(CodeName, CodeName);
        Type = CustomName.Split('(')[1].Split(')')[0];
        IsBroken = RandomizerLogic.BrokenItems.Contains(CodeName);
        HasQuantities = ItemsController.ItemsWithQuantities.Contains(CodeName);
    }

    public override string ToString()
    {
        return CustomName;
    }
}