namespace UTCP.Core.Models;

/// <summary>
/// Represents a tool in the UTCP protocol
/// </summary>
public record UtcpTool
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, object>? Parameters { get; init; }
    public required string Transport { get; init; }
    public required string Provider { get; init; }
}

/// <summary>
/// Extended tool with call template
/// </summary>
public record UtcpToolWithTemplate : UtcpTool
{
    public CallTemplate? CallTemplate { get; init; }
}
