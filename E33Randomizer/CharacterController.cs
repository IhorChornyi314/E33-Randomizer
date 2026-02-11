using System.IO;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class CharacterController: Controller<CharacterData>
{
    private UAsset _sophieJoinAsset;
    private UAsset _gustaveJoinAsset;
    private UAsset _replaceGustaveAsset;
    private UAsset _sophieLeaveAsset;
    private UAsset _luneJoinAsset;
    private UAsset _luneJoinCinematicAsset;
    private UAsset _maelleJoinAsset;
    private UAsset _maelleJoinCinematicAsset;
    private UAsset _scielJoinAsset;
    private UAsset _scielJoinCinematicAsset;
    private UAsset _ripGustaveAsset;
    private UAsset _versoReplaceGustaveAsset;
    private UAsset _monocoJoinAsset;
    private UAsset _monocoJoinCinematicAsset;
    private UAsset _gustaveReplaceVersoAsset;
    
    public CharacterData[] charactersJoinOrder = new CharacterData[6];
    
    public override void Initialize()
    {
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/character_data.json");
        charactersJoinOrder = [GetObject("Frey"), GetObject("Lune"), GetObject("Maelle"), GetObject("Sciel"), GetObject("Verso"), GetObject("Monoco")];
        ReadAssets($"{RandomizerLogic.DataDirectory}/ParandomizerData/CharacterJoinGAs");
    }

    public void ReadAssets(string filesDirectory)
    {
        _gustaveJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_TheDeparture.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _replaceGustaveAsset = new UAsset($"{filesDirectory}/DA_ReplaceCharacter_GustaveByRandom.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        _luneJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_LuneJoinGroup.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _luneJoinCinematicAsset = new UAsset($"{filesDirectory}/DA_GA_CIN_LuminizerIntro.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        _maelleJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_MaelleJoinsGroup.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _maelleJoinCinematicAsset = new UAsset($"{filesDirectory}/DA_GA_CIN_MaelleReunion.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        _scielJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_ScielJoinsGroup_P1.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _scielJoinCinematicAsset = new UAsset($"{filesDirectory}/DA_GA_CIN_PostGestralTournament.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        _ripGustaveAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_GustaveDieEndLevel.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _versoReplaceGustaveAsset = new UAsset($"{filesDirectory}/DA_ReplaceCharacter_GustaveByVerso_modified.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        _monocoJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_MonocoUnlock.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _monocoJoinCinematicAsset = new UAsset($"{filesDirectory}/DA_GA_CIN_MonocoJoinsTheGroup_GrandisOutside.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        _gustaveReplaceVersoAsset = new UAsset($"{filesDirectory}/DA_ReplaceCharacter_VersoByGustave.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
    }

    public void SetGustaveSpotCharacter(CharacterData newCharacter)
    {
        var characterName = newCharacter.CodeName;
        if (characterName == "Frey") return;
        
        var innerImportIndex = Utils.AddImportToUAsset(_gustaveJoinAsset, "BP_GameAction_ReplaceCharacter_C",
            "/Game/Gameplay/GameActionsSystem/ReplaceCharacter/Content/DA_ReplaceCharacter_GustaveByRandom",
            innerClassPackage: "/Game/Gameplay/GameActionsSystem/ReplaceCharacter/BP_GameAction_ReplaceCharacter");

        Utils.AddImportToUAsset(_gustaveJoinAsset, "BP_GameAction_ReplaceCharacter_C",
            "/Game/Gameplay/GameActionsSystem/ReplaceCharacter/BP_GameAction_ReplaceCharacter",
            "Default__BP_GameAction_ReplaceCharacter_C",
            "/Game/Gameplay/GameActionsSystem/ReplaceCharacter/BP_GameAction_ReplaceCharacter");
        
        var actionsArray = (_gustaveJoinAsset.Exports[0] as NormalExport).Data[0] as ArrayPropertyData;
        // Action 3 has external reference
        var newActionStruct = actionsArray.Value[3].Clone() as StructPropertyData;

        newActionStruct.Name = FName.FromString(_gustaveJoinAsset, "5");
        (newActionStruct.Value[0] as ObjectPropertyData).Value = innerImportIndex;
        actionsArray.Value = actionsArray.Value.Append(newActionStruct).ToArray();
        
        var replaceActionStruct = (_replaceGustaveAsset.Exports[0] as NormalExport).Data[0] as StructPropertyData;
        (replaceActionStruct.Value[1] as EnumPropertyData).FromString(["E_Characters", $"E_Characters::NewEnumerator{newCharacter.Enum}"], _replaceGustaveAsset);
        Utils.ReplaceNameReference(_ripGustaveAsset, "E_Characters::NewEnumerator0", $"E_Characters::NewEnumerator{newCharacter.Enum}");
        
        replaceActionStruct = (_versoReplaceGustaveAsset.Exports[0] as NormalExport).Data[0] as StructPropertyData;
        (replaceActionStruct.Value[0] as EnumPropertyData).FromString(["E_Characters", $"E_Characters::NewEnumerator{newCharacter.Enum}"], _versoReplaceGustaveAsset);
        
        replaceActionStruct = (_gustaveReplaceVersoAsset.Exports[0] as NormalExport).Data[0] as StructPropertyData;
        (replaceActionStruct.Value[1] as EnumPropertyData).FromString(["E_Characters", $"E_Characters::NewEnumerator{newCharacter.Enum}"], _gustaveReplaceVersoAsset);
    }

    public void SetLuneSpotCharacter(string characterName)
    {
        if (characterName == "Lune") return;
        Utils.ReplaceNameReference(_luneJoinAsset, "Lune", characterName);
        Utils.ReplaceNameReference(_luneJoinCinematicAsset, "Lune", characterName);
    }
    
    public void SetMaelleSpotCharacter(CharacterData newCharacter)
    {
        if (newCharacter.CodeName == "Maelle") return;
        
        Utils.ReplaceNameReference(_maelleJoinAsset, "Maelle", newCharacter.CodeName);
        Utils.ReplaceNameReference(_maelleJoinCinematicAsset, "Maelle", newCharacter.CodeName);
        Utils.ReplaceNameReference(_maelleJoinAsset, "E_Characters::NewEnumerator1", $"E_Characters::NewEnumerator{newCharacter.Enum}");
    }
    
    public void SetScielSpotCharacter(string characterName)
    {
        if (characterName == "Sciel") return;
        Utils.ReplaceNameReference(_scielJoinAsset, "Sciel", characterName);
        Utils.ReplaceNameReference(_scielJoinCinematicAsset, "Sciel", characterName);
    }
    
    public void SetVersoSpotCharacter(CharacterData newCharacter)
    {
        if (newCharacter.CodeName == "Verso") return;
        
        var replaceActionStruct = (_versoReplaceGustaveAsset.Exports[0] as NormalExport).Data[0] as StructPropertyData;
        (replaceActionStruct.Value[1] as EnumPropertyData).FromString(["E_Characters", $"E_Characters::NewEnumerator{newCharacter.Enum}"], _versoReplaceGustaveAsset);
        
        replaceActionStruct = (_gustaveReplaceVersoAsset.Exports[0] as NormalExport).Data[0] as StructPropertyData;
        (replaceActionStruct.Value[0] as EnumPropertyData).FromString(["E_Characters", $"E_Characters::NewEnumerator{newCharacter.Enum}"], _gustaveReplaceVersoAsset);
    }
    
    public void SetMonocoSpotCharacter(string characterName)
    {
        if (characterName == "Monoco") return;
        Utils.ReplaceNameReference(_monocoJoinAsset, "Monoco", characterName);
        Utils.ReplaceNameReference(_monocoJoinCinematicAsset, "Monoco", characterName);
    }
    
    public override void Randomize()
    {
        RandomizerLogic.rand.Shuffle(charactersJoinOrder);
    }

    public override void AddObjectToContainer(string objectCodeName, string containerCodeName)
    {
        throw new NotSupportedException("Adding multiple characters at a time is not supported yet.");
    }

    public override void RemoveObjectFromContainer(int objectIndex, string containerCodeName)
    {
        throw new NotSupportedException("Adding multiple characters at a time is not supported yet.");
    }

    public override void InitFromTxt(string text)
    {
        text = text.ReplaceLineEndings("\n");
        var characterCodeNames = text.Split('\n');
        charactersJoinOrder = characterCodeNames.Select(GetObject).ToArray();
    }

    public override void ApplyViewModel()
    {
        throw new NotSupportedException("Skill nodes must have exactly one skill in them.");
    }

    public override void UpdateViewModel()
    {
        throw new NotSupportedException("Skill nodes must have exactly one skill in them.");
    }

    public override string ConvertToTxt()
    {
        return string.Join("\n", charactersJoinOrder.Select(c => c.CodeName));
    }

    public override void Reset()
    {
        charactersJoinOrder = [GetObject("Frey"), GetObject("Lune"), GetObject("Maelle"), GetObject("Sciel"), GetObject("Verso"), GetObject("Monoco")];
    }

    public override void WriteAssets()
    {
        //TODO: Change the other characters in preset fights
        //TODO: Figure out what to do with the set character dialogues
        //TODO: Fix the party not behaving properly
        
        //As of now it would take too much effort to fix all this, so I have to cut this out
        
        SetGustaveSpotCharacter(charactersJoinOrder[0]);
        SetLuneSpotCharacter(charactersJoinOrder[1].CodeName);
        SetMaelleSpotCharacter(charactersJoinOrder[2]);
        SetScielSpotCharacter(charactersJoinOrder[3].CodeName);
        SetVersoSpotCharacter(charactersJoinOrder[4]);
        SetMonocoSpotCharacter(charactersJoinOrder[5].CodeName);
        
        Utils.WriteAsset(_gustaveJoinAsset);
        Utils.WriteAsset(_replaceGustaveAsset);
        Utils.WriteAsset(_luneJoinAsset);
        Utils.WriteAsset(_luneJoinCinematicAsset);
        Utils.WriteAsset(_maelleJoinAsset);
        Utils.WriteAsset(_maelleJoinCinematicAsset);
        Utils.WriteAsset(_scielJoinAsset);
        Utils.WriteAsset(_scielJoinCinematicAsset);
        Utils.WriteAsset(_ripGustaveAsset);
        Utils.WriteAsset(_versoReplaceGustaveAsset);
        Utils.WriteAsset(_monocoJoinAsset);
        Utils.WriteAsset(_monocoJoinCinematicAsset);
        Utils.WriteAsset(_gustaveReplaceVersoAsset);
    }
}