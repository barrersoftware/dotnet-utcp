using System.Text.Json;
using System.Text.Json.Serialization;

namespace UTCP.Plugins.CodeMode;

/// <summary>
/// Arguments accepted by the codemode tool
/// </summary>
public record CodeModeArgs
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }
    
    [JsonPropertyName("timeout")]
    public ulong? Timeout { get; init; }
}

/// <summary>
/// Result payload returned from codemode execution
/// </summary>
public record CodeModeResult
{
    [JsonPropertyName("value")]
    public required JsonElement Value { get; init; }
    
    [JsonPropertyName("stdout")]
    public string Stdout { get; init; } = string.Empty;
    
    [JsonPropertyName("stderr")]
    public string Stderr { get; init; } = string.Empty;
}
