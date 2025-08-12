using DotAgent.Models;

namespace DotAgent.Interfaces;

public interface IMemory
{
    void AddMessage(ChatMessage message);
    Task<IReadOnlyList<ChatMessage>> GetHistoryAsync();
    void ChangeSystemPrompt(string systemPrompt);
}
