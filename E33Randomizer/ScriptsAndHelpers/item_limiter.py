# Get the checks.txt from the rando app
# For config.json:
# Use the code name of the item (look them up in the Data/item_data.json file).
# In the locations field, indicate which locations you want it to appear in, enclosed in quotation marks and separated by a comma, like in the example.
# In the checks field, indicate which individual checks you want it to have an option to appear in. I have provided the check code names in the pastebin. For example, if you want it to be lootable from the mime in Lumiere Act I, use the check code name from the provided list - BP_Dialogue_LUAct1_Mime. 
# If you don't need one of the fields, delete it entirely to not confuse the script.
# During generation, the script will flip a coin to determine whether the item will be in a location or in a check, and then randomly choose which one to use. If it chooses a location, it will put it in an item pick-up somewhere in that location.



from sys import argv
from random import choice, randint
import json

print('Usage: python item_limiter.py "path_to_checks.txt" "path_to_config.json"')

checks_filename = argv[1] if len(argv) > 1 else 'checks.txt'
config_filename = argv[2] if len(argv) > 2 else 'confing.json'

with open(checks_filename) as f:
    checks_data = f.readlines()

with open('config.json') as f:
    config = json.load(f)

for item_code_name, item_config in config.items():
    item_entry = f'{item_code_name}:1:100:False:False'
    checks_data = [c_d.replace(item_entry + ',', '') for c_d in checks_data]
    checks_data = [c_d.replace(',' + item_entry, '') for c_d in checks_data]
    checks_data = [c_d.replace(item_entry + '\n', '\n') for c_d in checks_data]

    if 'locations' in item_config and 'checks' in item_config:
        new_check_choice = 'location' if randint(0, 1) == 0 else 'check'
    elif 'locations' in item_config or 'checks' in item_config:
        new_check_choice = 'location' if 'locations' in item_config else 'check'
    else:
        print(f'Item {item_code_name} is configured incorrectly.')
        print('Item config should contain either a "locations" or a "checks" field')
        continue

    if new_check_choice == 'location':
        item_random_location = choice(item_config['locations'])
        location_checks = [c_d.split('|')[0].split('#')[1] for c_d in checks_data if f'DT_ChestsContent#Chest_{item_random_location}' in c_d]
        item_random_check = choice(location_checks)
    else:
        item_random_check = choice(item_config['checks'])

    for i, check_line in enumerate(checks_data):
        if item_random_check in check_line:
            checks_data[i] = check_line.replace('\n', f',{item_entry}\n')
            break

with open(f'new_{checks_filename}', 'w') as f:
    f.writelines(checks_data)

# Location code names:
#  SpringMeadows: Spring Meadows
#  SeaCliff: Stone Wave Cliffs
#  GoblusLair: Flying Waters
#  ForgottenBattlefield: Forgotten Battlefield
#  GrandisStation: Monoco's Station
#  AncientSanctuary: Ancient Sanctuary
#  GestralVillage: Gestral Village
#  EsquiesNest: Esquie's Nest
#  OldLumiere: Old Lumiere
#  WorldMap: World Map
#  Manor: The Manor
#  Monolith: The Monolith
#  ACT1: Act 1 Lumiere
#  Lumiere: Act 3 Lumiere
#  YellowForest: Yellow Harvest
#  FallingLeaves: Falling Leaves
#  CrimsonForest: Crimson Forest
#  FrozenHearts: Frozen Hearts
#  Reacher: The Reacher
#  RenoirSDraft: Renoir's Drafts
#  DarkShores: Dark Shores
#  TwilightSanctuary: Twilight Sanctuary
#  FlyingManor: Flying Manor
#  RedWoods: Red Woods
#  02: Small Burgeon
#  StonewaveCliffsCave: Stone Wave Cliffs Cave
#  ChosenPath: The Chosen Path
#  FloatingIsland: Sky Island
#  RockTrailing: RockTrailing
#  Cemetery: Flying Cemetery
#  CoastalCave: Coastal Cave
#  DoorMaze: Esoteric Ruins
#  SinisterCave: Sinister Cave
#  MiniLevels: Fixed-Camera Level
#  FlyingCasinoEntrance: Flying Casino
#  TheCarrousel: The Carousel
#  WhiteSands: White Sands
#  SacredRiver: Sacred River}

# Dialogue code names:
#  BP_Dialogue_CleaWorkshop_Path1: Painting Workshop: Colour of the Beast
#  BP_Dialogue_CleaWorkshop_Path2: Painting Workshop: Shape of the Beast
#  BP_Dialogue_CleaWorkshop_Path3: Painting Workshop: Light of the Beast
#  BP_Dialogue_DarkGestralArena_Pots: Dark Gestral Arena: Pots' Rewards
#  BP_Dialogue_Eloise: Lumiere Act I: Eloise's Reward
#  BP_Dialogue_EsquieCamp_Quest_4: The Camp: Verso Gradient Unlock 2
#  BP_Dialogue_EsquieCamp_Quest_7: The Camp: Verso Gradient Unlock 3
#  BP_Dialogue_GestralBeach_Climb_GrandisMain: Gestral Beach: Climb the Wall Reward
#  BP_Dialogue_GestralBeach_OnlyUp_Top: Gestral Beach: Gestral Ascension Reward
#  BP_Dialogue_GestralBeach_WipeoutGestral2_End: Gestral Beach: Parkour Course Reward
#  BP_Dialogue_GestralRace: Gestral Beach: Time Race Reward
#  BP_Dialogue_Grandis_Carrousel: The Carousel: Grandis's Gift
#  BP_Dialogue_GV_ArenaRegistrar: Gestral Village: Tournament Rewards
#  BP_Dialog_Merchant_GV_GestralBazar1: Gestral Village: Weird Pictos Turn-In Reward
#  BP_Dialogue_GV_Father: Gestral Village: Gestral Father's Reward
#  BP_Dialogue_GV_GestralBazar6: Gestral Village: Reward for Beating Eesda
#  BP_Dialogue_GV_GestralBazar9: Gestral Village: Excalibur
#  BP_Dialogue_GV_GestralGambler: Gestral Village: Gambler's Gift
#  BP_Dialogue_GV_Golgra: Gestral Village: Beating Golgra Reward
#  BP_Dialogue_GV_JournalCollector: Gestral Village: Journal Collection Reward
#  BP_Dialogue_GV_Karatot: Gestral Village: Karatom's Reward
#  BP_Dialogue_GV_OnoPuncho: Gestral Village: Ono Puncho's Reward
#  BP_Dialogue_Harbour_HotelLove: Lumiere Act I: Hotel Door Reward
#  BP_Dialogue_HexgaLuster: Stone Wave Cliffs: White Hexga's Reward
#  BP_Dialogue_HiddenArena_Keeper: Hidden Gestral Arena: Prizes
#  BP_Dialogue_JudgeOfMercy: The Fountain: Blanche's Reward
#  BP_Dialogue_LUAct1_Mime: Lumiere Act I: Mime Loot Drop
#  BP_Dialogue_Gardens_Maelle_FirstDuel: Lumiere Act I: Maelle Duel Reward
#  BP_Dialogue_LuneCamp_Quest_4: The Camp: Lune Gradient Unlock 2
#  BP_Dialogue_LuneCamp_Quest_6: The Camp: Lune's Music Record
#  BP_Dialogue_LuneCamp_Quest_7: The Camp: Lune Gradient Unlock 3
#  BP_Dialogue_MaelleCamp_Quest_4: The Camp: Maelle Gradient Unlock 2
#  BP_Dialogue_MaelleCamp_Quest_7: The Camp: Maelle Gradient Unlock 3
#  BP_Dialogue_MainPlaza_Furnitures: Lumiere Act I: Furniture Found Item
#  BP_Dialogue_MainPlaza_Trashcan: Lumiere Act I: Trash-can Man
#  BP_Dialogue_MainPlaza_Trashcan_useless: BP_Dialogue_MainPlaza_Trashcan_useless
#  BP_Dialogue_Manor_Wardrobe: The Manor: Wardrobe
#  BP_Dialogue_MimeChromaZoneEntrance: Sunless Cliffs: Mime's True Art Unreserved
#  BP_Dialogue_MonocoCamp_Quest_3: The Camp: Verso and Monoco's Haircuts
#  BP_Dialogue_MonocoCamp_Quest_4: The Camp: Monoco Gradient Unlock 2
#  BP_Dialogue_MonocoCamp_Quest_6: The Camp: Monoco's Music Record
#  BP_Dialogue_MonocoCamp_Quest_7: The Camp: Monoco Gradient Unlock 3
#  BP_Dialogue_MS_Grandis_Fashionist_V2: Monoco's Station: Grandis Fashionista's Reward
#  BP_Dialogue_MS_Grandis_Grateful: Monoco's Station: Grandis's Gift
#  BP_Dialogue_MS_Grandis_WM_GuideOldLumiere: World Map Near Monoco's Station: Grandis's Reward
#  BP_Dialogue_Nicolas: Lumiere Act I: Nicolas's Reward
#  BP_Dialogue_Quest_LostGestralChief: The Camp: Lost Gestrals Rewards
#  BP_Dialogue_ScielCamp_Quest_4: The Camp: Sciel Gradient Unlock 2
#  BP_Dialogue_ScielCamp_Quest_6: The Camp: Sciel's Music Record
#  BP_Dialogue_ScielCamp_Quest_7: The Camp: Sciel Gradient Unlock 3
#  BP_Dialogue_SleepingBenisseur: Red Woods: Sleeping Benisseur's Drop
#  BP_Dialogue_TheAbyss_SimonP2Rematch: The Abyss: Simon Rematch Reward
#  BP_Dialogue_TroubadourCantPlay: Stone Quarry: White Troubadour's Reward
#  BP_Dialogue_VolleyBall: Gestral Beach: Volleyball Rewards
#  BP_Dialog_DanseuseDanceClass: Frozen Hearts: White Danseuse's Reward
#  BP_Dialog_FacelessBoy_CleasFlyingHouse_Main: Flying Manor: Faceless Boy's Reward
#  BP_Dialog_FacelessBoy_OrangeForest: Falling Leaves: Faceless Boy's Reward
#  BP_Dialog_Goblu_DemineurMissingMine: Flying Waters: White Demineur's Reward
#  BP_Dialog_GV_Gestral_FlyingCasino_InsideGuy: Flying Casino: Most Cultured Swine's Gift
#  BP_Dialog_GV_Gestral_InvisibleCave: Sinister Cave: Dead Gestral's Loot
#  BP_Dialog_JarNeedLight: Spring Meadows: White Jar's Reward
#  BP_Dialog_SpiritClea_CleasTower: The Endless Tower: Clea's Gift to Maelle
#  BP_Dialog_SpiritPortier: Esoteric Ruins: White Portier's Reward
#  BP_Dialog_WeaponlessChalier1: Flying Cemetery: White Chalier's Reward
#  BP_Dialogue_Richard: Lumiere Act I: Richard's Gift to Jules
#  BP_Dialogue_Boulangerie: Lumiere Act I NG+: Boulangerie
#  BP_Dialogue_Jules: Lumiere Act I: Jules' Gift to Gustave
#  BP_Dialogue_Lumiere_ExpFestival_Apprentices: Lumiere Act I: Gift from Gustave's Apprentices
#  BP_Dialogue_Lumiere_ExpFestival_Token_Artifact_Colette: Lumiere Act I: Colette's Artifact
#  BP_Dialogue_Lumiere_ExpFestival_Token_Haircut_Amandine: Lumiere Act I: Amandine's Gorgeous Haircut
#  BP_Dialogue_Lumiere_ExpFestival_Token_Pictos_Claude: Lumiere Act I: Tom's Personal Masterpiece
#  BP_Dialogue_Lumiere_ExpFestival_Maelle: Lumiere Act I: Maelle Festival Duel Reward

# Cutscene code names:
#  DA_GA_SQT_BossMirrorRenoir: The Monolith: Gustave's Renoir Outfit
#  DA_GA_SQT_BossSimon: The Abyss: Simon's Rewards
#  DA_GA_SQT_GradientTutorial: Monoco's Station: First Gradient Unlocks
#  DA_GA_SQT_RedAndWhiteTree: Lumiere Act III: Maelle's Real Me Outfits
#  DA_GA_SQT_CampAfterSecondAxonEntrance: The Camp: Barrier Breaker
#  DA_GA_SQT_CampAfterTheFirstAxonP2: The Camp: Lettre a Maelle
#  DA_GA_SQT_TheGommage: Lumiere Act I: The Gommage Sequence Items
#  SA_GA_SQT_EpilogueWithMaelle: Heart of the Canvas: A Life to Paint Outfits
#  SA_GA_SQT_EpilogueWithVerso: Heart of the Canvas: A Life to Love Outfits
#  DA_GA_GRADIENT_Lune2: The Camp: Lune Relationship Level 4 Reward
#  DA_GA_GRADIENT_Lune3: The Camp: Lune Relationship Level 7 Reward
#  DA_GA_GRADIENT_Maelle2: The Camp: Maelle Relationship Level 4 Reward
#  DA_GA_GRADIENT_Maelle3: The Camp: Maelle Relationship Level 7 Reward
#  DA_GA_GRADIENT_Monoco2: The Camp: Monoco Relationship Level 4 Reward
#  DA_GA_GRADIENT_Monoco3: The Camp: Monoco Relationship Level 7 Reward
#  DA_GA_GRADIENT_Sciel2: The Camp: Sciel Relationship Level 4 Reward
#  DA_GA_GRADIENT_Sciel3: The Camp: Sciel Relationship Level 7 Reward
#  DA_GA_GRADIENT_Verso2: The Camp: Esquie Relationship Level 4 Reward
#  DA_GA_GRADIENT_Verso3: The Camp: Esquie Relationship Level 7 Reward

# Merchant code names:
#  DT_Merchant_CleaIsland: Fusoka (Flying Manor)
#  DT_Merchant_CoastalCave_Bruler: Bruler (Coastal Cave)
#  DT_Merchant_CoastalCave_Cruler: Cruler (Coastal Cave)
#  DT_Merchant_FH_Custo_Danseuse: Verogo (Frozen Hearts)
#  DT_Merchant_ForgottenBattlefield: Kasumi (Forgotten Battlefield)
#  DT_Merchant_GestralVillage1: Jujubree (Gestral Village)
#  DT_Merchant_GestralVillage2: Eesda (Gestral Village)
#  DT_Merchant_GestralVillage3: Gestral Merchant (Sacred River)
#  DT_Merchant_GoblusLair: Noco (Flying Waters)
#  DT_Merchant_GrandisStation: Grandis (Monoco Station)
#  DT_Merchant_GV_1_CustoSuits_Guys: Delsitra (Gestral Village)
#  DT_Merchant_GV_1_CustoSuits_Ladies: Alexcyclo (Gestral Village)
#  DT_Merchant_Lumiere: Cribappa (Lumiere Act III)
#  DT_Merchant_MonocosMountain: Melosh (The Monolith)
#  DT_Merchant_Monolith: Mistra (The Monolith)
#  DT_Merchant_OldLumiere: Mandelgo (Old Lumiere)
#  DT_Merchant_Optional3: Grour (Renoir's Drafts)
#  DT_Merchant_OrangeForest: Persik (Falling Leaves)
#  DT_Merchant_Reacher: Eragol (The Reacher)
#  DT_Merchant_SeaCliff: Jerijeri (Stone Wave Cliffs)
#  DT_Merchant_Sirene: Klaudiso (Sirene)
#  DT_Merchant_TwilightSanctuary: Anthonypo (Endless Night Sanctuary)
#  DT_Merchant_Visages: Blooraga (Visages)
#  DT_Merchant_YellowForest: Pinabby (Yellow Harvest)
#  DT_Merchant_WM_1: Appla (World Map)
#  DT_Merchant_WM_2: Colaro (World Map)
#  DT_Merchant_WM_3_GustaveSuit: Carrabi (World Map)
#  DT_Merchant_WM_4: Strabami (World Map)
#  DT_Merchant_WM_5: Pecha (World Map)
#  DT_Merchant_WM_6: Blakora (World Map)
#  DT_Merchant_WM_7: Papasso (World Map)
#  DT_Merchant_WM_8: Rederi (World Map)
#  DT_Merchant_WM_9: Sodasso (World Map)
#  DT_Merchant_WM_9_Sirene: Pearo (World Map)
#  DT_Merchant_WM_10: Carnovi (World Map)
#  DT_Merchant_WM_11: Blabary (World Map)
#  DT_Merchant_WM_12: Geranjo (World Map)
#  DT_Merchant_WM_13: Granasori (World Map)
#  DT_Merchant_WM_14: Lucaroparfe (World Map)
#  DT_Merchant_WM_15: Jumeliba (World Map)
#  DT_Merchant_WM_16: Rubiju (World Map)
#  DT_Merchant_WM_17: Citrelo (World Map)

# Example config.json:
"""
{
  "Chainebum": {
    "locations": [
      "SpringMeadows",
      "SeaCliff"
    ],
    "checks": [
      "BP_Dialogue_LUAct1_Mime"
    ]
  }
}
"""