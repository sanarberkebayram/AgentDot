using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;

namespace DotAgent.Implementations;

public class ConsoleLogger : ITool
{
    public string Name => "ConsoleLogger";
    public string Description => "Logs messages to the console.";

    public IReadOnlyList<ToolInputParameter> GetParameters()
    {
        return new List<ToolInputParameter>
        {
            new ToolInputParameter { Name = "message", Type = "string", Description = "The message to log to the console.", IsRequired = true }
        };
    }

    public Task<string> ExecuteAsync(JsonElement parameters)
    {
        if (!parameters.TryGetProperty("message", out var messageElement) || messageElement.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException("Missing or invalid 'message' parameter.");
        }
        var message = messageElement.GetString();
        Console.WriteLine($"[ConsoleLogger]: {message}");
        return Task.FromResult("Message logged to console.");
    }
}
