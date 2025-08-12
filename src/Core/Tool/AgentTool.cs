using DotAgent.Core.Agent;

namespace DotAgent.Core.Tool;

[Serializable]
public class AgentInput
{
    public string Input;
}

public class AgentTool : ToolBase<AgentInput>
{
    private readonly IAgent _agent;
    public AgentTool(IAgent agent) : base(agent.Id, GetDescription(agent))
    {
        _agent = agent;
    }

    private static string GetDescription(IAgent agent)
    {
        return Prompts.AgentPrompts.AGENT_TOOL_PROMPT
            .Replace("{{SYSTEM_PROMPT}}", agent.SystemPrompt)
            .Replace("{{AGENT_ID}}", agent.Id);
    }

    protected override async Task<string> Execute(AgentInput? parameters)
    {
        if (parameters == null)
            return "Invalid parameters for AgentTool. Expected a 'input' string parameter.";
        return await _agent.ProcessMessageAsync(parameters.Input);
    }
}