namespace DotAgent.Interfaces;

public interface IOrchestrator : IAgent // An orchestrator is also an agent
{
    void AddAgent(IAgent agent);
}
