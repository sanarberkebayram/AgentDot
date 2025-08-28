using DotAgent.Core.Generator;
using DotAgent.Core.Memory;
using DotAgent.Core.Models;
using DotAgent.Core.Prompts;
using DotAgent.Core.Toolkit;

namespace DotAgent.Core.Agent
{
    /// <summary>
    /// Provides a base implementation for an agent, defining its core properties and abstract methods for message processing.
    /// </summary>
    public abstract class AgentBase : IAgent
    {
        /// <summary>
        /// Gets the unique identifier for the agent.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the system prompt associated with the agent.
        /// </summary>
        public string SystemPrompt { get; set; }

        /// <summary>
        /// Gets the memory component used by the agent to store conversation history.
        /// </summary>
        public IMemory Memory { get; protected set; }

        /// <summary>
        /// Gets the toolkit providing tools for the agent to use.
        /// </summary>
        public IToolkit Toolkit { get; protected set; }

        /// <summary>
        /// The generator used by the agent to produce responses.
        /// </summary>
        protected readonly IGenerator Generator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentBase"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the agent.</param>
        /// <param name="systemPrompt">The initial system prompt for the agent.</param>
        /// <param name="memory">The memory component for storing conversation history.</param>
        /// <param name="toolkit">The toolkit providing tools for the agent to use.</param>
        /// <param name="generator">The generator used by the agent to produce responses.</param>
        protected AgentBase(string id, string? systemPrompt, IMemory memory, IToolkit toolkit, IGenerator generator)
        {
            Id = id;
            Toolkit = toolkit;
            Memory = memory;
            SystemPrompt = BuildAgentPrompt(systemPrompt);
            Generator = generator;
        }

        /// <summary>
        /// Builds the agent's system prompt by incorporating the provided system prompt and tool prompts from the toolkit.
        /// </summary>
        /// <param name="systemPrompt">The base system prompt.</param>
        /// <returns>The complete system prompt for the agent.</returns>
        private string BuildAgentPrompt(string? systemPrompt)
        {
            return AgentPrompts.AgentPrompt
                .Replace("{{SYSTEM_PROMPT}}", systemPrompt)
                .Replace("{{TOOL_PROMPTS}}", Toolkit.ToolPrompt);
        }

        /// <summary>
        /// Processes an incoming message asynchronously.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>A task that represents the asynchronous operation, returning the agent's response message.</returns>
        public abstract Task<string?> ProcessMessageAsync(string? message);

        /// <summary>
        /// Handles the response received from the generator.
        /// </summary>
        /// <param name="response">The <see cref="GenerationResponse"/> from the generator.</param>
        /// <returns>A task that represents the asynchronous operation, returning the agent's final response message.</returns>
        protected abstract Task<string?> HandleGeneratorResponse(GenerationResponse response);

    }
}

