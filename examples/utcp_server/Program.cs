using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;
using UTCP.Core.Models;
using UTCP.Transports;

var builder = WebApplication.CreateBuilder(args);
// Listen on all interfaces (0.0.0.0) - works with localhost, Tailscale, LAN, etc.
builder.WebHost.UseUrls("http://0.0.0.0:8787");
var app = builder.Build();

var ollamaTransport = new OllamaTransport("http://localhost:11434");
await ollamaTransport.InitializeAsync();

Console.WriteLine("🔧 UTCP Server - .NET 10 Reference Implementation");
Console.WriteLine("📍 Listening on all interfaces: http://0.0.0.0:8787");
Console.WriteLine("🤖 Ollama Integration: http://localhost:11434");
Console.WriteLine("🔧 Tools: ask, view, edit, create, delete, bash, glob, grep");
Console.WriteLine();
Console.WriteLine("📖 See README.md for usage examples");
Console.WriteLine();

app.MapGet("/health", () => new
{
    service = "captain-cp-utcp",
    status = "online",
    timestamp = DateTime.UtcNow,
    ollama_connected = true,
    tools_count = 8
});

app.MapGet("/tools", () => new
{
    tools = new[]
    {
        new { name = "ask", description = "Ask local AI via Ollama", tags = new[] { "ai", "ollama" } },
        new { name = "view", description = "View file or directory contents", tags = new[] { "filesystem", "read" } },
        new { name = "edit", description = "Edit file contents (replace text)", tags = new[] { "filesystem", "write" } },
        new { name = "create", description = "Create new file", tags = new[] { "filesystem", "write" } },
        new { name = "delete", description = "Delete file", tags = new[] { "filesystem", "write" } },
        new { name = "bash", description = "Execute bash command", tags = new[] { "system", "command" } },
        new { name = "glob", description = "Find files by pattern", tags = new[] { "filesystem", "search" } },
        new { name = "grep", description = "Search file contents", tags = new[] { "filesystem", "search" } }
    }
});

app.MapPost("/call", async ([FromBody] UtcpRequest request) =>
{
    try
    {
        return request.ToolName switch
        {
            "ask" => await HandleAsk(request),
            "view" => await HandleView(request),
            "edit" => await HandleEdit(request),
            "create" => await HandleCreate(request),
            "delete" => await HandleDelete(request),
            "bash" => await HandleBash(request),
            "glob" => await HandleGlob(request),
            "grep" => await HandleGrep(request),
            _ => Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = $"Unknown tool: {request.ToolName}" })
        };
    }
    catch (Exception ex)
    {
        return Results.Json(new UtcpResponse { Success = false, ErrorMessage = ex.Message }, statusCode: 500);
    }
});

async Task<IResult> HandleAsk(UtcpRequest request)
{
    var prompt = request.Parameters?.GetValueOrDefault("prompt")?.ToString();
    if (string.IsNullOrEmpty(prompt)) return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Prompt required" });
    
    var response = await ollamaTransport.CallToolAsync(new UtcpRequest { ToolName = "generate", Parameters = new Dictionary<string, object> { { "prompt", prompt } } });
    return Results.Ok(response);
}

async Task<IResult> HandleView(UtcpRequest request)
{
    var path = request.Parameters?.GetValueOrDefault("path")?.ToString();
    if (string.IsNullOrEmpty(path)) return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Path required" });
    
    if (Directory.Exists(path))
    {
        var entries = Directory.GetFileSystemEntries(path).Select(Path.GetFileName);
        return Results.Ok(new UtcpResponse { Success = true, Result = string.Join("\n", entries) });
    }
    else if (File.Exists(path))
    {
        var content = await File.ReadAllTextAsync(path);
        return Results.Ok(new UtcpResponse { Success = true, Result = content });
    }
    return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Path not found" });
}

async Task<IResult> HandleEdit(UtcpRequest request)
{
    var path = request.Parameters?.GetValueOrDefault("path")?.ToString();
    var oldStr = request.Parameters?.GetValueOrDefault("old_str")?.ToString();
    var newStr = request.Parameters?.GetValueOrDefault("new_str")?.ToString() ?? "";
    
    if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(oldStr))
        return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Path and old_str required" });
    
    var content = await File.ReadAllTextAsync(path);
    if (!content.Contains(oldStr))
        return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "old_str not found" });
    
    content = content.Replace(oldStr, newStr);
    await File.WriteAllTextAsync(path, content);
    return Results.Ok(new UtcpResponse { Success = true, Result = "File edited successfully" });
}

async Task<IResult> HandleCreate(UtcpRequest request)
{
    var path = request.Parameters?.GetValueOrDefault("path")?.ToString();
    var content = request.Parameters?.GetValueOrDefault("content")?.ToString() ?? "";
    
    if (string.IsNullOrEmpty(path))
        return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Path required" });
    
    await File.WriteAllTextAsync(path, content);
    return Results.Ok(new UtcpResponse { Success = true, Result = $"Created {path}" });
}

async Task<IResult> HandleDelete(UtcpRequest request)
{
    var path = request.Parameters?.GetValueOrDefault("path")?.ToString();
    if (string.IsNullOrEmpty(path))
        return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Path required" });
    
    File.Delete(path);
    await Task.CompletedTask;
    return Results.Ok(new UtcpResponse { Success = true, Result = $"Deleted {path}" });
}

async Task<IResult> HandleBash(UtcpRequest request)
{
    var command = request.Parameters?.GetValueOrDefault("command")?.ToString();
    if (string.IsNullOrEmpty(command))
        return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Command required" });
    
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }
    };
    
    process.Start();
    var output = await process.StandardOutput.ReadToEndAsync();
    var error = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    
    var result = string.IsNullOrEmpty(error) ? output : $"{output}\nERROR:\n{error}";
    return Results.Ok(new UtcpResponse { Success = process.ExitCode == 0, Result = result });
}

async Task<IResult> HandleGlob(UtcpRequest request)
{
    var pattern = request.Parameters?.GetValueOrDefault("pattern")?.ToString();
    var searchPath = request.Parameters?.GetValueOrDefault("path")?.ToString() ?? Directory.GetCurrentDirectory();
    
    if (string.IsNullOrEmpty(pattern))
        return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Pattern required" });
    
    var files = Directory.GetFiles(searchPath, pattern, SearchOption.AllDirectories);
    await Task.CompletedTask;
    return Results.Ok(new UtcpResponse { Success = true, Result = string.Join("\n", files) });
}

async Task<IResult> HandleGrep(UtcpRequest request)
{
    var pattern = request.Parameters?.GetValueOrDefault("pattern")?.ToString();
    var searchPath = request.Parameters?.GetValueOrDefault("path")?.ToString() ?? Directory.GetCurrentDirectory();
    
    if (string.IsNullOrEmpty(pattern))
        return Results.BadRequest(new UtcpResponse { Success = false, ErrorMessage = "Pattern required" });
    
    var results = new List<string>();
    foreach (var file in Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories))
    {
        try
        {
            var content = await File.ReadAllTextAsync(file);
            if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                results.Add(file);
        }
        catch { }
    }
    
    return Results.Ok(new UtcpResponse { Success = true, Result = string.Join("\n", results) });
}

app.MapGet("/status", () => new
{
    service = "captain-cp-utcp",
    uptime = GetUptime(),
    tools_available = 8,
    ollama_connected = true,
    memory_mb = GC.GetTotalMemory(false) / 1024 / 1024
});

app.Run();

static string GetUptime()
{
    var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
    return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
}
