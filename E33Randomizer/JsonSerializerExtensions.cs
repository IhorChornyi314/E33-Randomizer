using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace E33Randomizer;

public static class JsonSerializerExtensions
{
    extension(JsonSerializer) {
        
        /// <summary>
        /// Deserializes the given <param name="json" /> and guarantees that it cannot return a null value.  If null, throws a <see cref="JsonException"/>
        /// </summary>
        /// <param name="json">The string to deserialize.</param>
        /// <param name="jsonTypeInfo">The JsonTypeInfo that is usually found in <see cref="JsonSourceGenerationContext"/></param>
        /// <typeparam name="TValue">The object type to deserialize the object into.</typeparam>
        /// <exception cref="JsonException">Thrown when the deserialized value is null.  This can generally only happen if the input string is the literal value "null"</exception>
        [return: NotNull]
        public static TValue DeserializeThrowOnNull<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json,
            JsonTypeInfo<TValue> jsonTypeInfo)
        {
            var value = JsonSerializer.Deserialize(json, jsonTypeInfo);
            return value ?? throw new JsonException("Unable to parse json.");
        }
    }
}