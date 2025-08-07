using System.Text.Json;

namespace DotAgent.Models;

public class PlanStep { public string? Rationale { get; set; }  public string? Prompt { get; set; } }
public class ToolCall { public string? ToolName { get; set; } public string ParametersJson { get; set; } public string? Rationale { get; set; }}

public class Plan
{
    public string? Goal { get; set; }
    public List<PlanStep> Steps { get; set; } = new();
}
