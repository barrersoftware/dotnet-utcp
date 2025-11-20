using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

/// <summary>
/// GraphQL Transport - Provides tool calling over GraphQL APIs
/// </summary>
public class GraphQLTransport : ITransport
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    public string TransportType => "graphql";

    public GraphQLTransport(string endpoint = "http://localhost:8080/graphql")
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
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
            var query = BuildGraphQLQuery(request);
            var variables = request.Parameters ?? new Dictionary<string, object>();

            var graphqlRequest = new
            {
                query = query,
                variables = variables
            };

            var requestJson = JsonSerializer.Serialize(graphqlRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpoint, content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new UtcpResponse
                {
                    Success = false,
                    ErrorMessage = $"GraphQL request failed: {response.StatusCode}"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var graphqlResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (graphqlResponse.TryGetProperty("errors", out var errors))
            {
                return new UtcpResponse
                {
                    Success = false,
                    ErrorMessage = $"GraphQL errors: {errors}"
                };
            }

            if (graphqlResponse.TryGetProperty("data", out var data))
            {
                return new UtcpResponse
                {
                    Success = true,
                    Result = data.ToString()
                };
            }

            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = "No data or errors in GraphQL response"
            };
        }
        catch (Exception ex)
        {
            return new UtcpResponse
            {
                Success = false,
                ErrorMessage = $"GraphQL transport error: {ex.Message}"
            };
        }
    }

    private string BuildGraphQLQuery(UtcpRequest request)
    {
        if (request.ToolName == "introspect" || request.ToolName == "list")
        {
            return @"query{__schema{queryType{name fields{name description}}}}";
        }

        var toolName = request.ToolName;
        var hasParams = request.Parameters != null && request.Parameters.Count > 0;

        if (hasParams)
        {
            var paramNames = string.Join(",", request.Parameters!.Keys.Select(k => $"${k}:String"));
            var paramRefs = string.Join(",", request.Parameters.Keys.Select(k => $"{k}:${k}"));
            return $"query {toolName}Query({paramNames}){{{toolName}({paramRefs})}}";
        }
        
        return $"query{{{toolName}}}";
    }
}
