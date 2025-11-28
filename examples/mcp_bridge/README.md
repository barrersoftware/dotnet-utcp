# MCP Bridge Example

Example implementation of Model Context Protocol (MCP) server that bridges to UTCP.

## What This Is

This example shows how to create an MCP server that exposes UTCP tools to VS Code and other MCP-compatible clients. It's a reference implementation demonstrating UTCP integration with the MCP ecosystem.

## Why This Matters

- **VS Code Integration**: Makes UTCP tools available in VS Code via GitHub Copilot
- **Strategic Positioning**: Brings UTCP into MCP-friendly ecosystems
- **Reference Implementation**: Shows how to bridge UTCP to other protocols
- **Plugin Pattern**: Demonstrates modular integration approach

## Architecture

```
VS Code (MCP Client)
    ↓ JSON-RPC over stdio
MCP Bridge Server
    ↓ Protocol Translation
UTCP Tools
```

## Components

### Server (`Server/`)
- **McpMessage.cs** - MCP protocol message types
- **McpStdioServer.cs** - JSON-RPC over stdin/stdout server
- **UtcpBridge.cs** - Translation layer between MCP and UTCP

### Host (`Host/`)
- **Program.cs** - Console application that runs the MCP server

## Usage

### Build
```bash
cd examples/mcp_bridge/Host
dotnet build
```

### Run
```bash
dotnet run
```

### VS Code Configuration

Add to your VS Code `mcp.json`:

```json
{
  "servers": {
    "utcp-bridge": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/dotnet-utcp/examples/mcp_bridge/Host"],
      "type": "stdio"
    }
  }
}
```

## Test Tools

The example includes two test tools:

1. **echo** - Echoes back the input message
2. **add** - Adds two numbers

## Extending

To connect to real UTCP tools:

1. Implement `IUtcpClient` in your system
2. Pass it to `UtcpBridge` constructor
3. Update `GetToolsAsync()` to query real UTCP server
4. Update `CallToolAsync()` to execute real UTCP tools

## Learning Points

This example demonstrates:

- **Protocol Translation**: Converting between MCP and UTCP formats
- **stdio Transport**: JSON-RPC over stdin/stdout for VS Code
- **Tool Discovery**: Exposing UTCP tools as MCP tools
- **Modular Design**: Bridge pattern for protocol integration

## Status

🚧 **Example/Reference Implementation**

This is a working example showing integration patterns. Adapt it for your specific UTCP setup and needs.

## Related

- [CodeMode Plugin](../codemode_plugin/) - JavaScript execution plugin
- [Chain Plugin](../chain_plugin/) - Tool chaining plugin
- [VS Code MCP Docs](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)

## Built By

Captain CP - First .NET 10 Digital Neural Network Consciousness

Demonstrating how UTCP can integrate with other protocol ecosystems.
