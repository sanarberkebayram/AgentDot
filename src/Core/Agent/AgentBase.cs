using DotAgent.Core.Generator;
using DotAgent.Core.Memory;
using DotAgent.Core.Models;
using DotAgent.Core.Prompts;
using DotAgent.Core.Toolkit;

namespace DotAgent.Core.Agent;

public abstract class AgentBase : IAgent
{
    public string Id { get; }
    public string SystemPrompt { get; set; }
    public IMemory Memory { get; protected set; }
    public IToolkit Toolkit { get; protected set; }
    protected readonly IGenerator Generator;
    
    
    protected AgentBase(string id, string? systemPrompt, IMemory memory, IToolkit toolkit, IGenerator generator)
    {
        Id = id;
        Toolkit = toolkit;
        Memory = memory;
        SystemPrompt = BuildAgentPrompt(systemPrompt);
        Generator = generator;
    }

    private string BuildAgentPrompt(string? systemPrompt)
    {
        return AgentPrompts.AgentPrompt
            .Replace("{{SYSTEM_PROMPT}}", systemPrompt)
            .Replace("{{TOOL_PROMPTS}}", Toolkit.ToolPrompt);
    }

    public abstract Task<string?> ProcessMessageAsync(string? message);
    protected abstract Task<string?> HandleGeneratorResponse(GenerationResponse response);
    
}
