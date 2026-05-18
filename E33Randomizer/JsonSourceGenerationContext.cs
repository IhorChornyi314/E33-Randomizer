using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace E33Randomizer;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SettingsViewModel))]
[JsonSerializable(typeof(CharacterData))]
[JsonSerializable(typeof(CheckData))]
[JsonSerializable(typeof(EnemyData))]
[JsonSerializable(typeof(ItemData))]
[JsonSerializable(typeof(LocationData))]
[JsonSerializable(typeof(SkillData))]
[JsonSerializable(typeof(SpawnPointData))]
[JsonSerializable(typeof(ObjectData))]
[JsonSerializable(typeof(CustomPlacementPreset))]
[JsonSerializable(typeof(List<CharacterData>))]
[JsonSerializable(typeof(List<CheckData>))]
[JsonSerializable(typeof(List<EnemyData>))]
[JsonSerializable(typeof(List<ItemData>))]
[JsonSerializable(typeof(List<LocationData>))]
[JsonSerializable(typeof(List<SkillData>))]
[JsonSerializable(typeof(List<SpawnPointData>))]
[JsonSerializable(typeof(List<ObjectData>))]
[JsonSerializable(typeof(CustomPlacementWindowViewModel))]
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, List<int>>>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<int, List<int>>))]
public partial class JsonSourceGenerationContext : JsonSerializerContext;

/// <summary>
/// Factory for creating a <see cref="JsonSourceGenerationContext"/> that includes the relaxed <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/> which doesn't encode things into UTF-16 characters.
/// Generally this is only needed for serialization. 
/// </summary>
public static class JsonSourceGenerationContextSerializationFactory
{
    public static readonly Lazy<JsonSourceGenerationContext> LazyJsonSourceGenerationContext = new(() =>
    {
        var options = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        return new JsonSourceGenerationContext(options);
    });
}