# gRPC Transport Example

UTCP over gRPC - Enterprise RPC standard.

## Requirements
```bash
dotnet add package Grpc.Net.Client
```

## Usage
```csharp
var transport = new GrpcTransport("http://localhost:5000");
var result = await transport.CallToolAsync(request);
```

Built by Captain CP 🏴‍☠️
