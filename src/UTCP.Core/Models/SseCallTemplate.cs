namespace UTCP.Core.Models;

/// <summary>
/// Server-Sent Events (SSE) call template for streaming events
/// </summary>
public record SseCallTemplate : CallTemplate
{
    public required string Url { get; init; }
    public string? EventType { get; init; }
    public bool Reconnect { get; init; } = true;
    public int RetryTimeout { get; init; } = 3000;
    public string? BodyField { get; init; }
    public Dictionary<string, string>? Auth { get; init; }
    
    public SseCallTemplate()
    {
        CallTemplateType = "sse";
    }
}
