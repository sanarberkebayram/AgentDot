using DotAgent.Models;

namespace DotAgent.Core.Memory;

public interface IMemory
{
    void AddMessage(ChatMessage message);
    Task<IReadOnlyList<ChatMessage>> GetHistoryAsync();
    void ChangeSystemPrompt(string systemPrompt);
}
