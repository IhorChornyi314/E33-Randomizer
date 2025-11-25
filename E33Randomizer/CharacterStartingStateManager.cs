using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public static class CharacterStartingStateManager
{
    private static UAsset _characterSaveStatesAsset;
    private static UAsset _characterDefinitionsAsset;
    private static bool _wasModified;
    
    public static void Init()
    {
        _characterSaveStatesAsset = new UAsset($"{RandomizerLogic.DataDirectory}/Originals/StartingInfoTables/DT_jRPG_CharacterSaveStates.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        _characterDefinitionsAsset = new UAsset($"{RandomizerLogic.DataDirectory}/Originals/StartingInfoTables/DT_jRPG_CharacterDefinitions.uasset", EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
    }

    public static void SetStartingWeapon(string characterName, ItemData weapon)
    {
        _wasModified = true;
        var tableData = (_characterSaveStatesAsset.Exports[0] as DataTableExport).Table.Data;

        foreach (var propertyData in tableData)
        {
            if (propertyData.Name.ToString() != characterName) continue;
            var mapValues = (propertyData.Value[10] as MapPropertyData).Value.Values.ToList();
            var nameProperty = mapValues[0] as NamePropertyData;
            _characterSaveStatesAsset.AddNameReference(FString.FromString(weapon.CodeName));
            nameProperty.Value = FName.FromString(_characterSaveStatesAsset, weapon.CodeName);
        }
    }

    public static void SetStartingCosmetics(string characterName, ItemData skin, ItemData face)
    {
        _wasModified = true;
        characterName = characterName == "Gustave" ? "Frey" : characterName;
        
        var tableData = (_characterDefinitionsAsset.Exports[0] as DataTableExport).Table.Data;
        
        foreach (var propertyData in tableData)
        {
            if (propertyData.Name.ToString() != characterName) continue;
            var cosmeticsStruct = propertyData.Value[21] as StructPropertyData;
                
            _characterDefinitionsAsset.AddNameReference(FString.FromString(skin.CodeName));
            _characterDefinitionsAsset.AddNameReference(FString.FromString(face.CodeName));
                
            (cosmeticsStruct.Value[0] as NamePropertyData).Value = FName.FromString(_characterDefinitionsAsset, skin.CodeName);
            (cosmeticsStruct.Value[1] as NamePropertyData).Value = FName.FromString(_characterDefinitionsAsset, face.CodeName);
        }
    }

    public static void SetStartingSkills(string characterName, List<SkillData> skills)
    {
        _wasModified = true;
        var tableData = (_characterSaveStatesAsset.Exports[0] as DataTableExport).Table.Data;

        foreach (var propertyData in tableData)
        {
            var unlockedSkillsStruct = propertyData.Value[12] as ArrayPropertyData;
            var equippedSkillsStruct = propertyData.Value[13] as ArrayPropertyData;
            for (int i = 0; i < Math.Min(unlockedSkillsStruct.Value.Length, skills.Count); i++)
            {
                if (propertyData.Name.ToString() != characterName) continue;
                var unlockedSkillNameProperty = unlockedSkillsStruct.Value[i] as NamePropertyData;
                _characterSaveStatesAsset.AddNameReference(FString.FromString(skills[i].NameID));
                unlockedSkillNameProperty.Value = FName.FromString(_characterSaveStatesAsset, skills[i].NameID);
                var equippedSkillNameProperty = equippedSkillsStruct.Value[i] as NamePropertyData;
                equippedSkillNameProperty.Value = FName.FromString(_characterSaveStatesAsset, skills[i].NameID);
            }
        }
    }
    
    public static void SaveAssets()
    {
        if (!_wasModified) return;
        Utils.WriteAsset(_characterSaveStatesAsset);
        Utils.WriteAsset(_characterDefinitionsAsset);
    }
}