using System.Text.Json;

namespace DotAgent.Models;

public abstract class PlanStep { public string? Rationale { get; set; } }
public class ToolStep : PlanStep { public string? ToolName { get; set; } public JsonElement Parameters { get; set; } }
public class TextGenerationStep : PlanStep { public string? Prompt { get; set; } }

public class Plan
{
    public string? Goal { get; set; }
    public List<PlanStep> Steps { get; set; } = new();
}
