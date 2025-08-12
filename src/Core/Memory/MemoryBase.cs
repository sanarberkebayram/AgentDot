using DotAgent.Logging;
using DotAgent.Models;

namespace DotAgent.Core.Memory;

public class MemoryBase : IMemory
{
    public List<ChatMessage> History { get; set; } = [];
    protected ChatMessage SystemPrompt { get; set; }
    
    public MemoryBase()
    {
        SystemPrompt = new ChatMessage { Role = ChatMessageRole.System, Content = "No System Prompt Given"};
        _ = Logger.LogAsync(Logger.LogType.Info, "Memory", "No System Prompt Given");
    }

    public MemoryBase(string systemPrompt)
    {
        SystemPrompt = new ChatMessage { Role = ChatMessageRole.System, Content = systemPrompt};
    }
    
    public void AddMessage(ChatMessage message)
    {
        History.Add(message);
    }

    public Task<IReadOnlyList<ChatMessage>> GetHistoryAsync()
    {
        var newHistory = History.ToList();
        newHistory.Insert(0, SystemPrompt);
        return Task.FromResult<IReadOnlyList<ChatMessage>>(newHistory);
    }

    public void ChangeSystemPrompt(string systemPrompt)
    {
        SystemPrompt.Content = systemPrompt;
    }
}