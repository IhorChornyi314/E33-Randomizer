using System.IO;
using System.Text;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace E33Randomizer;

public static class EncountersController
{
    public static List<Encounter> Encounters = new();

    public static Dictionary<string, string> LocationCodeNames = new Dictionary<string, string>()
    {
        {"SM", "Spring Meadows"},
        {"OF", "Falling Leaves"},
        {"AS", "Ancient Sanctuary"},
        {"GO", "Flying Waters"},
        {"SC", "Stone Wave Cliffs"},
        {"EN", "Esquie's Nest"},
        {"FB", "Forgotten Battlefield"},
        {"MM", "Frozen Hearts"},
        {"MF", "Visages"},
        {"GA", "Gestral Arena"},
        {"L", "Lumiere"},
        {"LU", "Lumiere"},
        {"SI", "Sirene"},
        {"MS", "Monoco's Station"},
        {"RE", "The Reacher"},
        {"RC", "The Reacher"},
        {"MO", "The Monolith"},
        {"ML", "The Monolith"},
        {"OL", "Old Lumiere"},
        {"GV", "Gestral Village"},
        {"WM", "World Map"},
        {"DS", "Dark Shores"},
        {"TS", "Endless Night Sanctuary"},
        {"CFH", "Flying Manor"},
        {"RD", "Renoir's Drafts"},
        {"TowerBattle", "Endless Tower"},
        {"CW", "Painting Workshop"},
        {"CC", "Coastal Cave"},
        {"FC", "Flying Cemetery"},
        {"CP", "The Chosen Path"},
        {"Merchant", "Merchants"},
        {"Petank", "Petanks"},
        {"QUEST", "Quests"},
        {"Quest", "Quests"},
        {"SL", "Crushing Cavern"},
        {"DarkGestralArena", "Dark Gestral Arena"},
        {"RF", "Crimson Forest"},
        {"RT", "Crimson Forest"},
        {"YF", "Preview Build"},
    };
    public static Dictionary<string, string> IrregularEncounters = new Dictionary<string, string>()
    {
        {"Boss_Simon*1", "The Abyss"},
        {"Boss_SimonPhase2*1", "The Abyss"},
        {"Portier_DoorSoul", "Esoteric Ruins"},
        {"FinalBossVerso", "Quests"},
        {"FinalBossMaelle", "Quests"},
        {"YF_Scavenger", "Falling Leaves"},
        {"GO_Curator_JumpTutorial*1", "The Manor"},
        {"GO_Curator_JumpTutorial_NoTuto*1", "The Manor"},
        {"YF_ChevaliereWhite*2", "Old Lumiere"},
        {"YF_ChevaliereWhite*1", "Old Lumiere"},
        {"YF_ChevaliereA*1B*1", "Old Lumiere"},
        {"YF_ChevaliereA*2B*1", "Old Lumiere"},
        {"YF_ChevaliereA*1B*2", "Old Lumiere"},
        {"MM_DanseuseAlpha*1", "Old Lumiere"},
        {"MM_DanseuseAlphaSummon", "Old Lumiere"},
        {"SL_GlissandoAlpha*1", "Sirene's Dress"},
        {"MM_Stalact_GradientAttackTutorial*1", "Monoco's Station"},
        {"MM_MirrorRenoir", "Cut Content"},
        {"MM_ClairObscurAlpha", "The Monolith"},
        {"MM_Pelerin_MonocoTuto*1", "Monoco's Station"},
        {"MM_StalactALPHA", "Cut Content"},
        {"SM_VolesterALPHA", "Cut Content"},
        {"MM_GargantALPHA", "Cut Content"},
        {"MM_PelerinALPHA", "Cut Content"},
        {"FB_DuallisteALPHA", "Cut Content"},
        {"SC_CultistMageALPHA", "Cut Content"},
        {"MF_ContorsionnisteALPHA", "Cut Content"},
        {"QUEST_DominiqueGiantFeet_ScielTuto*1", "Gestral Village"},
        {"CP_CultistHeavy*1", "Cut Content"},
        {"CP_Chapelier*1", "Cut Content"},
        {"SC_CultistDualSword*1", "Cut Content"},
        {"SC_CultistMage*1", "Cut Content"},
        {"SC_SapNevronBoss*1", "Cut Content"},
        {"SC_Gestral_Sonnyso*1", "Cut Content"},
        {"SC_CultistHeavy_Alpha", "Sirene"},
        {"SC_CultistMage*1_CultistDualSword*2", "Cut Content"},
        {"SC_CultistDualSword*2", "Cut Content"},
        {"SC_CultistDualSword*3", "Cut Content"},
        {"SC_CultistDualSword*2_FlyingCultist*1", "Cut Content"},
        {"SC_FearLight*2", "Cut Content"},
        {"SC_FearLight*3", "Cut Content"},
        {"SC_CultistDualSword*1_FlyingCultist*1_HeavyCultist*1", "Cut Content"},
        {"SC_CultistDualSword*2_HeavyCultist*1", "Cut Content"},
        {"SC_CultistDualSword*1_FlyingCultist*2_HeavyCultist*1", "Cut Content"},
        {"SC_CultistDualSwordALPHA", "Cut Content"},
        {"SC_CultistFlyingALPHA", "World Map"},
        {"SC_Hexga*1_DualSwordCultist*2", "Cut Content"},
        {"SC_CultistFlying*1DualSword*1", "Cut Content"},
        {"SC_CultistDualSword*1HeavyCultist*1", "Cut Content"},
        {"SC_CultistFlying*1Dual*2", "Cut Content"},
        {"SC_CultistFlying*2Dual*1", "Cut Content"},
        {"SC_Hexga*1_DualSwordCultist*1", "Cut Content"},
        {"SC_Hexga_Alpha*1", "Stone Wave Cliffs Cave"},
        {"FB_Prologue_Verso", "Cut Content"},
        {"MM_BraseleurALPHA*1", "The Reacher"},
        {"QUEST_MatthieuTheColossus*1", "Gestral Village"},
        {"QUEST_BertrandBigHands*1", "Gestral Village"},
        {"QUEST_DominiqueGiantFeet*1", "Gestral Village"},
        {"QUEST_JulienTinyHead*1", "Gestral Village"},
        {"Quest_WeaponlessChalier*1", "Flying Cemetery"},
        {"Quest_DemineurWithMine*1", "Flying Waters"},
        {"Quest_DemineurWithoutMine*1", "Flying Waters"},
        {"QUEST_Danseuse_DanceClass*1", "Frozen Hearts"},
        {"QUEST_JarNeedLight*1", "Spring Meadows"},
        {"QUEST_Gestral_OnoPuncho", "Gestral Village"},
        {"QUEST_PotatoBag_Boss_Upgraded", "Gestral Village"},
        {"QUEST_SleepingBenisseur*1", "Red Woods"},
        {"QUEST_HexgaLuster*1", "Stone Wave Cliffs"},
        {"QUEST_TroubadourCantPlay*1", "Stone Quarry"},
        {"QUEST_GrownBourgeon", "The Small Bourgeon"},
        {"QUEST_JudgeOfMercy*1", "The Fountain"},
        {"QUEST_Golgra_DarkArena*1", "Dark Gestral Arena"},
        {"QUEST_Golgra_SacredRiver*1", "Sacred River"},
        {"QUEST_Rocher_HexgaQuest*1", "Stone Wave Cliffs"},
        {"QUEST_Danseuse_DanceClass_Kill*1", "Frozen Hearts"},
        {"QUEST_Danseuse_DanceClass_Clone*1", "Frozen Hearts"},
        {"QUEST_TroubadourCantPlay_Kill*1", "Stone Quarry"},
        {"MF_BoucheclierALPHA", "Isle of the Eyes"},
        {"YF_GlaiseALPHA", "Sky Island"},
        {"WM_MimeBald*1", "Sunless Cliffs"},
        {"MF_ChapelieALPHA", "The Crows"},
        {"YF_Mime*1", "Yellow Harvest"},
        {"YF_Gault*1", "Yellow Harvest"},
        {"YF_GaultB*1", "Yellow Harvest"},
        {"YF_Potier_A*1", "Yellow Harvest"},
        {"YF_Jar*1", "Yellow Harvest"},
        {"YF_Gault*2", "Yellow Harvest"},
        {"YF_GlaiseBoss*1", "Yellow Harvest"}
    };
    
    public static Dictionary<String, List<Encounter>> GetEncountersByLocation()
    {
        var result = new Dictionary<String, List<Encounter>>();
        foreach (var encounter in Encounters)
        {
            var encounterLocationCodeName = encounter.Name.Split('_')[0];
            var encounterLocation = LocationCodeNames.ContainsKey(encounterLocationCodeName)
                ? LocationCodeNames[encounterLocationCodeName]
                : "Cut Content";
            if (IrregularEncounters.ContainsKey(encounter.Name))
            {
                encounterLocation = IrregularEncounters[encounter.Name];
            }
            
            if (!result.ContainsKey(encounterLocation))
            {
                result[encounterLocation] = new List<Encounter>();
            }
            result[encounterLocation].Add(encounter);
        }
        return result;
    }
    
    public static void ReadEncounterAsset(string assetPath)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        
        var dataTable = asset.Exports[0] as DataTableExport;
        var encountersTable = dataTable.Table.Data;

        var encounters = encountersTable.Select(encounterStruct => new Encounter(encounterStruct, asset)).ToList();
        encounters = encounters.FindAll(e => !Encounters.Contains(e));
        Encounters.AddRange(encounters);
    }
    
    public static void ReadEncounterAssets()
    {
        Encounters.Clear();
        ReadEncounterAsset("Data/Originals/DT_jRPG_Encounters.uasset");
        ReadEncounterAsset("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        ReadEncounterAsset("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        ReadEncounterAsset("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
    }

    public static void WriteEncounterAsset(string assetPath)
    {
        var asset = new UAsset(assetPath, EngineVersion.VER_UE5_4, RandomizerLogic.mappings);
        var assetFolder = asset.FolderName.Value.Replace("/Game", "randomizer/Sandfall/Content");
        PackEncounters(asset, Encounters);
        asset.Write($"{assetFolder}.uasset");
    }

    public static void WriteEncounterAssets()
    {
        WriteEncounterAsset("Data/Originals/DT_jRPG_Encounters.uasset");
        WriteEncounterAsset("Data/Originals/DT_jRPG_Encounters_CleaTower.uasset");
        WriteEncounterAsset("Data/Originals/Encounters_Datatables/DT_Encounters_Composite.uasset");
        WriteEncounterAsset("Data/Originals/Encounters_Datatables/DT_WorldMap_Encounters.uasset");
    }
    
    public static void ReadEncountersTxt(string fileName)
    {
        Encounters.Clear();
        foreach (var line in File.ReadLines(fileName, Encoding.UTF8))
        {
            var newEncounter = new Encounter(line.Split('|')[0], line.Split('|')[1].Split(',').ToList());
            Encounters.Add(newEncounter);
        }
    }

    public static void WriteEncountersTxt(string fileName)
    {
        var result = "";
        foreach (var encounter in Encounters)
        {
            result += encounter + "\n";
        }
        File.WriteAllText(fileName, result, Encoding.UTF8);
    }

    public static void GenerateNewEncounters()
    {
        Encounters.ForEach(e => ModifyEncounter(e));
    }

    public static Encounter ModifyEncounter(Encounter encounter)
    {
        if (!SpecialRules.Randomizable(encounter))
        {
            return encounter;
        }
        
        var newEncounterSize = !Settings.RandomizeEncounterSizes || Settings.PossibleEncounterSizes.Count == 0 ? encounter.Enemies.Count :
                Utils.Pick(Settings.PossibleEncounterSizes);

        if (Settings.EnableEnemyOnslaught)
        {
            newEncounterSize += Settings.EnemyOnslaughtAdditionalEnemies;
            newEncounterSize = int.Min(newEncounterSize, Settings.EnemyOnslaughtEnemyCap);
        }

        if (newEncounterSize < encounter.Size)
        {
            encounter.Enemies.RemoveRange(0, encounter.Size - newEncounterSize);
        }

        for (int i = 0; i < newEncounterSize; i++)
        {
            if (i < encounter.Size)
            {
                EnemyData newEnemy = CustomEnemyPlacement.Replace(encounter.Enemies[i]);
                encounter.Enemies[i] = newEnemy;
            }
            else
            {
                encounter.Enemies.Add(CustomEnemyPlacement.Replace(RandomizerLogic.GetRandomEnemy()));
            }
        }
        
        SpecialRules.ApplySpecialRules(encounter);
        return encounter;
    }

    public static void PackEncounters(UAsset asset, List<Encounter> encounters)
    {
        var dataTable = asset.Exports[0] as DataTableExport;
        var encountersTable = dataTable.Table.Data;

        foreach (var encounterStruct in encountersTable)
        {
            var originalEncounter = new Encounter(encounterStruct, asset);
            var newEncounter = encounters.Find(e => e.Name == originalEncounter.Name);
            if (newEncounter != null)
            {
                newEncounter.SaveToStruct(encounterStruct);
            }
        }
    }

    public static void AddEnemyToEncounter(string enemyCodeName, string encounterCodeName)
    {
        var enemyData = RandomizerLogic.GetEnemyData(enemyCodeName);
        Encounters.FindAll(e => e.Name == encounterCodeName).ForEach(e => e.Enemies.Add(enemyData));
    }
    
    public static void RemoveEnemyFromEncounter(string enemyCodeName, string encounterCodeName)
    {
        var enemyData = RandomizerLogic.GetEnemyData(enemyCodeName);
        Encounters.FindAll(e => e.Name == encounterCodeName).ForEach(e => e.Enemies.Remove(enemyData));
    }
}