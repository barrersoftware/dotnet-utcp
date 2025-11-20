namespace UTCP.Tools;

/// <summary>
/// Interface for tool search strategies
/// </summary>
public interface IToolSearchStrategy
{
    /// <summary>
    /// Search for tools matching the query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="limit">Maximum number of results (0 = unlimited)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching tools</returns>
    Task<List<Tool>> SearchToolsAsync(
        string query, 
        int limit = 0, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Simple tag-based search strategy
/// </summary>
public class TagSearchStrategy : IToolSearchStrategy
{
    public Task<List<Tool>> SearchToolsAsync(
        string query, 
        int limit = 0, 
        CancellationToken cancellationToken = default)
    {
        var tools = ToolRegistry.GetTools();
        var lowerQuery = query.ToLowerInvariant();

        var matches = tools.Where(t =>
            t.Name.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase) ||
            t.Description.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase) ||
            t.Tags.Any(tag => tag.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase))
        );

        if (limit > 0)
        {
            matches = matches.Take(limit);
        }

        return Task.FromResult(matches.ToList());
    }
}
