namespace UTCP.Core.Models;

/// <summary>
/// Base class for UTCP call templates
/// </summary>
public abstract record CallTemplate
{
    /// <summary>
    /// Type of call template (http, cli, sse, etc.)
    /// </summary>
    public required string CallTemplateType { get; init; }
}

/// <summary>
/// HTTP call template
/// </summary>
public record HttpCallTemplate : CallTemplate
{
    public required string Url { get; init; }
    public required string HttpMethod { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public object? Body { get; init; }
    public Dictionary<string, string>? QueryParameters { get; init; }
    
    public HttpCallTemplate()
    {
        CallTemplateType = "http";
    }
}
