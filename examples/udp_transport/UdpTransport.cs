using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

/// <summary>
/// UDP Transport - Provides tool calling over UDP datagrams
/// </summary>
public class UdpTransport : ITransport
{
    private readonly string _host;
    private readonly int _port;
    private readonly UdpClient _client;
    public string TransportType => "udp";

    public UdpTransport(string host = "localhost", int port = 9091)
    {
        _host = host;
        _port = port;
        _client = new UdpClient();
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await Task.CompletedTask;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_host == "localhost" ? "127.0.0.1" : _host), _port);

            // Build request
            var udpRequest = new
            {
                action = request.ToolName == "list" ? "list" : "call",
                tool = request.ToolName,
                args = request.Parameters
            };

            // Send datagram
            var requestJson = JsonSerializer.Serialize(udpRequest);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);
            await _client.SendAsync(requestBytes, requestBytes.Length, endpoint);

            // Receive response (with timeout)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var receiveTask = _client.ReceiveAsync(cts.Token);
            var result = await receiveTask;

            var responseJson = Encoding.UTF8.GetString(result.Buffer);
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
                ErrorMessage = $"UDP transport error: {ex.Message}"
            };
        }
    }
}
