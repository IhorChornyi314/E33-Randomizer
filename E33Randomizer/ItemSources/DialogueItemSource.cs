using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.Kismet.Bytecode.Expressions;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;

// BP_Dialogue_JudgeOfMercy quantity ['Exports', 3, 'ScriptBytecode', 691, 'ContextExpression', 'Parameters', 1, 'Value']

public class DialogueItemSource : ItemSource
{
    public static Dictionary<string, Dictionary<string, List<int>>> DialogueRewardPaths = null;
    public static Dictionary<string, Dictionary<string, List<int>>> DialogueRewardQuantitiesPaths = null;

    private static Dictionary<string, string> _dialogueNames = new()
    {
        { "BP_Dialogue_Benoit", "BP_Dialogue_Benoit" },
        { "BP_Dialogue_CleaWorkshop_Path1", "Painting Workshop: Colour of the Beast" },
        { "BP_Dialogue_CleaWorkshop_Path2", "Painting Workshop: Shape of the Beast" },
        { "BP_Dialogue_CleaWorkshop_Path3", "Painting Workshop: Light of the Beast" },
        { "BP_Dialogue_DarkGestralArena_Pots", "Dark Gestral Arena: Pots' Rewards" },
        { "BP_Dialogue_Eloise", "Lumiere Act I: Eloise's Reward" },
        { "BP_Dialogue_EsquieCamp_Quest_4", "The Camp: Verso Gradient Unlock 2" },
        { "BP_Dialogue_EsquieCamp_Quest_7", "The Camp: Verso Gradient Unlock 3" },
        { "BP_Dialogue_GestralBeach_Climb_GrandisMain", "Gestral Beach: Climb the Wall Reward" },
        { "BP_Dialogue_GestralBeach_OnlyUp_Top", "Gestral Beach: Gestral Ascension Reward" },
        { "BP_Dialogue_GestralBeach_WipeoutGestral2_End", "Gestral Beach: Parkour Course Reward" },
        { "BP_Dialogue_GestralRace", "Gestral Beach: Time Race Reward" },
        { "BP_Dialogue_Grandis_Carrousel", "The Carousel: Grandis's Gift" },
        { "BP_Dialogue_GV_ArenaRegistrar", "Gestral Village: Tournament Rewards" },
        { "BP_Dialog_Merchant_GV_GestralBazar1", "Gestral Village: Weird Pictos Turn-In Reward" },
        { "BP_Dialogue_GV_Father", "Gestral Village: Gestral Father's Reward" },
        { "BP_Dialogue_GV_GestralBazar6", "Gestral Village: Reward for Beating Eesda" },
        { "BP_Dialogue_GV_GestralBazar9", "Gestral Village: Excalibur" },
        { "BP_Dialogue_GV_GestralGambler", "Gestral Village: Gambler's Gift" },
        { "BP_Dialogue_GV_Golgra", "Gestral Village: Beating Golgra Reward" },
        { "BP_Dialogue_GV_JournalCollector", "Gestral Village: Journal Collection Reward" },
        { "BP_Dialogue_GV_Karatot", "Gestral Village: Karatom's Reward" },
        { "BP_Dialogue_GV_OnoPuncho", "Gestral Village: Ono Puncho's Reward" },
        { "BP_Dialogue_Harbour_HotelLove", "Lumiere Act I: Hotel Door Reward" },
        { "BP_Dialogue_HexgaLuster", "Stone Wave Cliffs: White Hexga's Reward" },
        { "BP_Dialogue_HiddenArena_Keeper", "Hidden Gestral Arena: Prizes" },
        { "BP_Dialogue_JudgeOfMercy", "The Fountain: Blanche's Reward" },
        { "BP_Dialogue_LUAct1_Mime", "Lumiere Act I: Mime Loot Drop" },
        { "BP_Dialogue_Gardens_Maelle_FirstDuel", "Lumiere Act I: Maelle Duel Reward" },
        { "BP_Dialogue_LuneCamp_Quest_4", "The Camp: Lune Gradient Unlock 2" },
        { "BP_Dialogue_LuneCamp_Quest_6", "The Camp: Lune's Music Record" },
        { "BP_Dialogue_LuneCamp_Quest_7", "The Camp: Lune Gradient Unlock 3" },
        { "BP_Dialogue_MaelleCamp_Quest_4", "The Camp: Maelle Gradient Unlock 2" },
        { "BP_Dialogue_MaelleCamp_Quest_7", "The Camp: Maelle Gradient Unlock 3" },
        { "BP_Dialogue_MainPlaza_Furnitures", "Lumiere Act I: Furniture Found Item" },
        { "BP_Dialogue_MainPlaza_Trashcan", "Lumiere Act I: Trash-can Man" },
        { "BP_Dialogue_MainPlaza_Trashcan_useless", "BP_Dialogue_MainPlaza_Trashcan_useless" },
        { "BP_Dialogue_Manor_Wardrobe", "The Manor: Wardrobe" },
        { "BP_Dialogue_MimeChromaZoneEntrance", "Sunless Cliffs: Mime's True Art Unreserved" },
        { "BP_Dialogue_MonocoCamp_Quest_3", "The Camp: Verso and Monoco's Haircuts" },
        { "BP_Dialogue_MonocoCamp_Quest_4", "The Camp: Monoco Gradient Unlock 2" },
        { "BP_Dialogue_MonocoCamp_Quest_6", "The Camp: Monoco's Music Record" },
        { "BP_Dialogue_MonocoCamp_Quest_7", "The Camp: Monoco Gradient Unlock 3" },
        { "BP_Dialogue_MS_Grandis_Fashionist_V2", "Monoco's Station: Grandis Fashionista's Reward" },
        { "BP_Dialogue_MS_Grandis_Grateful", "Monoco's Station: Grandis's Gift" },
        { "BP_Dialogue_MS_Grandis_WM_GuideOldLumiere", "World Map Near Monoco's Station: Grandis's Reward" },
        { "BP_Dialogue_Nicolas", "Lumiere Act I: Nicolas's Reward" },
        { "BP_Dialogue_Quest_LostGestralChief", "The Camp: Lost Gestrals Rewards" },
        { "BP_Dialogue_ScielCamp_Quest_4", "The Camp: Sciel Gradient Unlock 2" },
        { "BP_Dialogue_ScielCamp_Quest_6", "The Camp: Sciel's Music Record" },
        { "BP_Dialogue_ScielCamp_Quest_7", "The Camp: Sciel Gradient Unlock 3" },
        { "BP_Dialogue_SleepingBenisseur", "Red Woods: Sleeping Benisseur's Drop" },
        { "BP_Dialogue_TheAbyss_SimonP2Rematch", "The Abyss: Simon Rematch Reward" },
        { "BP_Dialogue_TroubadourCantPlay", "Stone Quarry: White Troubadour's Reward" },
        { "BP_Dialogue_VolleyBall", "Gestral Beach: Volleyball Rewards" },
        { "BP_Dialog_DanseuseDanceClass", "Frozen Hearts: White Danseuse's Reward" },
        { "BP_Dialog_FacelessBoy_CleasFlyingHouse_Main", "Flying Manor: Faceless Boy's Reward" },
        { "BP_Dialog_FacelessBoy_OrangeForest", "Falling Leaves: Faceless Boy's Reward" },
        { "BP_Dialog_Goblu_DemineurMissingMine", "Flying Waters: White Demineur's Reward" },
        { "BP_Dialog_GV_Gestral_FlyingCasino_InsideGuy", "Flying Casino: Most Cultured Swine's Gift" },
        { "BP_Dialog_GV_Gestral_InvisibleCave", "Sinister Cave: Dead Gestral's Loot" },
        { "BP_Dialog_JarNeedLight", "Spring Meadows: White Jar's Reward" },
        { "BP_Dialog_SpiritClea_CleasTower", "The Endless Tower: Clea's Gift to Maelle" },
        { "BP_Dialog_SpiritPortier", "Esoteric Ruins: White Portier's Reward" },
        { "BP_Dialog_WeaponlessChalier1", "Flying Cemetery: White Chalier's Reward" },
        { "BP_Dialogue_Richard", "Lumiere Act I: Richard's Gift to Jules" },
        { "BP_Dialogue_Boulangerie", "Lumiere Act I NG+: Boulangerie" },
        { "BP_Dialogue_Jules", "Lumiere Act I: Jules' Gift to Gustave" },
        { "BP_Dialogue_Lumiere_ExpFestival_Apprentices", "Lumiere Act I: Gift from Gustave's Apprentices" },
        { "BP_Dialogue_Lumiere_ExpFestival_Token_Artifact_Colette", "Lumiere Act I: Colette's Artifact" },
        { "BP_Dialogue_Lumiere_ExpFestival_Token_Haircut_Amandine", "Lumiere Act I: Amandine's Gorgeous Haircut" },
        { "BP_Dialogue_Lumiere_ExpFestival_Token_Pictos_Claude", "Lumiere Act I: Tom's Personal Masterpiece" },
        { "BP_Dialogue_Lumiere_ExpFestival_Maelle", "Lumiere Act I: Maelle Festival Duel Reward" },
    };

    private int _getItemQuantity(string key)
    {
        if (!DialogueRewardQuantitiesPaths.ContainsKey(FileName) && FileName != "BP_Dialogue_JudgeOfMercy") return -1;

        HasItemQuantities = true;
        if (FileName == "BP_Dialogue_JudgeOfMercy")
        {
            return 100;
        }

        var rewardQuantityPaths = DialogueRewardQuantitiesPaths[FileName];
        var path = rewardQuantityPaths[key];
        var quantityProperty =
            ((_asset.Exports[path[0]] as NormalExport).Data[path[1]] as ArrayPropertyData).Value[path[2]] as
            IntPropertyData;

        return quantityProperty.Value;
    }

    private void _setItemQuantityJudgeOfMercy(int quantity)
    {
        var bytecode = (_asset.Exports[3] as FunctionExport).ScriptBytecode[691] as EX_Context;
        var parameter = (bytecode.ContextExpression as EX_LocalVirtualFunction).Parameters[1] as EX_IntConst;
        parameter.Value = quantity;
    }

    private void _setItemQuantity(string key, int quantity)
    {
        if (FileName == "BP_Dialogue_JudgeOfMercy")
        {
            _setItemQuantityJudgeOfMercy(quantity);
            return;
        }

        if (!DialogueRewardQuantitiesPaths.ContainsKey(FileName)) return;
        var rewardQuantityPaths = DialogueRewardQuantitiesPaths[FileName];
        var path = rewardQuantityPaths[key];
        var quantityProperty =
            ((_asset.Exports[path[0]] as NormalExport).Data[path[1]] as ArrayPropertyData).Value[path[2]] as
            IntPropertyData;
        quantityProperty.Value = Math.Abs(quantity);
    }

    public override void LoadFromAsset(UAsset asset)
    {
        base.LoadFromAsset(asset);

        var rewardPaths = DialogueRewardPaths[FileName];

        foreach (var rewardPath in rewardPaths)
        {
            var checkName = rewardPath.Key;
            var checkPath = rewardPath.Value;
            var export = _asset.Exports[checkPath[0]] as NormalExport;
            var rewardStruct = export[checkPath[1]] as StructPropertyData;
            var arrayIndexOffest = 0;
            if (export[checkPath[1]] is ArrayPropertyData)
            {
                rewardStruct = (export[checkPath[1]] as ArrayPropertyData).Value[checkPath[2]] as StructPropertyData;
                arrayIndexOffest = 1;
            }
            if (export[checkPath[1]] is MapPropertyData)
            {
                rewardStruct = (export[checkPath[1]] as MapPropertyData).Value.Values.ToList()[checkPath[2]] as StructPropertyData;
                arrayIndexOffest = 1;
            }
            for (int i = 2 + arrayIndexOffest; i < checkPath.Count - 1; i++)
            {
                rewardStruct = rewardStruct.Value[checkPath[i]] as StructPropertyData;
            }

            var itemName = (rewardStruct.Value[checkPath.Last()] as NamePropertyData).ToString();
            var itemData = ItemsController.GetItemData(itemName);
            var itemQuantity = _getItemQuantity(checkName);

            SourceSections[checkName] = [new ItemSourceParticle(itemData, itemQuantity)];
            var check = new CheckData
            {
                CodeName = checkName,
                CustomName = $"{_dialogueNames.GetValueOrDefault(FileName, FileName)}: {itemData.CustomName}",
                IsBroken = false,
                IsPartialCheck = false,
                IsFixedSize = true,
                ItemSource = this,
                Key = checkName
            };
            Checks.Add(check);
        }
    }

    public override UAsset SaveToAsset()
    {
        var rewardPaths = DialogueRewardPaths[FileName];

        foreach (var rewardPath in rewardPaths)
        {
            var checkName = rewardPath.Key;
            var checkPath = rewardPath.Value;
            var export = _asset.Exports[checkPath[0]] as NormalExport;
            var rewardStruct = export[checkPath[1]] as StructPropertyData;
            var arrayIndexOffest = 0;
            if (export[checkPath[1]] is ArrayPropertyData)
            {
                rewardStruct = (export[checkPath[1]] as ArrayPropertyData).Value[checkPath[2]] as StructPropertyData;
                arrayIndexOffest = 1;
            }
            if (export[checkPath[1]] is MapPropertyData)
            {
                rewardStruct = (export[checkPath[1]] as MapPropertyData).Value.Values.ToList()[checkPath[2]] as StructPropertyData;
                arrayIndexOffest = 1;
            }
            for (int i = 2 + arrayIndexOffest; i < checkPath.Count - 1; i++)
            {
                rewardStruct = rewardStruct.Value[checkPath[i]] as StructPropertyData;
            }

            _asset.AddNameReference(FString.FromString(SourceSections[checkName][0].Item.CodeName));

            _setItemQuantity(checkName, SourceSections[checkName][0].Quantity);
            (rewardStruct.Value[checkPath.Last()] as NamePropertyData).Value =
                FName.FromString(_asset, SourceSections[checkName][0].Item.CodeName);
        }

        return _asset;
    }
}