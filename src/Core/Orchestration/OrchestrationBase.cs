using DotAgent.Core.Agent;
using DotAgent.Core.Generator;
using DotAgent.Core.Memory;
using DotAgent.Core.Tool;
using DotAgent.Core.Toolkit;

namespace DotAgent.Core.Orchestration
{
    /// <summary>
    /// Provides a base implementation for orchestration, extending <see cref="AgentDefault"/>
    /// to manage and interact with multiple agents as tools.
    /// </summary>
    public class OrchestrationBase : AgentDefault, IOrchestration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationBase"/> class with specified parameters.
        /// </summary>
        /// <param name="id">The unique identifier for the orchestration agent.</param>
        /// <param name="systemPrompt">The initial system prompt for the orchestration agent.</param>
        /// <param name="generator">The generator used by the orchestration agent.</param>
        /// <param name="memory">The memory component for storing conversation history.</param>
        /// <param name="toolkit">The toolkit providing tools for the orchestration agent.</param>
        public OrchestrationBase(string id, string? systemPrompt, IGenerator generator, IMemory? memory, IToolkit? toolkit)
         : base(id, systemPrompt, generator, memory, toolkit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationBase"/> class with a new GUID as ID and a specified system prompt.
        /// </summary>
        /// <param name="systemPrompt">The initial system prompt for the orchestration agent.</param>
        /// <param name="generator">The generator used by the orchestration agent.</param>
        public OrchestrationBase(string? systemPrompt, IGenerator generator)
            : base(Guid.NewGuid().ToString(), systemPrompt, generator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationBase"/> class with a new GUID as ID and the default orchestration prompt.
        /// </summary>
        /// <param name="generator">The generator used by the orchestration agent.</param>
        public OrchestrationBase(IGenerator generator)
            : base(Guid.NewGuid().ToString(), Prompts.OrchestrationPrompt.OrcPrompt, generator)
        {
        }

        /// <summary>
        /// Adds an agent to the orchestration's toolkit, making it available as a tool.
        /// </summary>
        /// <param name="agent">The agent to add.</param>
        public void AddAgent(IAgent agent)
        {
            Toolkit.AddTool(new AgentTool(agent));
        }
    }
}

