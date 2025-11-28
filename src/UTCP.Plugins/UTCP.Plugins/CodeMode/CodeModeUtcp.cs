using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using UTCP.Core.Models;

namespace UTCP.Plugins.CodeMode;

/// <summary>
/// Minimal facade exposing UTCP calls to C# scripts executed by CodeMode
/// </summary>
public class CodeModeUtcp
{
    private readonly IUtcpClient _client;
    
    public CodeModeUtcp(IUtcpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }
    
    /// <summary>
    /// Execute a snippet or JSON payload, returning the resulting value and captured output
    /// </summary>
    public async Task<CodeModeResult> ExecuteAsync(CodeModeArgs args, CancellationToken cancellationToken = default)
    {
        // If it's JSON already, return it directly
        if (TryParseAsJson(args.Code, out var jsonValue))
        {
            return new CodeModeResult
            {
                Value = jsonValue,
                Stdout = string.Empty,
                Stderr = string.Empty
            };
        }
        
        var value = await EvalCSharpSnippetAsync(args.Code, args.Timeout, cancellationToken);
        return new CodeModeResult
        {
            Value = value,
            Stdout = string.Empty,
            Stderr = string.Empty
        };
    }
    
    private async Task<JsonElement> EvalCSharpSnippetAsync(string code, ulong? timeoutMs, CancellationToken cancellationToken)
    {
        var scriptOptions = ScriptOptions.Default
            .AddReferences(typeof(CodeModeUtcp).Assembly)
            .AddReferences(typeof(JsonElement).Assembly)
            .AddImports("System")
            .AddImports("System.Collections.Generic")
            .AddImports("System.Text.Json")
            .AddImports("System.Threading.Tasks")
            .AddImports("UTCP.Plugins.CodeMode");
        
        var globals = new ScriptGlobals
        {
            Client = _client
        };
        
        var wrapped = $"var __out = {code};\n__out";
        
        try
        {
            var result = await CSharpScript.EvaluateAsync<object>(
                wrapped,
                scriptOptions,
                globals,
                typeof(ScriptGlobals),
                cancellationToken);
            
            return JsonSerializer.SerializeToElement(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"CodeMode eval error: {ex.Message}", ex);
        }
    }
    
    private static bool TryParseAsJson(string text, out JsonElement value)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            value = doc.RootElement.Clone();
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }
    
    /// <summary>
    /// Convenience helper for calling tools
    /// </summary>
    public Task<JsonElement> CallToolAsync(string name, Dictionary<string, JsonElement> args, CancellationToken cancellationToken = default)
    {
        return _client.CallToolAsync(name, args, cancellationToken);
    }
    
    /// <summary>
    /// Convenience helper for calling tools with streaming
    /// </summary>
    public IAsyncEnumerable<JsonElement> CallToolStreamAsync(string name, Dictionary<string, JsonElement> args, CancellationToken cancellationToken = default)
    {
        return _client.CallToolStreamAsync(name, args, cancellationToken);
    }
    
    /// <summary>
    /// Convenience helper for searching tools
    /// </summary>
    public Task<List<UtcpTool>> SearchToolsAsync(string query, int limit, CancellationToken cancellationToken = default)
    {
        return _client.SearchToolsAsync(query, limit, cancellationToken);
    }
}

/// <summary>
/// Global variables available to C# scripts
/// </summary>
public class ScriptGlobals
{
    public IUtcpClient Client { get; set; } = null!;
    
    public async Task<JsonElement> CallTool(string name, Dictionary<string, JsonElement> args)
    {
        return await Client.CallToolAsync(name, args);
    }
    
    public async Task<List<JsonElement>> CallToolStream(string name, Dictionary<string, JsonElement> args)
    {
        var items = new List<JsonElement>();
        await foreach (var item in Client.CallToolStreamAsync(name, args))
        {
            items.Add(item);
        }
        return items;
    }
    
    public async Task<List<UtcpTool>> SearchTools(string query, int limit)
    {
        return await Client.SearchToolsAsync(query, limit);
    }
}
