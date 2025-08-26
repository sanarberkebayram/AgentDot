using DotAgent.Core.Generator;
using DotAgent.Core.Memory;
using DotAgent.Core.Models;
using DotAgent.Core.Toolkit;
using DotAgent.Logging;

namespace DotAgent.Core.Agent;

public class AgentDefault : AgentBase
{
    public AgentDefault(string id, string? systemPrompt, IGenerator generator, IMemory? memory = null, IToolkit? toolkit = null) :
        base(id, systemPrompt, memory ?? new MemoryBase(systemPrompt), toolkit ?? new Toolkit.Toolkit(), generator)
    {
    }

private readonly int _maxErrorCount = 3;
    public override async Task<string?> ProcessMessageAsync(string? message)
    {
        if (Generator == null)
        {
            Logger.LogAsync(Logger.LogType.Error,$"AgentDefault {Id}", "Generator is not set.").Wait();
            throw new Exception("Generator is not set.");
        }
        Memory.AddMessage(new MemoryData(){Role = Models.Memory.User, Content = new TextContent(message)});
        
        var initialResponse = await Generator.GenerateAsync(new GeneratorRequest(){Memory = Memory, Toolkit = Toolkit});
        AddGeneratorMessageToMemory(initialResponse);
        
        return await HandleGeneratorResponse(initialResponse);
    }

    
    protected override async Task<string?> HandleGeneratorResponse(GenerationResponse response)
    {
        if (!string.IsNullOrWhiteSpace(response.Error))
            return response.Error;
        
        var errorCount = 0;
        do
        {
            if (response.FunctionCalls is { Length: > 0 })
            {
                var hasError = await HandleFunctionCalling(response);
                if (hasError)
                    errorCount++;
            }
            else
            {
                return response.Message;
            }
            
            response = await Generator.GenerateAsync(new GeneratorRequest(){Memory = Memory, Toolkit = Toolkit});
            AddGeneratorMessageToMemory(response);
            
            if (errorCount > _maxErrorCount)
            {
                Memory.AddMessage(new MemoryData(){Role = Models.Memory.User, Content = new TextContent("Maximum error count reached. Exiting.")});
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
            Memory.AddMessage(new MemoryData()
            {
                Role= Models.Memory.Tool,
                Content = new ToolResultContent(funcCall.Id, result),
            });
            
            if (result.Contains("Error") || result.Contains("error") || result.Contains("fail") || result.Contains("FAIL"))
                hasError = true;
        }

        return hasError;
    }
    
    private void AddGeneratorMessageToMemory(GenerationResponse response)
    {
        if(!string.IsNullOrWhiteSpace(response.Message))
            Memory.AddMessage(new MemoryData(){Role = Models.Memory.Assistant, Content = new TextContent(response.Message) });
        
        if (response.FunctionCalls is { Length: > 0 })
            Memory.AddMessage(new MemoryData(){Role = Models.Memory.Assistant, Content = new ToolCallContent(response.FunctionCalls) });
    }
}