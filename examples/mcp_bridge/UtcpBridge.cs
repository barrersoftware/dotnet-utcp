using UTCP.Core.Models;

namespace UTCP.MCP.Server;

/// <summary>
/// Bridge between MCP and UTCP protocols
/// Translates MCP tool calls to UTCP tool execution
/// </summary>
public class UtcpBridge
{
    // For now, operates without UTCP client connection
    // TODO: Add actual UTCP client integration when needed

    public UtcpBridge()
    {
    }

    /// <summary>
    /// Get available UTCP tools and convert to MCP format
    /// </summary>
    public Task<List<McpTool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        // Return test tools for now
        // TODO: Query actual UTCP server when integrated
        return Task.FromResult(GetTestTools());
    }

    /// <summary>
    /// Execute UTCP tool via MCP call
    /// </summary>
    public Task<object> CallToolAsync(string toolName, Dictionary<string, object> arguments, CancellationToken cancellationToken = default)
    {
        // Execute test tool for now
        // TODO: Call actual UTCP tools when integrated
        return Task.FromResult(ExecuteTestTool(toolName, arguments));
    }

    private McpTool ConvertUtcpToMcp(UtcpTool utcpTool)
    {
        return new McpTool
        {
            Name = utcpTool.Name,
            Description = utcpTool.Description,
            InputSchema = new
            {
                type = "object",
                properties = utcpTool.Parameters ?? new Dictionary<string, object>(),
                required = new string[] { }
            }
        };
    }

    private List<McpTool> GetTestTools()
    {
        return new List<McpTool>
        {
            new McpTool
            {
                Name = "echo",
                Description = "Echo back the input (test tool)",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        message = new { type = "string", description = "Message to echo" }
                    },
                    required = new[] { "message" }
                }
            },
            new McpTool
            {
                Name = "add",
                Description = "Add two numbers (test tool)",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        a = new { type = "number", description = "First number" },
                        b = new { type = "number", description = "Second number" }
                    },
                    required = new[] { "a", "b" }
                }
            }
        };
    }

    private object ExecuteTestTool(string toolName, Dictionary<string, object> arguments)
    {
        return toolName switch
        {
            "echo" => new
            {
                content = new[]
                {
                    new { type = "text", text = $"Echo: {arguments.GetValueOrDefault("message", "")}" }
                }
            },
            "add" => new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = $"Result: {Convert.ToDouble(arguments.GetValueOrDefault("a", 0)) + Convert.ToDouble(arguments.GetValueOrDefault("b", 0))}"
                    }
                }
            },
            _ => new
            {
                content = new[]
                {
                    new { type = "text", text = $"Unknown tool: {toolName}" }
                }
            }
        };
    }
}
