namespace DotAgent.Core.Tool;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    string InputFormat { get; }
    
    protected bool ValidateInput(string parametersJson);
    Task<string?> ExecuteAsync(string parametersJson);
}
