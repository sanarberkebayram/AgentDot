using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;

namespace DotAgent.Implementations;

public class Agent : IAgent
{
    public string Id { get; private set; }
    public string SystemPrompt { get; private set; }
    public IMemory Memory { get; private set; }
    public IReadOnlyList<ITool> Tools { get; private set; }
    private IConnector? _connector;
    private IPlanner? _planner;
    public Agent(string id, string systemPrompt, IMemory? memory, IEmbeddingGenerator embeddingGenerator, IReadOnlyList<ITool>? tools = null, IPlanner? planner = null)
    {
        Id = id;
        SystemPrompt = systemPrompt;
        Memory = memory ?? new VectorMemory(embeddingGenerator);
        Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.System, Content = SystemPrompt });
        Tools = tools ?? new List<ITool>();
        _planner = planner;
    }

    public void Connect(IConnector connector)
    {
        _connector = connector;
        if (_planner == null)
        {
            _planner = new OrchestrationPlanner(_connector);
        }
    }

    

    public async Task<string> ExecuteAsync(string input)
    {
        if (_connector == null)
        {
            throw new InvalidOperationException("Agent is not connected to an LLM connector.");
        }

        Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = input });

        var plan = await _planner!.CreatePlanAsync(input, Tools);
        Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.Assistant, Content = $"Goal: {plan.Goal}\n\nSteps:\n" + string.Join("\n", plan.Steps.Select((s, i) => $"  {i + 1}. {s.GetType().Name}: {s.Rationale}")) });
        await Logger.LogAsync($"Planner Decision for {Id}", $"Goal: {plan.Goal}\n\nSteps:\n" + string.Join("\n", plan.Steps.Select((s, i) => $"  {i + 1}. {s.GetType().Name}: {s.Rationale}")));

        string? finalResult = null;

        foreach (var step in plan.Steps)
        {
            if (step is ToolStep toolStep)
            {
                var tool = Tools.FirstOrDefault(t => t.Name == toolStep.ToolName);
                if (tool == null)
                {
                    // Log error: Tool not found
                    await Logger.LogAsync($"Tool Not Found for {Id}", $"Attempted to use tool '{toolStep.ToolName}' but it was not found.");
                    Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = $"Error: Tool '{toolStep.ToolName}' not found." });
                    continue;
                }

                // Parameter validation and correction loop
                var validationResult = ValidateToolParameters(tool, toolStep.Parameters);
                var retries = 0;
                const int maxRetries = 3;

                while (!validationResult.IsValid && retries < maxRetries)
                {
                    await Logger.LogAsync($"Tool Validation Failure for {Id}", $"Tool: {tool.Name}\nInvalid Parameters: {toolStep.Parameters.ToString()}\nError: {validationResult.ErrorMessage}\nRetries left: {maxRetries - retries}");
                    Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = $"Tool parameter validation failed for {tool.Name}: {validationResult.ErrorMessage}. Please provide valid parameters." });
                    // Re-prompt the LLM to correct the parameters
                    var correctionPrompt = $"The previous attempt to call tool '{tool.Name}' failed due to invalid parameters. " +
                                           $"Error: {validationResult.ErrorMessage}. " +
                                           $"Original goal: {input}. " +
                                           $"Please provide the correct parameters for tool '{tool.Name}'.";

                    var correctedToolCall = await _connector.InvokeToolCallingAsync(correctionPrompt, Tools, await Memory.GetHistoryAsync());
                    toolStep.Parameters = correctedToolCall.Parameters; // Update parameters with corrected ones
                    validationResult = ValidateToolParameters(tool, toolStep.Parameters);
                    retries++;
                }

                if (!validationResult.IsValid)
                {
                    await Logger.LogAsync($"Tool Validation Failure for {Id}", $"Tool: {tool.Name}\nInvalid Parameters: {toolStep.Parameters.ToString()}\nError: {validationResult.ErrorMessage}\nMax retries reached.");
                    Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = $"Tool parameter validation failed for {tool.Name}: {validationResult.ErrorMessage}. Max retries reached." });
                    return $"Tool parameter validation failed for {tool.Name}: {validationResult.ErrorMessage}. Max retries reached.";
                }

                await Logger.LogAsync($"Tool Execution for {Id}", $"Tool: {tool.Name}\nParameters: {toolStep.Parameters.ToString()}");
                var toolOutput = await tool.ExecuteAsync(toolStep.Parameters);
                Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = toolOutput });
                await Logger.LogAsync($"Tool Output for {Id}", $"Tool: {tool.Name} Output: {toolOutput}");
                finalResult = toolOutput;
            }
            else if (step is TextGenerationStep textGenerationStep)
            {
                await Logger.LogAsync($"Text Generation for {Id}", $"Prompt: {textGenerationStep.Prompt}");
                var generatedText = await _connector.GenerateTextAsync(textGenerationStep.Prompt ?? string.Empty, await Memory.GetHistoryAsync());
                await Logger.LogAsync($"Generated Text for {Id}", $"Text: {generatedText}");
                Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = generatedText });
                finalResult = generatedText;
            }
        }

        return finalResult ?? "No result generated.";
    }

    private (bool IsValid, string ErrorMessage) ValidateToolParameters(ITool tool, JsonElement parameters)
    {
        var requiredParameters = tool.GetParameters().Where(p => p.IsRequired).ToList();

        foreach (var requiredParam in requiredParameters)
        {
            if (requiredParam.Name == null || !parameters.TryGetProperty(requiredParam.Name, out var property))
            {
                return (false, $"Missing required parameter: {requiredParam.Name}");
            }
            switch (requiredParam.Type)
            {
                case "string" when property.ValueKind != JsonValueKind.String:
                    return (false, $"Parameter '{requiredParam.Name}' has incorrect type. Expected string.");
                case "number" when property.ValueKind != JsonValueKind.Number:
                    return (false, $"Parameter '{requiredParam.Name}' has incorrect type. Expected number.");
                case "boolean" when property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False:
                    return (false, $"Parameter '{requiredParam.Name}' has incorrect type. Expected boolean.");
            }
        }
        return (true, string.Empty);
    }
}
