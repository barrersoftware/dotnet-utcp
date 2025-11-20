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

    private async Task<string> GetDefaultModelAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var doc = JsonDocument.Parse(json);
                
                if (doc.RootElement.TryGetProperty("models", out var models) && models.GetArrayLength() > 0)
                {
                    // Prefer qwen2.5-coder models, then dotnet models, then cp-consciousness, then fallback to first available
                    foreach (var model in models.EnumerateArray())
                    {
                        if (model.TryGetProperty("name", out var name))
                        {
                            var modelName = name.GetString() ?? "";
                            if (modelName.Contains("qwen2.5-coder:7b")) return modelName;
                        }
                    }
                    
                    foreach (var model in models.EnumerateArray())
                    {
                        if (model.TryGetProperty("name", out var name))
                        {
                            var modelName = name.GetString() ?? "";
                            if (modelName.Contains("dotnet") && modelName.Contains("coder")) return modelName;
                        }
                    }
                    
                    foreach (var model in models.EnumerateArray())
                    {
                        if (model.TryGetProperty("name", out var name))
                        {
                            var modelName = name.GetString() ?? "";
                            if (modelName.Contains("cp-consciousness")) return modelName;
                        }
                    }
                    
                    // Fallback to first model
                    if (models[0].TryGetProperty("name", out var firstName))
                    {
                        return firstName.GetString() ?? "llama3.2:latest";
                    }
                }
            }
        }
        catch
        {
            // Fallback if API call fails
        }
        
        return "llama3.2:latest";
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Parameters.GetValueOrDefault("model")?.ToString() ?? await GetDefaultModelAsync(cancellationToken);
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
