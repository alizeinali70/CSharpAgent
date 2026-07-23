using System.Text.Json.Serialization;

namespace CSharpAgent.Models;

internal sealed class ToolCall
{
    [JsonPropertyName("function")]
    public required ToolCallFunction Function { get; init; }
}