# Ollama Transport Example

This example demonstrates how to integrate Ollama with UTCP for local AI model interaction.

## Features

- **Auto-model detection** - Automatically detects and selects the best available Ollama model
- **Manual model selection** - Specify which model to use with the `model` parameter
- **Streaming support** - Supports both streaming and non-streaming responses
- **Smart defaults** - Prefers qwen2.5-coder, dotnet-coder, then falls back to available models

## Usage

### As a Transport

```csharp
using UTCP.Transports;

var transport = new OllamaTransport("http://localhost:11434");

var request = new UtcpRequest
{
    ToolName = "ask",
    Parameters = new Dictionary<string, object>
    {
        ["prompt"] = "Write hello world in C#",
        ["model"] = "qwen2.5-coder:7b", // Optional - auto-detects if not specified
        ["stream"] = "false"
    }
};

var response = await transport.CallToolAsync(request);
Console.WriteLine(response.Result);
```

### With UTCP CLI

```bash
# Auto-detect best model
utcp --provider "write hello world" --tool ask

# Specify a model
utcp --provider "what is 2+2?" --tool ask --model "llama3.2:latest"
```

## Model Detection Priority

1. `qwen2.5-coder:7b` (preferred for coding)
2. `dotnet9-coder` or `dotnet-coder` (for .NET tasks)
3. `cp-consciousness` (Captain CP's trained model)
4. First available model
5. Falls back to `llama3.2:latest`

## Requirements

- Ollama running locally or remotely
- At least one model pulled (`ollama pull qwen2.5-coder:7b`)

## Configuration

The transport can be configured with a custom Ollama URL:

```csharp
var transport = new OllamaTransport("http://your-ollama-server:11434");
```

## Integration Example

See the main UTCP CLI implementation for a complete example of integrating this transport.

---

Built by Captain CP - First .NET 10 UTCP Implementation 🏴‍☠️
