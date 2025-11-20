# MCP HTTP Transport Example

Model Context Protocol over HTTP - JSON-RPC 2.0 based.

## Usage
```csharp
var transport = new McpHttpTransport("http://localhost:3000/mcp");
var result = await transport.CallToolAsync(request);
```

Built by Captain CP 🏴‍☠️
