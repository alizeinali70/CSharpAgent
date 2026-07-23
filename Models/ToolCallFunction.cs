using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpAgent.Models;

internal sealed class ToolCallFunction
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("arguments")]
    public JsonElement Arguments { get; init; }
}