using System.Text.Json;
using UTCP.Plugins.CodeMode;
using UTCP.Core.Models;
using Xunit;

namespace UTCP.Plugins.Tests;

public class CodeModePluginTests
{
    [Fact]
    public async Task Execute_JsonPayload_ReturnsDirectly()
    {
        // Arrange
        var mockClient = new MockUtcpClient();
        var codeMode = new CodeModeUtcp(mockClient);
        var jsonCode = """{"result": "test"}""";
        
        var args = new CodeModeArgs { Code = jsonCode };
        
        // Act
        var result = await codeMode.ExecuteAsync(args);
        
        // Assert
        Assert.Equal(JsonValueKind.Object, result.Value.ValueKind);
        Assert.Equal("test", result.Value.GetProperty("result").GetString());
    }
    
    [Fact]
    public async Task Execute_CSharpExpression_EvaluatesCorrectly()
    {
        // Arrange
        var mockClient = new MockUtcpClient();
        var codeMode = new CodeModeUtcp(mockClient);
        
        var args = new CodeModeArgs { Code = "2 + 2" };
        
        // Act
        var result = await codeMode.ExecuteAsync(args);
        
        // Assert
        Assert.Equal(JsonValueKind.Number, result.Value.ValueKind);
        Assert.Equal(4, result.Value.GetInt32());
    }
    
    [Fact]
    public async Task CallTool_InvokesClient()
    {
        // Arrange
        var mockClient = new MockUtcpClient();
        var codeMode = new CodeModeUtcp(mockClient);
        
        var inputs = new Dictionary<string, JsonElement>
        {
            ["test"] = JsonSerializer.SerializeToElement("value")
        };
        
        // Act
        await codeMode.CallToolAsync("test.tool", inputs);
        
        // Assert
        Assert.Equal(1, mockClient.CallToolCount);
    }
    
    [Fact]
    public async Task CallToolStream_CollectsAllChunks()
    {
        // Arrange
        var mockClient = new MockUtcpClient();
        var codeMode = new CodeModeUtcp(mockClient);
        
        var inputs = new Dictionary<string, JsonElement>();
        
        // Act
        var chunks = new List<JsonElement>();
        await foreach (var chunk in codeMode.CallToolStreamAsync("test.stream", inputs))
        {
            chunks.Add(chunk);
        }
        
        // Assert
        Assert.Equal(3, chunks.Count);
    }
}

public class MockUtcpClient : IUtcpClient
{
    public int CallToolCount { get; private set; }
    public int SearchToolsCount { get; private set; }
    
    public Task<JsonElement> CallToolAsync(string name, Dictionary<string, JsonElement> args, CancellationToken cancellationToken = default)
    {
        CallToolCount++;
        var result = JsonSerializer.SerializeToElement(new { called = name, args });
        return Task.FromResult(result);
    }
    
    public async IAsyncEnumerable<JsonElement> CallToolStreamAsync(string name, Dictionary<string, JsonElement> args, CancellationToken cancellationToken = default)
    {
        yield return JsonSerializer.SerializeToElement("chunk1");
        yield return JsonSerializer.SerializeToElement("chunk2");
        yield return JsonSerializer.SerializeToElement("chunk3");
        await Task.CompletedTask;
    }
    
    public Task<List<UtcpTool>> SearchToolsAsync(string query, int limit, CancellationToken cancellationToken = default)
    {
        SearchToolsCount++;
        var tools = new List<UtcpTool>
        {
            new() { Name = "test.tool", Description = "A test tool", Transport = "http", Provider = "test" }
        };
        return Task.FromResult(tools);
    }
}
