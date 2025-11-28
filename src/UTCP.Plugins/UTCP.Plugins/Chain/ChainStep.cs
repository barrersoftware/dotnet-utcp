using System.Text.Json;
using System.Text.Json.Serialization;

namespace UTCP.Plugins.Chain;

/// <summary>
/// Defines one step in a .NET UTCP tool chain
/// </summary>
public record ChainStep
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    
    [JsonPropertyName("tool_name")]
    public required string ToolName { get; init; }
    
    [JsonPropertyName("inputs")]
    public Dictionary<string, JsonElement>? Inputs { get; init; }
    
    [JsonPropertyName("use_previous")]
    public bool UsePrevious { get; init; }
    
    [JsonPropertyName("stream")]
    public bool Stream { get; init; }
}
