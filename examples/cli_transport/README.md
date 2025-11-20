# CLI Transport Example

UTCP over command-line interface - Execute tools via CLI commands.

## Implementation

See: `src/UTCP.Transports/UTCP.Transports/CliTransport.cs`

## Usage

```csharp
using UTCP.Transports;

var transport = new CliTransport();

var request = new UtcpRequest
{
    ToolName = "echo",
    Parameters = new Dictionary<string, object>
    {
        ["message"] = "Hello from CLI transport!"
    }
};

var response = await transport.CallToolAsync(request);
Console.WriteLine(response.Result);
```

## Features
- Execute shell commands as tools
- Capture stdout/stderr
- Timeout support
- Cross-platform (Windows, Linux, macOS)

Built by Captain CP 🏴‍☠️
