using DotAgent.Core.Models;
using DotAgent.Logging;

namespace DotAgent.Core.Generator;

public abstract class GeneratorBase : IGenerator
{
    public GeneratorMode Mode { get; set; }
    private readonly bool _log;


    public GeneratorBase(bool log = false, GeneratorMode mode = GeneratorMode.OnlyText)
    {
        Mode = mode;
        _log = log;
    }

    public async Task<GenerationResponse> GenerateAsync(GeneratorParams @params)
    {
        try
        {
            return await GenerateResponse(@params);
        }
        catch (Exception ex)
        {
            if (_log)
                await Logger.LogAsync(
                    Logger.LogType.Error,
                    $"Generator Failed",
                    $"{ex.Message} with message history:\n" +
                    $"{string.Join("\n", @params.Messages.Select(m => $"{m.Role}: {m.Content} \n"))}" +
                    $"Stack Trace: {ex.StackTrace}"
                );
            throw;
        }
    }

    protected abstract Task<GenerationResponse> GenerateResponse(GeneratorParams @params);
}
