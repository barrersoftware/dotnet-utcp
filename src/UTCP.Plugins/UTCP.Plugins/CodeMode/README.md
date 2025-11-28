# UTCP.Plugins.CodeMode

.NET 10 implementation of the UTCP CodeMode orchestrator, translated from [rs-utcp](https://github.com/universal-tool-calling-protocol/rs-utcp).

## Overview

CodeMode allows LLMs to orchestrate UTCP tool calls by generating and executing C# snippets. This is a 1:1 translation of the Rust implementation, using Microsoft.CodeAnalysis.CSharp.Scripting (Roslyn) for C# script execution instead of Rhai.

## Components

### CodeModeUtcp
Minimal facade exposing UTCP calls to C# scripts. Handles:
- Executing C# snippets with UTCP tool access
- Direct JSON passthrough
- Tool calling, streaming, and search

### CodeModeOrchestrator  
High-level orchestrator implementing the full flow:
1. Decide if tools are needed
2. Select relevant tools by name
3. Generate C# snippet using LLM
4. Execute snippet via CodeMode

### Script Globals
C# scripts have access to these helper methods:
- `await CallTool(name, args)` - Call a UTCP tool
- `await CallToolStream(name, args)` - Call a streaming UTCP tool, returns `List<JsonElement>`
- `await SearchTools(query, limit)` - Search available tools

## Differences from rs-utcp

- Uses C# scripting (Roslyn) instead of Rhai
- Dictionary syntax instead of Rhai map syntax
- Async/await instead of blocking with custom runtime
- `JsonElement` instead of `serde_json::Value`

## Status

✅ Core CodeMode implementation
✅ Orchestrator with LLM integration
✅ Streaming support
⏳ Examples and tests (TODO)

## Translation Notes

This is a pure translation of rs-utcp's codemode plugin to maintain ecosystem compatibility. Custom enhancements should go in dotnet-utcp-cp.
