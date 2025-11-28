using System.Text.Json;
using UTCP.Core.Models;
using UTCP.Plugins.Chain;

namespace UTCP.Examples;

/// <summary>
/// Example demonstrating Chain plugin usage
/// 
/// Chain allows chaining multiple UTCP tool calls with automatic result passing,
/// plus executing code in 15+ programming languages. Reduces token overhead by
/// orchestrating complex workflows locally.
/// </summary>
public class ChainExample
{
    public static async Task Main(string[] args)
    {
        // Create a UTCP client (implementation depends on your setup)
        var client = CreateUtcpClient();
        
        // Initialize Chain client
        var chainClient = new UtcpChainClient(client);
        
        // Example 1: Chain tool calls with automatic result passing
        var searchAndSummarize = new List<ChainStep>
        {
            new() 
            { 
                Id = "search",
                ToolName = "web.search", 
                Inputs = new() 
                { 
                    ["query"] = JsonSerializer.SerializeToElement("UTCP protocol") 
                }
            },
            new() 
            { 
                Id = "summarize",
                ToolName = "ai.summarize", 
                UsePrevious = true  // Automatically uses results from 'search' step
            }
        };
        
        var results = await chainClient.CallToolChainAsync(searchAndSummarize);
        Console.WriteLine($"Chain results: {results.Count} steps completed");
        
        // Example 2: Execute Python code
        var pythonCode = @"
import json
result = {'message': 'Hello from Python!', 'value': 42}
print(json.dumps(result))
";
        var pythonOutput = await chainClient.ExecuteCodeAsync("python", pythonCode);
        Console.WriteLine($"Python output: {pythonOutput}");
        
        // Example 3: Execute JavaScript code
        var jsCode = @"
const result = { message: 'Hello from Node.js!', value: 100 };
console.log(JSON.stringify(result));
";
        var jsOutput = await chainClient.ExecuteCodeAsync("javascript", jsCode);
        Console.WriteLine($"JavaScript output: {jsOutput}");
        
        // Example 4: Complex chain with multiple languages
        var complexChain = new List<ChainStep>
        {
            new() 
            { 
                Id = "data_fetch",
                ToolName = "api.get",
                Inputs = new() { ["url"] = JsonSerializer.SerializeToElement("https://api.example.com/data") }
            },
            new() 
            { 
                Id = "process",
                ToolName = "chain.execute_code",
                Inputs = new()
                {
                    ["language"] = JsonSerializer.SerializeToElement("python"),
                    ["code"] = JsonSerializer.SerializeToElement("import json; data = input(); print(json.dumps({'processed': len(data)}))")
                },
                UsePrevious = true
            },
            new() 
            { 
                Id = "store",
                ToolName = "db.save",
                UsePrevious = true
            }
        };
        
        var complexResults = await chainClient.CallToolChainAsync(complexChain);
        Console.WriteLine($"Complex chain completed: {complexResults.Count} steps");
    }
    
    private static IUtcpClient CreateUtcpClient()
    {
        // Replace with your actual UTCP client initialization
        throw new NotImplementedException("Initialize your UTCP client here");
    }
}
