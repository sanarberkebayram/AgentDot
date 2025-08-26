namespace DotAgent.Core.Models;

[Serializable]
public class FunctionCall
{
    public required string Name { get; set; }
    public required string Parameters { get; set; }
    public required string Id { get; set; }
}
[Serializable]
public class GenerationResponse
{
    public FunctionCall[] FunctionCalls { get; set; } = null!;
    public string? Message { get; set; } = "";
    public string? Error { get; set; } = "";
}