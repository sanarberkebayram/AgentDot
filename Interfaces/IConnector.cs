using DotAgent.Models;

namespace DotAgent.Interfaces;

public interface IConnector
{
    Task<string> GenerateTextAsync(string prompt, IReadOnlyList<ChatMessage> history);
    Task<ToolCallResult> InvokeToolCallingAsync(string prompt, IReadOnlyList<ITool> tools, IReadOnlyList<ChatMessage> history);
}
