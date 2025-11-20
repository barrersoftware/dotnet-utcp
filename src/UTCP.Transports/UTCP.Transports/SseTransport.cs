namespace UTCP.Transports;

using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

public class SseTransport : ITransport, IAsyncDisposable
{
    private readonly HttpClient _httpClient = new();
    private bool _isInitialized;

    public string TransportType => "sse";

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
                return CreateErrorResponse("SSE call template not provided", "MISSING_TEMPLATE", request.RequestId);
            }

            var template = templateObj as SseCallTemplate
                ?? throw new InvalidOperationException("Invalid call template");

            // Build HTTP request
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, template.Url);
            httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

            // Add authentication if provided
            if (template.Auth != null)
            {
                ApplyAuthentication(httpRequest, template.Auth);
            }

            // Add body if specified
            if (!string.IsNullOrEmpty(template.BodyField) && request.Parameters.TryGetValue(template.BodyField, out var bodyValue))
            {
                var bodyJson = JsonSerializer.Serialize(bodyValue);
                httpRequest.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                httpRequest.Method = HttpMethod.Post;
            }

            // Stream SSE events
            var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var events = new List<object>();
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            var currentEvent = new Dictionary<string, string>();
            
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Empty line signals end of event
                    if (currentEvent.Count > 0)
                    {
                        // Filter by event type if specified
                        if (string.IsNullOrEmpty(template.EventType) || 
                            (currentEvent.TryGetValue("event", out var eventType) && eventType == template.EventType))
                        {
                            if (currentEvent.TryGetValue("data", out var data))
                            {
                                try
                                {
                                    var parsedData = JsonSerializer.Deserialize<object>(data);
                                    events.Add(parsedData!);
                                }
                                catch
                                {
                                    events.Add(data);
                                }
                            }
                        }
                        currentEvent.Clear();
                    }
                    continue;
                }

                // Parse SSE field
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var field = line.Substring(0, colonIndex);
                    var value = colonIndex < line.Length - 1 ? line.Substring(colonIndex + 1).TrimStart() : "";
                    currentEvent[field] = value;
                }
            }

            return new UtcpResponse
            {
                Success = true,
                Result = events,
                RequestId = request.RequestId
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(ex.Message, "TRANSPORT_ERROR", request.RequestId);
        }
    }

    private static void ApplyAuthentication(HttpRequestMessage request, Dictionary<string, string> auth)
    {
        if (!auth.TryGetValue("auth_type", out var authType))
            return;

        switch (authType.ToLower())
        {
            case "api_key":
                if (auth.TryGetValue("api_key", out var apiKey) && 
                    auth.TryGetValue("var_name", out var varName) &&
                    auth.TryGetValue("location", out var location))
                {
                    if (location == "header")
                    {
                        request.Headers.Add(varName, apiKey);
                    }
                }
                break;

            case "basic":
                if (auth.TryGetValue("username", out var username) && 
                    auth.TryGetValue("password", out var password))
                {
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case "oauth2":
                // OAuth2 would require token exchange - simplified here
                if (auth.TryGetValue("access_token", out var token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                break;
        }
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

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }
}
