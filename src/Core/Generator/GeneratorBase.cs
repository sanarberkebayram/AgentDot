using DotAgent.Core.Models;
using DotAgent.Logging;

namespace DotAgent.Core.Generator
{
    public abstract class GeneratorBase : IGenerator
    {
        private readonly bool _log;

        public GeneratorBase(bool log = false)
        {
            _log = log;
        }

        protected abstract Task<GenerationResponse> GenerateResponse(GeneratorRequest request);

        public async Task<GenerationResponse> GenerateAsync(GeneratorRequest request)
        {
            try
            {
                return await GenerateResponse(request);
            }
            catch (Exception ex)
            {
                if (!_log) throw;

                var messages = await request.Memory?.GetHistoryAsync()!;
                await Logger.LogAsync(
                    Logger.LogType.Error,
                    $"Generator Failed",
                    $"{ex.Message} with message history:\n" +
                    $"{string.Join("\n", messages.Select(m => $"{m.Role}: {m.Content} \n"))}" +
                    $"Stack Trace: {ex.StackTrace}"
                );

                throw;
            }
        }
    }
}
