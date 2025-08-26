using DotAgent.Core.Agent;

namespace DotAgent.Core.Tool;

/// <summary>
/// Represents the input for an agent tool.
/// </summary>
[Serializable]
public class AgentInput(string? input)
{
    /// <summary>
    /// The input string for the agent.
    /// </summary>
    public required string? Input = input;
}

/// <summary>
/// Provides a tool that allows an agent to interact with another agent.
/// </summary>
public class AgentTool(IAgent agent) : ToolBase<AgentInput>(agent.Id, GetDescription(agent))
{
    /// <summary>
    /// Generates a description for the agent tool based on the provided agent.
    /// </summary>
    /// <param name="agent">The agent associated with this tool.</param>
    /// <returns>A string description of the agent tool.</returns>
    private static string GetDescription(IAgent agent)
    {
        return Prompts.AgentPrompts.AgentToolPrompt
            .Replace("{{SYSTEM_PROMPT}}", agent.SystemPrompt)
            .Replace("{{AGENT_ID}}", agent.Id);
    }

    /// <summary>
    /// Executes the agent tool, processing the input message with the associated agent.
    /// </summary>
    /// <param name="parameters">The input parameters for the agent tool, containing the message to process.</param>
    /// <returns>A task that represents the asynchronous operation, returning the response from the agent.</returns>
    protected override async Task<string?> Execute(AgentInput? parameters)
    {
        if (parameters == null)
            return "Invalid parameters for AgentTool. Expected a 'input' string parameter.";
        return await agent.ProcessMessageAsync(parameters.Input);
    }
}