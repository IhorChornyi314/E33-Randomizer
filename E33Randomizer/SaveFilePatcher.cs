using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace E33Randomizer;

public static class SaveFilePatcher
{
    private const string JumpCounterNid = "8e3263a7-493b-f6fd-f260-549af74ea0db";
    private const string GradientCounterNid = "30e2946e-432c-b0e8-363e-d29811577e30";
    private const string LumiereCurtainNid = "aa6633b1-4fed-a61d-092e-ed80cd949751";
    
    private const string NamedIDsStatesPropName = "NamedIDsStates_0";
    private const string NamedIDsStatesSchemaName = "NamedIDsStates";

    private const string NidTemplateJson =
        """
        {
          "key" : "NID",
          "value" : VALUE
        }
        """;
    
    private const string NamedIDsStatesJson =
        """
        {
            "data" : {
                "Map" : {
                    "key_type" : {
                        "Struct" : {
                            "struct_type" : "Guid",
                            "id" : "00000000-0000-0000-0000-000000000000"
                        }
                    },
                    "value_type" : {
                        "Other" : "BoolProperty"
                    }
                }
            }
        }
        """;
    
    private static string GetFlagJson(string flagId, bool flagValue = true)
    {
        return NidTemplateJson.Replace("NID", flagId).Replace("VALUE", flagValue.ToString().ToLower());
    }
    
    private static void HandleJson(string pathToJson, Dictionary<string, bool> flags)
    {
        var json = File.ReadAllText(pathToJson);
        
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
            propertiesNode[NamedIDsStatesPropName] = new JsonArray();
            saveObj["schemas"]!["schemas"]![NamedIDsStatesSchemaName] = JsonNode.Parse(NamedIDsStatesJson);
        }

        var properties = propertiesNode[NamedIDsStatesPropName]!.AsArray();

        var flagsPresent = new List<string>();
        
        foreach (var node in properties)
        {
            if (node is null) continue;

            var key = node["key"];
            if (key is null) continue;
            
            var guid = key.GetValue<string>();
            if (flags.TryGetValue(guid, out var flag))  
            {
                flagsPresent.Add(guid);
                node["value"] = flag;
            }
        }

        foreach (var flag in flags)
        {
            if (flagsPresent.Contains(flag.Key)) continue;
            properties.Add(JsonNode.Parse(GetFlagJson(flag.Key, flag.Value)));
        }
        string output = saveObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("save.json", output);
    }
    
    public static void Patch(string saveFilePath, Dictionary<string, bool> flags)
    {
        var toJsonArgs = $"to-json -i \"{saveFilePath}\" -o save.json";
        var fromJsonArgs = $"from-json -i save.json -o \"{saveFilePath}\"";

        string ueSaveCommand = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "uesave.exe",
            PlatformID.Unix or PlatformID.MacOSX  => "uesave",
            _ => throw new NotSupportedException()
        };
        
        Process.Start(ueSaveCommand, toJsonArgs).WaitForExit();
        
        HandleJson("save.json", flags);
        
        Process.Start(ueSaveCommand, fromJsonArgs);
    }

    public static void AddCounters(string saveFilePath)
    {
        var flags = new Dictionary<string, bool>()
        {
            {JumpCounterNid, true},
            {GradientCounterNid, true}
        };
        Patch(saveFilePath, flags);
    }

    public static void FixCurtain(string saveFilePath)
    {
        var flags = new Dictionary<string, bool>()
        {
            {LumiereCurtainNid, false}
        };
        Patch(saveFilePath, flags);
    }
}