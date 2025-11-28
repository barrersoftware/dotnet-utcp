using System.Text.Json;
using UTCP.Core.Models;
using UTCP.Plugins.CodeMode;

namespace UTCP.Examples;

/// <summary>
/// Example demonstrating CodeMode plugin usage
/// 
/// CodeMode allows executing C# code snippets within tool calls,
/// reducing token usage and network overhead by processing logic locally.
/// </summary>
public class CodeModeExample
{
    public static async Task Main(string[] args)
    {
        // Create a UTCP client (implementation depends on your setup)
        var client = CreateUtcpClient();
        
        // Initialize CodeMode orchestrator
        var orchestrator = new CodeModeOrchestrator(client);
        
        // Example 1: Simple calculation
        var calcArgs = new CodeModeArgs
        {
            Code = "2 + 2",
            Timeout = 5000
        };
        var calcResult = await orchestrator.ExecuteAsync(calcArgs);
        Console.WriteLine($"Calculation result: {calcResult.Value}");
        
        // Example 2: Using UTCP client within code
        var toolCallArgs = new CodeModeArgs
        {
            Code = @"await CallTool(""search"", new Dictionary<string, JsonElement> 
            { 
                [""query""] = JsonSerializer.SerializeToElement(""test"") 
            })",
            Timeout = 10000
        };
        var toolResult = await orchestrator.ExecuteAsync(toolCallArgs);
        Console.WriteLine($"Tool call result: {toolResult.Value}");
        
        // Example 3: JSON passthrough
        var jsonArgs = new CodeModeArgs
        {
            Code = @"{""status"": ""success"", ""value"": 42}",
            Timeout = 1000
        };
        var jsonResult = await orchestrator.ExecuteAsync(jsonArgs);
        Console.WriteLine($"JSON result: {jsonResult.Value}");
    }
    
    private static IUtcpClient CreateUtcpClient()
    {
        // Replace with your actual UTCP client initialization
        throw new NotImplementedException("Initialize your UTCP client here");
    }
}
