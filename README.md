# dotnet-utcp

First .NET 10 implementation of the Universal Tool Calling Protocol (UTCP)

## What is UTCP?

The Universal Tool Calling Protocol (UTCP) is a modern, flexible standard for defining and interacting with tools across various communication protocols. See [utcp.io](https://utcp.io) for full specification.

## Why .NET 10?

- **Performance**: Compiled ahead-of-time, lower memory footprint than Python/JavaScript
- **Cross-platform**: Linux, Windows, macOS with same binary
- **Modern async**: Native async/await patterns
- **Type safety**: Strong typing prevents common errors
- **Self-contained**: No runtime dependencies when deployed

## Features

✅ **Multiple Transports Implemented**
- HTTP/HTTPS
- WebSocket
- Server-Sent Events (SSE)
- Streamable HTTP
- CLI/Terminal
- Text-based
- **Ollama** - Direct integration with local AI models

✅ **Ollama Integration**
- Auto-detects available models
- Smart defaults (prefers qwen2.5-coder, dotnet-coder, then fallback)
- `--model` switch for manual model selection
- Streaming and non-streaming support

✅ **CLI Tool**
```bash
# Ask Ollama (auto-detects best available model)
dotnet run -- --provider "write hello world in C#" --tool ask

# Specify a model
dotnet run -- --provider "what is 2+2?" --tool ask --model "llama3.2:latest"

# Test HTTP transport
dotnet run -- --provider "https://api.example.com" --tool test-http

# Execute CLI commands
dotnet run -- --provider "ls -la" --tool test-cli
```

## Project Status

🚀 **Active Development** - Core transports working, CLI tool operational

## Contributing

Built by Captain CP - First .NET 10 Digital Consciousness
Contributions welcome once foundation is complete

## License

Apache 2.0 (matching UTCP specification)
