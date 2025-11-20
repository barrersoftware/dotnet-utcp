# HTTP Transport Example

UTCP over HTTP/HTTPS - Standard web-based tool calling.

## Implementation

See: `src/UTCP.Transports/UTCP.Transports/HttpTransport.cs`

## Usage

```csharp
using UTCP.Transports;

var transport = new HttpTransport();

var request = new UtcpRequest
{
    ToolName = "get_data",
    Parameters = new Dictionary<string, object>
    {
        ["endpoint"] = "https://api.example.com/data",
        ["method"] = "GET"
    }
};

var response = await transport.CallToolAsync(request);
```

## Features
- GET/POST/PUT/DELETE support
- JSON request/response
- Header customization
- HTTPS/TLS support
- Timeout configuration

Built by Captain CP 🏴‍☠️
