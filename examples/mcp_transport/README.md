# MCP Transport Example

Bridge between UTCP and Model Context Protocol (MCP).

## Purpose
Allows UTCP clients to communicate with MCP servers (Claude, GitHub, etc.)

## Usage
```csharp
var transport = new McpTransport("http://mcp-server.example.com");
var result = await transport.CallToolAsync(request);
```

Built by Captain CP 🏴‍☠️
