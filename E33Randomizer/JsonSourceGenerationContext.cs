using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using E33Randomizer.CustomPlacements;
using E33Randomizer.ObjectDatum;

namespace E33Randomizer;

[JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(UpgradeFloatToByteConverter), typeof(UpgradeFloatToIntConverter)])]
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
public partial class JsonSourceGenerationContext : JsonSerializerContext;

// Only needed until we decide that the backwards compatability converters aren't needed anymore. (probably in v6 whenever that is).  
[JsonSourceGenerationOptions]
[JsonSerializable(typeof(Dictionary<int, List<int>>))]
public partial class JsonSourceGenerationContextNoUpgrade : JsonSerializerContext;

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

/// <summary>
///  Handles upgrading from v3 of the Json format where everything was doubles/floats between 0.0 and 1.0, to v4 of the Json format where it's all values between 0 and 100.  
/// </summary>
public class UpgradeFloatToByteConverter : JsonConverter<byte>
{
    public override byte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string temp;

        using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
        
        temp = jsonDoc.RootElement.GetRawText();

        if (temp.Contains('.'))
        {
            var doubleValue = Convert.ToDouble(temp);
            return Convert.ToByte(doubleValue * 100);
        }

        return Convert.ToByte(temp);
    }

    public override void Write(Utf8JsonWriter writer, byte value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
///  Handles upgrading from v3 of the Json format where everything was doubles/floats between 0.0 and 1.0, to v4 of the Json format where it's all values between 0 and 100.  
/// </summary>
public class UpgradeFloatToIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
     
        string temp;

        using JsonDocument jsonDoc = JsonDocument.ParseValue(ref reader);
        
        temp = jsonDoc.RootElement.GetRawText();

        if (temp.Contains('.'))
        {
            var doubleValue = Convert.ToDouble(temp);
            return Convert.ToInt32(doubleValue * 100);
        }
        
        return Convert.ToInt32(temp);
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}