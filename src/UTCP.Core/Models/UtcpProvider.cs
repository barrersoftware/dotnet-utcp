namespace UTCP.Core.Models;

/// <summary>
/// Represents a UTCP provider configuration
/// </summary>
public record UtcpProvider
{
    public required string Name { get; init; }
    public required string Transport { get; init; }
    public string? Endpoint { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
    public List<UtcpTool>? Tools { get; init; }
}
