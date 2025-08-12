using DotAgent.Core.Generator;
using DotAgent.Core.Models;
using DotAgent.Core.Toolkit;
using DotAgent.Implementations;
using DotAgent.Interfaces;
using DotAgent.Logging;
using DotAgent.Models;

namespace DotAgent.Core.Agent;

public class AgentDefault(string id, string systemPrompt, IGenerator generator, IMemory? memory, IToolkit? toolkit)
    : AgentBase(id, systemPrompt, memory ?? new MemoryBase(systemPrompt), toolkit ?? new Toolkit.Toolkit(), generator)
{
    private readonly int _maxErrorCount = 3;
    public override async Task<string> ProcessMessageAsync(string message)
    {
        if (Generator == null)
        {
            Logger.LogAsync(Logger.LogType.Error,$"AgentDefault {Id}", "Generator is not set.").Wait();
            throw new Exception("Generator is not set.");
        }
        Memory.AddMessage(new ChatMessage(){Role = ChatMessageRole.User, Content = message});
        
        var initialResponse = await Generator.GenerateAsync(await Memory.GetHistoryAsync());
        Memory.AddMessage(new ChatMessage(){Role = ChatMessageRole.Assistant, Content = initialResponse.Message });
        
        return await HandleGeneratorResponse(initialResponse);
    }
    
    public override async Task<string> HandleGeneratorResponse(GenerationResponse response)
    {
        var errorCount = 0;
        do
        {
            if (response.Type == ResponseType.ToolCalling)
            {
                var hasError = await HandleFunctionCalling(response);
                if (hasError)
                    errorCount++;
            }
            else
            {
                return response.Message;
            }
            
            response = await Generator.GenerateAsync(await Memory.GetHistoryAsync());
            Memory.AddMessage(new ChatMessage(){Role = ChatMessageRole.Assistant, Content = response.Message });
            
            
            if (errorCount > _maxErrorCount)
            {
                Memory.AddMessage(new ChatMessage(){Role = ChatMessageRole.User, Content = "Maximum error count reached. Exiting."});
                return "Maximum error count reached. Exiting.";
            }
        } while (true);
    }
    
    private async Task<bool> HandleFunctionCalling(GenerationResponse response)
    {
        var hasError = false;
        foreach (var funcCall in response.FunctionCalls)
        {
            var result = await Toolkit.ExecuteToolAsync(funcCall.Name, funcCall.Parameters);
            Memory.AddMessage(new ToolMessage(){Role = ChatMessageRole.Tool, Content = result, ToolCallId = funcCall.Id});
            
            if ( result.Contains("Error") || result.Contains("error") || result.Contains("fail") || result.Contains("FAIL"))
                hasError = true;
        }

        return hasError;
    }
}