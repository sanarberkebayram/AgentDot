using System.Text.Json;

namespace DotAgent.Models;

public class ToolCallResult
{
    public string? ToolName { get; set; }
    public JsonElement Parameters { get; set; }
    public string? ThoughtProcess { get; set; } // The LLM's reasoning for choosing this tool.
}
