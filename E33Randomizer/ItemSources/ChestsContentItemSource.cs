using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class ChestsContentItemSource: ItemSource
{
    private static Dictionary<string, string> LocationNames = new ()
    {
        {"SpringMeadows", "Spring Meadows"},
        {"SeaCliff", "Stone Wave Cliffs"},
        {"GoblusLair", "Flying Waters"},
        {"ForgottenBattlefield", "Forgotten Battlefield"},
        {"GrandisStation", "Monoco's Station"},
        {"AncientSanctuary", "Ancient Sanctuary"},
        {"GestralVillage", "Gestral Village"},
        {"EsquiesNest", "Esquie's Nest"},
        {"OldLumiere", "Old Lumiere"},
        {"WorldMap", "World Map"},
        {"Manor", "The Manor"},
        {"Monolith", "The Monolith"},
        {"ACT1", "Act 1 Lumiere"},
        {"Lumiere", "Act 3 Lumiere"},
        {"YellowForest", "Yellow Harvest"},
        {"FallingLeaves", "Falling Leaves"},
        {"CrimsonForest", "Crimson Forest"},
        {"FrozenHearts", "Frozen Hearts"},
        {"Reacher", "The Reacher"},
        {"RenoirSDraft", "Renoir's Drafts"},
        {"DarkShores", "Dark Shores"},
        {"TwilightSanctuary", "Twilight Sanctuary"},
        {"FlyingManor", "Flying Manor"},
        {"RedWoods", "Red Woods"},
        {"02", "Small Burgeon"},
        {"StonewaveCliffsCave", "Stone Wave Cliffs Cave"},
        {"ChosenPath", "The Chosen Path"},
        {"FloatingIsland", "Sky Island"},
        {"RockTrailing", "RockTrailing"}, // TODO: Find out what this is
        {"Cemetery", "Flying Cemetery"},
        {"CoastalCave", "Coastal Cave"},
        {"DoorMaze", "Esoteric Ruins"},
        {"SinisterCave", "Sinister Cave"},
        {"MiniLevels", "Fixed-Camera Level"},
        {"FlyingCasinoEntrance", "Flying Casino"},
        {"TheCarrousel", "The Carousel"},
        {"WhiteSands", "White Sands"},
        {"SacredRiver", "Sacred River"},
        {"VersosDraft", "Verso's Drafts"},
        {"CleasTower", "Endless Tower"}
    };
    
    public override void LoadFromAsset(UAsset asset)
    {
        HasItemQuantities = true;
        base.LoadFromAsset(asset);
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var chestData in tableData)
        {
            var chestName = chestData.Name.ToString();
            if ((chestData.Value[0] as ArrayPropertyData).Value.Length > 0)
            {
                var lootTableItemData = Controllers.ItemsController.GetObject("UpgradeMaterial_Level1");
                SourceSections[chestName] = [new ItemSourceParticle(lootTableItemData, 1, 100, true)];
                continue;
            }

            List<ItemSourceParticle> items = new();
            foreach (var itemStruct in (((chestData.Value[1] as ArrayPropertyData).Value[0] as StructPropertyData).Value[3] as ArrayPropertyData).Value)
            {
                var itemData = Controllers.ItemsController.GetObject(((itemStruct as StructPropertyData).Value[0] as NamePropertyData).Value.ToString());
                items.Add(new ItemSourceParticle(itemData, ((itemStruct as StructPropertyData).Value[2] as IntPropertyData).Value));
            }
            SourceSections[chestName] = items;

            var areaName = chestName.Split('_')[^2];
            var translatedAreaName = LocationNames.GetValueOrDefault(areaName, areaName);
            
            var check = new CheckData
            {
                CodeName = chestName,
                CustomName = $"{translatedAreaName}: {items[0].Item.CustomName}",
                IsBroken = false,
                IsPartialCheck = true,
                ItemSource = this,
                Key = chestName
            };
            Checks.Add(check);
        }
    }

    public override UAsset SaveToAsset()
    {
        var tableData = (_asset.Exports[0] as DataTableExport).Table.Data;
        StructPropertyData dummyLootStruct = null;
        StructPropertyData dummyLootTableStruct = null;

        foreach (var chestData in tableData)
        {
            if ((chestData.Value[0] as ArrayPropertyData).Value.Length > 0)
            {
                dummyLootTableStruct = (chestData.Value[0] as ArrayPropertyData).Value[0] as StructPropertyData;
            }
            else
            {
                dummyLootStruct = (chestData.Value[1] as ArrayPropertyData).Value[0] as StructPropertyData;
            }

            if (dummyLootStruct != null && dummyLootTableStruct != null)
            {
                break;
            }
        }
        
        foreach (var chestData in tableData)
        {
            var chestName = chestData.Name.ToString();
            
            var chestContent = SourceSections[chestName];

            List<PropertyData> newLootTableStructs = [];
            List<PropertyData> newLootStructs = [];
            
            foreach (var item in chestContent)
            {
                if (item.IsLootTableChest)
                {
                    var newLootTableStruct = dummyLootTableStruct.Clone() as StructPropertyData;
                    newLootTableStructs.Add(newLootTableStruct);
                }
                else
                {
                    var newLootStruct = dummyLootStruct.Clone() as StructPropertyData;
                    var newLootEntryStruct = (newLootStruct.Value[3] as ArrayPropertyData).Value[0].Clone() as StructPropertyData;
                    
                    (newLootEntryStruct.Value[0] as NamePropertyData).Value = FName.FromString(_asset, item.Item.CodeName);
                    (newLootEntryStruct.Value[2] as IntPropertyData).Value = Math.Max(item.Quantity, 1);
                    (newLootStruct.Value[3] as ArrayPropertyData).Value[0] = newLootEntryStruct;
                    newLootStructs.Add(newLootStruct);
                    _asset.AddNameReference(FString.FromString(item.Item.CodeName));
                }
            }
            
            (chestData.Value[0] as ArrayPropertyData).Value = newLootTableStructs.ToArray();
            (chestData.Value[1] as ArrayPropertyData).Value = newLootStructs.ToArray();
        }

        return _asset;
    }

    public override void Randomize()
    {
        _minNumberOfItems = RandomizerLogic.Settings.ChestContentsNumberMin;
        _maxNumberOfItems = RandomizerLogic.Settings.ChestContentsNumberMax;
        _changeNumberOfItems = RandomizerLogic.Settings.ChangeNumberOfChestContents;
        base.Randomize();
        foreach (var chestData in SourceSections)
        {
            foreach (var item in chestData.Value)
            {
                var newItemName = item.Item.CodeName;
                item.IsLootTableChest = newItemName.StartsWith("UpgradeMaterial_Level") && !newItemName.EndsWith('5');
            }
        }
    }
}