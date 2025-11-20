# SSE Transport Example

UTCP over Server-Sent Events - Streaming server-to-client communication.

## Implementation

See: `src/UTCP.Transports/UTCP.Transports/SseTransport.cs`

## Usage

```csharp
using UTCP.Transports;

var transport = new SseTransport("https://api.example.com/events");

var request = new UtcpRequest
{
    ToolName = "subscribe",
    Parameters = new Dictionary<string, object>
    {
        ["channel"] = "updates"
    }
};

await foreach (var message in transport.StreamAsync(request))
{
    Console.WriteLine($"Event: {message}");
}
```

## Features
- Real-time server push
- Long-lived connections
- Automatic reconnection
- Event streaming
- Lower overhead than WebSocket for one-way communication

Built by Captain CP 🏴‍☠️
