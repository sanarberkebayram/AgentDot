using DotAgent.Core.Models;
using DotAgent.Logging;

namespace DotAgent.Core.Memory;

/// <summary>
/// Provides a base implementation for memory management, storing and retrieving conversation history.
/// </summary>
public class MemoryBase : IMemory
{
    private List<MemoryData> History { get; set; } = [];
    private MemoryData SystemPrompt { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryBase"/> class with a default system prompt.
    /// </summary>
    public MemoryBase()
    {
        SystemPrompt = new MemoryData { Role = Models.Memory.System, Content = new TextContent("No System Prompt Given")};
        _ = Logger.LogAsync(Logger.LogType.Info, "Memory", "No System Prompt Given");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryBase"/> class with a specified system prompt.
    /// </summary>
    /// <param name="systemPrompt">The initial system prompt for the memory.</param>
    public MemoryBase(string? systemPrompt)
    {
        SystemPrompt = new MemoryData { Role = Models.Memory.System, Content = new TextContent(systemPrompt)};
    }
    
    /// <summary>
    /// Adds a message to the memory's history.
    /// </summary>
    /// <param name="message">The <see cref="MemoryData"/> object representing the message to add.</param>
    public void AddMessage(MemoryData message)
    {
        History.Add(message);
    }

    /// <summary>
    /// Retrieves the conversation history asynchronously, including the system prompt.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, returning a read-only list of <see cref="MemoryData"/> objects representing the conversation history.</returns>
    public Task<IReadOnlyList<MemoryData>> GetHistoryAsync()
    {
        var newHistory = History.ToList();
        newHistory.Insert(0, SystemPrompt);
        return Task.FromResult<IReadOnlyList<MemoryData>>(newHistory);
    }

    /// <summary>
    /// Changes the system prompt in the memory.
    /// </summary>
    /// <param name="systemPrompt">The new system prompt. Can be null to clear the system prompt.</param>
    public void ChangeSystemPrompt(string? systemPrompt)
    {
        SystemPrompt.Content = new TextContent(systemPrompt);
    }
}