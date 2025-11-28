using System.Text;
using System.Text.Json;
using UTCP.Core.Models;

namespace UTCP.Plugins.CodeMode;

/// <summary>
/// High-level orchestrator mirroring rs-utcp's CodeMode flow:
/// 1) Decide if tools are needed
/// 2) Select tools by name  
/// 3) Ask the model to emit a C# snippet using CallTool helpers
/// 4) Execute the snippet via CodeMode
/// </summary>
public class CodeModeOrchestrator
{
    private readonly CodeModeUtcp _codeMode;
    private readonly ILlmModel _model;
    private string? _toolSpecsCache;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    
    public CodeModeOrchestrator(CodeModeUtcp codeMode, ILlmModel model)
    {
        _codeMode = codeMode ?? throw new ArgumentNullException(nameof(codeMode));
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }
    
    /// <summary>
    /// Run the full orchestration flow. Returns null if the model says no tools are needed
    /// or fails to pick any tools. Otherwise returns the codemode execution result.
    /// </summary>
    public async Task<JsonElement?> CallPromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var specs = await RenderToolSpecsAsync(cancellationToken);
        
        if (!await DecideIfToolsNeededAsync(prompt, specs, cancellationToken))
        {
            return null;
        }
        
        var selectedTools = await SelectToolsAsync(prompt, specs, cancellationToken);
        if (selectedTools.Count == 0)
        {
            return null;
        }
        
        var snippet = await GenerateSnippetAsync(prompt, selectedTools, specs, cancellationToken);
        var result = await _codeMode.ExecuteAsync(new CodeModeArgs
        {
            Code = snippet,
            Timeout = 20_000
        }, cancellationToken);
        
        return result.Value;
    }
    
    private async Task<string> RenderToolSpecsAsync(CancellationToken cancellationToken)
    {
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_toolSpecsCache != null)
            {
                return _toolSpecsCache;
            }
            
            var tools = await _codeMode.SearchToolsAsync("", 200, cancellationToken) ?? new List<UtcpTool>();
            var sb = new StringBuilder();
            sb.AppendLine("UTCP TOOL REFERENCE (use exact field names and required keys):");
            
            foreach (var tool in tools)
            {
                sb.AppendLine($"TOOL: {tool.Name} - {tool.Description}");
                sb.AppendLine("INPUTS:");
                
                if (tool.Parameters != null && tool.Parameters.Count > 0)
                {
                    foreach (var (key, value) in tool.Parameters)
                    {
                        sb.AppendLine($"  - {key}: {SchemaTypeHint(value)}");
                    }
                }
                else
                {
                    sb.AppendLine("  - none");
                }
                
                sb.AppendLine("OUTPUTS:");
                sb.AppendLine("  - (shape unspecified)");
                sb.AppendLine();
            }
            
            _toolSpecsCache = sb.ToString();
            return _toolSpecsCache;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
    
    private async Task<bool> DecideIfToolsNeededAsync(string prompt, string specs, CancellationToken cancellationToken)
    {
        var request = $"You can call tools described below. Respond with only 'yes' or 'no'.\n\nTOOLS:\n{specs}\n\nUSER:\n{prompt}";
        var respVal = await _model.CompleteAsync(request, cancellationToken);
        var respText = respVal.GetString() ?? string.Empty;
        return respText.TrimStart().ToLowerInvariant().StartsWith('y');
    }
    
    private async Task<List<string>> SelectToolsAsync(string prompt, string specs, CancellationToken cancellationToken)
    {
        var request = $"Choose relevant tool names from the list. Respond with a comma-separated list of names only.\n\nTOOLS:\n{specs}\n\nUSER:\n{prompt}";
        var respVal = await _model.CompleteAsync(request, cancellationToken);
        var respText = respVal.GetString() ?? string.Empty;
        
        var tools = new List<string>();
        foreach (var name in respText.Split(','))
        {
            var trimmed = name.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                tools.Add(trimmed);
            }
        }
        return tools;
    }
    
    private async Task<string> GenerateSnippetAsync(string prompt, List<string> tools, string specs, CancellationToken cancellationToken)
    {
        var toolList = string.Join(", ", tools);
        var request = $@"Generate a C# snippet that chains UTCP tool calls to satisfy the user request.
Use ONLY these tools: {toolList}.
Helpers available: await CallTool(name, args), await CallToolStream(name, args) returns List<JsonElement> of streamed chunks, await SearchTools(query, limit).
Use C# dictionary syntax new Dictionary<string, JsonElement> {{ {{ ""field"", JsonSerializer.SerializeToElement(value) }} }} with exact input field names; include required fields and never invent new keys.
You may call multiple tools, store results in variables, and pass them into subsequent tools.
When using CallToolStream, treat the returned List<JsonElement> as the streamed items and chain it into later calls or the final output.
Return the final value as the last expression. No markdown or commentary, code only.

USER:
{prompt}

TOOLS (use exact field names):
{specs}";
        
        var respVal = await _model.CompleteAsync(request, cancellationToken);
        var respText = respVal.GetString() ?? string.Empty;
        return respText.Trim();
    }
    
    private static string SchemaTypeHint(object? value)
    {
        if (value == null) return "any";
        
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => "string",
                JsonValueKind.Number => "number",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Array => "array",
                JsonValueKind.Object => "object",
                _ => "any"
            };
        }
        
        if (value is IDictionary<string, object> dict)
        {
            if (dict.TryGetValue("type", out var typeVal))
            {
                return typeVal?.ToString() ?? "any";
            }
        }
        
        return "any";
    }
}
