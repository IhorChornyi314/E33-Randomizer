using System.IO;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public static class StaticFileChanger
{
    
    public static void GenerateConditionCheckerFile(string questName)
    {
        var asset = new UAsset($"{RandomizerLogic.DataDirectory}/Originals/DA_ConditionChecker_Merchant_GrandisStation.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);

        var newConditionalName = $"DA_ConditionChecker_Merchant_{questName}";
        
        asset.SetNameReference(2, FString.FromString(questName.Split("999")[0]));
        asset.SetNameReference(20, FString.FromString(questName.Split("999")[1]));
        asset.SetNameReference(5, FString.FromString(newConditionalName));
        asset.SetNameReference(6, FString.FromString($"/Game/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker/{newConditionalName}"));

        var e = asset.Exports[1] as NormalExport;
        (e.Data[0] as TextPropertyData).Value = FString.FromString("ST_GM_MERCHANT_CONDITION_REACH_A_POINT");
        (e.Data[1] as TextPropertyData).Value = FString.FromString("ST_GM_MERCHANT_CONDITION_REACH_A_POINT_DESC");
        
        
        asset.FolderName = FString.FromString($"/Game/Gameplay/Inventory/Merchant/Merchants_ConditionsChecker/{newConditionalName}");
        
        Utils.WriteAsset(asset);
    }

    public static void CreateEsquieRocks()
    {
        var relationshipQuests = new UAsset($"D:\\Mods\\Unreal Engine Mods\\retoc\\out\\Sandfall\\Content\\Levels\\WorldMap\\Camps\\GameActions\\Quests\\DA_GA_SQT_COND_RelationshipQuests.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        relationshipQuests.AddNameReference(FString.FromString("BP_ConditionChecker_ItemInInventory_C"));
        relationshipQuests.AddNameReference(FString.FromString("/Game/Gameplay/ConditionChecker/Inventory/BP_ConditionChecker_ItemInInventory"));
        relationshipQuests.AddNameReference(FString.FromString("Default__BP_ConditionChecker_ItemInInventory_C"));
        
        Utils.AddImportToUAsset(relationshipQuests, "CompositeDataTable", "/Game/jRPGTemplate/Datatables/DT_jRPG_Items_Composite");
        var itemDataTableIndex = Utils.AddImportToUAsset(relationshipQuests, "DataTable",
            "/Game/Gameplay/Quests/Data/DT_QuestsDataTable");
        
        var outerImport = new Import("/Script/CoreUObject", "Package", FPackageIndex.FromRawIndex(0),
            "/Game/Gameplay/ConditionChecker/Inventory/BP_ConditionChecker_ItemInInventory", false, relationshipQuests);
        var outerIndex = relationshipQuests.AddImport(outerImport);
        var innerImport = new Import("/Script/Engine", "BlueprintGeneratedClass", outerIndex, "BP_ConditionChecker_ItemInInventory_C", false, relationshipQuests);
        var defaultImport = new Import("/Game/Gameplay/ConditionChecker/Inventory/BP_ConditionChecker_ItemInInventory", "BP_ConditionChecker_ItemInInventory_C", outerIndex, "Default__BP_ConditionChecker_ItemInInventory_C", false, relationshipQuests);
        var defaultIndex = relationshipQuests.AddImport(defaultImport);
        
        var innerIndex = relationshipQuests.AddImport(innerImport);

        string[] rocks = ["Florrie", "Dorrie", "Soarrie"];
        
        for (int i = 0; i < 3; i++)
        {
            var oldDoorInteract = new UAsset($"D:\\Mods\\Unreal Engine Mods\\retoc\\out\\Sandfall\\Content\\Levels\\OldLumiere\\GameActions\\Quest\\DA_GA_COND_OldDoorInteract.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);

            relationshipQuests.AddNameReference(FString.FromString($"QUEST_EsquieRock{rocks[i]}"));
            var e = (relationshipQuests.Exports[i + 2] as NormalExport);
            var c = (relationshipQuests.Exports[i + 2] as NormalExport).Data[2];
            e.Data = (oldDoorInteract.Exports[0] as NormalExport).Data;

            e.Data[0] = e.Data[0].Clone() as StructPropertyData;
            (e.Data[0] as StructPropertyData).Value[0] = (e.Data[0] as StructPropertyData).Value[0].Clone() as StructPropertyData;
            ((e.Data[0] as StructPropertyData).Value[0] as StructPropertyData).Value[0] = ((e.Data[0] as StructPropertyData).Value[0] as StructPropertyData).Value[0].Clone() as NamePropertyData;
            e.Data[1] = c;

            (((e.Data[0] as StructPropertyData).Value[0] as StructPropertyData).Value[0] as NamePropertyData).Value =
                FName.FromString(relationshipQuests, $"QUEST_EsquieRock{rocks[i]}");
        
            e.ObjectName.Value = FString.FromString($"BP_ConditionChecker_ItemInInventory_C_{i}");
            e.ClassIndex = innerIndex;
            e.TemplateIndex = defaultIndex;
            
            var innerRemoveRockImport =
                new ImportData(
                    "/Game/Gameplay/GameActionsSystem/SequentialGameAction/BP_GameAction_Sequential",
                    "BP_GameAction_Sequential_C", $"DA_GA_SQT_Give{rocks[i]}ToEsquie");
            var outerRemoveRockImport =
                new ImportData($"/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_SQT_Give{rocks[i]}ToEsquie");

            var removeRockIndex = Utils.AddImportToUAsset(relationshipQuests, innerRemoveRockImport, outerRemoveRockImport);
        
            var ca = relationshipQuests.Exports[7 + i] as NormalExport;

            var gaReference = (ca.Data[0] as StructPropertyData).Value[0] as ObjectPropertyData;
            gaReference.Name = FName.FromString(relationshipQuests, "GameActionReference");
            gaReference.Value = removeRockIndex;
        }

        var seq = (relationshipQuests.Exports[17] as NormalExport).Data[0] as ArrayPropertyData;
        seq.Value = [seq.Value[0], seq.Value[3], seq.Value[4]];
        
        relationshipQuests.SetNameReference(0, FString.FromString("/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_SQT_COND_EsquieRocks"));
        relationshipQuests.SetNameReference(3, FString.FromString("DA_GA_SQT_COND_EsquieRocks"));

        relationshipQuests.FolderName =
            FString.FromString("/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_SQT_COND_EsquieRocks");
        
        Utils.WriteAsset(relationshipQuests);
    }

    public static void CreateEsquieNotif(string capacity)
    {
        var esquieNotif = new UAsset($"D:\\Mods\\Unreal Engine Mods\\retoc\\out\\Sandfall\\Content\\Levels\\WorldMap\\Camps\\GameActions\\Quests\\DA_GA_NOTIF_EsquieUnderwater.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        esquieNotif.SetNameReference(11, FString.FromString($"/Game/UI/Widgets/FullScreenNotificationSystem/Data/WorldMapCapacities/DA_FullscreenNotification_WMC_{capacity}"));
        esquieNotif.SetNameReference(12, FString.FromString($"DA_FullscreenNotification_WMC_{capacity}"));
        
        esquieNotif.SetNameReference(1, FString.FromString($"/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_NOTIF_CustomEsquie{capacity}"));
        esquieNotif.SetNameReference(0, FString.FromString($"DA_GA_NOTIF_CustomEsquie{capacity}"));

        esquieNotif.FolderName =
            FString.FromString($"/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_NOTIF_CustomEsquie{capacity}");
        
        Utils.WriteAsset(esquieNotif);
    }

    public static void CreateRockGA(string rockName, string capacityEnum, string capacityName)
    {
        var gustaveDie = new UAsset($"D:\\Mods\\Unreal Engine Mods\\retoc\\out\\Sandfall\\Content\\Levels\\SeaCliff\\GameActions\\DA_GA_SQT_GustaveDieEndLevel.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        var innerNotifGAImport = new ImportData("/Script/Engine", "BlueprintGeneratedClass",
            "BP_GameAction_ShowFullscreenNotification_C");
        var outerNotifGAImport =
            new ImportData(
                "/Game/Gameplay/GameActionsSystem/FullscreenNotification/BP_GameAction_ShowFullscreenNotification");
        var defaultNotifGAImport =
            new ImportData(
                "/Game/Gameplay/GameActionsSystem/FullscreenNotification/BP_GameAction_ShowFullscreenNotification",
                "BP_GameAction_ShowFullscreenNotification_C", "Default__BP_GameAction_ShowFullscreenNotification_C");
        
        Utils.AddImportToUAsset(gustaveDie, innerNotifGAImport, outerNotifGAImport, defaultNotifGAImport);
        
        var innerEsquieNotifImport =
            new ImportData(
                "/Game/Gameplay/GameActionsSystem/FullscreenNotification/BP_GameAction_ShowFullscreenNotification",
                "BP_GameAction_ShowFullscreenNotification_C", $"DA_GA_NOTIF_CustomEsquie{capacityName}");
        var outerEsquieNotifImport =
            new ImportData($"/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_NOTIF_CustomEsquie{capacityName}");

        var esquieNotifIndex = Utils.AddImportToUAsset(gustaveDie, innerEsquieNotifImport, outerEsquieNotifImport);
        
        gustaveDie.SetNameReference(7, FString.FromString($"QUEST_EsquieRock{rockName}"));
        gustaveDie.SetNameReference(4, FString.FromString(capacityEnum));

        var dummy =
            ((gustaveDie.Exports[4] as NormalExport).Data[0] as ArrayPropertyData).Value[0] as StructPropertyData;
        var removeItemAction = dummy.Clone() as StructPropertyData;
        (removeItemAction.Value[1] as ObjectPropertyData).Value = FPackageIndex.FromRawIndex(4);
        
        var unlockExplorationAction = dummy.Clone() as StructPropertyData;
        (unlockExplorationAction.Value[1] as ObjectPropertyData).Value = FPackageIndex.FromRawIndex(11);
        
        var showNotifAction = dummy.Clone() as StructPropertyData;
        (showNotifAction.Value[0] as ObjectPropertyData).Value = esquieNotifIndex;
        (showNotifAction.Value[1] as ObjectPropertyData).Value = FPackageIndex.FromRawIndex(0);

        ((gustaveDie.Exports[4] as NormalExport).Data[0] as ArrayPropertyData).Value =
            [removeItemAction, unlockExplorationAction, showNotifAction];
        
        gustaveDie.SetNameReference(15, FString.FromString($"DA_GA_SQT_Give{rockName}ToEsquie"));
        gustaveDie.SetNameReference(20, FString.FromString($"/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_SQT_Give{rockName}ToEsquie"));

        gustaveDie.FolderName =
            FString.FromString($"/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_SQT_Give{rockName}ToEsquie");
        
        Utils.WriteAsset(gustaveDie);
    }

    public static void CreateRelationshipQuests()
    {
        var relationshipQuests = new UAsset($"{RandomizerLogic.DataDirectory}/ItemData/StoryReplacementsData/GameActions/DA_GA_SQT_COND_RelationshipQuests.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        var innerRemoveRockImport =
            new ImportData(
                "/Game/Gameplay/GameActionsSystem/SequentialGameAction/BP_GameAction_Sequential",
                "BP_GameAction_Sequential_C", $"DA_GA_SQT_COND_EsquieRocks");
        var outerRemoveRockImport =
            new ImportData($"/Game/Levels/WorldMap/Camps/GameActions/Quests/DA_GA_SQT_COND_EsquieRocks");

        var removeRockIndex = Utils.AddImportToUAsset(relationshipQuests, innerRemoveRockImport, outerRemoveRockImport);
        
        var seq = (relationshipQuests.Exports[17] as NormalExport).Data[0] as ArrayPropertyData;
        
        var dummy = seq.Value[0].Clone() as StructPropertyData;
        (dummy.Value[0] as ObjectPropertyData).Value = removeRockIndex;
        (dummy.Value[1] as ObjectPropertyData).Value = FPackageIndex.FromRawIndex(0);

        seq.Value = [seq.Value[0], seq.Value[1], seq.Value[2], seq.Value[3], seq.Value[4], dummy];
        
        Utils.WriteAsset(relationshipQuests);
    }
    
    public static void Run()
    {
        if (Directory.Exists("randomizer"))
        {
            Directory.Delete("randomizer", true);
        }
        // CreateEsquieNotif("Swim");
        // CreateEsquieNotif("SwimBoost");
        // CreateEsquieNotif("Fly");
        // CreateRockGA("Florrie", "E_WorldMapExplorationCapacity::NewEnumerator2", "Swim");
        // CreateRockGA("Dorrie", "E_WorldMapExplorationCapacity::NewEnumerator3", "SwimBoost");
        // CreateRockGA("Soarrie", "E_WorldMapExplorationCapacity::NewEnumerator4", "Fly");
        CreateEsquieRocks();
        // CreateRelationshipQuests();
        Console.WriteLine();
    }
}