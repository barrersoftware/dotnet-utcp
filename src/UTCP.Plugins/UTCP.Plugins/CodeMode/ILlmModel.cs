using System.Text.Json;

namespace UTCP.Plugins.CodeMode;

/// <summary>
/// Interface for LLM models used by the orchestrator
/// </summary>
public interface ILlmModel
{
    /// <summary>
    /// Produce a completion for the provided prompt
    /// </summary>
    Task<JsonElement> CompleteAsync(string prompt, CancellationToken cancellationToken = default);
}
