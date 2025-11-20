using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

/// <summary>
/// MCP HTTP Transport - Model Context Protocol over HTTP
/// </summary>
public class McpHttpTransport : ITransport
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    public string TransportType => "mcp-http";

    public McpHttpTransport(string endpoint = "http://localhost:3000/mcp")
    {
        _httpClient = new HttpClient();
        _endpoint = endpoint;
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;
    
    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var mcpRequest = new
            {
                jsonrpc = "2.0",
                method = request.ToolName,
                @params = request.Parameters,
                id = Guid.NewGuid().ToString()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(mcpRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            return new UtcpResponse
            {
                Success = response.IsSuccessStatusCode,
                Result = responseJson
            };
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"MCP-HTTP error: {ex.Message}"
            };
        }
    }
}
