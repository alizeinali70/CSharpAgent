namespace CSharpAgent.Models;

internal static class ToolDefinitions
{
    public static List<ToolDefinition> All { get; } =
    [
        CreateCalculatorTool(),
        CreateCurrentTimeTool(),
        CreateReadmeTool()
    ];

    private static ToolDefinition CreateCalculatorTool()
    {
        return new ToolDefinition
        {
            Function = new FunctionDefinition
            {
                Name = "calculate",
                Description =
                    "Performs basic arithmetic using two numbers.",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        left = new
                        {
                            type = "number",
                            description = "The first number."
                        },
                        right = new
                        {
                            type = "number",
                            description = "The second number."
                        },
                        operation = new
                        {
                            type = "string",
                            description =
                                "The arithmetic operation to perform.",
                            @enum = new[]
                            {
                                "add",
                                "subtract",
                                "multiply",
                                "divide"
                            }
                        }
                    },
                    required = new[]
                    {
                        "left",
                        "right",
                        "operation"
                    }
                }
            }
        };
    }

    private static ToolDefinition CreateCurrentTimeTool()
    {
        return new ToolDefinition
        {
            Function = new FunctionDefinition
            {
                Name = "get_current_time",
                Description =
                    "Gets the current time in an IANA time zone.",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        timezone_id = new
                        {
                            type = "string",
                            description =
                                "An IANA time-zone ID, such as " +
                                "Europe/Berlin, Asia/Tokyo, or " +
                                "America/New_York."
                        }
                    },
                    required = new[]
                    {
                        "timezone_id"
                    }
                }
            }
        };
    }

    private static ToolDefinition CreateReadmeTool()
    {
        return new ToolDefinition
        {
            Function = new FunctionDefinition
            {
                Name = "write_readme",
                Description =
                    "Creates or updates README.md in the current " +
                    "project directory.",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        content = new
                        {
                            type = "string",
                            description =
                                "The complete Markdown content " +
                                "for README.md."
                        },
                        overwrite = new
                        {
                            type = "boolean",
                            description =
                                "True only when the user explicitly " +
                                "requested that an existing README.md " +
                                "be overwritten."
                        }
                    },
                    required = new[]
                    {
                        "content",
                        "overwrite"
                    }
                }
            }
        };
    }
}