using DotAgent.Core.Agent;

namespace DotAgent.Core.Tool;

[Serializable]
public class AgentInput(string? input)
{
    public required string? Input = input;
}

public class AgentTool(IAgent agent) : ToolBase<AgentInput>(agent.Id, GetDescription(agent))
{
    private static string GetDescription(IAgent agent)
    {
        return Prompts.AgentPrompts.AgentToolPrompt
            .Replace("{{SYSTEM_PROMPT}}", agent.SystemPrompt)
            .Replace("{{AGENT_ID}}", agent.Id);
    }

    protected override async Task<string?> Execute(AgentInput? parameters)
    {
        if (parameters == null)
            return "Invalid parameters for AgentTool. Expected a 'input' string parameter.";
        return await agent.ProcessMessageAsync(parameters.Input);
    }
}