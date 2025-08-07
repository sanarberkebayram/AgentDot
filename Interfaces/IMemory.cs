using DotAgent.Models;

namespace DotAgent.Interfaces;

public interface IMemory
{
    void AddMessage(ChatMessage message);
    Task<IReadOnlyList<ChatMessage>> GetHistoryAsync();
    Task<IReadOnlyList<ChatMessage>> FindRelevantMessagesAsync(string query, int maxResults);
    Task SummarizeAsync();
}
