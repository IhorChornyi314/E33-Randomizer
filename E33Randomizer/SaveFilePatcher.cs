using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public static class SaveFilePatcher
{
    private const string JUMP_COUNTER_NID = "8e3263a7-493b-f6fd-f260-549af74ea0db";
    private const string GRADIENT_COUNTER_NID = "30e2946e-432c-b0e8-363e-d29811577e30";

    private const string NamedIDsStates_JSON =
        "{\n        \"tag\": {\n          \"data\": {\n            \"Map\": {\n              \"key_type\": {\n                \"Struct\": {\n                  \"struct_type\": \"Guid\",\n                  \"id\": \"00000000-0000-0000-0000-000000000000\"\n                }\n              },\n              \"value_type\": {\n                \"Other\": \"BoolProperty\"\n              }\n            }\n          }\n        },\n        \"Map\": [\n          {\n            \"key\": {\n              \"Struct\": {\n                \"Guid\": \"8e3263a7-493b-f6fd-f260-549af74ea0db\"\n              }\n            },\n            \"value\": {\n              \"Bool\": true\n            }\n          },\n          {\n            \"key\": {\n              \"Struct\": {\n                \"Guid\": \"30e2946e-432c-b0e8-363e-d29811577e30\"\n              }\n            },\n            \"value\": {\n              \"Bool\": true\n            }\n          }\n        ]\n      }";

    private const string JUMP_COUNTER_JSON =
        "{\n            \"key\": {\n              \"Struct\": {\n                \"Guid\": \"8e3263a7-493b-f6fd-f260-549af74ea0db\"\n              }\n            },\n            \"value\": {\n              \"Bool\": true\n            }\n          }";

    private const string GRADIENT_COUNTER_JSON =
        "{\n            \"key\": {\n              \"Struct\": {\n                \"Guid\": \"30e2946e-432c-b0e8-363e-d29811577e30\"\n              }\n            },\n            \"value\": {\n              \"Bool\": true\n            }\n          }";
    
    private static void HandleJson(string pathToJSON)
    {
        var json = File.ReadAllText(pathToJSON);
        dynamic saveObj = JsonConvert.DeserializeObject(json);

        if (saveObj.root.properties.NamedIDsStates_0 == null)
        {
            saveObj.root.properties.NamedIDsStates_0 = JsonConvert.DeserializeObject(NamedIDsStates_JSON);
        }
        else
        {
            bool jumpCounterPresent = false;
            bool gradientCounterPresent = false;
            
            foreach (var key in saveObj.root.properties.NamedIDsStates_0.Map)
            {
                if (key.key.Struct.Guid == JUMP_COUNTER_NID)
                {
                    jumpCounterPresent = true;
                }

                if (key.key.Struct.Guid == GRADIENT_COUNTER_NID)
                {
                    gradientCounterPresent = true;
                }
            }

            if (!jumpCounterPresent)
            {
                saveObj.root.properties.NamedIDsStates_0.Map.Add(JsonConvert.DeserializeObject(JUMP_COUNTER_JSON));
            }
            if (!gradientCounterPresent)
            {
                saveObj.root.properties.NamedIDsStates_0.Map.Add(JsonConvert.DeserializeObject(GRADIENT_COUNTER_JSON));
            }
        }

        string output = JsonConvert.SerializeObject(saveObj, Formatting.Indented);
        File.WriteAllText("save.json", output);
    }
    
    public static void Patch(string saveFilePath)
    {
        var to_json_args = $"to-json -i \"{saveFilePath}\" -o save.json";
        var from_json_args = $"from-json -i save.json -o \"{saveFilePath}\"";

        Process.Start("uesave.exe", to_json_args).WaitForExit();
        
        HandleJson("save.json");
        
        Process.Start("uesave.exe", from_json_args);
    }
}