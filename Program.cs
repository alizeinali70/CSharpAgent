using CSharpAgent.Models;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

const string model = "qwen3:8b";
const string ollamaUrl = "http://localhost:11434/api/chat";
const int maxAgentSteps = 8;
const int maximumReadmeLength = 100_000;

string projectDirectory = Directory.GetCurrentDirectory();

var serializerOptions =
    new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull
    };

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromMinutes(5)
};

var messages = new List<ChatMessage>
{
    new()
    {
        Role = "system",
        Content =
            """
            You are a local AI assistant running inside a C# application.

            You have access to tools for:
            - arithmetic calculations
            - retrieving the current time
            - creating or updating README.md

            Rules:
            1. Use tools when they are needed.
            2. Never invent tool results.
            3. You may call multiple tools before answering.
            4. Do not claim a file was created unless the tool confirms it.
            5. Do not overwrite README.md unless the user explicitly asks.
            6. When generating README.md, produce complete valid Markdown.
            7. Do not invent project features that the user has not described.
            8. When you have enough information, answer clearly.
            """
    }
};

Console.WriteLine($"Local agent started with model '{model}'.");
Console.WriteLine($"Project directory: {projectDirectory}");
Console.WriteLine();
Console.WriteLine("Commands:");
Console.WriteLine("  clear  Clear the conversation");
Console.WriteLine("  exit   Stop the application");

while (true)
{
    Console.Write("\nYou: ");

    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (input.Equals(
            "exit",
            StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (input.Equals(
            "clear",
            StringComparison.OrdinalIgnoreCase))
    {
        ChatMessage systemMessage = messages[0];

        messages.Clear();
        messages.Add(systemMessage);

        Console.WriteLine("Conversation cleared.");
        continue;
    }

    messages.Add(new ChatMessage
    {
        Role = "user",
        Content = input
    });

    try
    {
        string answer = await RunAgentAsync(messages);

        Console.WriteLine($"\nAgent: {answer}");
    }
    catch (HttpRequestException exception)
    {
        Console.WriteLine(
            "\nCould not connect to Ollama. " +
            "Check that Ollama is running and the model is installed.");

        Console.WriteLine(exception.Message);
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine(
            "\nThe Ollama request timed out.");
    }
    catch (Exception exception)
    {
        Console.WriteLine(
            $"\nAgent error: {exception.Message}");
    }
}

async Task<string> RunAgentAsync(
    List<ChatMessage> conversation)
{
    for (int step = 1; step <= maxAgentSteps; step++)
    {
        var request = new ChatRequest
        {
            Model = model,
            Stream = false,
            Messages = conversation,
            Tools = ToolDefinitions.All
        };

        using HttpResponseMessage response =
            await httpClient.PostAsJsonAsync(
                ollamaUrl,
                request,
                serializerOptions);

        string responseJson =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Ollama returned HTTP " +
                $"{(int)response.StatusCode}: {responseJson}");
        }

        ChatResponse? chatResponse =
            JsonSerializer.Deserialize<ChatResponse>(
                responseJson,
                serializerOptions);

        ChatMessage assistantMessage =
            chatResponse?.Message
            ?? throw new InvalidOperationException(
                "Ollama returned no assistant message.");

        conversation.Add(assistantMessage);

        if (assistantMessage.ToolCalls is not { Count: > 0 })
        {
            return string.IsNullOrWhiteSpace(
                assistantMessage.Content)
                ? "The model returned an empty response."
                : assistantMessage.Content;
        }

        foreach (ToolCall toolCall in assistantMessage.ToolCalls)
        {
            string toolName = toolCall.Function.Name;

            Console.WriteLine(
                $"[Agent step {step}: calling {toolName}]");

            string toolResult;

            try
            {
                toolResult = ExecuteTool(toolCall);
            }
            catch (Exception exception)
            {
                toolResult = SerializeResult(
                    new
                    {
                        success = false,
                        error = exception.Message
                    });
            }

            Console.WriteLine(
                $"[Tool result: {toolResult}]");

            conversation.Add(new ChatMessage
            {
                Role = "tool",
                ToolName = toolName,
                Content = toolResult
            });
        }
    }

    throw new InvalidOperationException(
        $"The agent exceeded the limit of " +
        $"{maxAgentSteps} steps.");
}

string ExecuteTool(ToolCall toolCall)
{
    ArgumentNullException.ThrowIfNull(toolCall);
    ArgumentNullException.ThrowIfNull(toolCall.Function);

    return toolCall.Function.Name switch
    {
        "calculate" =>
            Calculate(toolCall.Function.Arguments),

        "get_current_time" =>
            GetCurrentTime(toolCall.Function.Arguments),

        "write_readme" =>
            WriteReadme(toolCall.Function.Arguments),

        _ =>
            SerializeResult(
                new
                {
                    success = false,
                    error =
                        $"Unknown tool: " +
                        $"{toolCall.Function.Name}"
                })
    };
}

string Calculate(JsonElement arguments)
{
    double left =
        GetRequiredDouble(arguments, "left");

    double right =
        GetRequiredDouble(arguments, "right");

    string operation =
        GetRequiredString(arguments, "operation");

    double result =
        operation.ToLowerInvariant() switch
        {
            "add" =>
                left + right,

            "subtract" =>
                left - right,

            "multiply" =>
                left * right,

            "divide" when right != 0 =>
                left / right,

            "divide" =>
                throw new DivideByZeroException(
                    "Cannot divide by zero."),

            _ =>
                throw new ArgumentException(
                    $"Unsupported operation: {operation}")
        };

    return SerializeResult(
        new
        {
            success = true,
            left,
            right,
            operation,
            result
        });
}

string GetCurrentTime(JsonElement arguments)
{
    string timezoneId =
        GetRequiredString(
            arguments,
            "timezone_id");

    TimeZoneInfo timezone;

    try
    {
        timezone =
            TimeZoneInfo.FindSystemTimeZoneById(
                timezoneId);
    }
    catch (TimeZoneNotFoundException)
    {
        throw new ArgumentException(
            $"Unknown time zone: {timezoneId}");
    }
    catch (InvalidTimeZoneException)
    {
        throw new ArgumentException(
            $"Invalid time zone: {timezoneId}");
    }

    DateTimeOffset currentTime =
        TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            timezone);

    return SerializeResult(
        new
        {
            success = true,
            timezoneId,
            currentTime = currentTime.ToString(
                "O",
                CultureInfo.InvariantCulture),
            formattedTime = currentTime.ToString(
                "yyyy-MM-dd HH:mm:ss zzz",
                CultureInfo.InvariantCulture)
        });
}

string WriteReadme(JsonElement arguments)
{
    string content =
        GetRequiredString(arguments, "content");

    bool overwrite =
        GetRequiredBoolean(arguments, "overwrite");

    if (string.IsNullOrWhiteSpace(content))
    {
        throw new ArgumentException(
            "README content cannot be empty.");
    }

    if (content.Length > maximumReadmeLength)
    {
        throw new InvalidOperationException(
            $"README content exceeds the maximum length of " +
            $"{maximumReadmeLength} characters.");
    }

    string readmePath =
        Path.GetFullPath(
            Path.Combine(
                projectDirectory,
                "README.md"));

    string expectedProjectPath =
        Path.GetFullPath(projectDirectory)
            .TrimEnd(Path.DirectorySeparatorChar)
        + Path.DirectorySeparatorChar;

    if (!readmePath.StartsWith(
            expectedProjectPath,
            StringComparison.OrdinalIgnoreCase))
    {
        throw new UnauthorizedAccessException(
            "The README path is outside the project directory.");
    }

    if (File.Exists(readmePath) && !overwrite)
    {
        return SerializeResult(
            new
            {
                success = false,
                error =
                    "README.md already exists. " +
                    "Ask explicitly to overwrite it.",
                path = readmePath
            });
    }

    File.WriteAllText(
        readmePath,
        content,
        new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false));

    return SerializeResult(
        new
        {
            success = true,
            message =
                "README.md was written successfully.",
            path = readmePath,
            characterCount = content.Length,
            overwritten = overwrite
        });
}

string SerializeResult<T>(T result)
{
    return JsonSerializer.Serialize(
        result,
        serializerOptions);
}

static string GetRequiredString(
    JsonElement arguments,
    string propertyName)
{
    ValidateArgumentObject(arguments);

    if (!arguments.TryGetProperty(
            propertyName,
            out JsonElement value))
    {
        throw new ArgumentException(
            $"Missing argument '{propertyName}'.");
    }

    if (value.ValueKind != JsonValueKind.String)
    {
        throw new ArgumentException(
            $"Argument '{propertyName}' must be a string.");
    }

    return value.GetString()
        ?? throw new ArgumentException(
            $"Argument '{propertyName}' cannot be null.");
}

static double GetRequiredDouble(
    JsonElement arguments,
    string propertyName)
{
    ValidateArgumentObject(arguments);

    if (!arguments.TryGetProperty(
            propertyName,
            out JsonElement value))
    {
        throw new ArgumentException(
            $"Missing argument '{propertyName}'.");
    }

    if (!value.TryGetDouble(out double result))
    {
        throw new ArgumentException(
            $"Argument '{propertyName}' must be a number.");
    }

    return result;
}

static bool GetRequiredBoolean(
    JsonElement arguments,
    string propertyName)
{
    ValidateArgumentObject(arguments);

    if (!arguments.TryGetProperty(
            propertyName,
            out JsonElement value))
    {
        throw new ArgumentException(
            $"Missing argument '{propertyName}'.");
    }

    if (value.ValueKind is not
        JsonValueKind.True and not JsonValueKind.False)
    {
        throw new ArgumentException(
            $"Argument '{propertyName}' must be a Boolean.");
    }

    return value.GetBoolean();
}

static void ValidateArgumentObject(
    JsonElement arguments)
{
    if (arguments.ValueKind != JsonValueKind.Object)
    {
        throw new ArgumentException(
            "Tool arguments must be a JSON object.");
    }
}