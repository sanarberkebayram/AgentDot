using DotAgent.Models;

namespace DotAgent.Core.Generator;

public interface IRequestCreator
{
    string CreateRequestBody(IReadOnlyList<ChatMessage> history, GeneratorMode mode);
}