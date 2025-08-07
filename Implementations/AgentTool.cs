using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;

namespace DotAgent.Implementations;

public class AgentTool : ITool
{
    private readonly IAgent _agent;

    public string Name => _agent.Id;
    public string Description => $"Invokes the '{_agent.Id}' agent. System Prompt: '{_agent.SystemPrompt}'";

    public AgentTool(IAgent agent)
    {
        _agent = agent;
    }

    public IReadOnlyList<ToolInputParameter> GetParameters()
    {
        return new List<ToolInputParameter>
        {
            new ToolInputParameter { Name = "input", Type = "string", Description = "The input task for the agent.", IsRequired = true }
        };
    }

    public async Task<string> ExecuteAsync(JsonElement parameters)
    {
        if (parameters.TryGetProperty("input", out var inputElement) && inputElement.ValueKind == JsonValueKind.String)
        {
            var input = inputElement.GetString();
            if (input != null)
            {
                    return await _agent.ExecuteAsync(input);
            }
        }
        throw new ArgumentException("Invalid parameters for AgentTool. Expected a 'input' string parameter.");
    }
}
