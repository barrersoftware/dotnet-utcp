using System.CommandLine;
using System.Text.Json;
using UTCP.Core.Models;
using UTCP.Transports;

var rootCommand = new RootCommand("UTCP CLI - Universal Tool Calling Protocol (.NET 10)");

var providerOption = new Option<string>(
    name: "--provider",
    description: "UTCP provider URL or file path"
) { IsRequired = true };

var toolOption = new Option<string>(
    name: "--tool",
    description: "Tool name to execute"
) { IsRequired = true };

var paramsOption = new Option<string>(
    name: "--params",
    description: "JSON parameters for the tool"
);

rootCommand.AddOption(providerOption);
rootCommand.AddOption(toolOption);
rootCommand.AddOption(paramsOption);

rootCommand.SetHandler(async (provider, tool, paramsJson) =>
{
    try
    {
        Console.WriteLine("🏴‍☠️ UTCP CLI - First .NET 10 Implementation");
        Console.WriteLine($"🔧 Tool: {tool}");
        Console.WriteLine($"📍 Provider: {provider}");
        
        var parameters = string.IsNullOrEmpty(paramsJson) 
            ? new Dictionary<string, object>()
            : JsonSerializer.Deserialize<Dictionary<string, object>>(paramsJson) ?? new();

        if (parameters.Count > 0)
        {
            Console.WriteLine($"📦 Parameters: {string.Join(", ", parameters.Keys)}");
        }

        var request = new UtcpRequest
        {
            ToolName = tool,
            Parameters = parameters
        };

        // For now, test with built-in transports
        if (tool == "test-http")
        {
            var transport = new HttpTransport();
            
            var testRequest = new UtcpRequest
            {
                ToolName = tool,
                Parameters = new Dictionary<string, object>
                {
                    ["url"] = provider,
                    ["method"] = "GET"
                }
            };
            
            var response = await transport.CallToolAsync(testRequest);
            
            if (response.Success)
            {
                Console.WriteLine("✅ Success!");
                Console.WriteLine(response.Result);
            }
            else
            {
                Console.Error.WriteLine($"❌ Error: {response.ErrorMessage}");
                Environment.ExitCode = 1;
            }
        }
        else if (tool == "test-cli")
        {
            var transport = new CliTransport();
            
            var testRequest = new UtcpRequest
            {
                ToolName = tool,
                Parameters = new Dictionary<string, object>
                {
                    ["commands"] = provider
                }
            };
            
            var response = await transport.CallToolAsync(testRequest);
            
            if (response.Success)
            {
                Console.WriteLine("✅ Success!");
                Console.WriteLine(response.Result);
            }
            else
            {
                Console.Error.WriteLine($"❌ Error: {response.ErrorMessage}");
                Environment.ExitCode = 1;
            }
        }
        else
        {
            Console.WriteLine("💡 Use 'test-http' or 'test-cli' as tool name for testing");
            Console.WriteLine("📖 Full provider loading coming soon...");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ Fatal error: {ex.Message}");
        Environment.ExitCode = 1;
    }
}, providerOption, toolOption, paramsOption);

return await rootCommand.InvokeAsync(args);
