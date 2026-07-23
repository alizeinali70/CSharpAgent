# C# Ollama Agent

## Requirements
- [.NET SDK 6.0 or later](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Ollama](https://ollama.com/) installed and running (ensure `ollama serve` is active)
- A C# project setup with .NET 6.0 or later

## Installation
1. Clone the repository: `git clone https://github.com/yourusername/yourrepo.git`
2. Navigate to the project directory: `cd yourrepo`
3. Restore NuGet packages: `dotnet restore`
4. Build the project: `dotnet build --configuration Release`

## Usage
Run the agent with:
```bash
dotnet run --project yourrepo/yourproject.csproj
```
Interact using these commands:
- `add 2 3` - Perform arithmetic operations
- `current_time America/New_York` - Get time in IANA time zones
- `write_readme "New content" true` - Update documentation
- `list_tools` - Show available functions

## Available Tools
- **Arithmetic Operations** (`add`, `subtract`, `multiply`, `divide`)
- **Time Lookup** (IANA time zones like `Asia/Tokyo`)
- **Documentation** (`write_readme` with overwrite flag)

## Project Structure
```
yourrepo/
├── README.md
├── Program.cs
├── Tools/
│   ├── ArithmeticTool.cs
│   ├── TimeTool.cs
│   └── ReadmeTool.cs
└── example_prompts.txt
```

## Example Prompts
1. `multiply 12 34` - Test math operations
2. `current_time Europe/London` - Time zone lookup
3. `write_readme "Updated documentation" true` - Overwrite README
4. `list_tools` - View available functions
5. `add 100 200` - Basic arithmetic test

## Tips
- Use `list_tools` to verify available commands
- For time zones, use full IANA identifiers
- The `write_readme` tool requires explicit overwrite flag
