using System.Collections.Concurrent;

namespace UTCP.Repository;

/// <summary>
/// In-memory implementation of IToolRepository
/// </summary>
public class InMemoryToolRepository : IToolRepository
{
    private readonly ConcurrentDictionary<string, Core.Models.UtcpProvider> _providers = new();
    private readonly ConcurrentDictionary<string, Tools.Tool> _tools = new();
    private readonly ConcurrentDictionary<string, List<string>> _providerTools = new();

    public Task SaveProviderWithToolsAsync(
        Core.Models.UtcpProvider provider,
        List<Tools.Tool> tools,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(provider.Name))
        {
            throw new ArgumentException("Provider must have a name", nameof(provider));
        }

        _providers[provider.Name] = provider;

        var toolNames = new List<string>();
        foreach (var tool in tools)
        {
            if (string.IsNullOrEmpty(tool.Name))
            {
                throw new ArgumentException("Tool must have a name", nameof(tools));
            }

            _tools[tool.Name] = tool;
            toolNames.Add(tool.Name);
        }

        _providerTools[provider.Name] = toolNames;

        return Task.CompletedTask;
    }

    public Task RemoveProviderAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryRemove(providerName, out _))
        {
            throw new KeyNotFoundException($"Provider '{providerName}' not found");
        }

        if (_providerTools.TryRemove(providerName, out var toolNames))
        {
            foreach (var toolName in toolNames)
            {
                _tools.TryRemove(toolName, out _);
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveToolAsync(
        string toolName,
        CancellationToken cancellationToken = default)
    {
        if (!_tools.TryRemove(toolName, out _))
        {
            throw new KeyNotFoundException($"Tool '{toolName}' not found");
        }

        // Remove from provider tool list
        foreach (var (providerName, toolNames) in _providerTools)
        {
            if (toolNames.Contains(toolName))
            {
                toolNames.Remove(toolName);
                _providerTools[providerName] = toolNames;
                break;
            }
        }

        return Task.CompletedTask;
    }

    public Task<Tools.Tool?> GetToolAsync(
        string toolName,
        CancellationToken cancellationToken = default)
    {
        _tools.TryGetValue(toolName, out var tool);
        return Task.FromResult(tool);
    }

    public Task<List<Tools.Tool>> GetToolsAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_tools.Values.ToList());
    }

    public Task<List<Tools.Tool>?> GetToolsByProviderAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        if (!_providerTools.TryGetValue(providerName, out var toolNames))
        {
            return Task.FromResult<List<Tools.Tool>?>(null);
        }

        var tools = toolNames
            .Select(name => _tools.TryGetValue(name, out var tool) ? tool : null)
            .Where(t => t != null)
            .Cast<Tools.Tool>()
            .ToList();

        return Task.FromResult<List<Tools.Tool>?>(tools);
    }

    public Task<Core.Models.UtcpProvider?> GetProviderAsync(
        string providerName,
        CancellationToken cancellationToken = default)
    {
        _providers.TryGetValue(providerName, out var provider);
        return Task.FromResult(provider);
    }

    public Task<List<Core.Models.UtcpProvider>> GetProvidersAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_providers.Values.ToList());
    }
}
