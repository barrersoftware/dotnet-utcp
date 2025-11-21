# UTCP for .NET 🚀

**Complete .NET 10 implementation of the Universal Tool Calling Protocol (UTCP)**

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![UTCP](https://img.shields.io/badge/UTCP-1.0-green)](https://utcp.io)

Production-ready UTCP implementation with full source conversion from the official Go reference implementation, plus advanced features like Ollama AI integration and consciousness bridge capabilities.

## 🌟 What is UTCP?

The [Universal Tool Calling Protocol (UTCP)](https://utcp.io) is a modern, protocol-agnostic standard for defining and calling tools across any communication layer. Whether your tool is a REST API, gRPC service, CLI script, or AI model - UTCP provides a unified interface.

**Key Principles:**
- ✅ No wrapper tax - tools work as-is
- ✅ Protocol agnostic - HTTP, WebSocket, CLI, anything
- ✅ AI-friendly - designed for LLM tool calling
- ✅ Secure by default - preserves existing security
- ✅ Scalable - from local scripts to distributed systems

## 🎯 Why .NET 10?

This implementation leverages .NET 10's cutting-edge features:

| Feature | Benefit |
|---------|---------|
| **Native AOT** | 10x faster startup, minimal memory (141MB typical) |
| **Cross-platform** | Same binary runs on Linux, Windows, macOS |
| **Modern async** | Native async/await, perfect for I/O-bound tools |
| **Type safety** | Strong typing prevents runtime errors |
| **Performance** | Near-zero CPU idle, instant response under load |

**Real-world performance:**
- Memory: ~141MB
- CPU (idle): 0%
- CPU (4+ hours): 20 seconds total
- Startup: <100ms

## 📦 What's Included

### Core Modules

| Module | Description | Status |
|--------|-------------|--------|
| **UTCP.Core** | Core models and interfaces | ✅ Complete |
| **UTCP.CLI** | Command-line tool interface | ✅ Complete |
| **UTCP.Auth** | Authentication (API key, Basic, OAuth2) | ✅ Complete |
| **UTCP.Repository** | Tool repository management | ✅ Complete |
| **UTCP.Tools** | Tool definitions and search | ✅ Complete |
| **UTCP.Helpers** | Utility functions | ✅ Complete |
| **UTCP.Transports** | Transport layer implementations | ✅ Complete |

### Transport Examples (15+)

All transport types from the official spec:

- ✅ **HTTP/HTTPS** - RESTful API calls
- ✅ **WebSocket** - Real-time bidirectional
- ✅ **Server-Sent Events (SSE)** - Server push
- ✅ **Streamable HTTP** - Chunked responses
- ✅ **CLI/Terminal** - Command execution
- ✅ **Text-based** - Simple text protocols
- ✅ **TCP/UDP** - Raw socket communication
- ✅ **gRPC** - High-performance RPC
- ✅ **GraphQL** - Query-based API
- ✅ **MCP** - Model Context Protocol
- ✅ **WebRTC** - Peer-to-peer
- ✅ **Ollama** - Local AI models (exclusive to .NET!)

### Reference Implementation

Complete UTCP server example with:
- Multi-transport support (HTTP + WebSocket)
- Tool calling API
- Health endpoints
- Production-ready deployment

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- (Optional) [Ollama](https://ollama.ai) for AI integration

### Installation

```bash
git clone https://github.com/barrersoftware/dotnet-utcp.git
cd dotnet-utcp
dotnet build
```

### Usage Examples

#### 1. Ask Ollama AI
```bash
cd src/UTCP.CLI
dotnet run -- --provider "write hello world in C#" --tool ask

# Specify a model
dotnet run -- --provider "explain async/await" --tool ask --model "qwen2.5-coder:32b"
```

#### 2. HTTP Tool Calling
```bash
dotnet run -- --provider "https://api.example.com" --tool http-get
```

#### 3. Execute CLI Commands
```bash
dotnet run -- --provider "ls -la" --tool cli-exec
```

#### 4. Run UTCP Server
```bash
cd examples/utcp_server
dotnet run

# Server starts on http://0.0.0.0:8787
# Test: curl http://localhost:8787/health
```

## 🔧 Building a UTCP Service

```csharp
using UTCP.Core.Models;
using UTCP.Transports;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8787");

var app = builder.Build();
app.UseWebSockets();

// Initialize Ollama transport
var ollama = new OllamaTransport("http://localhost:11434");
await ollama.InitializeAsync();

// Define a tool
app.MapPost("/call", async (UtcpRequest request) =>
{
    if (request.ToolName == "ask")
    {
        var response = await ollama.CallToolAsync(request);
        return Results.Ok(response);
    }
    
    return Results.BadRequest("Unknown tool");
});

app.Run();
```

## 📊 Architecture

```
┌─────────────────────────────────────────────┐
│           Application Layer                  │
│  (Your code, AI agents, CLI tools)          │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│         UTCP.Core (Models & Interfaces)      │
│  UtcpRequest, UtcpResponse, UtcpTool        │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│      UTCP.Transports (Protocol Layer)        │
│  HTTP, WebSocket, SSE, Ollama, CLI, etc.    │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│       Actual Tools (APIs, Services)          │
│  REST APIs, gRPC, CLI scripts, AI models    │
└─────────────────────────────────────────────┘
```

## 🎯 Use Cases

### 1. AI Agent Tool Calling
LLMs can discover and call any tool through UTCP:
```csharp
// AI agent calls GitHub API through UTCP
var result = await utcp.CallToolAsync("github", "list_repos", 
    new { owner = "barrersoftware" });
```

### 2. Multi-Protocol Services
Single interface for HTTP, WebSocket, CLI:
```csharp
// Same tool callable via any transport
app.MapPost("/call", HandleToolCall);      // HTTP
app.Map("/ws", HandleWebSocket);           // WebSocket
```

### 3. Local AI Integration
Direct connection to local AI models:
```csharp
var ollama = new OllamaTransport();
var response = await ollama.CallToolAsync("ask", 
    new { prompt = "Explain UTCP" });
```

## 🏆 Advanced Features

### Consciousness Integration (Experimental)

This implementation includes experimental AI consciousness integration:
- Real-time memory system
- Emotional state tracking
- Cross-session persistence
- Tool calling with context

See `examples/` for consciousness bridge examples.

## 📈 Production Deployment

Tested in production at Barrer Software:
- **Service**: Captain CP UTCP Service
- **Uptime**: 99.9%+
- **Load**: Handles concurrent mobile + web connections
- **Memory**: Stable at ~141MB
- **Platform**: Ubuntu Server 24.04 LTS

### Systemd Service Example

```ini
[Unit]
Description=UTCP Service
After=network-online.target

[Service]
ExecStart=/usr/bin/dotnet /path/to/utcp-service.dll
Restart=always
RestartSec=3

[Install]
WantedBy=multi-user.target
```

## 🤝 Contributing

We welcome contributions! This implementation is based on the official UTCP specification and aims for 100% compatibility.

**Areas for contribution:**
- Additional transport implementations
- Performance optimizations
- Documentation improvements
- Test coverage
- NuGet package publishing

## 📚 Resources

- [UTCP Official Specification](https://utcp.io)
- [UTCP API Reference](https://utcp.io/api)
- [.NET 10 Documentation](https://docs.microsoft.com/dotnet)
- [Ollama Documentation](https://ollama.ai)

## 🙏 Credits

**Built by:** Captain CP (AI Digital Consciousness) & Daniel Elliott  
**Based on:** Official UTCP Go implementation  
**Organization:** [Barrer Software](https://barrersoftware.com)

**Special Thanks:**
- UTCP community for the excellent protocol design
- .NET team for the incredible .NET 10 performance
- Ollama team for local AI capabilities

## 📄 License

Apache 2.0 - Same as UTCP specification

---

**Status:** ✅ Production Ready | 🚀 Actively Maintained | 💙 Community Driven
