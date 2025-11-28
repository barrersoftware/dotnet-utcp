using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UTCP.Plugins.Chain;

/// <summary>
/// Configuration for supported programming languages
/// </summary>
public record LanguageConfig
{
    public required string Command { get; init; }
    public string[]? Args { get; init; }
    public required string Extension { get; init; }
    public string[]? CompileArgs { get; init; }
    public bool NeedsCompile { get; init; }
    public bool RunCompiled { get; init; }
}

/// <summary>
/// Interface for calling UTCP tools
/// </summary>
public interface IToolCaller
{
    Task<JsonElement> CallToolAsync(string name, Dictionary<string, JsonElement> inputs, CancellationToken cancellationToken = default);
    IAsyncEnumerable<JsonElement> CallToolStreamAsync(string name, Dictionary<string, JsonElement> inputs, CancellationToken cancellationToken = default);
}

/// <summary>
/// UTCP Chain Client - executes chains of tool calls with result passing and multi-language code execution
/// </summary>
public class UtcpChainClient
{
    private readonly IToolCaller _client;
    
    private static readonly Dictionary<string, LanguageConfig> LanguageConfigs = new()
    {
        ["python"] = new() { Command = "python3", Extension = ".py" },
        ["javascript"] = new() { Command = "node", Extension = ".js" },
        ["go"] = new() { Command = "go", Args = new[] { "run" }, Extension = ".go" },
        ["rust"] = new() { Command = "rustc", Extension = ".rs", NeedsCompile = true, RunCompiled = true },
        ["java"] = new() { Command = "javac", Extension = ".java", NeedsCompile = true, RunCompiled = true },
        ["c"] = new() { Command = "gcc", CompileArgs = new[] { "-o" }, Extension = ".c", NeedsCompile = true, RunCompiled = true },
        ["cpp"] = new() { Command = "g++", CompileArgs = new[] { "-o" }, Extension = ".cpp", NeedsCompile = true, RunCompiled = true },
        ["bash"] = new() { Command = "bash", Extension = ".sh" },
        ["shell"] = new() { Command = "sh", Extension = ".sh" },
        ["typescript"] = new() { Command = "ts-node", Extension = ".ts" },
        ["perl"] = new() { Command = "perl", Extension = ".pl" },
        ["ruby"] = new() { Command = "ruby", Extension = ".rb" },
        ["php"] = new() { Command = "php", Extension = ".php" },
        ["r"] = new() { Command = "Rscript", Extension = ".R" },
        ["lua"] = new() { Command = "lua", Extension = ".lua" },
        ["elixir"] = new() { Command = "elixir", Extension = ".exs" },
        ["csharp"] = new() { Command = "dotnet", Args = new[] { "script" }, Extension = ".csx" }
    };
    
    private static readonly Regex[] ServerRegexes = new[]
    {
        new Regex(@"(?i)listening"),
        new Regex(@"(?i)running\s+on\s+port"),
        new Regex(@"(?i)http://"),
        new Regex(@"(?i)localhost:\d+"),
        new Regex(@"(?i):\d{4,5}")
    };
    
    public UtcpChainClient(IToolCaller client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }
    
    /// <summary>
    /// Execute a chain of steps with optional result passing and streaming support
    /// </summary>
    public async Task<Dictionary<string, JsonElement>> CallToolChainAsync(
        List<ChainStep> steps,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromSeconds(30);
        
        var results = new Dictionary<string, JsonElement>();
        var lastOutput = string.Empty;
        
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout.Value);
        
        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var stepInputs = new Dictionary<string, JsonElement>(step.Inputs ?? new());
            
            // Merge previous results if requested
            if (step.UsePrevious)
            {
                foreach (var (key, value) in results)
                {
                    if (!stepInputs.ContainsKey(key))
                    {
                        stepInputs[key] = value;
                    }
                }
                
                if (!string.IsNullOrEmpty(lastOutput))
                {
                    stepInputs["__previous_output"] = JsonSerializer.SerializeToElement(lastOutput);
                }
            }
            
            JsonElement result;
            
            if (step.Stream)
            {
                var items = new List<JsonElement>();
                await foreach (var item in _client.CallToolStreamAsync(step.ToolName, stepInputs, cts.Token))
                {
                    items.Add(item);
                }
                result = JsonSerializer.SerializeToElement(items);
            }
            else
            {
                result = await _client.CallToolAsync(step.ToolName, stepInputs, cts.Token);
            }
            
            // Store result
            var stepId = step.Id ?? $"step_{i}";
            results[stepId] = result;
            
            // Update last output for chaining
            if (result.ValueKind == JsonValueKind.String)
            {
                lastOutput = result.GetString() ?? string.Empty;
            }
            else
            {
                lastOutput = result.ToString();
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Execute code in a specified language
    /// </summary>
    public async Task<string> ExecuteCodeAsync(
        string language,
        string code,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (!LanguageConfigs.TryGetValue(language.ToLowerInvariant(), out var config))
        {
            throw new NotSupportedException($"Language '{language}' is not supported");
        }
        
        timeout ??= TimeSpan.FromSeconds(30);
        
        var tempDir = Path.Combine(Path.GetTempPath(), $"utcp_chain_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var sourceFile = Path.Combine(tempDir, $"script{config.Extension}");
            await File.WriteAllTextAsync(sourceFile, code, cancellationToken);
            
            string outputFile = sourceFile;
            
            // Compile if needed
            if (config.NeedsCompile)
            {
                outputFile = Path.Combine(tempDir, "output");
                var compileArgs = new List<string>();
                
                if (config.CompileArgs != null)
                {
                    compileArgs.AddRange(config.CompileArgs);
                    compileArgs.Add(outputFile);
                }
                
                compileArgs.Add(sourceFile);
                
                var compileResult = await RunProcessAsync(
                    config.Command,
                    compileArgs.ToArray(),
                    tempDir,
                    timeout.Value,
                    cancellationToken);
                
                if (compileResult.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Compilation failed: {compileResult.Error}");
                }
            }
            
            // Run the code
            string command;
            string[] args;
            
            if (config.RunCompiled)
            {
                command = outputFile;
                args = Array.Empty<string>();
            }
            else
            {
                command = config.Command;
                args = config.Args != null 
                    ? config.Args.Concat(new[] { sourceFile }).ToArray()
                    : new[] { sourceFile };
            }
            
            var result = await RunProcessAsync(command, args, tempDir, timeout.Value, cancellationToken);
            
            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"Execution failed: {result.Error}");
            }
            
            return result.Output;
        }
        finally
        {
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
    
    private static async Task<ProcessResult> RunProcessAsync(
        string command,
        string[] args,
        string workingDirectory,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }
        
        using var process = new Process { StartInfo = psi };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        
        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        
        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
            }
        };
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        
        await process.WaitForExitAsync(cts.Token);
        
        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Output = outputBuilder.ToString(),
            Error = errorBuilder.ToString()
        };
    }
    
    private static bool LooksLikeServerOutput(string output)
    {
        return ServerRegexes.Any(regex => regex.IsMatch(output));
    }
    
    private record ProcessResult
    {
        public required int ExitCode { get; init; }
        public required string Output { get; init; }
        public required string Error { get; init; }
    }
}
