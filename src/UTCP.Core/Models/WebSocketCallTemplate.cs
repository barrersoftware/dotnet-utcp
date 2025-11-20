namespace UTCP.Core.Models;

/// <summary>
/// WebSocket call template following UTCP spec
/// </summary>
public record WebSocketCallTemplate : CallTemplate
{
    public required string Url { get; init; }
    public object? Message { get; init; }
    public bool CloseAfterResponse { get; init; } = true;
    public int? PingInterval { get; init; }
    public int ExpectedResponses { get; init; } = 1;
    public int? ResponseTimeout { get; init; }
    
    public WebSocketCallTemplate()
    {
        CallTemplateType = "websocket";
    }
}
