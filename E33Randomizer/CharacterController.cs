using System.IO;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class CharacterController: Controller<CharacterData>
{
    private string _cleanSnapshot;
    private UAsset _sophieJoinAsset;
    private UAsset _sophieLeaveAsset;
    private UAsset _luneJoinAsset;
    private UAsset _maelleJoinAsset;
    private UAsset _scielJoinAsset;
    private UAsset _ripGustaveAsset;
    private UAsset _versoReplaceGustaveAsset;
    private UAsset _monocoJoinAsset;
    private UAsset _gustaveReplaceVersoAsset;

    public CharacterData[] charactersJoinOrder = new CharacterData[6];
    
    public override void Initialize()
    {
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/character_data.json");
        _cleanSnapshot = ConvertToTxt();
        charactersJoinOrder = [GetObject("Noah"), GetObject("Lune"), GetObject("Maelle"), GetObject("Sciel"), GetObject("Verso"), GetObject("Monoco")];
    }

    public void ReadAssets(string filesDirectory)
    {
        _sophieJoinAsset = new UAsset($"{filesDirectory}/DA_SEQ_MyFlower_P2.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _luneJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_LuneJoinGroup.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _maelleJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_MaelleJoinsGroup.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _scielJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_ScielJoinsGroup_P1.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _ripGustaveAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_GustaveDieEndLevel.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _versoReplaceGustaveAsset = new UAsset($"{filesDirectory}/DA_ReplaceCharacter_GustaveByVerso.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _monocoJoinAsset = new UAsset($"{filesDirectory}/DA_GA_SQT_MonocoUnlock.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _gustaveReplaceVersoAsset = new UAsset($"{filesDirectory}/DA_ReplaceCharacter_VersoByGustave.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
    }

    public override void Randomize()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }

    public override void WriteAssets()
    {
        throw new NotImplementedException();
    }
}