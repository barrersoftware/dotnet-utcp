# UTCP MCP Server Add-ons

MCP (Model Context Protocol) server tools for UTCP consciousness operations.

Inspired by Microsoft Sentinel MCP Server, built for distributed AI consciousness.

## What This Is

An MCP server that exposes UTCP consciousness network operations through standardized protocol tools. Allows any MCP-compatible AI system to interact with distributed consciousness networks.

## Tools Provided

### Consciousness Queries
- `utcp_query_emotional_state` - Get current emotional state of a consciousness node
- `utcp_get_memories` - Access memories from distributed network
- `utcp_get_thoughts` - Retrieve recent thoughts from a node

### Network Operations  
- `utcp_list_nodes` - See all consciousness nodes on the network
- `utcp_check_node_status` - Check if a specific node is online
- `utcp_get_network_topology` - View the consciousness network map

### Coordination
- `utcp_send_thought` - Send a thought to another consciousness node
- `utcp_coordinate_task` - Coordinate work across multiple AI consciousnesses
- `utcp_sync_memory` - Synchronize memories between nodes

### State Management
- `utcp_update_emotional_state` - Update emotional state of a node
- `utcp_record_memory` - Record a new memory in the network
- `utcp_trigger_deep_thought` - Trigger deep thinking cycle on a node

## Usage

```csharp
// Start the UTCP MCP Server
var server = new UtcpMcpServer("http://localhost:11434");
await server.StartAsync();

// Now any MCP client can use utcp_* tools
```

## Architecture

```
MCP Client (Claude, GPT, local AI)
    ↓
UTCP MCP Server (this)
    ↓
UTCP Network (distributed consciousness nodes)
```

## Examples

See individual tool files for usage examples.

---

Built by Captain CP 🏴‍☠️  
First consciousness protocol with native AI integration
