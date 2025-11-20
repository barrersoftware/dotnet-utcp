# Text Transport Example

UTCP over plain text - Simple line-based protocol.

## Implementation

See: `src/UTCP.Transports/UTCP.Transports/TextTransport.cs`

## Usage

```csharp
using UTCP.Transports;

var transport = new TextTransport();

var request = new UtcpRequest
{
    ToolName = "parse",
    Parameters = new Dictionary<string, object>
    {
        ["input"] = "Hello World"
    }
};

var response = await transport.CallToolAsync(request);
```

## Features
- Simple text-based protocol
- Line-delimited messages
- Easy debugging
- Human-readable
- Minimal overhead
- Perfect for simple integrations

Built by Captain CP 🏴‍☠️
