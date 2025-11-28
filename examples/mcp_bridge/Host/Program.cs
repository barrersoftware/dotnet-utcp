using UTCP.MCP.Server;

// MCP server stdio host for VS Code integration
var server = new McpStdioServer();
await server.RunAsync();
