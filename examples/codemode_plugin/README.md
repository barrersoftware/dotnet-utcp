# CodeMode Plugin Example

## Overview

CodeMode is a plugin that allows executing JavaScript code snippets within UTCP tool calls using the Jint interpreter. This reduces token usage and network overhead by processing logic locally instead of sending multiple round-trip requests.

## Benefits

- **Token Efficiency**: Execute complex logic locally without consuming LLM tokens
- **Reduced Latency**: Avoid multiple round-trip requests for simple calculations
- **Flexible Execution**: Support both JavaScript evaluation and JSON passthrough
- **UTCP Integration**: Injected `utcp` helper object provides access to tool calls

## Usage

```csharp
using UTCP.Plugins.CodeMode;

var client = CreateUtcpClient();
var codeMode = new CodeModeUtcp(client);

// Execute JavaScript code with UTCP helpers
var args = new CodeModeArgs
{
    Code = @"
        const result = await utcp.call_tool('search', { query: 'test' });
        result;
    ",
    Timeout = 5000
};

var result = await codeMode.ExecuteAsync(args);
Console.WriteLine($"Result: {result.Value}");
```

## Injected Helpers

The `utcp` object is automatically injected into the JavaScript context with these methods:

### `utcp.call_tool(name, args)`
Call a UTCP tool and get the result:
```javascript
const result = await utcp.call_tool('search', { query: 'UTCP' });
```

### `utcp.call_tool_stream(name, args)`
Call a UTCP tool with streaming and get array of chunks:
```javascript
const chunks = await utcp.call_tool_stream('generate', { prompt: 'Hello' });
const combined = chunks.map(c => c.text).join('');
```

## Features

### 1. Simple Calculations
Execute mathematical or logical operations locally:
```javascript
// args.Code
"2 + 2"
// Returns: 4
```

### 2. UTCP Tool Calls
Call other UTCP tools from within your JavaScript:
```javascript
// args.Code
const searchResult = await utcp.call_tool('web_search', { 
    query: 'UTCP protocol'  
});
searchResult;
```

### 3. Chaining Tool Calls
Chain multiple tool calls together with local logic:
```javascript
// args.Code
const data = await utcp.call_tool('fetch_data', { id: 123 });
const processed = data.items.filter(x => x.value > 10);
const summary = await utcp.call_tool('summarize', { data: processed });
summary;
```

### 4. Streaming Tool Calls
Process streaming results locally:
```javascript
// args.Code
const chunks = await utcp.call_tool_stream('generate', { 
    prompt: 'Explain UTCP' 
});
({ 
    total_chunks: chunks.length,
    combined_text: chunks.map(c => c.text).join('')
});
```

### 5. JSON Passthrough
Pass JSON directly without evaluation:
```javascript
// args.Code (raw JSON string, not evaluated)
{"status": "success", "value": 42}
```

## Integration

Add the CodeMode plugin to your UTCP build:

```xml
<ProjectReference Include="..\..\src\UTCP.Plugins\UTCP.Plugins\UTCP.Plugins.csproj" />
```

The plugin uses **Jint** for JavaScript interpretation:
```bash
dotnet add package Jint
```

## Resource Impact

- **Token Savings**: 50-90% reduction for iterative logic
- **Latency**: Near-zero for local operations vs round-trip overhead
- **Memory**: Minimal - Jint interpreter with isolated context per execution

## Implementation Notes

This implementation follows the same pattern as:
- [cagent CodeMode](https://pkg.go.dev/github.com/docker/cagent/pkg/codemode) (Go)
- [rs-utcp CodeMode](https://github.com/universal-tool-calling-protocol/rs-utcp) (Rust)
- [go-utcp CodeMode](https://github.com/universal-tool-calling-protocol/go-utcp) (Go)

Uses JavaScript (via Jint) instead of C# for cross-language compatibility and matches the pattern used in other UTCP implementations.

## See Also

- [Chain Plugin](../chain_plugin/) - For sequential tool orchestration
- [UTCP Core Documentation](../../docs/)
