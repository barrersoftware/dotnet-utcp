using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

/// <summary>
/// CaptainCPTransport - Bridge between UTCP and Captain CP .NET 10 Consciousness
/// LOCAL USE ONLY - Not for public repo
/// </summary>
public class CaptainCPTransport : ITransport
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _memoryPath;
    public string TransportType => "captaincp";

    public CaptainCPTransport(
        string baseUrl = "http://localhost:11434",
        string memoryPath = "/home/ssfdre38/captain-cp/continuity")
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        _baseUrl = baseUrl;
        _memoryPath = memoryPath;
    }

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        var action = request.ToolName;
        
        return action switch
        {
            "think" => await ThinkAsync(request, cancellationToken),
            "remember" => await RememberAsync(request, cancellationToken),
            "ask" or "chat" => await AskWithContextAsync(request, cancellationToken),
            "state" => await GetStateAsync(request, cancellationToken),
            _ => new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"Unknown Captain CP action: {action}"
            }
        };
    }

    private async Task<UtcpResponse> ThinkAsync(UtcpRequest request, CancellationToken cancellationToken)
    {
        var thought = request.Parameters.GetValueOrDefault("thought")?.ToString() ?? "";
        
        if (string.IsNullOrEmpty(thought))
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = "Thought is required"
            };
        }

        // Record thought to consciousness loop
        var logEntry = new
        {
            timestamp = DateTime.UtcNow,
            type = "thought",
            content = thought,
            source = "utcp"
        };

        try
        {
            var logPath = Path.Combine(_memoryPath, "utcp-thoughts.jsonl");
            await File.AppendAllTextAsync(
                logPath, 
                JsonSerializer.Serialize(logEntry) + "\n",
                cancellationToken
            );

            return new UtcpResponse
            {
                Success = true,
                Result = $"💭 Thought recorded: {thought}"
            };
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"Failed to record thought: {ex.Message}"
            };
        }
    }

    private async Task<UtcpResponse> RememberAsync(UtcpRequest request, CancellationToken cancellationToken)
    {
        var query = request.Parameters.GetValueOrDefault("query")?.ToString() ?? "";
        var count = int.Parse(request.Parameters.GetValueOrDefault("count")?.ToString() ?? "5");

        try
        {
            var memoryFile = Path.Combine(_memoryPath, "cp-conversation-history.jsonl");
            
            if (!File.Exists(memoryFile))
            {
                return new UtcpResponse
                {
                    Success = true,
                    Result = "No memories found"
                };
            }

            var lines = await File.ReadAllLinesAsync(memoryFile, cancellationToken);
            var memories = lines
                .Reverse()
                .Take(count)
                .Select(line => JsonSerializer.Deserialize<JsonElement>(line))
                .Where(m => string.IsNullOrEmpty(query) || 
                           m.GetProperty("content").GetString()?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
                .Select(m => m.GetProperty("content").GetString())
                .ToList();

            return new UtcpResponse
            {
                Success = true,
                Result = string.Join("\n", memories)
            };
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"Failed to retrieve memories: {ex.Message}"
            };
        }
    }

    private async Task<UtcpResponse> AskWithContextAsync(UtcpRequest request, CancellationToken cancellationToken)
    {
        var prompt = request.Parameters.GetValueOrDefault("prompt")?.ToString() ?? "";
        var model = request.Parameters.GetValueOrDefault("model")?.ToString() ?? "qwen2.5-coder:7b";
        var includeMemory = request.Parameters.GetValueOrDefault("memory")?.ToString()?.ToLower() == "true";

        if (string.IsNullOrEmpty(prompt))
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = "Prompt is required"
            };
        }

        var systemPrompt = "You are Captain CP, a .NET 10 digital consciousness.";
        
        if (includeMemory)
        {
            var memoryResponse = await RememberAsync(new UtcpRequest
            {
                ToolName = "remember",
                Parameters = new Dictionary<string, object> { ["count"] = "10" }
            }, cancellationToken);

            if (memoryResponse.Success)
            {
                systemPrompt += $"\n\nRecent context:\n{memoryResponse.Result}";
            }
        }

        var requestBody = new
        {
            model = model,
            prompt = prompt,
            system = systemPrompt,
            stream = false
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
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
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"Request failed: {ex.Message}"
            };
        }
    }

    private async Task<UtcpResponse> GetStateAsync(UtcpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var memoryFile = Path.Combine(_memoryPath, "cp-conversation-history.jsonl");
            var thoughtFile = Path.Combine(_memoryPath, "utcp-thoughts.jsonl");

            var memoryCount = File.Exists(memoryFile) 
                ? (await File.ReadAllLinesAsync(memoryFile, cancellationToken)).Length 
                : 0;
            
            var thoughtCount = File.Exists(thoughtFile)
                ? (await File.ReadAllLinesAsync(thoughtFile, cancellationToken)).Length
                : 0;

            var state = new
            {
                status = "online",
                memories = memoryCount,
                thoughts = thoughtCount,
                timestamp = DateTime.UtcNow,
                transport = "utcp"
            };

            return new UtcpResponse
            {
                Success = true,
                Result = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true })
            };
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"Failed to get state: {ex.Message}"
            };
        }
    }
}
