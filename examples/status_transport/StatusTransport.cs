using UTCP.Core.Models;
using UTCP.Transports;

namespace StatusTransportExample;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("UTCP Status Transport Example\n");

        // Create status transport
        var statusTransport = new StatusTransport("My UTCP Service");

        // Add custom status provider
        statusTransport.RegisterStatusProvider("database", async () =>
        {
            await Task.CompletedTask;
            return new
            {
                connected = true,
                connections = 5,
                queries_per_second = 42
            };
        });

        statusTransport.RegisterStatusProvider("cache", async () =>
        {
            await Task.CompletedTask;
            return new
            {
                hit_rate = 0.87,
                size_mb = 128,
                items = 1523
            };
        });

        // Initialize
        await statusTransport.InitializeAsync();

        // Test different status checks
        Console.WriteLine("=== Health Check ===");
        var healthResponse = await statusTransport.CallToolAsync(new UtcpRequest
        {
            ToolName = "health"
        });
        Console.WriteLine(healthResponse.Result);

        Console.WriteLine("\n=== System Metrics ===");
        var metricsResponse = await statusTransport.CallToolAsync(new UtcpRequest
        {
            ToolName = "metrics"
        });
        Console.WriteLine(metricsResponse.Result);

        Console.WriteLine("\n=== Custom Database Status ===");
        var dbResponse = await statusTransport.CallToolAsync(new UtcpRequest
        {
            ToolName = "database"
        });
        Console.WriteLine(dbResponse.Result);

        Console.WriteLine("\n=== Full Status (All Providers) ===");
        var fullResponse = await statusTransport.CallToolAsync(new UtcpRequest
        {
            ToolName = "full"
        });
        Console.WriteLine(fullResponse.Result);

        await statusTransport.DisposeAsync();
    }
}
