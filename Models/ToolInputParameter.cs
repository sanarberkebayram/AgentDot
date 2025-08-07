namespace DotAgent.Models;

public class ToolInputParameter
{
    public string? Name { get; set; }
    public string? Type { get; set; } // e.g., "string", "number", "boolean"
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
}
