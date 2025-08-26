using DotAgent.Core.Models;

namespace DotAgent.Core.Memory;

public interface IMemory
{
    void AddMessage(MemoryData message);
    Task<IReadOnlyList<MemoryData>> GetHistoryAsync();
    void ChangeSystemPrompt(string? systemPrompt);
}
