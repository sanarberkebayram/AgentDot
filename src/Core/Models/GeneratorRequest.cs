using DotAgent.Core.Memory;
using DotAgent.Core.Toolkit;

namespace DotAgent.Core.Models;

[Serializable]
public class GeneratorRequest
{
    public IMemory? Memory { get; set; }
    public IToolkit? Toolkit { get; set; }
}