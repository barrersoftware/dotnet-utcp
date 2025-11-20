# UTCP Server Example

Complete working UTCP server implementation in .NET 10.

## Features

- **8 Tools**: ask (Ollama AI), view, edit, create, delete, bash, glob, grep
- **Full System Access**: File operations and command execution
- **Ollama Integration**: Local AI via Ollama
- **REST API**: Standard UTCP endpoints
- **Production Ready**: Systemd service included

## Quick Start

```bash
cd examples/utcp_server
dotnet run
```

Server starts on `http://0.0.0.0:8787`

## Endpoints

- `GET /health` - Health check
- `GET /tools` - Tool discovery
- `POST /call` - Execute tool
- `GET /status` - Server status

## Tool Calls

### Ask AI (via Ollama)
```bash
curl -X POST http://localhost:8787/call \
  -H "Content-Type: application/json" \
  -d '{"toolName":"ask","parameters":{"prompt":"Hello!"}}'
```

### View File
```bash
curl -X POST http://localhost:8787/call \
  -H "Content-Type: application/json" \
  -d '{"toolName":"view","parameters":{"path":"/etc/hostname"}}'
```

### Execute Command
```bash
curl -X POST http://localhost:8787/call \
  -H "Content-Type: application/json" \
  -d '{"toolName":"bash","parameters":{"command":"whoami"}}'
```

## Configuration

Edit `Program.cs` to:
- Change listening port (default: 8787)
- Change Ollama URL (default: http://localhost:11434)
- Add/remove tools
- Customize security

## Production Deployment

```bash
# Copy systemd service
sudo cp utcp-server.service /etc/systemd/system/
sudo systemctl enable utcp-server
sudo systemctl start utcp-server
```

## Security Notes

⚠️ **This server has full system access!** 

For production:
- Add authentication
- Restrict file paths
- Limit command execution
- Use HTTPS
- Run as restricted user

## License

Apache 2.0 (matching UTCP specification)
