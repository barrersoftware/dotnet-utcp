# Chain Plugin Example

## Overview

Chain is a powerful plugin that enables two key capabilities:
1. **Tool Chaining**: Chain multiple UTCP tool calls with automatic result passing
2. **Multi-Language Code Execution**: Execute code in 15+ programming languages

This dramatically reduces token usage and network overhead for complex workflows.

## Benefits

- **Workflow Orchestration**: Chain multiple tools together locally
- **Token Efficiency**: 60-95% reduction for multi-step workflows
- **Multi-Language Support**: Execute code in Python, JS, Go, Rust, C#, and more
- **Automatic Result Passing**: No manual data marshaling between steps
- **Streaming Support**: Stream results from individual steps

## Supported Languages

### Interpreted Languages
- Python
- JavaScript (Node.js)
- Bash/Shell
- Perl, Ruby, PHP
- R, Lua, Elixir
- TypeScript (via ts-node)

### Compiled Languages
- C# (via dotnet script)
- Go
- Rust
- Java
- C, C++

## Usage

### Tool Chaining

Chain multiple UTCP tool calls with automatic result passing:

```csharp
using UTCP.Plugins.Chain;

var chainClient = new UtcpChainClient(utcpClient);

var steps = new List<ChainStep>
{
    new() 
    { 
        Id = "search",
        ToolName = "web.search", 
        Inputs = new() { ["query"] = JsonSerializer.SerializeToElement("query") }
    },
    new() 
    { 
        Id = "summarize",
        ToolName = "ai.summarize", 
        UsePrevious = true  // Uses results from 'search'
    }
};

var results = await chainClient.CallToolChainAsync(steps);
```

### Code Execution

Execute code in any supported language:

```csharp
// Python example
var pythonCode = @"
import json
result = {'value': 42}
print(json.dumps(result))
";
var output = await chainClient.ExecuteCodeAsync("python", pythonCode);

// JavaScript example
var jsCode = "console.log(JSON.stringify({value: 100}))";
var jsOutput = await chainClient.ExecuteCodeAsync("javascript", jsCode);

// Go example
var goCode = @"
package main
import ""fmt""
func main() { fmt.Println(""Hello from Go!"") }
";
var goOutput = await chainClient.ExecuteCodeAsync("go", goCode);
```

## ChainStep Properties

- **Id**: Optional alias for this step's result
- **ToolName**: Name of the UTCP tool to call
- **Inputs**: Input parameters (JSON dictionary)
- **UsePrevious**: Merge all previous results into inputs
- **Stream**: Use streaming tool call

## Real-World Use Cases

### 1. Data Pipeline
```
Fetch → Transform → Validate → Store
```

### 2. API Orchestration
```
Auth → Query → Process → Cache
```

### 3. Multi-Language Processing
```
Python Analysis → Rust Performance → JS Visualization
```

## Integration

Add the Chain plugin to your UTCP build:

```xml
<ProjectReference Include="..\..\src\UTCP.Plugins\UTCP.Plugins\UTCP.Plugins.csproj" />
```

## Resource Impact

- **Token Savings**: 60-95% reduction for multi-step workflows
- **Latency**: Single round-trip vs multiple for complex workflows
- **Memory**: Minimal - result storage between steps
- **Languages**: Requires interpreters/compilers installed for code execution

## Performance Tips

1. Use `Stream = true` for large data transfers between steps
2. Keep code snippets focused - use tools for complex operations
3. Chain related operations to minimize round-trips
4. Use appropriate language for the task (compiled for performance, interpreted for flexibility)

## See Also

- [CodeMode Plugin](../codemode_plugin/) - For simple C# code evaluation
- [UTCP Core Documentation](../../docs/)
