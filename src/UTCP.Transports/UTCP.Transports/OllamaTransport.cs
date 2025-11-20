using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

public class OllamaTransport : ITransport
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    public string TransportType => "ollama";

    public OllamaTransport(string baseUrl = "http://localhost:11434")
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        _baseUrl = baseUrl;
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Parameters.GetValueOrDefault("model")?.ToString() ?? "qwen2.5-coder:32b";
        var prompt = request.Parameters.GetValueOrDefault("prompt")?.ToString() ?? "";
        var system = request.Parameters.GetValueOrDefault("system")?.ToString();
        var stream = request.Parameters.GetValueOrDefault("stream")?.ToString()?.ToLower() == "true";

        if (string.IsNullOrEmpty(prompt))
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = "Prompt is required"
            };
        }

        var requestBody = new
        {
            model = model,
            prompt = prompt,
            system = system,
            stream = stream
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new UtcpResponse
                {
                    Success = false,
                    ErrorMessage = $"Ollama API error: {response.StatusCode}"
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (stream)
            {
                var lines = responseBody.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var responses = new List<string>();
                
                foreach (var line in lines)
                {
                    var jsonDoc = JsonDocument.Parse(line);
                    if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                    {
                        responses.Add(responseElement.GetString() ?? "");
                    }
                }

                return new UtcpResponse
                {
                    Success = true,
                    Result = string.Join("", responses)
                };
            }
            else
            {
                var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                {
                    return new UtcpResponse
                    {
                        Success = true,
                        Result = responseElement.GetString() ?? ""
                    };
                }

                return new UtcpResponse
                {
                    Success = false,
                    ErrorMessage = "No response from Ollama"
                };
            }
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"Ollama request failed: {ex.Message}"
            };
        }
    }
}
