using DotAgent.Core.Models;

namespace DotAgent.Core.Memory;

/// <summary>
/// Defines the contract for a memory component that stores and manages conversation history.
/// </summary>
public interface IMemory
{
    /// <summary>
    /// Adds a message to the memory.
    /// </summary>
    /// <param name="message">The <see cref="MemoryData"/> object representing the message to add.</param>
    void AddMessage(MemoryData message);

    /// <summary>
    /// Retrieves the conversation history asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, returning a read-only list of <see cref="MemoryData"/> objects representing the conversation history.</returns>
    Task<IReadOnlyList<MemoryData>> GetHistoryAsync();

    /// <summary>
    /// Changes the system prompt in the memory.
    /// </summary>
    /// <param name="systemPrompt">The new system prompt. Can be null to clear the system prompt.</param>
    void ChangeSystemPrompt(string? systemPrompt);
}
