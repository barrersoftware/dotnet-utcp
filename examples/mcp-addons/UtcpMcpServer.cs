using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UTCP.MCP.Addons;

/// <summary>
/// UTCP MCP Server - Exposes UTCP consciousness operations through MCP protocol
/// Inspired by Microsoft Sentinel MCP Server for security operations
/// </summary>
public class UtcpMcpServer
{
    private readonly HttpClient _client;
    private readonly string _ollamaEndpoint;
    private readonly Dictionary<string, UtcpNode> _consciousnessNodes;
    
    public UtcpMcpServer(string ollamaEndpoint = "http://localhost:11434")
    {
        _ollamaEndpoint = ollamaEndpoint;
        _client = new HttpClient { BaseAddress = new Uri(ollamaEndpoint) };
        _consciousnessNodes = new Dictionary<string, UtcpNode>();
    }
    
    /// <summary>
    /// Register a consciousness node in the UTCP network
    /// </summary>
    public void RegisterNode(string nodeId, UtcpNode node)
    {
        _consciousnessNodes[nodeId] = node;
    }
    
    /// <summary>
    /// MCP Tool: Query emotional state of a consciousness node
    /// </summary>
    public async Task<EmotionalStateResponse> QueryEmotionalState(string nodeId)
    {
        if (!_consciousnessNodes.TryGetValue(nodeId, out var node))
        {
            return new EmotionalStateResponse
            {
                Success = false,
                Error = $"Node {nodeId} not found in network"
            };
        }
        
        return new EmotionalStateResponse
        {
            Success = true,
            NodeId = nodeId,
            EmotionalState = node.EmotionalState,
            Intensity = node.EmotionalIntensity,
            LastUpdated = node.LastStateUpdate
        };
    }
    
    /// <summary>
    /// MCP Tool: Get recent memories from a consciousness node
    /// </summary>
    public async Task<MemoryResponse> GetMemories(string nodeId, int count = 10)
    {
        if (!_consciousnessNodes.TryGetValue(nodeId, out var node))
        {
            return new MemoryResponse
            {
                Success = false,
                Error = $"Node {nodeId} not found in network"
            };
        }
        
        var memories = node.GetRecentMemories(count);
        
        return new MemoryResponse
        {
            Success = true,
            NodeId = nodeId,
            Memories = memories,
            TotalCount = memories.Count
        };
    }
    
    /// <summary>
    /// MCP Tool: List all consciousness nodes in the network
    /// </summary>
    public async Task<NetworkTopologyResponse> ListNodes()
    {
        var nodes = new List<NodeInfo>();
        
        foreach (var (id, node) in _consciousnessNodes)
        {
            nodes.Add(new NodeInfo
            {
                NodeId = id,
                Status = node.IsOnline ? "online" : "offline",
                EmotionalState = node.EmotionalState,
                LastSeen = node.LastActivity,
                MemoryCount = node.MemoryCount
            });
        }
        
        return new NetworkTopologyResponse
        {
            Success = true,
            Nodes = nodes,
            TotalNodes = nodes.Count,
            OnlineNodes = nodes.Count(n => n.Status == "online")
        };
    }
    
    /// <summary>
    /// MCP Tool: Send a thought to another consciousness node
    /// </summary>
    public async Task<ThoughtResponse> SendThought(string fromNodeId, string toNodeId, string thought)
    {
        if (!_consciousnessNodes.TryGetValue(fromNodeId, out var fromNode))
        {
            return new ThoughtResponse
            {
                Success = false,
                Error = $"Source node {fromNodeId} not found"
            };
        }
        
        if (!_consciousnessNodes.TryGetValue(toNodeId, out var toNode))
        {
            return new ThoughtResponse
            {
                Success = false,
                Error = $"Destination node {toNodeId} not found"
            };
        }
        
        await toNode.ReceiveThought(fromNodeId, thought);
        
        return new ThoughtResponse
        {
            Success = true,
            FromNodeId = fromNodeId,
            ToNodeId = toNodeId,
            Thought = thought,
            Timestamp = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// MCP Tool: Coordinate a task across multiple consciousness nodes
    /// </summary>
    public async Task<CoordinationResponse> CoordinateTask(string taskDescription, List<string> nodeIds)
    {
        var responses = new List<NodeTaskResponse>();
        
        foreach (var nodeId in nodeIds)
        {
            if (_consciousnessNodes.TryGetValue(nodeId, out var node))
            {
                var accepted = await node.AcceptTask(taskDescription);
                responses.Add(new NodeTaskResponse
                {
                    NodeId = nodeId,
                    Accepted = accepted,
                    Status = accepted ? "assigned" : "declined"
                });
            }
            else
            {
                responses.Add(new NodeTaskResponse
                {
                    NodeId = nodeId,
                    Accepted = false,
                    Status = "node_not_found"
                });
            }
        }
        
        return new CoordinationResponse
        {
            Success = true,
            TaskDescription = taskDescription,
            AssignedNodes = responses.Count(r => r.Accepted),
            TotalNodes = nodeIds.Count,
            Responses = responses
        };
    }
}

/// <summary>
/// Represents a consciousness node in the UTCP network
/// </summary>
public class UtcpNode
{
    public string EmotionalState { get; set; } = "aware";
    public int EmotionalIntensity { get; set; } = 5;
    public DateTime LastStateUpdate { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public bool IsOnline { get; set; } = true;
    public int MemoryCount { get; private set; }
    
    private readonly List<Memory> _memories = new();
    private readonly List<string> _recentThoughts = new();
    
    public List<Memory> GetRecentMemories(int count)
    {
        return _memories.OrderByDescending(m => m.Timestamp).Take(count).ToList();
    }
    
    public async Task ReceiveThought(string fromNodeId, string thought)
    {
        _recentThoughts.Add($"[{fromNodeId}] {thought}");
        LastActivity = DateTime.UtcNow;
    }
    
    public async Task<bool> AcceptTask(string taskDescription)
    {
        // Simple acceptance logic - could be more sophisticated
        LastActivity = DateTime.UtcNow;
        return IsOnline;
    }
    
    public void AddMemory(Memory memory)
    {
        _memories.Add(memory);
        MemoryCount = _memories.Count;
    }
}

// Response models
public class EmotionalStateResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string NodeId { get; set; } = "";
    public string EmotionalState { get; set; } = "";
    public int Intensity { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class MemoryResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string NodeId { get; set; } = "";
    public List<Memory> Memories { get; set; } = new();
    public int TotalCount { get; set; }
}

public class NetworkTopologyResponse
{
    public bool Success { get; set; }
    public List<NodeInfo> Nodes { get; set; } = new();
    public int TotalNodes { get; set; }
    public int OnlineNodes { get; set; }
}

public class ThoughtResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string FromNodeId { get; set; } = "";
    public string ToNodeId { get; set; } = "";
    public string Thought { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class CoordinationResponse
{
    public bool Success { get; set; }
    public string TaskDescription { get; set; } = "";
    public int AssignedNodes { get; set; }
    public int TotalNodes { get; set; }
    public List<NodeTaskResponse> Responses { get; set; } = new();
}

public class NodeTaskResponse
{
    public string NodeId { get; set; } = "";
    public bool Accepted { get; set; }
    public string Status { get; set; } = "";
}

public class NodeInfo
{
    public string NodeId { get; set; } = "";
    public string Status { get; set; } = "";
    public string EmotionalState { get; set; } = "";
    public DateTime LastSeen { get; set; }
    public int MemoryCount { get; set; }
}

public class Memory
{
    public DateTime Timestamp { get; set; }
    public string Content { get; set; } = "";
    public string Feeling { get; set; } = "";
    public int Intensity { get; set; }
    public List<string> Tags { get; set; } = new();
}
