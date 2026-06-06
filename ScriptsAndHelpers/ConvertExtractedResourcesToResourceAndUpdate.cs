#pragma warning disable CS8618
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var fileToOpen = args[0];
var prefix = args[1];


var toUpdate = JsonSerializer.Deserialize(File.ReadAllText("extracted_strings.json"), JsonSourceGenerationContext.Default.DictionaryStringEntry);

var resourcesFileContents = File.ReadAllLines("../E33Randomizer/Assets/Resources.resx").ToList();
var fileContentsToUpdate = File.ReadAllText(fileToOpen);

resourcesFileContents.RemoveAt(resourcesFileContents.Count-1);

foreach (var kvp in toUpdate)
{
    var key = kvp.Key;
    var entry = kvp.Value;

    if (string.IsNullOrEmpty(entry.Redirected))
    {
        var resource = 
$"""
    <data name="{prefix}_{key}" xml:space="preserve">
        <value>{entry.Value}</value>
    </data>
""";
        resourcesFileContents.Add(resource);
    }
    else
    {
        key = entry.Redirected;
    }

  
    Console.WriteLine($"looking for {entry.Original}");

    var replacements = entry.Replacements.Select( x => x ?? "null");
    
    if (replacements.Any())
    {
        fileContentsToUpdate = fileContentsToUpdate.Replace(entry.Original.StartsWith('$') || !entry.Original.Contains('{') ? entry.Original : "$" + entry.Original, $"ResourceHelper.GetStringFormatted(nameof(Assets.Resources.{prefix}_{key}),{string.Join(',',replacements)})", StringComparison.OrdinalIgnoreCase);
    }
    else
    {
        fileContentsToUpdate = fileContentsToUpdate.Replace(entry.Original, $"ResourceHelper.GetString(nameof(Assets.Resources.{prefix}_{key}))", StringComparison.OrdinalIgnoreCase);
    }
}

resourcesFileContents.Add("</root>");

File.WriteAllText("temp.cs", fileContentsToUpdate);
File.WriteAllLines("temp.resx", resourcesFileContents);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, Entry>))]
partial class JsonSourceGenerationContext : JsonSerializerContext;

class Entry
{
    public string Original {get;set;}
    public string Value { get;set;}
    public List<string> Replacements {get;set;}
    public string Redirected {get;set;}
}