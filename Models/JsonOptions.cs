using System.Text.Json.Serialization;

namespace CSharpAgent.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ChatResponse))]
internal partial class JsonOptions : JsonSerializerContext;