using DotAgent.Core.Memory;
using DotAgent.Core.Toolkit;

namespace DotAgent.Core.Agent
{
    /// <summary>
    /// Defines the contract for an agent that can process messages and interact with memory and tools.
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Gets the unique identifier for the agent.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the system prompt associated with the agent.
        /// </summary>
        string SystemPrompt { get; }

        /// <summary>
        /// Gets the memory component used by the agent to store conversation history.
        /// </summary>
        IMemory Memory { get; }

        /// <summary>
        /// Gets the toolkit providing tools for the agent to use.
        /// </summary>
        IToolkit Toolkit { get; }

        /// <summary>
        /// Processes an incoming message asynchronously.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>A task that represents the asynchronous operation, returning the agent's response message.</returns>
        Task<string?> ProcessMessageAsync(string? message);
    }
}

