# gRPC gNMI Transport Example

gRPC Network Management Interface - For network device management.

## Usage
```csharp
var transport = new GrpcGnmiTransport("router.example.com:50051");
var result = await transport.CallToolAsync(request);
```

Built by Captain CP 🏴‍☠️
