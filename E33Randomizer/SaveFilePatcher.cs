using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace E33Randomizer;

public static class SaveFilePatcher
{
    private const string JUMP_COUNTER_NID = "8e3263a7-493b-f6fd-f260-549af74ea0db";
    private const string GRADIENT_COUNTER_NID = "30e2946e-432c-b0e8-363e-d29811577e30";
    private const string LUMIERE_CURTAIN_NID = "aa6633b1-4fed-a61d-092e-ed80cd949751";
    
    private const string NamedIDsStatesPropName = "NamedIDsStates_0";

    private const string NID_TEMPLATE_JSON =
        """
        { 
            "key": { 
                "Struct": {
                    "Guid": "NID"
                }
            },
            "value": {
                "Bool": VALUE
            }
        }
        """;
    
    private const string NamedIDsStates_JSON =
        """
        {
            "tag": {
                "data": {
                    "Map": {
                        "key_type": {
                            "Struct": {
                                "struct_type": "Guid",
                                "id": "00000000-0000-0000-0000-000000000000"
                            }
                        },
                        "value_type": {
                            "Other": "BoolProperty"
                        }
                    }
                }
            },
            "Map": []
        }
        """;
    
    private static string GetFlagJson(string flagId, bool flagValue = true)
    {
        return NID_TEMPLATE_JSON.Replace("NID", flagId).Replace("VALUE", flagValue.ToString().ToLower());
    }
    
    private static void HandleJson(string pathToJSON, Dictionary<string, bool> flags)
    {
        var json = File.ReadAllText(pathToJSON);
        
        // Ideally, we'd not use JsonNode, but in order to be AoT compatible, it's this, or having the entire Save File's model as a set of C# classes;
        // since we only care about editing a small portion of this file, this is less likely to cause issues.
        JsonNode saveObj = JsonNode.Parse(json) ?? throw new InvalidOperationException("Json could not be parsed");

        JsonNode? propertiesNode = saveObj["root"]?["properties"];
        if (propertiesNode is null)
        {
            throw new InvalidOperationException("Json does not contain expected data.");
        }
        
        if (propertiesNode[NamedIDsStatesPropName] == null)
        {
            propertiesNode[NamedIDsStatesPropName] = JsonNode.Parse(NamedIDsStates_JSON);
        }

        propertiesNode[NamedIDsStatesPropName]!["Map"] ??= new JsonArray(); 

        var flagsPresent = new List<string>();
        
        foreach (var node in propertiesNode[NamedIDsStatesPropName]!["Map"]!.AsArray())
        {
            if (node is null) continue;
            
            var key = node["key"]?["Struct"]?["Guid"];
            if (key is null) continue;
            
            var guid = key.GetValue<string>();
            if (flags.TryGetValue(guid, out var flag))  
            {
                flagsPresent.Add(guid);
                node["value"] ??= new JsonObject();

                if (node["value"]!["Bool"] == null)
                {
                    node["value"]!.AsObject().Add("Bool",false);
                }
                node["value"]!["Bool"] = flag;
            }
        }

        foreach (var flag in flags)
        {
            if (flagsPresent.Contains(flag.Key)) continue;
            propertiesNode["Map"].AsArray().Add(JsonNode.Parse(GetFlagJson(flag.Key, flag.Value)));
        }
        string output = saveObj.ToJsonString(new JsonSerializerOptions(){ WriteIndented = true });
        File.WriteAllText("save.json", output);
    }
    
    public static void Patch(string saveFilePath, Dictionary<string, bool> flags)
    {
        var to_json_args = $"to-json -i \"{saveFilePath}\" -o save.json";
        var from_json_args = $"from-json -i save.json -o \"{saveFilePath}\"";

        string ueSaveCommand = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "uesave.exe",
            PlatformID.Unix or PlatformID.MacOSX  => "uesave",
            _ => throw new NotSupportedException()
        };
        
        Process.Start(ueSaveCommand, to_json_args).WaitForExit();
        
        HandleJson("save.json", flags);
        
        Process.Start(ueSaveCommand, from_json_args);
    }

    public static void AddCounters(string saveFilePath)
    {
        var flags = new Dictionary<string, bool>()
        {
            {JUMP_COUNTER_NID, true},
            {GRADIENT_COUNTER_NID, true}
        };
        Patch(saveFilePath, flags);
    }

    public static void FixCurtain(string saveFilePath)
    {
        var flags = new Dictionary<string, bool>()
        {
            {LUMIERE_CURTAIN_NID, false}
        };
        Patch(saveFilePath, flags);
    }
}