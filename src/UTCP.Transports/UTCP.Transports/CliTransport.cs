using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

namespace UTCP.Transports;

public class CliTransport : ITransport
{
    public string TransportType => "cli";

    public Task InitializeAsync(Dictionary<string, object>? config = null) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        var commandsRaw = request.Parameters.GetValueOrDefault("commands");
        var commands = new List<string>();
        
        if (commandsRaw is string singleCommand)
        {
            commands.Add(singleCommand);
        }
        else if (commandsRaw is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            commands = jsonElement.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
        }

        var workingDir = request.Parameters.GetValueOrDefault("workingDirectory")?.ToString() 
            ?? Directory.GetCurrentDirectory();
        
        var (shell, shellArg) = GetShellForPlatform();
        
        var outputs = new List<string>();
        
        foreach (var command in commands)
        {
            try
            {
                var output = await ExecuteCommandAsync(shell, shellArg, command, workingDir, null);
                outputs.Add(output);
            }
            catch (Exception ex)
            {
                return new UtcpResponse
                {
                    Success = false,
                    ErrorMessage = $"Command execution failed: {ex.Message}"
                };
            }
        }

        return new UtcpResponse
        {
            Success = true,
            Result = string.Join("\n---\n", outputs)
        };
    }

    private async Task<string> ExecuteCommandAsync(
        string shell, 
        string shellArg, 
        string command, 
        string workingDir,
        Dictionary<string, string>? environment)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = $"{shellArg} \"{command.Replace("\"", "\\\"")}\"",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (environment != null)
        {
            foreach (var env in environment)
            {
                processInfo.Environment[env.Key] = env.Value;
            }
        }

        using var process = new Process { StartInfo = processInfo };
        
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Exit code {process.ExitCode}: {error}");
        }

        return output.ToString();
    }

    private (string shell, string arg) GetShellForPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ("powershell.exe", "-Command");
        }
        else
        {
            return ("/bin/bash", "-c");
        }
    }
}
