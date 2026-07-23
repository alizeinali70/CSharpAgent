That is normal. Building something and teaching it are different skills.

To teach this project, do not start with the full code. Teach it in layers:

## 1. Explain the goal

“This application is a local AI agent written in C#. It sends messages to Ollama, lets the model choose tools, executes those tools in C#, and returns the results to the model.”

## 2. Teach the architecture

Use this flow:

```text
User input
   ↓
C# sends conversation to Ollama
   ↓
Ollama either answers directly
or requests a tool
   ↓
C# executes the tool
   ↓
Tool result goes back to Ollama
   ↓
Final answer
```

That is the core idea. Everything else is implementation detail.

## 3. Teach only five concepts

### A. Messages

```csharp
var messages = new List<ChatMessage>();
```

This stores the conversation.

### B. Request to Ollama

```csharp
await httpClient.PostAsJsonAsync(
    ollamaUrl,
    request,
    serializerOptions);
```

This sends the model, conversation, and available tools.

### C. Tool definitions

```csharp
Name = "calculate"
```

This tells the model which functions exist and which arguments they require.

### D. Tool execution

```csharp
return toolCall.Function.Name switch
{
    "calculate" => Calculate(...),
    "write_readme" => WriteReadme(...)
};
```

The model does not execute C# code. It only requests a tool. Your application executes it.

### E. Agent loop

```csharp
for (int step = 1; step <= maxAgentSteps; step++)
```

The loop continues until the model produces a normal answer instead of another tool call.

## 4. Teach it through one example

Use this prompt:

```text
Calculate 20 multiplied by 5.
```

Then explain the sequence:

```text
1. User sends the question.
2. Ollama sees the calculate tool.
3. Ollama returns a calculate tool call.
4. C# executes Calculate(20, 5, "multiply").
5. C# sends result 100 back to Ollama.
6. Ollama answers: "The result is 100."
```

Only after that should you show the README tool.

## 5. Use this teaching script

# Building a Local AI Agent in C# with Ollama

Today we will build a simple AI agent using C# and Ollama.

The goal is not only to create a chatbot. The goal is to create an application where the AI model can decide when to call C# functions.

## What is an AI agent?

A normal chatbot receives a message and returns an answer.

An AI agent can also use tools.

For example, the model can request:

* a calculation
* the current time
* the creation of a README file

The model does not execute these operations directly. It asks the C# application to execute them.

## Main workflow

The application works like this:

1. The user enters a message.
2. C# sends the message and available tools to Ollama.
3. Ollama either returns a normal response or requests a tool.
4. C# executes the requested tool.
5. The tool result is added to the conversation.
6. C# sends the updated conversation back to Ollama.
7. Ollama produces the final answer.

## Important classes

`ChatMessage` represents one message in the conversation.

`ChatRequest` represents the request sent to Ollama.

`ChatResponse` represents the response returned by Ollama.

`ToolDefinition` describes a tool that the model may use.

`ToolCall` represents a tool request produced by the model.

## The agent loop

The agent loop is the most important part of the project.

It repeatedly sends the conversation to Ollama.

When Ollama returns a tool call, the application executes the tool and continues the loop.

When Ollama returns a normal message without a tool call, the loop stops and the answer is shown to the user.

A maximum step count is used to prevent an infinite loop.

## Example: calculator tool

The calculator tool contains:

* a tool definition
* input validation
* C# calculation logic
* a JSON result

The model may request:

```json
{
  "name": "calculate",
  "arguments": {
    "left": 20,
    "right": 5,
    "operation": "multiply"
  }
}
```

The C# application executes the calculation and returns:

```json
{
  "success": true,
  "result": 100
}
```

The model then uses that result to answer the user.

## Example: README tool

The README tool receives:

* the Markdown content
* an overwrite flag

The C# application writes the content to `README.md`.

The write operation is restricted to the project directory. This is important because AI-generated tool calls must always be treated as untrusted input.

## Safety rules

The model should not have unrestricted access to:

* shell commands
* arbitrary file paths
* database writes
* credentials
* deletion operations

Tool arguments must always be validated in C#.

## Final idea

The AI model makes decisions.

The C# application controls execution.

This separation is the foundation of an AI agent.

Teach from that script, then demonstrate the code incrementally. Do not attempt to explain the entire `Program.cs` line by line in one session.
