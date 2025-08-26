using DotAgent.Core.Generator;
using DotAgent.Core.Memory;
using DotAgent.Core.Models;
using DotAgent.Core.Toolkit;
using DotAgent.Logging;

namespace DotAgent.Core.Agent;

/// <summary>
/// Represents a default agent implementation that processes messages, interacts with a generator, and handles tool calls.
/// </summary>
public class AgentDefault : AgentBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentDefault"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the agent.</param>
    /// <param name="systemPrompt">The initial system prompt for the agent's memory.</param>
    /// <param name="generator">The generator used by the agent to produce responses.</param>
    /// <param name="memory">The memory component for storing conversation history. If null, a new <see cref="MemoryBase"/> is used.</param>
    /// <param name="toolkit">The toolkit providing tools for the agent to use. If null, a new <see cref="Toolkit.Toolkit"/> is used.</param>
    public AgentDefault(string id, string? systemPrompt, IGenerator generator, IMemory? memory = null, IToolkit? toolkit = null) :
        base(id, systemPrompt, memory ?? new MemoryBase(systemPrompt), toolkit ?? new Toolkit.Toolkit(), generator)
    {
    }

    private readonly int _maxErrorCount = 3;

    /// <summary>
    /// Processes an incoming message, generates a response, and handles any tool calls.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <returns>The agent's response message, or an error if the generator is not set.</returns>
    /// <exception cref="Exception">Thrown if the generator is not set.</exception>
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

    /// <summary>
    /// Handles the response received from the generator, including processing function calls.
    /// </summary>
    /// <param name="response">The <see cref="GenerationResponse"/> from the generator.</param>
    /// <returns>The final response message from the agent, or an error message if applicable.</returns>
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
    
    /// <summary>
    /// Handles the execution of function calls returned by the generator.
    /// </summary>
    /// <param name="response">The <see cref="GenerationResponse"/> containing function calls.</param>
    /// <returns>True if any errors occurred during function execution, otherwise false.</returns>
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
    
    /// <summary>
    /// Adds the generator's response message or function calls to the agent's memory.
    /// </summary>
    /// <param name="response">The <see cref="GenerationResponse"/> to add to memory.</param>
    private void AddGeneratorMessageToMemory(GenerationResponse response)
    {
        if(!string.IsNullOrWhiteSpace(response.Message))
            Memory.AddMessage(new MemoryData(){Role = Models.Memory.Assistant, Content = new TextContent(response.Message) });
        
        if (response.FunctionCalls is { Length: > 0 })
            Memory.AddMessage(new MemoryData(){Role = Models.Memory.Assistant, Content = new ToolCallContent(response.FunctionCalls) });
    }
}