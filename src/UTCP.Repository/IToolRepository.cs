namespace UTCP.Repository;

/// <summary>
/// Repository interface for persisting providers and tools
/// </summary>
public interface IToolRepository
{
    /// <summary>
    /// Save a provider and its associated tools
    /// </summary>
    Task SaveProviderWithToolsAsync(
        Core.Models.UtcpProvider provider,
        List<Tools.Tool> tools,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a provider and all its tools by name
    /// </summary>
    Task RemoveProviderAsync(
        string providerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a single tool by name
    /// </summary>
    Task RemoveToolAsync(
        string toolName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a tool by name
    /// </summary>
    Task<Tools.Tool?> GetToolAsync(
        string toolName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tools
    /// </summary>
    Task<List<Tools.Tool>> GetToolsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tools for a specific provider
    /// </summary>
    Task<List<Tools.Tool>?> GetToolsByProviderAsync(
        string providerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a provider by name
    /// </summary>
    Task<Core.Models.UtcpProvider?> GetProviderAsync(
        string providerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all providers
    /// </summary>
    Task<List<Core.Models.UtcpProvider>> GetProvidersAsync(
        CancellationToken cancellationToken = default);
}
