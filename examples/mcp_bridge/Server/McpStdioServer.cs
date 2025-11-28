using System.Text.Json;

namespace UTCP.MCP.Server;

/// <summary>
/// MCP Server using stdio transport (JSON-RPC over stdin/stdout)
/// </summary>
public class McpStdioServer
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly SemaphoreSlim _outputLock = new(1, 1);
    private readonly UtcpBridge _bridge;

    public McpStdioServer(TextReader? input = null, TextWriter? output = null)
    {
        _input = input ?? Console.In;
        _output = output ?? Console.Out;
        _bridge = new UtcpBridge();
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await _input.ReadLineAsync(cancellationToken);
            if (line == null) break; // EOF

            try
            {
                var request = JsonSerializer.Deserialize<McpMessage>(line);
                if (request == null) continue;

                var response = await HandleRequestAsync(request, cancellationToken);
                await SendMessageAsync(response, cancellationToken);
            }
            catch (Exception ex)
            {
                // Send error response
                var errorResponse = new McpMessage
                {
                    Id = null,
                    Error = new McpError
                    {
                        Code = -32603,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                };
                await SendMessageAsync(errorResponse, cancellationToken);
            }
        }
    }

    private async Task<McpMessage> HandleRequestAsync(McpMessage request, CancellationToken cancellationToken)
    {
        var response = new McpMessage { Id = request.Id };

        try
        {
            response.Result = request.Method switch
            {
                "initialize" => await HandleInitializeAsync(request, cancellationToken),
                "tools/list" => await HandleToolsListAsync(request, cancellationToken),
                "tools/call" => await HandleToolsCallAsync(request, cancellationToken),
                _ => throw new InvalidOperationException($"Unknown method: {request.Method}")
            };
        }
        catch (Exception ex)
        {
            response.Error = new McpError
            {
                Code = -32603,
                Message = ex.Message
            };
        }

        return response;
    }

    private Task<object> HandleInitializeAsync(McpMessage request, CancellationToken cancellationToken)
    {
        var result = new McpInitializeResult
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new McpServerCapabilities
            {
                Tools = new McpToolsCapability { ListChanged = true }
            },
            ServerInfo = new McpServerInfo
            {
                Name = "UTCP-MCP Bridge",
                Version = "1.0.0"
            }
        };

        return Task.FromResult<object>(result);
    }

    private async Task<object> HandleToolsListAsync(McpMessage request, CancellationToken cancellationToken)
    {
        var tools = await _bridge.GetToolsAsync(cancellationToken);
        return new { tools };
    }

    private async Task<object> HandleToolsCallAsync(McpMessage request, CancellationToken cancellationToken)
    {
        var paramsJson = JsonSerializer.Serialize(request.Params);
        var callParams = JsonSerializer.Deserialize<Dictionary<string, object>>(paramsJson);
        
        var toolName = callParams?["name"]?.ToString() ?? "unknown";
        var argumentsElement = callParams?["arguments"] as JsonElement?;
        
        // Convert JsonElement to Dictionary
        var arguments = new Dictionary<string, object>();
        if (argumentsElement.HasValue)
        {
            foreach (var prop in argumentsElement.Value.EnumerateObject())
            {
                arguments[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString() ?? "",
                    JsonValueKind.Number => prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => prop.Value.ToString()
                };
            }
        }

        return await _bridge.CallToolAsync(toolName, arguments, cancellationToken);
    }

    private async Task SendMessageAsync(McpMessage message, CancellationToken cancellationToken)
    {
        await _outputLock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(message);
            await _output.WriteLineAsync(json.AsMemory(), cancellationToken);
            await _output.FlushAsync(cancellationToken);
        }
        finally
        {
            _outputLock.Release();
        }
    }
}
