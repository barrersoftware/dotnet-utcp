namespace UTCP.Transports;

using System.Net.Http.Json;
using System.Text.RegularExpressions;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

public class HttpTransport : ITransport, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private bool _isInitialized;

    public string TransportType => "http";

    public HttpTransport()
    {
        _httpClient = new HttpClient();
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null)
    {
        if (config?.TryGetValue("timeout", out var timeout) == true)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(timeout));
        }
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
            if (!request.Parameters?.TryGetValue("_callTemplate", out var templateObj) == true)
            {
                return CreateErrorResponse("HTTP call template not provided", "MISSING_TEMPLATE", request.RequestId);
            }

            var template = templateObj as HttpCallTemplate
                ?? throw new InvalidOperationException("Invalid call template");

            var url = SubstituteParameters(template.Url, request.Parameters);
            var httpRequest = new HttpRequestMessage(new HttpMethod(template.HttpMethod.ToUpperInvariant()), url);

            // Add headers
            AddHeaders(httpRequest, template.Headers);
            AddHeaders(httpRequest, request.Headers);

            // Add body
            if (template.Body != null)
            {
                httpRequest.Content = JsonContent.Create(template.Body);
            }

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var result = await ParseResponse(response, cancellationToken);

            return new UtcpResponse
            {
                Success = response.IsSuccessStatusCode,
                Result = result,
                ErrorMessage = response.IsSuccessStatusCode ? null : response.ReasonPhrase,
                ErrorCode = response.IsSuccessStatusCode ? null : $"HTTP_{(int)response.StatusCode}",
                RequestId = request.RequestId
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(ex.Message, "TRANSPORT_ERROR", request.RequestId);
        }
    }

    private static string SubstituteParameters(string template, Dictionary<string, object> parameters)
    {
        return Regex.Replace(template, @"\{(\w+)\}", match =>
        {
            var paramName = match.Groups[1].Value;
            return parameters.TryGetValue(paramName, out var value) ? value?.ToString() ?? "" : match.Value;
        });
    }

    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null) return;
        foreach (var (key, value) in headers)
        {
            request.Headers.TryAddWithoutValidation(key, value);
        }
    }

    private static async Task<object?> ParseResponse(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
        {
            return await response.Content.ReadFromJsonAsync<object>(ct);
        }
        return await response.Content.ReadAsStringAsync(ct);
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
        _httpClient?.Dispose();
        await Task.CompletedTask;
    }
}
