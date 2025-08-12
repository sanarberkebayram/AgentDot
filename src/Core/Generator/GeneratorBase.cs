using DotAgent.Core.Models;
using DotAgent.Implementations;
using DotAgent.Logging;
using DotAgent.Models;

namespace DotAgent.Core.Generator;

public abstract class GeneratorBase : IGenerator
{
    public GeneratorMode Mode { get; set; }
    private readonly IRequestCreator _requestCreator;
    private readonly IResponseCreator _responseCreator;
    private readonly bool _log;
    

    public GeneratorBase(IRequestCreator requestCreator, IResponseCreator responseCreator,bool log = false, GeneratorMode mode = GeneratorMode.OnlyText)
    {
        Mode = mode;
        _requestCreator = requestCreator;
        _responseCreator = responseCreator;
        _log = log;
    }

    public async Task<GenerationResponse> GenerateAsync(IReadOnlyList<ChatMessage> history)
    {
        try
        {
            var requestBody = _requestCreator.CreateRequestBody(history, Mode);
            var response = await GenerateAsyncInternal(history);
            return _responseCreator.CreateResponse(response);
        }
        catch (Exception ex)
        {
            await Logger.LogAsync(
                Logger.LogType.Error,
                $"Generator Failed",
                $"{ex.Message} with message history:\n" +
                $"{string.Join("\n", history.Select(m => $"{m.Role}: {m.Content} \n"))}"+
                $"Stack Trace: {ex.StackTrace}"
                );
            throw;
        }
    }
    
    protected abstract Task<string> GenerateAsyncInternal(IReadOnlyList<ChatMessage> history);
}