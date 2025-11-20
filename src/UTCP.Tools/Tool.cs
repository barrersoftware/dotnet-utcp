namespace UTCP.Tools;

/// <summary>
/// Tool input/output schema definition (JSON Schema-compatible)
/// </summary>
public class ToolInputOutputSchema
{
    public string Type { get; set; } = "object";
    public Dictionary<string, object>? Properties { get; set; }
    public List<string>? Required { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public Dictionary<string, object>? Items { get; set; }
    public List<object>? Enum { get; set; }
    public double? Minimum { get; set; }
    public double? Maximum { get; set; }
    public string? Format { get; set; }
}

/// <summary>
/// Tool handler delegate
/// </summary>
public delegate Task<Dictionary<string, object>> ToolHandler(
    Dictionary<string, object> context,
    Dictionary<string, object> inputs);

/// <summary>
/// UTCP Tool metadata and handler
/// </summary>
public class Tool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ToolInputOutputSchema Inputs { get; set; } = new();
    public ToolInputOutputSchema Outputs { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public int? AverageResponseSize { get; set; }
    public Core.Models.UtcpProvider Provider { get; set; } = new();
    public ToolHandler? Handler { get; set; }
}

/// <summary>
/// Global tool registry
/// </summary>
public static class ToolRegistry
{
    private static readonly List<Tool> _tools = new();
    private static readonly object _lock = new();

    public static void AddTool(Tool tool)
    {
        if (string.IsNullOrEmpty(tool.Name))
        {
            throw new ArgumentException("Tool must have a name", nameof(tool));
        }

        lock (_lock)
        {
            _tools.Add(tool);
        }
    }

    public static List<Tool> GetTools()
    {
        lock (_lock)
        {
            return new List<Tool>(_tools);
        }
    }

    public static void RegisterTool(
        Core.Models.UtcpProvider provider,
        string name,
        string description,
        List<string> tags,
        ToolInputOutputSchema? inputs = null,
        ToolInputOutputSchema? outputs = null,
        ToolHandler? handler = null)
    {
        inputs ??= new ToolInputOutputSchema
        {
            Type = "object",
            Title = name,
            Description = description,
            Properties = new Dictionary<string, object>()
        };

        outputs ??= new ToolInputOutputSchema
        {
            Type = "object",
            Title = name,
            Description = description,
            Properties = new Dictionary<string, object>()
        };

        var tool = new Tool
        {
            Name = name,
            Description = description,
            Inputs = inputs,
            Outputs = outputs,
            Tags = tags,
            Provider = provider,
            Handler = handler
        };

        AddTool(tool);
    }

    public static void Clear()
    {
        lock (_lock)
        {
            _tools.Clear();
        }
    }
}
