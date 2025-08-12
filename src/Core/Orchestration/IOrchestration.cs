using DotAgent.Core.Agent;

namespace DotAgent.Core.Orchestration;

public interface IOrchestration : IAgent
{
    void AddAgent(IAgent agent);
}