using DotAgent.Core.Agent;
using DotAgent.Core.Generator;
using DotAgent.Core.Memory;
using DotAgent.Core.Tool;
using DotAgent.Core.Toolkit;

namespace DotAgent.Core.Orchestration;

public class OrchestrationBase : AgentDefault, IOrchestration
{
    public OrchestrationBase(string id, string? systemPrompt, IGenerator generator, IMemory? memory, IToolkit? toolkit)
     : base(id, systemPrompt, generator, memory, toolkit)
    {
    }

    public OrchestrationBase(string? systemPrompt, IGenerator generator)
        : base(Guid.NewGuid().ToString(), systemPrompt, generator)
    {
    }

    public OrchestrationBase(IGenerator generator)
        : base(Guid.NewGuid().ToString(), Prompts.OrchestrationPrompt.OrcPrompt, generator)
    {
    }

    public void AddAgent(IAgent agent)
    {
        Toolkit.AddTool(new AgentTool(agent));
    }
}
