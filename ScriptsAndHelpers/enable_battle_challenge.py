import sys
from sys import argv
import json
import subprocess
from glob import glob

CHALLENGE_JSON = '''
{
  "tag": {
    "data": {
      "Map": {
        "key_type": {
          "Other": "NameProperty"
        },
        "value_type": {
          "Other": "NameProperty"
        }
      }
    }
  },
  "Map": [
    {
      "key": {
        "Name": "BATTLECHALLENGE_LIMITDAMAGE"
      },
      "value": {
        "Name": "<DAMAGE_LIMIT>"
      }
    },
    {
      "key": {
        "Name": "BATTLECHALLENGE_ENEMYLIFEMULT"
      },
      "value": {
        "Name": "<LIFE_MULT>"
      }
    }
  ]
}'''

damage_limits = ['LIMIT_100K', 'LIMIT_1M']
life_multipliers = ['MULT_2', 'MULT_5', 'MULT_10', 'MULT_20', 'MULT_50', 'MULT_100']



NID_TEMPLATE_JSON = '{"key": {"Struct": {"Guid": "NID"}},"value": {"Bool": VALUE}}'

NamedIDsStates_JSON = '{"tag": {"data": {"Map": {"key_type": {"Struct": {"struct_type": "Guid", "id": "00000000-0000-0000-0000-000000000000"}},"value_type": {"Other": "BoolProperty"}}}},"Map": []}'


def get_challenge_json(flag_id: str, flag_value: bool = True) -> str:
    return NID_TEMPLATE_JSON.replace("NID", flag_id).replace("VALUE", str(flag_value).lower())


def handle_json(path_to_json: str, damage_limit: str, life_mult: str) -> None:
    with open(path_to_json, 'r', encoding='utf8') as f:
        json_content = f.read()

    save_obj = json.loads(json_content)
    challenge_json = json.loads(CHALLENGE_JSON)

    difficulty_struct = save_obj['root']['properties']['GameDifficultyData_0']['Struct']['Struct']

    if life_mult in life_multipliers:
        challenge_json['Map'][1]['value']['Name'] = life_mult
    else:
        del challenge_json['Map'][1]

    if damage_limit in damage_limits:
        challenge_json['Map'][0]['value']['Name'] = damage_limit
    else:
        del challenge_json['Map'][0]

    if challenge_json['Map']:
        difficulty_struct['ActivatedChallengeIDs_12_D7F8D90845065AFBB7E341AA177A43A9_0'] = challenge_json
    elif 'ActivatedChallengeIDs_12_D7F8D90845065AFBB7E341AA177A43A9_0' in difficulty_struct:
        del difficulty_struct['ActivatedChallengeIDs_12_D7F8D90845065AFBB7E341AA177A43A9_0']

    with open("save.json", 'w') as f:
        json.dump(save_obj, f, indent=2)


def patch(save_file_path: str, damage_limit: str, life_mult: str) -> None:
    to_json_args = f'to-json -i "{save_file_path}" -o save.json'
    from_json_args = f'from-json -i save.json -o "{save_file_path}"'

    subprocess.run(f"uesave.exe {to_json_args}", shell=True, check=True)

    handle_json("save.json", damage_limit, life_mult)

    subprocess.run(f"uesave.exe {from_json_args}", shell=True)


if __name__ == '__main__':
    if not glob('uesave.exe'):
        print('uesave.exe not found, please put it in the same directory as this script.')
        input('Press enter to exit...')
        exit()
    if len(sys.argv) != 4:
        print('Please indicate the save file path, damage limit, and life multiplier.')
        input('Press enter to exit...')
        exit()
    filename = argv[1]
    damage_limit_arg = argv[2]
    life_mult_arg = argv[3]
    patch(filename, damage_limit_arg, life_mult_arg)


# Damage limits options:
#  LIMIT_100K, LIMIT_1M

# Life multipliers:
#  MULT_2, MULT_5, MULT_10, MULT_20, MULT_50, MULT_100