using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class MerchantInventoryItemSource: ItemSource
{
    private static Dictionary<string, string> MerchantNames = new ()
    {
        {"DT_Merchant_CleaIsland", "Fusoka (Flying Manor)"},
        {"DT_Merchant_CoastalCave_Bruler", "Bruler (Coastal Cave)"},
        {"DT_Merchant_CoastalCave_Cruler", "Cruler (Coastal Cave)"},
        {"DT_Merchant_FH_Custo_Danseuse", "Verogo (Frozen Hearts)"},
        {"DT_Merchant_ForgottenBattlefield", "Kasumi (Forgotten Battlefield)"},
        {"DT_Merchant_GestralVillage1", "Jujubree (Gestral Village)"},
        {"DT_Merchant_GestralVillage2", "Eesda (Gestral Village)"},
        {"DT_Merchant_GestralVillage3", "Gestral Merchant (Sacred River)"},
        {"DT_Merchant_GoblusLair", "Noco (Flying Waters)"},
        {"DT_Merchant_GrandisStation", "Grandis (Monoco Station)"},
        {"DT_Merchant_GV_1_CustoSuits_Guys", "Delsitra (Gestral Village)"},
        {"DT_Merchant_GV_1_CustoSuits_Ladies", "Alexcyclo (Gestral Village)"},
        {"DT_Merchant_Lumiere", "Cribappa (Lumiere Act III)"},
        {"DT_Merchant_MonocosMountain", "Melosh (The Monolith)"},
        {"DT_Merchant_Monolith", "Mistra (The Monolith)"},
        {"DT_Merchant_OldLumiere", "Mandelgo (Old Lumiere)"},
        {"DT_Merchant_Optional3", "Grour (Renoir's Drafts)"},
        {"DT_Merchant_OrangeForest", "Persik (Falling Leaves)"},
        {"DT_Merchant_Reacher", "Eragol (The Reacher)"},
        {"DT_Merchant_SeaCliff", "Jerijeri (Stone Wave Cliffs)"},
        {"DT_Merchant_Sirene", "Klaudiso (Sirene)"},
        {"DT_Merchant_TwilightSanctuary", "Anthonypo (Endless Night Sanctuary)"},
        {"DT_Merchant_Visages", "Blooraga (Visages)"},
        {"DT_Merchant_YellowForest", "Pinabby (Yellow Harvest)"},
        {"DT_Merchant_WM_1", "Appla (World Map)"},
        {"DT_Merchant_WM_2", "Colaro (World Map)"},
        {"DT_Merchant_WM_3_GustaveSuit", "Carrabi (World Map)"},
        {"DT_Merchant_WM_4", "Strabami (World Map)"},
        {"DT_Merchant_WM_5", "Pecha (World Map)"},
        {"DT_Merchant_WM_6", "Blakora (World Map)"},
        {"DT_Merchant_WM_7", "Papasso (World Map)"},
        {"DT_Merchant_WM_8", "Rederi (World Map)"},
        {"DT_Merchant_WM_9", "Sodasso (World Map)"},
        {"DT_Merchant_WM_9_Sirene", "Pearo (World Map)"},
        {"DT_Merchant_WM_10", "Carnovi (World Map)"},
        {"DT_Merchant_WM_11", "Blabary (World Map)"},
        {"DT_Merchant_WM_12", "Geranjo (World Map)"},
        {"DT_Merchant_WM_13", "Granasori (World Map)"},
        {"DT_Merchant_WM_14", "Lucaroparfe (World Map)"},
        {"DT_Merchant_WM_15", "Jumeliba (World Map)"},
        {"DT_Merchant_WM_16", "Rubiju (World Map)"},
        {"DT_Merchant_WM_17", "Citrelo (World Map)"}
    };
    
    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);
        HasItemQuantities = true;
        SourceSections[""] = [];
        var tableData = (asset.Exports[0] as DataTableExport).Table.Data;
        foreach (var soldItemData in tableData)
        {
            var itemName = soldItemData.Name.ToString();
            var itemData = ItemsController.GetItemData(itemName);
            var itemQuantity = (soldItemData.Value[3] as IntPropertyData).Value;
            var itemLocked = (soldItemData.Value[4] as ObjectPropertyData).Value.Index != 0;
            
            SourceSections[""].Add(new ItemSourceParticle(itemData, itemQuantity, locked: itemLocked));
        }
        
        var check = new CheckData
        {
            CodeName = FileName,
            CustomName = MerchantNames.GetValueOrDefault(FileName, FileName),
            IsBroken = false,
            IsPartialCheck = false,
            ItemSource = this,
            Key = ""
        };
        Checks.Add(check);
    }

    public override UAsset SaveToAsset()
    {
        var tableData = (_asset.Exports[0] as DataTableExport).Table.Data;

        ObjectPropertyData dummyConditionStructLocked = null;
        ObjectPropertyData dummyConditionStructUnlocked = null;
        
        foreach (var itemData in tableData)
        {
            if ((itemData.Value[4] as ObjectPropertyData).Value.Index != 0)
            {
                dummyConditionStructLocked = itemData.Value[4] as ObjectPropertyData;
            }
            else
            {
                dummyConditionStructUnlocked = itemData.Value[4] as ObjectPropertyData;
            }

            if (dummyConditionStructLocked != null && dummyConditionStructUnlocked != null)
            {
                break;
            }
        }
        
        var dummyItemStruct = tableData[0].Clone() as StructPropertyData;
        tableData.Clear();
        
        foreach (var inventoryItem in SourceSections[""])
        {
            _asset.AddNameReference(FString.FromString(inventoryItem.Item.CodeName));
            var newItemStruct = dummyItemStruct.Clone() as StructPropertyData;
            newItemStruct.Name = FName.FromString(_asset, inventoryItem.Item.CodeName);
            (newItemStruct.Value[0] as NamePropertyData).Value = FName.FromString(_asset, inventoryItem.Item.CodeName);
            (newItemStruct.Value[3] as IntPropertyData).Value = Math.Max(inventoryItem.Quantity, 1);
            if (dummyConditionStructLocked != null && inventoryItem.MerchantInventoryLocked || dummyConditionStructUnlocked == null)
            {
                newItemStruct.Value[4] = dummyConditionStructLocked.Clone() as ObjectPropertyData;
            }
            else
            {
                newItemStruct.Value[4] = dummyConditionStructUnlocked;
            }
            
            tableData.Add(newItemStruct);
        }

        return _asset;
    }
    
    public override void Randomize()
    {
        if (RandomizerLogic.Settings.ChangeMerchantInventorySize) RandomizeNumberOfItems(RandomizerLogic.Settings.MerchantInventorySizeMin, RandomizerLogic.Settings.MerchantInventorySizeMax);
        foreach (var item in SourceSections[""])
        {
            var newItemName = RandomizerLogic.CustomItemPlacement.Replace(item.Item.CodeName);
            item.Item = ItemsController.GetItemData(newItemName);
            if (RandomizerLogic.Settings.ChangeItemQuantity && item.Item.Type == "Upgrade Material")
            {
                item.Quantity = Utils.Between(RandomizerLogic.Settings.ItemQuantityMin, RandomizerLogic.Settings.ItemQuantityMax);
            }

            if (RandomizerLogic.Settings.ChangeMerchantInventoryLocked)
            {
                var change = RandomizerLogic.rand.Next(0, 100) <
                             RandomizerLogic.Settings.MerchantInventoryLockedChancePercent;
                item.MerchantInventoryLocked = change;
            }
        }
    }
}