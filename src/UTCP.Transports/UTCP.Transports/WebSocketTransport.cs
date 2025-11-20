namespace UTCP.Transports;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

public class WebSocketTransport : ITransport, IAsyncDisposable
{
    private readonly Dictionary<string, ClientWebSocket> _connections = new();
    private bool _isInitialized;

    public string TransportType => "websocket";

    public Task InitializeAsync(Dictionary<string, object>? config = null)
    {
        _isInitialized = true;
        return Task.CompletedTask;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            return CreateErrorResponse("Transport not initialized", "NOT_INITIALIZED", request.RequestId);
        }

        try
        {
            if (request.Parameters == null || !request.Parameters.TryGetValue("_callTemplate", out var templateObj))
            {
                return CreateErrorResponse("WebSocket call template not provided", "MISSING_TEMPLATE", request.RequestId);
            }

            var template = templateObj as WebSocketCallTemplate
                ?? throw new InvalidOperationException("Invalid call template");

            var ws = await GetOrCreateConnectionAsync(template.Url, cancellationToken);
            
            // Send message
            if (template.Message != null)
            {
                var messageJson = JsonSerializer.Serialize(template.Message);
                var messageBytes = Encoding.UTF8.GetBytes(messageJson);
                await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, cancellationToken);
            }

            // Receive responses
            var responses = new List<object>();
            var buffer = new byte[4096];
            var timeout = template.ResponseTimeout ?? 30;
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeout));

            var expectedResponses = template.ExpectedResponses;
            var receivedCount = 0;

            while (expectedResponses == -1 || receivedCount < expectedResponses)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var responseObj = JsonSerializer.Deserialize<object>(message);
                responses.Add(responseObj!);
                receivedCount++;

                if (template.CloseAfterResponse && receivedCount >= expectedResponses)
                    break;
            }

            // Close connection if requested
            if (template.CloseAfterResponse)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed", cancellationToken);
                _connections.Remove(template.Url);
            }

            return new UtcpResponse
            {
                Success = true,
                Result = expectedResponses == 1 ? responses.FirstOrDefault() : responses,
                RequestId = request.RequestId
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(ex.Message, "TRANSPORT_ERROR", request.RequestId);
        }
    }

    private async Task<ClientWebSocket> GetOrCreateConnectionAsync(string url, CancellationToken cancellationToken)
    {
        if (_connections.TryGetValue(url, out var existingWs) && existingWs.State == WebSocketState.Open)
        {
            return existingWs;
        }

        var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri(url), cancellationToken);
        _connections[url] = ws;
        return ws;
    }

    private static UtcpResponse CreateErrorResponse(string message, string code, string? requestId)
    {
        return new UtcpResponse
        {
            Success = false,
            ErrorMessage = message,
            ErrorCode = code,
            RequestId = requestId
        };
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var ws in _connections.Values)
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);
            }
            ws.Dispose();
        }
        _connections.Clear();
    }
}
