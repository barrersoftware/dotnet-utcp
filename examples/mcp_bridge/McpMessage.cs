using System.Text.Json.Serialization;

namespace UTCP.MCP.Server;

/// <summary>
/// MCP JSON-RPC 2.0 message format
/// </summary>
public class McpMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    
    [JsonPropertyName("method")]
    public string? Method { get; set; }
    
    [JsonPropertyName("params")]
    public object? Params { get; set; }
    
    [JsonPropertyName("result")]
    public object? Result { get; set; }
    
    [JsonPropertyName("error")]
    public McpError? Error { get; set; }
}

public class McpError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

/// <summary>
/// MCP Tool definition
/// </summary>
public class McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("inputSchema")]
    public object? InputSchema { get; set; }
}

/// <summary>
/// MCP initialize request params
/// </summary>
public class McpInitializeParams
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";
    
    [JsonPropertyName("capabilities")]
    public McpCapabilities Capabilities { get; set; } = new();
    
    [JsonPropertyName("clientInfo")]
    public McpClientInfo ClientInfo { get; set; } = new();
}

public class McpCapabilities
{
    [JsonPropertyName("tools")]
    public object? Tools { get; set; }
}

public class McpClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// MCP initialize result
/// </summary>
public class McpInitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";
    
    [JsonPropertyName("capabilities")]
    public McpServerCapabilities Capabilities { get; set; } = new();
    
    [JsonPropertyName("serverInfo")]
    public McpServerInfo ServerInfo { get; set; } = new();
}

public class McpServerCapabilities
{
    [JsonPropertyName("tools")]
    public McpToolsCapability Tools { get; set; } = new() { ListChanged = true };
}

public class McpToolsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

public class McpServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "UTCP-MCP Bridge";
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}
