using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using UAssetAPI;
using UAssetAPI.UnrealTypes;

namespace E33Randomizer;

public class SkillsController: Controller<SkillData>
{
    private string _cleanSnapshot;

    private Dictionary<string, string> skillIcons = new() { 
        {"DA_Skill_Verso_NEW_FromFire", "Game/Content/UI/Resources/Textures/Icons/Skills/Gustave/T_UI_Skill_Gustave_FromFire.uasset"},
        {"DA_Skill_MarkingShot", "Game/Content/UI/Resources/Textures/Icons/Skills/Gustave/T_UI_Skill_Gustave_MarkingShot.uasset"},
        {"DA_Skill_PerfectRecovery", "Game/Content/UI/Resources/Textures/Icons/Skills/Gustave/T_UI_Skill_Gustave_PerfectRecovery.uasset"},
        {"DA_Skill_Powerful", "Game/Content/UI/Resources/Textures/Icons/Skills/Gustave/T_UI_Skill_Gustave_Powerful.uasset"},
        {"DA_Skill_Lune_CripplingTsunami", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_CripplingTsunami.uasset"},
        {"DA_Skill_Lune_CrustalCrush", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_CrustalCrush.uasset"},
        {"DA_Skill_Lune_Electrify", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Electrify.uasset"},
        {"DA_Skill_Lune_ElementalGenesis", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_ElementalGenesis.uasset"},
        {"DA_Skill_Lune_ElementalTrick", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_ElementalTrick.uasset"},
        {"DA_Skill_Lune_FireRage", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_FireRage.uasset"},
        {"DA_Skill_Lune_HealingLight", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_HealingLight.uasset"},
        {"DA_Skill_Lune_Hell", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Hell.uasset"},
        {"DA_Skill_Lune_IceGust", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_IceGust.uasset"},
        {"DA_Skill_Lune_Immolation", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Immolation.uasset"},
        {"DA_Skill_Lune_Mayhem", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Mayhem.uasset"},
        {"DA_Skill_Lune_Rebirth", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Rebirth.uasset"},
        {"DA_Skill_Lune_SkyBreak", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_SkyBreak.uasset"},
        {"DA_Skill_Lune_StormCaller", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_StormCaller.uasset"},
        {"DA_Skill_Lune_ThermalTransfer", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_ThermalTransfer.uasset"},
        {"DA_Skill_Lune_Thunderfall", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Thunderfall.uasset"},
        {"DA_Skill_Lune_TreeOfLife", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_TreeOfLife.uasset"},
        {"DA_Skill_Lune_Tremor", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Tremor.uasset"},
        {"DA_Skill_Lune_Typhoon", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Typhoon.uasset"},
        {"DA_Skill_Lune_Wildfire", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Wildfire.uasset"},
        {"DA_Skill_Maelle_BreakingRules", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_BreakingRules.uasset"},
        {"DA_Skill_Maelle_Degagement", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Degagement.uasset"},
        {"DA_Skill_Maelle_NEW7_Egide", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Egide.uasset"},
        {"DA_Skill_Maelle_FencersFlurry", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_FencersFlurry.uasset"},
        {"DA_Skill_Maelle_NEW4_FireRise", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_FireRise.uasset"},
        {"DA_Skill_Maelle_FleuretFury", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_FleuretFury.uasset"},
        {"DA_Skill_Maelle_Gommage", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Gommage.uasset"},
        {"DA_Skill_Maelle_GuardDown", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_GuardDown.uasset"},
        {"DA_Skill_Maelle_GuardUp", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_GuardUp.uasset"},
        {"DA_Skill_Maelle_NEW19_InvigoratingFire", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_InvigoratingFire.uasset"},
        {"DA_Skill_Maelle_LastChance", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_LastChance.uasset"},
        {"DA_Skill_Maelle_NEW11_MezzoForte", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_MezzoForte.uasset"},
        {"DA_Skill_Maelle_MomentumStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_MomentumStrike.uasset"},
        {"DA_Skill_Maelle_OffensiveSwitch", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_OffensiveSwitch.uasset"},
        {"DA_Skill_Maelle_NEW13_Payback", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Payback.uasset"},
        {"DA_Skill_Maelle_NEW14_Percee", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Percee.uasset"},
        {"DA_Skill_Maelle_PhantomStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_PhantomStrike.uasset"},
        {"DA_Skill_Maelle_PhoenixFlame", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_PhoenixFlame.uasset"},
        {"DA_Skill_Maelle_Pyrolyse", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Pyrolyse.uasset"},
        {"DA_Skill_Maelle_Revenge", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Revenge.uasset"},
        {"DA_Skill_Maelle_NEW18_Spark", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Spark.uasset"},
        {"DA_Skill_Maelle_Stendhal", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_Stendhal.uasset"},
        {"DA_Skill_Maelle_SwordBallet", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_SwordBallet.uasset"},
        {"DA_Skill_Maelle_VirtuoseStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_VirtuoseStrike.uasset"},
        {"DA_Skill_Transfo_AbbestMelee", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Abbest.uasset"},
        {"DA_Skill_Transfo_BenisseurMortar", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Benisseur.uasset"},
        {"DA_Skill_Transfo_BraseleurHammerSmash", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Braseleur.uasset"},
        {"DA_Skill_Transfo_BrulerAnchorsmash", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Bruler.uasset"},
        {"DA_Skill_Transfo_AberrationBurningLight", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_BurningLight.uasset"},
        {"DA_Skill_Transfo_ChalierRelentlessSword", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Chalier.uasset"},
        {"DA_Skill_Transfo_ChapelierAxeSlash", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Chapelier.uasset"},
        {"DA_Skill_Transfo_ChevaliereBAoECombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_ChevaliereB.uasset"},
        {"DA_Skill_Transfo_ChevaliereCAC", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_ChevaliereA.uasset"},
        {"DA_Skill_Transfo_CrulerShield", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Cruler.uasset"},
        {"DA_Skill_Transfo_DanseuseWingDance", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Danseuse.uasset"},
        {"DA_Skill_Transfo_DemineurThunderStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Demineur.uasset"},
        {"DA_Skill_Transfo_ClairEnfeeble", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Enfeeble.uasset"},
        {"DA_Skill_Transfo_EvequeSpear", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Eveque.uasset"},
        {"DA_Skill_Transfo_CreationFromTheVoid", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_FromTheVoid.uasset"},
        {"DA_Skill_Transfo_GaultCombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Gault.uasset"},
        {"DA_Skill_Transfo_GlaiseEarthquakes", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Glaise.uasset"},
        {"DA_Skill_Transfo_RocherHammering", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Hammering.uasset"},
        {"DA_Skill_Transfo_HexgaCombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Hexga.uasset"},
        {"DA_Skill_Transfo_JarCombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Jar.uasset"},
        {"DA_Skill_Transfo_Lancelier", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Lancelier.uasset"},
        {"DA_Skill_Transfo_LusterCombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Luster.uasset"},
        {"DA_Skill_MightyStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_MightyStrike.uasset"},
        {"DA_Skill_Transfo_OrphelinBuff", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Orphelin.uasset"},
        {"DA_Skill_Transfo_PelerinFreshAir", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Pelerin.uasset"},
        {"DA_Skill_Transfo_PortierCrashingDown", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Portier.uasset"},
        {"DA_Skill_Transfo_PotatoBagMageThunderThrows", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_PotatoBagMage.uasset"},
        {"DA_Skill_Transfo_PotierBuff", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Potier.uasset"},
        {"DA_Skill_Transfo_RamasseurBonk", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Ramasseur.uasset"},
        {"DA_Skill_Sanctuary", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Sanctuary.uasset"},
        {"DA_Skill_Transfo_SaplingAbsorption", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Sapling.uasset"},
        {"DA_Skill_Transfo_LampmasterSwordOfLight", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_SwordOfLight.uasset"},
        {"DA_Skill_Transfo_TroubadourBuff", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Troubadour.uasset"},
        {"DA_Skill_Transfo_GrosseTeteWrecking", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Wrecking.uasset"},
        {"DA_Skill_Sciel_AllSet", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_AllSet.uasset"},
        {"DA_Skill_Sciel_BadOmen", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_BadOmen.uasset"},
        {"DA_Skill_Sciel_CardWeaver", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_CardWeaver.uasset"},
        {"DA_Skill_Sciel_DarkCleansing", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_DarkCleansing.uasset"},
        {"DA_Skill_Sciel_DarkWave", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_DarkWave.uasset"},
        {"DA_Skill_Sciel_Doom", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_Doom.uasset"},
        {"DA_Skill_Sciel_EndSlice", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_EndSlice.uasset"},
        {"DA_Skill_Sciel_FinalPath", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_FinalPath.uasset"},
        {"DA_Skill_Sciel_FiringShadow", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_FiringShadow.uasset"},
        {"DA_Skill_Sciel_FortunesFury", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_FortunesFury.uasset"},
        {"DA_Skill_Sciel_new3_GrimPrediction", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_GrimPrediction.uasset"},
        {"DA_Skill_Sciel_Harvest", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_Harvest.uasset"},
        {"DA_Skill_Sciel_Intervention", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_Intervention.uasset"},
        {"DA_Skill_Sciel_OurSacrifice", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_OurSacrifice.uasset"},
        {"DA_Skill_Sciel_PlentifulHarvest", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_PlentifulHarvest.uasset"},
        {"DA_Skill_Sciel_new18_Postponed", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_Postponed.uasset"},
        {"DA_Skill_Sciel_SealedFate", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_SealedFate.uasset"},
        {"DA_Skill_Sciel_SearingBond", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_SearingBond.uasset"},
        {"DA_Skill_Sciel_ShadowBringer", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_ShadowBringer.uasset"},
        {"DA_Skill_Sciel_new14_ShadowTargeting", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_ShadowTargeting.uasset"},
        {"DA_Skill_Sciel_SpectralSweep", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_SpectralSweep.uasset"},
        {"DA_Skill_Sciel_SpeedUp", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_SpeedUp.uasset"},
        {"DA_Skill_Sciel_TwilightDance", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_TwilightDance.uasset"},
        {"DA_Skill_Verso_AscendingAssault", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_AscendingAssault.uasset"},
        {"DA_Skill_Verso_BerserkSlash", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_BerserkSlash.uasset"},
        {"DA_Skill_Blitz", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_Blitz.uasset"},
        {"DA_Skill_Verso_Burden", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_Burden.uasset"},
        {"DA_Skill_Verso_DefiantStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_DefiantStrike.uasset"},
        {"DA_Skill_Verso_NEW_Leadership", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_Leadership.uasset"},
        {"DA_Skill_LightHolder", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_LightHolder.uasset"},
        {"DA_Skill_Verso_NEW_Overcharge", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_Overcharge.uasset"},
        {"DA_Skill_Verso_NEW_ParadigmShift", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_ParadigmShift.uasset"},
        {"DA_Skill_Verso_PerfectBreak", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_PerfectBreak.uasset"},
        {"DA_Skill_Verso_NEW_PhantomStars", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_PhantomStars.uasset"},
        {"DA_Skill_Verso_NEW_Purification", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_Purification.uasset"},
        {"DA_Skill_Verso_NEW_QuickStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_QuickStrike.uasset"},
        {"DA_Skill_Verso_RadiantSlash", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_RadiantSlash.uasset"},
        {"DA_Skill_Verso_NEW_Sabotage", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_Sabotage.uasset"},
        {"DA_Skill_Verso_SteeledStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_SteeledStrike.uasset"},
        {"DA_Skill_Verso_Striker", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_Striker.uasset"},
        {"DA_Skill_MediumEnergyTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_EnergyTintB"},
        {"DA_Skill_MediumHealingTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_HealingTintB"},
        {"DA_Skill_MediumReviveTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_ReviveTintB"},
        {"DA_Skill_SmallEnergyTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_EnergyTintA"},
        {"DA_Skill_SmallHealingTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_HealingTintA"},
        {"DA_Skill_SmallReviveTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_ReviveTintA"},
        {"DA_Skill_StrongEnergyTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_EnergyTintS"},
        {"DA_Skill_StrongHealingTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_HealingTintS"},
        {"DA_Skill_StrongReviveTint", "/Game/UI/Resources/Textures/GameMenu/Ico/Consumable/T_UI_ReviveTintS"},
        {"DA_Skill_Combo1", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_LumiereAssault.uasset"},
        {"DA_Skill_Verso_Strikestorm", "Game/Content/UI/Resources/Textures/Icons/Skills/Gustave/T_UI_Skill_Gustave_StrikeStorm.uasset"},
        {"DA_Skill_Gustave_UnleashCharge", "Game/Content/UI/Resources/Textures/Icons/Skills/Gustave/T_UI_Skill_Gustave_UnleashedCharge.uasset"},
        {"DA_Skill_Gustave_Combo1", "Game/Content/UI/Resources/Textures/Icons/Skills/Gustave/T_UI_Skill_Gustave_LumiereAssault.uasset"},
        {"DA_Skill_Lune_EarthQuake", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_EarthRising.uasset"},
        {"DA_Skill_Lune_EarthSpike", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_RockSlide.uasset"},
        {"DA_Skill_Lune_LightningDance", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_LightningStrike.uasset"},
        {"DA_Skill_Lune_Revitalization", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_Regeneration.uasset"},
        {"DA_Skill_Lune_Terraquake", "Game/Content/UI/Resources/Textures/Icons/Skills/Lune/T_UI_Skill_Lune_TerraQuake.uasset"},
        {"DA_Skill_Maelle_BurningCanvas", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_RunningCanvas.uasset"},
        {"DA_Skill_Maelle_GustaveSMemoire", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_GustaveMemoires.uasset"},
        {"DA_Skill_Maelle_NEW9_RainOfFire", "Game/Content/UI/Resources/Textures/Icons/Skills/Maelle/T_UI_Skill_Maelle_ReignOfFire.uasset"},
        {"DA_Skill_BreakPoint", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Breakpoint.uasset"},
        {"DA_Skill_Transfo_BalletCharm", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Servante.uasset"},
        {"DA_Skill_Transfo_BoucheclierShield", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Bouchclier.uasset"},
        {"DA_Skill_Transfo_ChevaliereCAoECombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_ChevaliereA.uasset"},
        {"DA_Skill_Transfo_ContorsionnisteAngryBlast", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Contortioniste.uasset"},
        {"DA_Skill_Transfo_DuallistStormBlood", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Duelliste.uasset"},
        {"DA_Skill_Transfo_EchassierCombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_DoubleStab.uasset"},
        {"DA_Skill_Transfo_FlyingCultistSlash", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_CultistMage.uasset"},
        {"DA_Skill_Transfo_HeavyCultistBloodSword", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_CultistHeavy.uasset"},
        {"DA_Skill_Transfo_MoissonneuseVendange", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_Moissoneuse.uasset"},
        {"DA_Skill_Transfo_ObscurCombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_DarkSlashes.uasset"},
        {"DA_Skill_Transfo_PotatobagBoss_FireShots", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_BossPotatobagA.uasset"},
        {"DA_Skill_Transfo_PotatobagRangerThunderEstoc", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_PotatoBagFighter.uasset"},
        {"DA_Skill_Transfo_PotatobagTankSlam", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_PotatoBagHeavy.uasset"},
        {"DA_Skill_Transfo_StalactMeleeCombo", "Game/Content/UI/Resources/Textures/Icons/Skills/Monoco/T_UI_Skill_Monoco_StalactA.uasset"},
        {"DA_Skill_NEW_4", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_LightHolder.uasset"},
        {"DA_Skill_Verso_Endbringer", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_EndBringer.uasset"},
        {"DA_Skill_Verso_NEW_AngelsEyes", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_AngelEyes.uasset"},
        {"DA_Skill_Verso_NEW_Followup", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_FollowUp.uasset"},
        {"DA_Skill_Verso_NEW_Speedburst", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_SpeedBurst.uasset"},
        {"DA_Skill_Verso_RadiantStrike", "Game/Content/UI/Resources/Textures/Icons/Skills/Verso/T_UI_Skill_Verso_RadiantSlash.uasset"},
        {"DA_Skill_Sciel_new2_Foretelling2", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_FocusedForetell.uasset"},
        {"DA_Skill_Sciel_new4_ShadowDrop", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_PhantomBlade.uasset"},
        {"DA_Skill_Sciel_new7_DarkAbsorption", "Game/Content/UI/Resources/Textures/Icons/Skills/Sciel/T_UI_Skill_Sciel_DarkAbsorbtion.uasset"},
    };
    private Dictionary<string, string> skillStrings = new() { };
    
    public List<SkillGraph> SkillGraphs = new();

    public override void Initialize()
    {
        ReadObjectsData($"{RandomizerLogic.DataDirectory}/skill_data.json");
        ReadAssets($"{RandomizerLogic.DataDirectory}/SkillsData");
        ViewModel.ContainerName = "Skill Tree";
        ViewModel.ObjectName = "Skill";
        _cleanSnapshot = ConvertToTxt();
        UpdateViewModel();
    }

    public override void InitFromTxt(string text)
    {
        text = text.ReplaceLineEndings("\n");
        var graphLines = text.Split('\n');
        foreach (var line in graphLines)
        {
            var characterName = line.Split('|')[0];
            var skillGraph = SkillGraphs.Find(sG => sG.CharacterName == characterName);
            skillGraph?.DecodeTxt(line);
        }
        UpdateViewModel();
    }

    public override string ConvertToTxt()
    {
        return string.Join('\n', SkillGraphs.Select(sG => sG.EncodeTxt()));
    }
    
    public override void Reset()
    {
        InitFromTxt(_cleanSnapshot);
    }
    
    public override void ApplyViewModel()
    {
        foreach (var categoryViewModel in ViewModel.Categories)
        {
            foreach (var skillNodeViewModel in categoryViewModel.Containers)
            {
                var codeName = skillNodeViewModel.CodeName;
                var characterName = codeName.Split("#")[0];
                var skillCodeName = codeName.Split("#")[1];
                var skillGraph = SkillGraphs.FirstOrDefault(sG => sG.CharacterName == characterName);
                var node = skillGraph.Nodes.FirstOrDefault(c => c.OriginalSkillCodeName == skillCodeName);
                var skillViewModel = skillNodeViewModel.Objects[0];
                node.SkillData = GetObject(skillViewModel.CodeName);
                node.UnlockCost = skillViewModel.IntProperty;
                node.IsStarting = skillViewModel.BoolProperty;
            }
        }
    }

    public override void UpdateViewModel()
    {
        ViewModel.FilteredCategories.Clear();
        ViewModel.Categories.Clear();
        if (ViewModel.AllObjects.Count == 0)
        {
            ViewModel.AllObjects = new ObservableCollection<ObjectViewModel>(ObjectsData.Select(i => new ObjectViewModel(i)));
            foreach (var objectViewModel in ViewModel.AllObjects)
            {
                objectViewModel.IntProperty = GetObject(objectViewModel.CodeName).CharacterName == "Monoco" ? -1 : 1;
            }
        }

        foreach (var characterGraph in SkillGraphs)
        {
            var newTypeViewModel = new CategoryViewModel();
            newTypeViewModel.CategoryName = characterGraph.CharacterName;
            newTypeViewModel.Containers = new ObservableCollection<ContainerViewModel>();

            foreach (var node in characterGraph.Nodes)
            {
                var originalCustomName = GetObject(node.OriginalSkillCodeName).CustomName;
                
                var newContainer = new ContainerViewModel($"{characterGraph.CharacterName}#{node.OriginalSkillCodeName}", originalCustomName);
                newContainer.Objects = new ObservableCollection<ObjectViewModel>();
                newContainer.Objects.Add(new ObjectViewModel(node.SkillData));
                newContainer.Objects[0].CanDelete = false;
                newContainer.Objects[0].Index = 0;
                
                newContainer.Objects[0].IntProperty = node.UnlockCost;
                newContainer.Objects[0].BoolProperty = node.IsStarting;
                newContainer.Objects[0].HasBoolPropertyControl = true;
                
                newContainer.CanAddObjects = false;
                
                newTypeViewModel.Containers.Add(newContainer);
                if (ViewModel.CurrentContainer != null && $"{characterGraph.CharacterName}#{node.OriginalSkillCodeName}" == ViewModel.CurrentContainer.CodeName)
                { 
                    ViewModel.CurrentContainer = newContainer;
                    ViewModel.UpdateDisplayedObjects();
                }
            }
            
            if (newTypeViewModel.Containers.Count > 0)
            {
                ViewModel.Categories.Add(newTypeViewModel);
            }
        }

        ViewModel.UpdateFilteredCategories();
    }

    public void ReadAssets(string filesDirectory)
    {
        SkillGraphs.Clear();
        var fileEntries = new List<string> (Directory.GetFiles(filesDirectory));
        foreach (var fileEntry in fileEntries.Where(f => f.Contains("DA_SkillGraph") && f.EndsWith(".uasset")))
        {
            var graphAsset = new UAsset(fileEntry, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
            var skillGraph = new SkillGraph(graphAsset);
            SkillGraphs.Add(skillGraph);
        }
    }

    public override void Randomize()
    {
        //TODO: Add option to lock/unlock Gustave locked skills
        //TODO: Modify the character save states uasset to account for the new skill trees
        SpecialRules.Reset();
        Reset();
        var cutContentAlreadyExcluded = RandomizerLogic.CustomSkillPlacement.Excluded.Contains("Cut Content Skills");
        if (!RandomizerLogic.Settings.IncludeCutContentSkills)
        {
            RandomizerLogic.CustomSkillPlacement.AddExcluded("Cut Content Skills");
        }
        RandomizerLogic.CustomSkillPlacement.Update();
        
        
        RandomizerLogic.CustomSkillPlacement.Update();
        foreach (var skillGraph in SkillGraphs)
        {
            SpecialRules.ResetSkillsPool();
            skillGraph.Randomize();
        }
        
        UpdateViewModel();
        if (!RandomizerLogic.Settings.IncludeCutContentSkills && !cutContentAlreadyExcluded)
        {
            RandomizerLogic.CustomSkillPlacement.RemoveExcluded("Cut Content Skills");
        }
    }

    public override void AddObjectToContainer(string objectCodeName, string containerCodeName)
    {
        throw new NotSupportedException("Skill nodes must have exactly one skill in them.");
    }

    public override void RemoveObjectFromContainer(int objectIndex, string containerCodeName)
    {
        throw new NotSupportedException("Skill nodes must have exactly one skill in them.");
    }

    public override void WriteAssets()
    {
        foreach (var skillGraph in SkillGraphs)
        {
            Utils.WriteAsset(skillGraph.ToAsset());
        }
    }
}