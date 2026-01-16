using E33Randomizer;

namespace Tests.Rules;

public class IncludeGearInPrologue: OutputRuleBase
{
    private static string[] _prologueCheckNames =
    {
        "BP_Dialogue_Eloise#Consumable_LuminaPoint",
        "BP_Dialogue_Gardens_Maelle_FirstDuel#Consumable_LuminaPoint",
        "BP_Dialogue_Harbour_HotelLove#Consumable_LuminaPoint",
        "BP_Dialogue_Jules#SkinGustave_Default_Red",
        "BP_Dialogue_LUAct1_Mime#MusicRecord_2",
        "BP_Dialogue_Lumiere_ExpFestival_Apprentices#Quest_ApprenticesJournal",
        "BP_Dialogue_Lumiere_ExpFestival_Maelle#FestivalToken",
        "BP_Dialogue_Lumiere_ExpFestival_Token_Artifact_Colette#Quest_OldKey",
        "BP_Dialogue_Lumiere_ExpFestival_Token_Haircut_Amandine#FaceGustave_Bun",
        "BP_Dialogue_Lumiere_ExpFestival_Token_Pictos_Claude#Quest_WeirdPictos",
        "BP_Dialogue_MainPlaza_Furnitures#Consumable_LuminaPoint",
        "BP_Dialogue_MainPlaza_Trashcan#UpgradeMaterial_Level1",
        "BP_Dialogue_MainPlaza_Trashcan#UpgradeMaterial_Level5",
        "BP_Dialogue_Nicolas#Consumable_Respec",
        "BP_Dialogue_Richard#Quest_UniformForSon",
        "DA_GA_SQT_TheGommage#BP_GameAction_AddItemToInventory_C_0",
        "DA_GA_SQT_TheGommage#BP_GameAction_AddItemToInventory_C_1",
    };
    
    public override bool IsSatisfied(Output output, Config config)
    {
        if (config.Settings.IncludeGearInPrologue) return true;
        var prologueChecks = _prologueCheckNames.Select(output.GetCheck);
        foreach (var check in prologueChecks)
        {
            // I know this probably shouldn't use the IsGearItem method but eh
            if (check.Items.Any(i => Controllers.ItemsController.IsGearItem(i.Item)))
            {
                FailureMessage += $"{check.Name} contains a gear item";
                return false;
            }
        }
        return true;
    }
}