using System.Text.Json;

namespace UTCP.Helpers;

/// <summary>
/// Helper utilities for UTCP operations
/// </summary>
public static class UtcpHelpers
{
    /// <summary>
    /// Decode tools discovery response
    /// </summary>
    public static async Task<List<Tools.Tool>> DecodeToolsResponseAsync(
        Stream responseStream,
        CancellationToken cancellationToken = default)
    {
        var response = await JsonSerializer.DeserializeAsync<ToolsResponse>(
            responseStream,
            cancellationToken: cancellationToken);

        return response?.Tools ?? new List<Tools.Tool>();
    }

    /// <summary>
    /// Decode tools discovery response from string
    /// </summary>
    public static List<Tools.Tool> DecodeToolsResponse(string json)
    {
        var response = JsonSerializer.Deserialize<ToolsResponse>(json);
        return response?.Tools ?? new List<Tools.Tool>();
    }

    private class ToolsResponse
    {
        public List<Tools.Tool> Tools { get; set; } = new();
    }
}
