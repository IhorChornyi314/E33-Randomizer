using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;

namespace E33Randomizer;

public class ItemData
{
    public string CodeName = "PlaceHolderItem";
    public string CustomName = "PlaceHolderItem";
    public string Category = "";
    public bool IsBroken = true;

    public ItemData()
    {
    }

    public ItemData(StructPropertyData compositeTableEntryStruct)
    {
        CodeName = compositeTableEntryStruct.Name.ToString();
        CustomName = RandomizerLogic.ItemCustomNames.GetValueOrDefault(CodeName, CodeName);
        Category = CustomName.Split('(')[1].Split(')')[0];
        IsBroken = Category == "Invalid";
    }

    public override string ToString()
    {
        return CustomName;
    }
}