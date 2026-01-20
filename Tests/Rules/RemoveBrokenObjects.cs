namespace Tests.Rules;

public class RemoveBrokenObjects: OutputRuleBase
{
    public static List<string> BrokenEnemies =
    [
        "QUEST_WeaponlessChalier",
        "FB_Dualliste_Phase1",
        "YF_Jar_AlternativeA",
        "YF_Jar_AlternativeB",
        "SM_Volester_AlternativA",
        "SM_Volester_AlternativB",
        "SM_Volester_AlternativC",
        "Petank_Parent"
    ];
    public static List<string> BrokenEncounters =
    [
        "Quest_WeaponlessChalier*1",
        "FB_Dualliste_Phase1*1",
        "JarAlternativeA*1",
        "JarAlternativeA*1_Gault*2",
        "JarAlternativeB*1",
        "VolesterAlternativA*1",
        "VolesterAlternativB*1",
        "VolesterAlternativC*1",
        "WM_Potier*1_Glaise*1_Volester*1",
    ];
    
    public static List<string> BrokenItems =
    [
        "MitigatedPerfection",
        "Chroma",
        "04_Key_Placeholder",
        "Gold_Small",
        "Gold_Medium",
        "Gold_Big",
        "Consumable_SkillPoint",
        "VeilleurFoot",
        "PetankFoot",
        "NoireFoot",
        "BourgeonFoot",
        "RiskSeeker",
        "LimonsolPictos",
        "02_ArmPicto_Placeholder",
        "AntiShock",
        "NullPhysical",
        "NullFire",
        "NullIce",
        "NullEarth",
        "NullThunder",
        "NullDark",
        "NullLight",
        "AbsorbPhysical",
        "AbsorbFire",
        "AbsorbIce",
        "AbsorbEarth",
        "AbsorbLight",
        "AbsorbThunder",
        "AbsorbDark",
        "Speedster",
        "Blitz",
        "AngelGrace",
        "AngelPresent",
        "AngelicChance",
        "RiskTaker",
        "HighOnPerfect",
        "FasterThanHisShadow",
        "BrambleSkin",
        "SpreadingBrambleSkin",
        "BrambleParry",
        "FastAttacker",
        "ReviveBombFire",
        "ReviveBombIce",
        "ReviveBombThunder",
        "ReviveBombEarth",
        "ReviveWithPrecision",
        "FireSkin",
        "FrozenSkin",
        "InvertedSkin",
        "OverConfident",
        "Fugitive",
        "TurboKiller",
        "FlashDodge",
        "DeathBombFire",
        "DeathBombFrozen",
        "DeathBombThunder",
        "DeathBombEarth",
        "DeathBombLight",
        "DeathBombDark",
        "DeathBombVoid",
        "PhysicalModifier",
        "MakeItQuick",
        "AutoPrecision",
        "BramblePerformer"
    ];
    
    public override bool IsSatisfied(Output output, Config config)
    {
        foreach (var encounter in output.Encounters)
        {
            if (encounter.Enemies.Any(i => BrokenEnemies.Contains(i.CodeName)))
            {
                FailureMessage += $"{encounter} contains broken enemies";
                return false;
            }
        }

        foreach (var check in output.Checks)
        {
            if (check.Items.Any(i => BrokenItems.Contains(i.Item.CodeName)))
            {
                FailureMessage += $"{check} contains broken items";
                return false;
            }
        }
        return true;
    }
}