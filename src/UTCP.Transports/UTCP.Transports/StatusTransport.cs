using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

/// <summary>
/// Generic Status Transport - Monitor system health and metrics
/// Provides real-time status of services, resources, and custom metrics
/// </summary>
public class StatusTransport : ITransport
{
    private readonly string _serviceName;
    private readonly Dictionary<string, Func<Task<object>>> _statusProviders;
    public string TransportType => "status";

    public StatusTransport(string serviceName = "UTCP Service")
    {
        _serviceName = serviceName;
        _statusProviders = new Dictionary<string, Func<Task<object>>>();
        
        // Register default status providers
        RegisterDefaultProviders();
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Register a custom status provider
    /// </summary>
    public void RegisterStatusProvider(string name, Func<Task<object>> provider)
    {
        _statusProviders[name] = provider;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var action = request.ToolName ?? "health";

            if (_statusProviders.ContainsKey(action))
            {
                var result = await _statusProviders[action]();
                return new UtcpResponse
                {
                    Success = true,
                    Result = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })
                };
            }

            // Default to full status
            return await GetFullStatusAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"Status transport error: {ex.Message}"
            };
        }
    }

    private void RegisterDefaultProviders()
    {
        // Health check
        _statusProviders["health"] = async () =>
        {
            await Task.CompletedTask;
            return new
            {
                service = _serviceName,
                status = "online",
                timestamp = DateTime.UtcNow,
                uptime = GetUptime()
            };
        };

        // System metrics
        _statusProviders["metrics"] = async () =>
        {
            await Task.CompletedTask;
            var process = System.Diagnostics.Process.GetCurrentProcess();
            return new
            {
                memory_mb = process.WorkingSet64 / 1024 / 1024,
                cpu_time_seconds = process.TotalProcessorTime.TotalSeconds,
                threads = process.Threads.Count,
                handles = process.HandleCount
            };
        };

        // Environment info
        _statusProviders["environment"] = async () =>
        {
            await Task.CompletedTask;
            return new
            {
                dotnet_version = Environment.Version.ToString(),
                os = Environment.OSVersion.ToString(),
                machine_name = Environment.MachineName,
                processor_count = Environment.ProcessorCount,
                is_64bit = Environment.Is64BitProcess
            };
        };
    }

    private async Task<UtcpResponse> GetFullStatusAsync(CancellationToken cancellationToken)
    {
        var statuses = new Dictionary<string, object>();

        foreach (var (name, provider) in _statusProviders)
        {
            try
            {
                statuses[name] = await provider();
            }
            catch (Exception ex)
            {
                statuses[name] = new { error = ex.Message };
            }
        }

        return new UtcpResponse
        {
            Success = true,
            Result = JsonSerializer.Serialize(statuses, new JsonSerializerOptions { WriteIndented = true })
        };
    }

    private static string GetUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
    }
}
