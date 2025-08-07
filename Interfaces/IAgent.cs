using DotAgent.Core.Tool;

namespace DotAgent.Interfaces;

public interface IAgent
{
    string Id { get; }
    string SystemPrompt { get; }
    IMemory Memory { get; }
    IReadOnlyList<ITool> Tools { get; }
    Task<string> ExecuteAsync(string input);
}