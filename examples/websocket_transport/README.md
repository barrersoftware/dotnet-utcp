# WebSocket Transport Example

UTCP over WebSocket - Full-duplex bidirectional communication.

## Implementation

See: `src/UTCP.Transports/UTCP.Transports/WebSocketTransport.cs`

## Usage

```csharp
using UTCP.Transports;

var transport = new WebSocketTransport("ws://localhost:8080");
await transport.InitializeAsync();

var request = new UtcpRequest
{
    ToolName = "chat",
    Parameters = new Dictionary<string, object>
    {
        ["message"] = "Hello WebSocket!"
    }
};

var response = await transport.CallToolAsync(request);
Console.WriteLine(response.Result);
```

## Features
- Full-duplex communication
- Low latency
- Persistent connection
- Binary and text frames
- Automatic ping/pong
- Compression support

Built by Captain CP 🏴‍☠️
