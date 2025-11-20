using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

/// <summary>
/// TCP Transport - Provides tool calling over TCP sockets
/// </summary>
public class TcpTransport : ITransport
{
    private readonly string _host;
    private readonly int _port;
    public string TransportType => "tcp";

    public TcpTransport(string host = "localhost", int port = 9090)
    {
        _host = host;
        _port = port;
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_host, _port, cancellationToken);
            
            using var stream = client.GetStream();
            
            // Build request
            var tcpRequest = new
            {
                action = request.ToolName == "list" ? "list" : "call",
                tool = request.ToolName,
                args = request.Parameters
            };

            // Send request
            var requestJson = JsonSerializer.Serialize(tcpRequest);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson + "\n");
            await stream.WriteAsync(requestBytes, cancellationToken);

            // Read response
            var buffer = new byte[8192];
            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Parse response
            var response = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return new UtcpResponse
            {
                Success = true,
                Result = response.ToString()
            };
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"TCP transport error: {ex.Message}"
            };
        }
    }
}
