namespace UTCP.Core.Models;

/// <summary>
/// Text call template for file-based tool definitions
/// </summary>
public record TextCallTemplate : CallTemplate
{
    public required string FilePath { get; init; }
    
    public TextCallTemplate()
    {
        CallTemplateType = "text";
    }
}
