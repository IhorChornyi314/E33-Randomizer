using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace E33Randomizer;

public static class SaveFilePatcher
{
    private const string JUMP_COUNTER_NID = "8e3263a7-493b-f6fd-f260-549af74ea0db";
    private const string GRADIENT_COUNTER_NID = "30e2946e-432c-b0e8-363e-d29811577e30";
    private const string LUMIERE_CURTAIN_NID = "aa6633b1-4fed-a61d-092e-ed80cd949751";

    private const string NID_TEMPLATE_JSON =
        "{\"key\": {\"Struct\": {\"Guid\": \"NID\"}},\"value\": {\"Bool\": VALUE}}";
    
    private const string NamedIDsStates_JSON =
        "{\"tag\": {\"data\": {\"Map\": {\"key_type\": {\"Struct\": {\"struct_type\": \"Guid\", \"id\": \"00000000-0000-0000-0000-000000000000\"}},\"value_type\": {\"Other\": \"BoolProperty\"}}}},\"Map\": []}";
    
    private static string GetFlagJson(string flagId, bool flagValue = true)
    {
        return NID_TEMPLATE_JSON.Replace("NID", flagId).Replace("VALUE", flagValue.ToString().ToLower());
    }
    
    private static void HandleJson(string pathToJSON, Dictionary<string, bool> flags)
    {
        var json = File.ReadAllText(pathToJSON);
        dynamic saveObj = JsonConvert.DeserializeObject(json);

        if (saveObj.root.properties.NamedIDsStates_0 == null)
        {
            saveObj.root.properties.NamedIDsStates_0 = JsonConvert.DeserializeObject(NamedIDsStates_JSON);
        }

        var flagsPresent = new List<string>();
        
        foreach (var kvPair in saveObj.root.properties.NamedIDsStates_0.Map)
        {
            var guid = kvPair.key.Struct.Guid.ToString();
            if (flags.ContainsKey(guid))
            {
                flagsPresent.Add(guid);
                kvPair.value.Bool = flags[guid];
            }
        }

        foreach (var flag in flags)
        {
            if (flagsPresent.Contains(flag.Key)) continue;
            saveObj.root.properties.NamedIDsStates_0.Map.Add(JsonConvert.DeserializeObject(GetFlagJson(flag.Key, flag.Value)));
        }
        string output = JsonConvert.SerializeObject(saveObj, Formatting.Indented);
        File.WriteAllText("save.json", output);
    }
    
    public static void Patch(string saveFilePath, Dictionary<string, bool> flags)
    {
        var to_json_args = $"to-json -i \"{saveFilePath}\" -o save.json";
        var from_json_args = $"from-json -i save.json -o \"{saveFilePath}\"";

        Process.Start("uesave.exe", to_json_args).WaitForExit();
        
        HandleJson("save.json", flags);
        
        Process.Start("uesave.exe", from_json_args);
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