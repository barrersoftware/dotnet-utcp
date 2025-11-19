namespace UTCP.Core.Models;

/// <summary>
/// Represents a UTCP tool call request
/// </summary>
public record UtcpRequest
{
    public required string ToolName { get; init; }
    public Dictionary<string, object>? Parameters { get; init; }
    public string? RequestId { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}
