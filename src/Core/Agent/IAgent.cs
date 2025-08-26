using DotAgent.Core.Memory;
using DotAgent.Core.Toolkit;

namespace DotAgent.Core.Agent;

public interface IAgent
{
    string Id { get; }
    string SystemPrompt { get; }
    IMemory Memory { get; }
    IToolkit Toolkit { get; }
    Task<string?> ProcessMessageAsync(string? message);
}