using System.Text.Json;
using UTCP.Plugins.Chain;
using Xunit;

namespace UTCP.Plugins.Tests;

public class ChainPluginTests
{
    [Fact]
    public async Task ChainStep_CanExecuteSequentialSteps()
    {
        // Arrange
        var mockClient = new MockToolCaller();
        var chainClient = new UtcpChainClient(mockClient);
        
        var steps = new List<ChainStep>
        {
            new() { Id = "step1", ToolName = "test.echo", Inputs = new() { ["message"] = JsonSerializer.SerializeToElement("hello") } },
            new() { Id = "step2", ToolName = "test.echo", Inputs = new() { ["message"] = JsonSerializer.SerializeToElement("world") } }
        };
        
        // Act
        var results = await chainClient.CallToolChainAsync(steps);
        
        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results.ContainsKey("step1"));
        Assert.True(results.ContainsKey("step2"));
    }
    
    [Fact]
    public async Task ChainStep_UsePreviousPassesResults()
    {
        // Arrange
        var mockClient = new MockToolCaller();
        var chainClient = new UtcpChainClient(mockClient);
        
        var steps = new List<ChainStep>
        {
            new() { Id = "step1", ToolName = "test.echo", Inputs = new() { ["message"] = JsonSerializer.SerializeToElement("data") } },
            new() { Id = "step2", ToolName = "test.echo", UsePrevious = true }
        };
        
        // Act
        var results = await chainClient.CallToolChainAsync(steps);
        
        // Assert
        Assert.Equal(2, mockClient.CallCount);
        // UsePrevious should merge previous step results
        Assert.True(mockClient.LastInputs.ContainsKey("step1") || mockClient.LastInputs.ContainsKey("__previous_output"));
    }
    
    [Fact]
    public async Task ExecuteCode_Python_ReturnsOutput()
    {
        // Arrange
        var mockClient = new MockToolCaller();
        var chainClient = new UtcpChainClient(mockClient);
        
        var pythonCode = "print('Hello from Python')";
        
        // Act
        var output = await chainClient.ExecuteCodeAsync("python", pythonCode);
        
        // Assert
        Assert.Contains("Hello from Python", output);
    }
    
    [Fact]
    public async Task ExecuteCode_UnsupportedLanguage_ThrowsException()
    {
        // Arrange
        var mockClient = new MockToolCaller();
        var chainClient = new UtcpChainClient(mockClient);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            async () => await chainClient.ExecuteCodeAsync("cobol", "DISPLAY 'Hello'"));
    }
}

public class MockToolCaller : IToolCaller
{
    public int CallCount { get; private set; }
    public Dictionary<string, JsonElement> LastInputs { get; private set; } = new();
    
    public Task<JsonElement> CallToolAsync(string name, Dictionary<string, JsonElement> inputs, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastInputs = inputs;
        
        // Echo back the input
        var result = JsonSerializer.SerializeToElement(new { echo = inputs });
        return Task.FromResult(result);
    }
    
    public async IAsyncEnumerable<JsonElement> CallToolStreamAsync(string name, Dictionary<string, JsonElement> inputs, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastInputs = inputs;
        
        yield return JsonSerializer.SerializeToElement("chunk1");
        yield return JsonSerializer.SerializeToElement("chunk2");
        await Task.CompletedTask;
    }
}
