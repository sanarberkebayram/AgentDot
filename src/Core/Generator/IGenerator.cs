using DotAgent.Models;

namespace DotAgent.Core.Generator;

public interface IGenerator
{
    Task<string> GenerateTextAsync(string prompt, IReadOnlyList<ChatMessage> history);
}