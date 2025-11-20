# UTCP Status Transport Example

Generic status monitoring transport for UTCP services.

## Features

- **Built-in Providers**:
  - `health` - Service health check
  - `metrics` - System resource metrics (memory, CPU, threads)
  - `environment` - .NET runtime and OS information

- **Custom Providers**: Register your own status providers
- **Extensible**: Add monitoring for databases, caches, queues, etc.

## Usage

```csharp
// Create status transport
var statusTransport = new StatusTransport("My Service");

// Add custom status provider
statusTransport.RegisterStatusProvider("database", async () =>
{
    return new { connected = true, connections = 5 };
});

// Check health
var response = await statusTransport.CallToolAsync(new UtcpRequest
{
    ToolName = "health"
});

// Get all status
var fullStatus = await statusTransport.CallToolAsync(new UtcpRequest
{
    ToolName = "full"
});
```

## Custom Status Providers

```csharp
statusTransport.RegisterStatusProvider("my_metric", async () =>
{
    // Your custom logic here
    return new { value = 42, timestamp = DateTime.UtcNow };
});
```

## Available Status Checks

- `health` - Basic health check
- `metrics` - System metrics
- `environment` - Runtime environment
- `full` - All registered providers
- Any custom provider you register
