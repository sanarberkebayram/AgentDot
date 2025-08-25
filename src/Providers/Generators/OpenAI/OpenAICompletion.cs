using DotAgent.Core.Generator;
using DotAgent.Core.Models;

namespace DotAgent.Providers.Generators.OpenAI;

public class OpenAICompletion : GeneratorBase
{
    private readonly string _model;

    public OpenAICompletion(string model)
    {
        _model = model;
    }

    protected override Task<GenerationResponse> GenerateResponse(GeneratorParams @params)
    {
        throw new NotImplementedException();
    }
}
