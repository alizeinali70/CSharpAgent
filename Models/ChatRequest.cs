using System.Text.Json.Serialization;

namespace CSharpAgent.Models;

internal sealed class ChatRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("messages")]
    public required List<ChatMessage> Messages { get; init; }

    [JsonPropertyName("tools")]
    public required List<ToolDefinition> Tools { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }
}