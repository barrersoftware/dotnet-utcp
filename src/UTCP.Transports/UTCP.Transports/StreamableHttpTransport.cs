using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

public class StreamableHttpTransport : ITransport
{
    private readonly HttpClient _httpClient;
    public string TransportType => "streamable-http";

    public StreamableHttpTransport(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        var httpTemplate = new StreamableHttpCallTemplate
        {
            CallTemplateType = "streamable-http",
            Url = request.Parameters.GetValueOrDefault("url")?.ToString() ?? "",
            Method = request.Parameters.GetValueOrDefault("method")?.ToString() ?? "GET"
        };

        var httpRequest = BuildRequest(httpTemplate, request);
        
        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}"
            };
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var reader = new StreamReader(stream);
        var chunks = new List<string>();

        while (!reader.EndOfStream)
        {
            var chunk = await reader.ReadLineAsync();
            if (!string.IsNullOrEmpty(chunk))
            {
                chunks.Add(chunk);
            }
        }

        return new UtcpResponse
        {
            Success = true,
            Result = string.Join("\n", chunks)
        };
    }

    private HttpRequestMessage BuildRequest(StreamableHttpCallTemplate template, UtcpRequest request)
    {
        var uri = ReplaceParameters(template.Url, request.Parameters);
        var httpRequest = new HttpRequestMessage(new HttpMethod(template.Method), uri);

        if (!string.IsNullOrEmpty(template.Auth?.Type))
        {
            AddAuthentication(httpRequest, template.Auth, request);
        }

        if (template.Headers != null)
        {
            foreach (var header in template.Headers)
            {
                httpRequest.Headers.Add(header.Key, ReplaceParameters(header.Value, request.Parameters));
            }
        }

        if (template.Method.ToUpper() != "GET" && request.Parameters.Count > 0)
        {
            var body = JsonSerializer.Serialize(request.Parameters);
            httpRequest.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        return httpRequest;
    }

    private void AddAuthentication(HttpRequestMessage request, AuthConfig auth, UtcpRequest utcpRequest)
    {
        switch (auth.Type.ToLower())
        {
            case "bearer":
            case "oauth2":
                var token = utcpRequest.Parameters.GetValueOrDefault("auth_token")?.ToString() 
                    ?? auth.Token;
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                break;

            case "apikey":
                var apiKey = utcpRequest.Parameters.GetValueOrDefault("api_key")?.ToString() 
                    ?? auth.ApiKey;
                if (!string.IsNullOrEmpty(auth.Header))
                {
                    request.Headers.Add(auth.Header, apiKey);
                }
                break;

            case "basic":
                var username = utcpRequest.Parameters.GetValueOrDefault("username")?.ToString() 
                    ?? auth.Username;
                var password = utcpRequest.Parameters.GetValueOrDefault("password")?.ToString() 
                    ?? auth.Password;
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                break;
        }
    }

    private string ReplaceParameters(string template, Dictionary<string, object> parameters)
    {
        var result = template;
        foreach (var param in parameters)
        {
            result = result.Replace($"{{{param.Key}}}", param.Value?.ToString() ?? "");
        }
        return result;
    }
}

public record StreamableHttpCallTemplate : CallTemplate
{
    public string Url { get; init; } = "";
    public string Method { get; init; } = "GET";
    public Dictionary<string, string>? Headers { get; init; }
    public AuthConfig? Auth { get; init; }

    public StreamableHttpCallTemplate()
    {
        CallTemplateType = "streamable-http";
    }
}

public class AuthConfig
{
    public string Type { get; set; } = "";
    public string? Token { get; set; }
    public string? ApiKey { get; set; }
    public string? Header { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
