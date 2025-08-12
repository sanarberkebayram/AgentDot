using DotAgent.Core.Models;
using DotAgent.Models;

namespace DotAgent.Core.Generator;

public interface IGenerator
{
    Task<GenerationResponse> GenerateAsync(IReadOnlyList<ChatMessage> history);
}