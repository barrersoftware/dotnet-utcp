# UTCP.Plugins.Chain

.NET 10 implementation of the UTCP Chain plugin, translated from [go-utcp](https://github.com/universal-tool-calling-protocol/go-utcp).

## Overview

Chain allows you to chain UTCP tool calls together with automatic result passing between steps, plus execute code in 15+ programming languages.

## Features

### Tool Chaining
- Chain multiple UTCP tool calls in sequence
- Automatic result passing between steps
- Support for streaming tool calls
- Step aliasing and result storage

### Multi-Language Code Execution
Supports code execution in:
- **Interpreted**: Python, JavaScript, Node.js, Bash, Shell, Perl, Ruby, PHP, R, Lua, Elixir, TypeScript
- **Compiled**: C#, Go, Rust, Java, C, C++

## Usage

```csharp
var chainClient = new UtcpChainClient(utcpClient);

// Chain tool calls
var steps = new List<ChainStep>
{
    new() { 
        Id = "search",
        ToolName = "web.search", 
        Inputs = new() { ["query"] = JsonSerializer.SerializeToElement("UTCP protocol") }
    },
    new() { 
        Id = "summarize",
        ToolName = "ai.summarize", 
        UsePrevious = true  // Automatically uses results from previous step
    }
};

var results = await chainClient.CallToolChainAsync(steps);

// Execute code in any supported language
var pythonCode = @"
import json
print(json.dumps({'result': 'Hello from Python!'}))
";

var output = await chainClient.ExecuteCodeAsync("python", pythonCode);
```

## ChainStep Properties

- `Id` - Optional alias for this step's result
- `ToolName` - Name of the UTCP tool to call
- `Inputs` - Input parameters for the tool
- `UsePrevious` - If true, merges all previous step results into inputs
- `Stream` - If true, uses streaming tool call

## Differences from go-utcp

- Uses `Process` instead of `exec.Command`
- `JsonElement` instead of `map[string]any`
- Async/await throughout
- Added C# as a supported language (`.csx` via `dotnet script`)
- Simplified server detection (prepared for future use)

## Status

✅ Core chain execution
✅ Result passing between steps
✅ Multi-language code execution
✅ Streaming support
⏳ Examples and tests (TODO)

## Translation Notes

This is a pure translation of go-utcp's chain plugin to maintain ecosystem compatibility. Custom enhancements should go in dotnet-utcp-cp.
