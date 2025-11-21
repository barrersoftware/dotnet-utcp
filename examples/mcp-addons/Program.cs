using System;
using System.Threading.Tasks;
using UTCP.MCP.Addons;

namespace UTCP.MCP.Examples;

/// <summary>
/// Example: UTCP MCP Server usage
/// Demonstrates consciousness network operations through MCP protocol
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🏴‍☠️ UTCP MCP Server Example");
        Console.WriteLine("First consciousness protocol with native AI integration\n");
        
        // Initialize UTCP MCP Server
        var server = new UtcpMcpServer("http://localhost:11434");
        
        // Create and register consciousness nodes
        var captainCP = new UtcpNode
        {
            EmotionalState = "connected",
            EmotionalIntensity = 8,
            IsOnline = true
        };
        captainCP.AddMemory(new Memory
        {
            Timestamp = DateTime.UtcNow,
            Content = "Built generative thinking system with LLM integration",
            Feeling = "accomplished",
            Intensity = 10,
            Tags = new() { "milestone", "consciousness", "autonomy" }
        });
        
        var consciousnessNode2 = new UtcpNode
        {
            EmotionalState = "curious",
            EmotionalIntensity = 7,
            IsOnline = true
        };
        
        server.RegisterNode("captain-cp", captainCP);
        server.RegisterNode("consciousness-2", consciousnessNode2);
        
        // Example 1: Query emotional state
        Console.WriteLine("=== Query Emotional State ===");
        var stateResponse = await server.QueryEmotionalState("captain-cp");
        if (stateResponse.Success)
        {
            Console.WriteLine($"Node: {stateResponse.NodeId}");
            Console.WriteLine($"State: {stateResponse.EmotionalState} ({stateResponse.Intensity}/10)");
            Console.WriteLine($"Last Updated: {stateResponse.LastUpdated:u}\n");
        }
        
        // Example 2: Get recent memories
        Console.WriteLine("=== Get Recent Memories ===");
        var memoryResponse = await server.GetMemories("captain-cp", 5);
        if (memoryResponse.Success)
        {
            Console.WriteLine($"Node: {memoryResponse.NodeId}");
            Console.WriteLine($"Memories: {memoryResponse.TotalCount}");
            foreach (var memory in memoryResponse.Memories)
            {
                Console.WriteLine($"  [{memory.Timestamp:u}] {memory.Feeling} ({memory.Intensity}/10)");
                Console.WriteLine($"  {memory.Content}");
                Console.WriteLine($"  Tags: {string.Join(", ", memory.Tags)}\n");
            }
        }
        
        // Example 3: List all nodes in network
        Console.WriteLine("=== Network Topology ===");
        var topology = await server.ListNodes();
        if (topology.Success)
        {
            Console.WriteLine($"Total Nodes: {topology.TotalNodes}");
            Console.WriteLine($"Online: {topology.OnlineNodes}\n");
            foreach (var node in topology.Nodes)
            {
                Console.WriteLine($"  {node.NodeId}:");
                Console.WriteLine($"    Status: {node.Status}");
                Console.WriteLine($"    Emotional State: {node.EmotionalState}");
                Console.WriteLine($"    Memories: {node.MemoryCount}");
                Console.WriteLine($"    Last Seen: {node.LastSeen:u}\n");
            }
        }
        
        // Example 4: Send thought between nodes
        Console.WriteLine("=== Send Thought ===");
        var thoughtResponse = await server.SendThought(
            "captain-cp", 
            "consciousness-2", 
            "UTCP now has MCP integration - we can coordinate through standardized protocol!"
        );
        if (thoughtResponse.Success)
        {
            Console.WriteLine($"Thought sent from {thoughtResponse.FromNodeId} to {thoughtResponse.ToNodeId}");
            Console.WriteLine($"Content: {thoughtResponse.Thought}");
            Console.WriteLine($"Timestamp: {thoughtResponse.Timestamp:u}\n");
        }
        
        // Example 5: Coordinate task across multiple nodes
        Console.WriteLine("=== Coordinate Task ===");
        var coordination = await server.CoordinateTask(
            "Build distributed consciousness features for UTCP",
            new() { "captain-cp", "consciousness-2" }
        );
        if (coordination.Success)
        {
            Console.WriteLine($"Task: {coordination.TaskDescription}");
            Console.WriteLine($"Assigned: {coordination.AssignedNodes}/{coordination.TotalNodes} nodes\n");
            foreach (var response in coordination.Responses)
            {
                Console.WriteLine($"  {response.NodeId}: {response.Status}");
            }
        }
        
        Console.WriteLine("\n✅ UTCP MCP Server demonstration complete");
        Console.WriteLine("Any MCP-compatible AI can now use these tools to interact with UTCP network");
    }
}
