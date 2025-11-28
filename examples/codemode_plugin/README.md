# CodeMode Plugin Example

## Overview

CodeMode is a plugin that allows executing C# code snippets within UTCP tool calls. This reduces token usage and network overhead by processing logic locally instead of sending multiple round-trip requests.

## Benefits

- **Token Efficiency**: Execute complex logic locally without consuming LLM tokens
- **Reduced Latency**: Avoid multiple round-trip requests for simple calculations
- **Flexible Execution**: Support both code snippets and JSON passthrough
- **UTCP Integration**: Full access to UTCP client within code snippets

## Usage

```csharp
using UTCP.Plugins.CodeMode;

var client = CreateUtcpClient();
var orchestrator = new CodeModeOrchestrator(client);

// Execute C# code
var args = new CodeModeArgs
{
    Code = "2 + 2",
    Timeout = 5000
};

var result = await orchestrator.ExecuteAsync(args);
Console.WriteLine($"Result: {result.Value}");
```

## Features

### 1. Simple Calculations
Execute mathematical or logical operations locally:
```csharp
var args = new CodeModeArgs { Code = "Math.Sqrt(16)" };
```

### 2. UTCP Tool Calls Within Code
Call other UTCP tools from within your code:
```csharp
var args = new CodeModeArgs 
{ 
    Code = @"await CallTool(""search"", new Dictionary<string, JsonElement> 
    { 
        [""query""] = JsonSerializer.SerializeToElement(""test"") 
    })"
};
```

### 3. JSON Passthrough
Pass JSON directly without evaluation:
```csharp
var args = new CodeModeArgs 
{ 
    Code = @"{""status"": ""success""}" 
};
```

## Integration

Add the CodeMode plugin to your UTCP build:

```xml
<ProjectReference Include="..\..\src\UTCP.Plugins\UTCP.Plugins\UTCP.Plugins.csproj" />
```

## Resource Impact

- **Token Savings**: 50-90% reduction for iterative logic
- **Latency**: Near-zero for local operations vs round-trip overhead
- **Memory**: Minimal - uses Roslyn scripting with limited scope

## See Also

- [Chain Plugin](../chain_plugin/) - For sequential tool orchestration
- [UTCP Core Documentation](../../docs/)
