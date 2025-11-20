namespace UTCP.Transports;

using System.Text.Json;
using UTCP.Core.Interfaces;
using UTCP.Core.Models;

public class TextTransport : ITransport, IAsyncDisposable
{
    private string? _rootDirectory;
    private bool _isInitialized;

    public string TransportType => "text";

    public Task InitializeAsync(Dictionary<string, object>? config = null)
    {
        if (config?.TryGetValue("root_directory", out var rootDir) == true)
        {
            _rootDirectory = rootDir.ToString();
        }
        else
        {
            _rootDirectory = Directory.GetCurrentDirectory();
        }
        
        _isInitialized = true;
        return Task.CompletedTask;
    }

    public async Task<UtcpResponse> CallToolAsync(UtcpRequest request, CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            return CreateErrorResponse("Transport not initialized", "NOT_INITIALIZED", request.RequestId);
        }

        try
        {
            if (request.Parameters == null || !request.Parameters.TryGetValue("_callTemplate", out var templateObj))
            {
                return CreateErrorResponse("Text call template not provided", "MISSING_TEMPLATE", request.RequestId);
            }

            var template = templateObj as TextCallTemplate
                ?? throw new InvalidOperationException("Invalid call template");

            // Resolve file path (relative to root directory if not absolute)
            var filePath = template.FilePath;
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(_rootDirectory!, filePath);
            }

            // Check if file exists
            if (!File.Exists(filePath))
            {
                return CreateErrorResponse($"File not found: {filePath}", "FILE_NOT_FOUND", request.RequestId);
            }

            // Read file content
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);

            // Try to parse as JSON (for UTCP manuals or OpenAPI specs)
            object result;
            try
            {
                result = JsonSerializer.Deserialize<object>(content) ?? content;
            }
            catch
            {
                // If not JSON, return as plain text
                result = content;
            }

            return new UtcpResponse
            {
                Success = true,
                Result = result,
                RequestId = request.RequestId
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(ex.Message, "TRANSPORT_ERROR", request.RequestId);
        }
    }

    private static UtcpResponse CreateErrorResponse(string message, string code, string? requestId)
    {
        return new UtcpResponse
        {
            Success = false,
            ErrorMessage = message,
            ErrorCode = code,
            RequestId = requestId
        };
    }

    public ValueTask DisposeAsync()
    {
        // Nothing to dispose for file reading
        return ValueTask.CompletedTask;
    }
}
