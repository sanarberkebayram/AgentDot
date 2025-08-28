using DotAgent.Core.Models;

namespace DotAgent.Core.Generator
{
    public interface IGenerator
    {
        Task<GenerationResponse> GenerateAsync(GeneratorRequest request);
    }
}

