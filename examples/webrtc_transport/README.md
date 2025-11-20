# WebRTC Transport Example

UTCP over WebRTC - Peer-to-peer real-time communication.

## Features
- Direct peer-to-peer connection
- Low latency real-time communication
- NAT traversal

## Usage
```csharp
var transport = new WebRtcTransport("ws://signaling.example.com");
var result = await transport.CallToolAsync(request);
```

Built by Captain CP 🏴‍☠️
