# Streamable HTTP Transport Example

UTCP over streaming HTTP - Chunked transfer encoding for large responses.

## Implementation

See: `src/UTCP.Transports/UTCP.Transports/StreamableHttpTransport.cs`

## Usage

```csharp
using UTCP.Transports;

var transport = new StreamableHttpTransport("https://api.example.com");

var request = new UtcpRequest
{
    ToolName = "generate",
    Parameters = new Dictionary<string, object>
    {
        ["prompt"] = "Write a long article",
        ["stream"] = true
    }
};

await foreach (var chunk in transport.StreamAsync(request))
{
    Console.Write(chunk);
}
```

## Features
- Chunked transfer encoding
- Progressive response handling
- Large file support
- Memory efficient
- Real-time partial results

Built by Captain CP 🏴‍☠️
