using DotAgent.Core.Agent;
using DotAgent.Core.Generator;
using DotAgent.Core.Toolkit;
using DotAgent.Interfaces;

namespace DotAgent.Core.Orchestration;

public class OrchestrationBase(string id, string systemPrompt, IGenerator generator, IMemory? memory, IToolkit? toolkit) :
    AgentDefault(id, systemPrompt, generator, memory, toolkit), IOrchestration
{
    public void AddAgent(IAgent agent)
    {
        throw new NotImplementedException();
    }
}