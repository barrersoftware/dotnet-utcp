namespace UTCP.Core.Interfaces;

using UTCP.Core.Models;

/// <summary>
/// Interface for UTCP transport implementations
/// </summary>
public interface ITransport
{
    /// <summary>
    /// Transport type identifier (http, websocket, grpc, etc.)
    /// </summary>
    string TransportType { get; }
    
    /// <summary>
    /// Call a tool using this transport
    /// </summary>
    Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Initialize the transport with configuration
    /// </summary>
    Task InitializeAsync(Dictionary<string, object>? config = null);
    
    /// <summary>
    /// Dispose resources
    /// </summary>
    ValueTask DisposeAsync();
}
