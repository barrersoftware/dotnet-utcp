using System.Text.Json;
using UTCP.Core.Models;

namespace UTCP.Plugins.CodeMode;

/// <summary>
/// Interface exposing UTCP client operations for CodeMode orchestration
/// </summary>
public interface IUtcpClient
{
    Task<JsonElement> CallToolAsync(string name, Dictionary<string, JsonElement> args, CancellationToken cancellationToken = default);
    IAsyncEnumerable<JsonElement> CallToolStreamAsync(string name, Dictionary<string, JsonElement> args, CancellationToken cancellationToken = default);
    Task<List<UtcpTool>> SearchToolsAsync(string query, int limit, CancellationToken cancellationToken = default);
}
