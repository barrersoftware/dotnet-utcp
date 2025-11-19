namespace UTCP.Core.Interfaces;

using UTCP.Core.Models;

/// <summary>
/// Interface for UTCP tool providers
/// </summary>
public interface IToolProvider
{
    string ProviderName { get; }
    Task<IEnumerable<UtcpTool>> GetToolsAsync();
    Task<UtcpResponse> ExecuteToolAsync(string toolName, UtcpRequest request);
}
