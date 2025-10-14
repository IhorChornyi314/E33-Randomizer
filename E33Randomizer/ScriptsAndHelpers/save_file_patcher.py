from sys import argv
import json
import subprocess
from typing import Dict
from glob import glob


NID_TEMPLATE_JSON = '{"key": {"Struct": {"Guid": "NID"}},"value": {"Bool": VALUE}}'

NamedIDsStates_JSON = '{"tag": {"data": {"Map": {"key_type": {"Struct": {"struct_type": "Guid", "id": "00000000-0000-0000-0000-000000000000"}},"value_type": {"Other": "BoolProperty"}}}},"Map": []}'


def get_flag_json(flag_id: str, flag_value: bool = True) -> str:
    return NID_TEMPLATE_JSON.replace("NID", flag_id).replace("VALUE", str(flag_value).lower())


def handle_json(path_to_json: str, flags: Dict[str, bool]) -> None:
    with open(path_to_json, 'r') as f:
        json_content = f.read()

    save_obj = json.loads(json_content)

    if "NamedIDsStates_0" not in save_obj["root"]["properties"]:
        save_obj["root"]["properties"]["NamedIDsStates_0"] = json.loads(NamedIDsStates_JSON)

    flags_present = []

    for kv_pair in save_obj["root"]["properties"]["NamedIDsStates_0"]["Map"]:
        guid = kv_pair["key"]["Struct"]["Guid"]
        if guid in flags:
            flags_present.append(guid)
            kv_pair["value"]["Bool"] = flags[guid]

    for flag_key, flag_value in flags.items():
        if flag_key in flags_present:
            continue
        save_obj["root"]["properties"]["NamedIDsStates_0"]["Map"].append(
            json.loads(get_flag_json(flag_key, flag_value))
        )

    with open("save.json", 'w') as f:
        json.dump(save_obj, f, indent=2)


def patch(save_file_path: str, flags: Dict[str, bool]) -> None:
    to_json_args = f'to-json -i "{save_file_path}" -o save.json'
    from_json_args = f'from-json -i save.json -o "{save_file_path}"'

    subprocess.run(f"uesave.exe {to_json_args}", shell=True, check=True)

    handle_json("save.json", flags)

    subprocess.run(f"uesave.exe {from_json_args}", shell=True)

if __name__ == '__main__':
    if not glob('uesave.exe'):
        print('uesave.exe not found, please put it in the same directory as this script.')
        input('Press enter to exit...')
        exit()
    filename = argv[1]
    flag_guid = argv[2]
    flag_value = argv[3] == 'true' if len(argv) > 3 else True
    patch(filename, {flag_guid: flag_value})



# GUIDs:
#  GestralBullyGroup	b4f50992-4f19-0cee-8049-03a051b9a827
#  CampAfterFirstAxonPlayed	ca8e462b-45c2-393e-311b-30bb4bfee7bf
#  ChromaDoorComplete	aa0b8a81-4ca6-3a5c-dcdf-999ff8195bd6
#  ChromaZoneComplete	28021534-4b41-b644-fd14-859f34f4ed51
#  MonocoUnlock	e98e414e-48eb-0fbf-5771-d6aff6b64955
#  MetCleaInFlyingHouse	70e5f818-4bad-d566-25a3-59b3ded04d35
#  CIN_EnteringEsquieNestPlayed	c8c70cd3-4fe1-a9cc-5c8a-a99bd37034f2
#  EsquieNest_FrancoisDefeated	ae65dd07-458c-4ad1-390f-db89acfd953e
#  FrancoisIntroPlayed	d0a6dfb1-454b-657f-6e9c-efbfdbfe9929
#  DuallistIntroPlayed	2c768577-486f-9298-58f1-31b5e6f96f87
#  GradientCounterTutorialStart	30e2946e-432c-b0e8-363e-d29811577e30
#  GargantDefeated	3d55c71a-4111-a9b6-056b-7db25599408b
#  GestralVillage_BackToTheChiefToHelpHim	d7ead0f1-49d5-8a87-6cfc-7c896862da0e
#  GestralVillage_ChadGestralLootStolen	acd5ca85-4c32-0927-8832-e192a1776070
#  GestralVillage_TalkToTheChief	d9761a73-47b1-d711-238d-bd93781aae5c
#  GestralVillage_TalkToTheShaman	d246dd66-473f-b16d-7d9c-e79bb59c8984
#  ScielUnlock	d863d1f9-4730-f13b-a8d3-fb8fe465e774
#  GobluDefeated	efdc9a38-4f93-1063-3386-c7933f81a2e2
#  IntroCINPlayed	073067fa-4fa9-1572-2f29-32a51b1fb722
#  JumpTutorialStart	8e3263a7-493b-f6fd-f260-549af74ea0db
#  MaelleUnlock	3a601f56-44a2-228f-d7d7-7594e7374a08
#  GommageCineSeen	2dc8b1d6-4456-3a7b-ce34-74b2e791d807
#  Lumiere_CuratorP2	aa6633b1-4fed-a61d-092e-ed80cd949751
#  Lumiere_MyFlowerCINPlayed	19863a9c-4e4a-936c-1857-f398a42d9f80
#  MonocoStation_GrandisFashion_EloquenceBattle	d8ad6611-4162-c20f-58a7-a2af4fa5cd79
#  MonocoStation_StalactAttack	71bae11f-4574-e90d-e907-fbab4f471ab7
#  MonocoStation_StalactDefeated	bedc3616-4c33-59e2-a24c-01b54e081257
#  PaintressBattle	a082f7f0-4a94-eab3-59a6-93a07c60205b
#  MultiFacesDefeated	01c09c28-46d9-772a-f0b7-bc8dabdc9316
#  MonocoSmash	b1fd5ca7-4379-6a7c-0fad-8e89733ee79a
#  OldLumiere_MirrorRenoirDefeated	4f8ed036-465a-b5f2-edfd-69a6b07e90de
#  VersoDisapeared	79ffe61f-4291-1df0-7200-c3a067dfbf1b
#  MetSimon	fd4eaf4d-4aa4-b3ae-d1ed-1ba16108c00a
#  Reacher_AliciaDefeated	426cc205-4586-ee41-dd4f-65877ecd78a4
#  Reacher_ReacherBossVisible	88b11d0f-4862-554e-dda3-5eb09310cdd8
#  SireneDefeat	e8123cd5-455f-98bb-b3e8-a682a318ca99
#  SireneDefeated	190f0499-4f3b-c25d-b0d8-5d8052bbc94e
#  TisseurDefeated	e56beae9-4856-7fe3-1605-4abb9eb3ae57
#  FirstLancelierBeaten	06e8d109-4c06-4007-a98f-a190e7eacfee
#  LuneUnlock	606d3e62-4bc4-27ff-9bff-ad97284286a0
#  CharacterOverviewTutorial	21ca87f4-4e35-8a24-ddea-ba84881a95db
#  CuratorExpeditionTutorial	f9ef27d7-4862-e895-0231-5d98a6b77155
#  CuratorTutorial	7d5f9151-4a90-38ed-37ec-d191bc5cceb1
#  LuminaTutorial	f87b8c10-46c6-12a9-b90e-29a9500396dd
#  PictosTutorial	8d843b41-4bd4-92a5-72e6-0b9109ead34e
#  AngerMaskDefeated	657b4f90-4b86-4cfd-f748-24a74bb2e0fc
#  JoyMaskDefeated	060e6aed-4743-c27d-41a2-22ab22d097c0
#  SadnessMaskDefeated	1262ed04-4c4b-3314-a7a2-81a299720310
#  VisagesDefeated	ad00a708-4e82-96c4-61f6-0a98d4cb487b
#  BarrierDown	6a9553f7-4df3-dc95-2141-9590cca043aa
#  BarrierStands	a242bf37-482d-7f94-12cc-44a65ca9a584
#  EnterWorldMap	430e49b6-4dee-3a1b-7c2d-329b17f480fb
#  EsquieHardWaterUnlock	a89eb2b3-4659-098b-5cd0-80b1285d0d25
#  EsquieUnderwaterUnlocked	c6049eb0-4851-b254-e484-769ab9f84714
#  CampAftermathDialogs	1c5f62cb-44df-660f-9a3f-22a819516533
#  Camp_DS_AftermathLadies_Trigger	117dbeb4-4b2e-5608-54b7-d8bb10acdb08
#  Camp_DS_CampMaelleLune	541e32d9-4886-c428-fb2c-13992629b2d9
#  Camp_DS_CampMaelleSciel	62be1385-4db5-da8e-5250-aa98d5946a4b
#  Camp_DS_CampScielEsquie	8bca1363-4e12-c628-3c39-afaad03a3b5c
#  Camp_DS_Sisters_Trigger	cd2568aa-4bbd-270b-102e-d3a05fac0324
#  Camp_PostLumiereAttack	df673638-42b3-72b3-8fc8-bb8a2a98f8f2
#  DatingSciel	c9740bc8-429d-79a7-de87-bb82d824aa6e
#  EsquieArmbandUnlocked	894303d3-4324-8548-df59-51b64725db32
#  LuneRelationshipLvl6_Quest	2a55f5a4-4ef0-b15c-7b4d-53b6f3e5cc39
#  MaelleRelationshipLvl6_Quest	e3367623-40bc-b33c-788d-9cb7ffe1b656
#  Monoco_RelationshipLvl6_Quest	a73a9b68-4ace-acd1-721b-cb901f428538
#  AliciaEncountered    b155c643-4303-f75c-389c-1f86a7f09b7c