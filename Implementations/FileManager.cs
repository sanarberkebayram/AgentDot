using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;

namespace DotAgent.Implementations;

public class FileManager : ITool
{
    public string Name => "FileManager";
    public string Description => "Reads and writes files to the current folder." +
                                 " Use 'read' action to read content from a file, and 'write' action to write content to a file." +
                                 " For 'write' action, 'content' parameter is required. " +
                                 " Path of the target should be exact, asterisk is prohibited" +
                                 " Can not create folders. ";

    public IReadOnlyList<ToolInputParameter> GetParameters()
    {
        return new List<ToolInputParameter>
        {
            new ToolInputParameter { Name = "action", Type = "string", Description = "The action to perform: 'read' or 'write'.", IsRequired = true },
            new ToolInputParameter { Name = "filePath", Type = "string", Description = "The path to the file. It can not be './**'. Asterisk and multiple selection is prohibited", IsRequired = true },
            new ToolInputParameter { Name = "content", Type = "string", Description = "The content to write to the file (required for 'write' action).", IsRequired = false }
        };
    }

    public async Task<string> ExecuteAsync(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("action", out var actionElement) || actionElement.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException("Missing or invalid 'action' parameter. Expected 'read' or 'write'.");
        }
        var action = actionElement.GetString();

        if (!parameters.TryGetProperty("filePath", out var filePathElement) || filePathElement.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException("Missing or invalid 'filePath' parameter.");
        }
        var filePath = filePathElement.GetString();

        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be empty.");
        }
        
        // Check directory exists
        var dirName = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dirName) &&  !Directory.Exists(Path.GetDirectoryName(filePath)))
            Directory.CreateDirectory(dirName);
            

        switch (action?.ToLower())
        {
            case "read":
                if (!File.Exists(filePath))
                {
                    return $"Error: File not found at {filePath}";
                }
                return await File.ReadAllTextAsync(filePath);
            case "write":
                if (!parameters.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.String)
                {
                    throw new ArgumentException("Missing or invalid 'content' parameter for 'write' action.");
                }
                var content = contentElement.GetString();
                await File.WriteAllTextAsync(filePath, content);
                return $"Successfully wrote to {filePath}";
            default:
                throw new ArgumentException($"Unsupported action: {action}. Expected 'read' or 'write'.");
        }
    }
}
