using System.Text.Json;
using UTCP.Core.Models;
using UTCP.Plugins.CodeMode;

namespace UTCP.Examples;

/// <summary>
/// Example demonstrating CodeMode plugin usage
/// 
/// CodeMode allows executing JavaScript code snippets within tool calls,
/// reducing token usage and network overhead by processing logic locally.
/// Uses Jint JavaScript interpreter with injected utcp helper functions.
/// </summary>
public class CodeModeExample
{
    public static async Task Main(string[] args)
    {
        // Create a UTCP client (implementation depends on your setup)
        var client = CreateUtcpClient();
        
        // Initialize CodeMode with client
        var codeMode = new CodeModeUtcp(client);
        
        // Example 1: Simple calculation
        var calcArgs = new CodeModeArgs
        {
            Code = "2 + 2",
            Timeout = 5000
        };
        var calcResult = await codeMode.ExecuteAsync(calcArgs);
        Console.WriteLine($"Calculation result: {calcResult.Value}");
        
        // Example 2: Using UTCP client within JavaScript code
        // The 'utcp' object is injected with helper functions
        var toolCallArgs = new CodeModeArgs
        {
            Code = @"
                // Call a UTCP tool using injected helper
                const result = await utcp.call_tool('search', { query: 'test' });
                result;
            ",
            Timeout = 10000
        };
        var toolResult = await codeMode.ExecuteAsync(toolCallArgs);
        Console.WriteLine($"Tool call result: {toolResult.Value}");
        
        // Example 3: Chaining multiple tool calls
        var chainArgs = new CodeModeArgs
        {
            Code = @"
                // First call
                const searchResult = await utcp.call_tool('search', { query: 'UTCP protocol' });
                
                // Second call using first result
                const summaryResult = await utcp.call_tool('summarize', { text: searchResult.content });
                
                // Return final result
                summaryResult;
            ",
            Timeout = 15000
        };
        var chainResult = await codeMode.ExecuteAsync(chainArgs);
        Console.WriteLine($"Chain result: {chainResult.Value}");
        
        // Example 4: Using streaming tool calls
        var streamArgs = new CodeModeArgs
        {
            Code = @"
                // Call tool with streaming
                const chunks = await utcp.call_tool_stream('generate', { prompt: 'Hello' });
                
                // Process all chunks
                const combined = chunks.map(c => c.text).join('');
                ({ chunks: chunks.length, combined: combined });
            ",
            Timeout = 20000
        };
        var streamResult = await codeMode.ExecuteAsync(streamArgs);
        Console.WriteLine($"Stream result: {streamResult.Value}");
        
        // Example 5: JSON passthrough (no evaluation)
        var jsonArgs = new CodeModeArgs
        {
            Code = @"{""status"": ""success"", ""value"": 42}",
            Timeout = 1000
        };
        var jsonResult = await codeMode.ExecuteAsync(jsonArgs);
        Console.WriteLine($"JSON result: {jsonResult.Value}");
    }
    
    private static IUtcpClient CreateUtcpClient()
    {
        // Replace with your actual UTCP client initialization
        throw new NotImplementedException("Initialize your UTCP client here");
    }
}
