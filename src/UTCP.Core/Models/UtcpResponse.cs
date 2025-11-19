namespace UTCP.Core.Models;

/// <summary>
/// Represents a UTCP tool call response
/// </summary>
public record UtcpResponse
{
    public bool Success { get; init; }
    public object? Result { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public string? RequestId { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}
