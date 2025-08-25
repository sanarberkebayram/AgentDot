using DotAgent.Core.Toolkit;
using DotAgent.Models;

namespace DotAgent.Core.Generator;

[Serializable]
public class GeneratorParams
{
    public IReadOnlyList<ChatMessage> Messages;
    public IToolkit Toolkit;
}
