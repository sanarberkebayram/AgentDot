using DotAgent.Core.Prompts;
using DotAgent.Core.Toolkit;
using DotAgent.Interfaces;

namespace DotAgent.Core.Agent;

public abstract class AgentBase : IAgent
{
    public string Id { get; }
    public string SystemPrompt { get; set; }
    public IMemory Memory { get; }
    public IToolkit Toolkit { get; }
    
    
    protected AgentBase(string id, string systemPrompt, IMemory memory, IToolkit toolkit)
    {
        Id = id;
        Toolkit = toolkit;
        Memory = memory;
        SystemPrompt = BuildAgentPrompt(systemPrompt);
    }

    private string BuildAgentPrompt(string systemPrompt)
    {
        return AgentPrompts.AGENT_PROMPT
            .Replace("{{SYSTEM_PROMPT}}", systemPrompt)
            .Replace("{{TOOL_PROMPTS}}", Toolkit.ToolPrompt);
    }

    public abstract Task<string> ProcessMessageAsync(string message);
}