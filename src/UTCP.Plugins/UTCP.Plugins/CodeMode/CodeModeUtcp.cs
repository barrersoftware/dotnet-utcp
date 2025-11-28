using System.Text.Json;
using Jint;
using Jint.Native;
using UTCP.Core.Models;

namespace UTCP.Plugins.CodeMode;

/// <summary>
/// CodeMode executor using Jint JavaScript interpreter with injected UTCP helpers
/// Matches pattern from cagent/go-utcp/rs-utcp
/// </summary>
public class CodeModeUtcp
{
    private readonly IUtcpClient _client;
    
    public CodeModeUtcp(IUtcpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }
    
    /// <summary>
    /// Execute a JavaScript snippet with UTCP helper functions injected
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
        
        var value = await EvalJavaScriptAsync(args.Code, args.Timeout, cancellationToken);
        return new CodeModeResult
        {
            Value = value,
            Stdout = string.Empty,
            Stderr = string.Empty
        };
    }
    
    private async Task<JsonElement> EvalJavaScriptAsync(string code, ulong? timeoutMs, CancellationToken cancellationToken)
    {
        var engine = new Engine(options =>
        {
            options.TimeoutInterval(TimeSpan.FromMilliseconds(timeoutMs ?? 10000));
        });
        
        // Inject utcp helper object with call_tool and call_tool_stream
        engine.SetValue("utcp", new
        {
            call_tool = new Func<string, object, Task<object>>(async (name, args) =>
            {
                var argsDict = ConvertToDict(args);
                var result = await _client.CallToolAsync(name, argsDict, cancellationToken);
                return JsonSerializer.Deserialize<object>(result.GetRawText()) ?? new { };
            }),
            call_tool_stream = new Func<string, object, Task<object[]>>(async (name, args) =>
            {
                var argsDict = ConvertToDict(args);
                var results = new List<object>();
                await foreach (var item in _client.CallToolStreamAsync(name, argsDict, cancellationToken))
                {
                    results.Add(JsonSerializer.Deserialize<object>(item.GetRawText()) ?? new { });
                }
                return results.ToArray();
            })
        });
        
        try
        {
            var result = engine.Evaluate(code);
            
            // Convert JS result to JSON string
            var jsonString = ConvertJsValueToJsonString(result);
            
            return JsonDocument.Parse(jsonString).RootElement.Clone();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"CodeMode eval error: {ex.Message}", ex);
        }
    }
    
    private string ConvertJsValueToJsonString(JsValue value)
    {
        return value.Type switch
        {
            Jint.Runtime.Types.Undefined => "null",
            Jint.Runtime.Types.Null => "null",
            Jint.Runtime.Types.Boolean => value.AsBoolean().ToString().ToLower(),
            Jint.Runtime.Types.Number => value.AsNumber().ToString(System.Globalization.CultureInfo.InvariantCulture),
            Jint.Runtime.Types.String => JsonSerializer.Serialize(value.AsString()),
            Jint.Runtime.Types.Object => SerializeJsObject(value.AsObject()),
            _ => "null"
        };
    }
    
    private string SerializeJsObject(Jint.Native.Object.ObjectInstance obj)
    {
        // Simple serialization - convert to dictionary
        var dict = new Dictionary<string, object?>();
        foreach (var prop in obj.GetOwnProperties())
        {
            var propValue = obj.Get(prop.Key);
            dict[prop.Key.ToString()] = ConvertJsValueToObject(propValue);
        }
        return JsonSerializer.Serialize(dict);
    }
    
    private object? ConvertJsValueToObject(JsValue value)
    {
        return value.Type switch
        {
            Jint.Runtime.Types.Undefined => null,
            Jint.Runtime.Types.Null => null,
            Jint.Runtime.Types.Boolean => value.AsBoolean(),
            Jint.Runtime.Types.Number => value.AsNumber(),
            Jint.Runtime.Types.String => value.AsString(),
            Jint.Runtime.Types.Object => ConvertJsObjectToDictionary(value.AsObject()),
            _ => null
        };
    }
    
    private Dictionary<string, object?> ConvertJsObjectToDictionary(Jint.Native.Object.ObjectInstance obj)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in obj.GetOwnProperties())
        {
            dict[prop.Key.ToString()] = ConvertJsValueToObject(obj.Get(prop.Key));
        }
        return dict;
    }
    
    private Dictionary<string, JsonElement> ConvertToDict(object args)
    {
        var json = JsonSerializer.Serialize(args);
        var doc = JsonDocument.Parse(json);
        var dict = new Dictionary<string, JsonElement>();
        
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            dict[prop.Name] = prop.Value.Clone();
        }
        
        return dict;
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
