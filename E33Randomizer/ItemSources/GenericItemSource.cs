using System.Net;
using UAssetAPI;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer.ItemSources;


public class GenericItemSource: ItemSource
{
    private static List<string> _questRequirementItems = ["Quest_Resin", "Quest_WoodBoards", "LostGestral", "Quest_Mine", "FestivalToken", "Quest_UniformForSon", "Quest_HexgaRock", "Quest_Mushroom"];

    private static Dictionary<string, string> _musicRecords = new()
    {
        { "BP_Dialog_FacelessBoy_OrangeForest", "MusicRecord_9" },
        { "BP_Dialogue_LUAct1_Mime", "MusicRecord_2" },
        { "BP_Dialogue_LuneCamp_Quest_6", "MusicRecord_5" },
        { "BP_Dialogue_ScielCamp_Quest_6", "MusicRecord_6" },
        { "BP_Dialogue_MonocoCamp_Quest_6", "MusicRecord_7" }
    };

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
            if (name == "MusicRecord")
            {
                name = _musicRecords.GetValueOrDefault(FileName, name);
            }
            if (_questRequirementItems.Contains(name))
            {
                continue;
            }
            if (ItemsController.IsItem(name))
            {
                var newItem = ItemsController.GetItemData(name);
                _originalNameReferenceIndexes.Add(i);
                SourceSections[FileName].Add(new ItemSourceParticle(newItem));
            }
        }
        var check = new CheckData
        {
            CodeName = FileName,
            CustomName = _dialogueNames.GetValueOrDefault(FileName, FileName),
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
            var newItem = ItemsController.GetItemData(RandomizerLogic.CustomItemPlacement.Replace(oldItem.CodeName));
            itemSourceParticle.Item = newItem;
        }
    }
}