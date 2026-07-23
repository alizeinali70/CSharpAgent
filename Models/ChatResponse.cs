using System.Text.Json.Serialization;

namespace CSharpAgent.Models;

internal sealed class ChatResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("message")]
    public ChatMessage? Message { get; init; }

    [JsonPropertyName("done")]
    public bool Done { get; init; }

    [JsonPropertyName("done_reason")]
    public string? DoneReason { get; init; }

    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; init; }

    [JsonPropertyName("load_duration")]
    public long? LoadDuration { get; init; }

    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvaluationCount { get; init; }

    [JsonPropertyName("eval_count")]
    public int? EvaluationCount { get; init; }
}