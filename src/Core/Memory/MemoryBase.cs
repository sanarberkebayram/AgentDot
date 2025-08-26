using DotAgent.Core.Models;
using DotAgent.Logging;

namespace DotAgent.Core.Memory;

public class MemoryBase : IMemory
{
    private List<MemoryData> History { get; set; } = [];
    private MemoryData SystemPrompt { get; set; }
    
    public MemoryBase()
    {
        SystemPrompt = new MemoryData { Role = Models.Memory.System, Content = new TextContent("No System Prompt Given")};
        _ = Logger.LogAsync(Logger.LogType.Info, "Memory", "No System Prompt Given");
    }

    public MemoryBase(string? systemPrompt)
    {
        SystemPrompt = new MemoryData { Role = Models.Memory.System, Content = new TextContent(systemPrompt)};
    }
    
    public void AddMessage(MemoryData message)
    {
        History.Add(message);
    }

    public Task<IReadOnlyList<MemoryData>> GetHistoryAsync()
    {
        var newHistory = History.ToList();
        newHistory.Insert(0, SystemPrompt);
        return Task.FromResult<IReadOnlyList<MemoryData>>(newHistory);
    }

    public void ChangeSystemPrompt(string? systemPrompt)
    {
        SystemPrompt.Content = new TextContent(systemPrompt);
    }
}