using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotAgent.Implementations;

public class Orchestrator : IOrchestrator
{
    public string Id { get; private set; }
    public string SystemPrompt { get; private set; }
    public IMemory Memory { get; private set; }
    public IReadOnlyList<ITool> Tools { get; private set; }
    private IConnector? _connector;
    private IPlanner? _planner;
    private readonly List<IAgent> _agents = new();

    public Orchestrator(string id, string systemPrompt, IMemory? memory, IEmbeddingGenerator embeddingGenerator, IReadOnlyList<IAgent>? agents = null, IPlanner? planner = null)
    {
        Id = id;
        SystemPrompt = systemPrompt;
        Memory = memory ?? new VectorMemory(embeddingGenerator);
        Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.System, Content = SystemPrompt });
        
        _agents = agents?.ToList() ?? new List<IAgent>();
        Tools = _agents.Select(a => new AgentTool(a)).ToList();
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

    public void AddAgent(IAgent agent)
    {
        _agents.Add(agent);
        Tools = _agents.Select(a => new AgentTool(a)).ToList();
    }
    

    public async Task<string> ExecuteAsync(string input)
    {
        try
        {
            if (_connector == null)
            {
                throw new InvalidOperationException("Orchestrator is not connected to an LLM connector.");
            }

            Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = input });

            var plan = await _planner!.CreatePlanAsync(input, Tools);
            Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.Assistant, 
                Content = "Goal: {plan.Goal}\n\nSteps:\n" 
                          + string.Join("\n", plan.Steps.Select((s, i) => $"  {i + 1}. {s.GetType().Name}: {s.Rationale}")) });
            await Logger.LogAsync($"Orchestrator Planner Decision for {Id}",
                $"Goal: {plan.Goal}\n\nSteps:\n" + string.Join("\n",
                    plan.Steps.Select((s, i) => $"  {i + 1}. {s.GetType().Name}: {s.Rationale}")));

            string? finalResult = null;
            foreach (var step in plan.Steps)
            {
                if (step is ToolStep toolStep)
                {
                    var tool = Tools.FirstOrDefault(t => t.Name == toolStep.ToolName);
                    if (tool == null)
                    {
                        await Logger.LogAsync($"Orchestrator Tool Not Found for {Id}",
                            $"Attempted to use tool '{toolStep.ToolName}' but it was not found.");
                        Memory.AddMessage(new ChatMessage
                        {
                            Role = ChatMessageRole.User, Content = $"Error: Tool '{toolStep.ToolName}' not found."
                        });
                        continue;
                    }

                    // Parameter validation (simplified for now, can reuse Agent's validation logic)
                    if (!toolStep.Parameters.TryGetProperty("input", out var inputElement) ||
                        inputElement.ValueKind != JsonValueKind.String)
                    {
                        await Logger.LogAsync($"Orchestrator Tool Validation Failure for {Id}",
                            $"Tool: {tool.Name}\nInvalid Parameters: {toolStep.Parameters.ToString()}\nError: Missing or invalid 'input' parameter.");
                        Memory.AddMessage(new ChatMessage
                        {
                            Role = ChatMessageRole.User,
                            Content =
                                $"Tool parameter validation failed for {tool.Name}: Missing or invalid 'input' parameter."
                        });
                        throw new InvalidOperationException(
                            $"Tool parameter validation failed for {tool.Name}: Missing or invalid 'input' parameter.");
                    }

                    await Logger.LogAsync($"Orchestrator Tool Execution for {Id}",
                        $"Tool: {tool.Name}\nParameters: {toolStep.Parameters.ToString()}");
                    finalResult = await tool.ExecuteAsync(toolStep.Parameters);
                }
                else if (step is TextGenerationStep textGenerationStep)
                {
                    await Logger.LogAsync($"Orchestrator Text Generation for {Id}",
                        $"Prompt: {textGenerationStep.Prompt}");
                    var generatedText = await _connector.GenerateTextAsync(textGenerationStep.Prompt ?? string.Empty,
                        await Memory.GetHistoryAsync());
                    await Logger.LogAsync($"Orchestrator Generated Text for {Id}", $"Text: {generatedText}");
                    Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.User, Content = generatedText });
                    finalResult = generatedText;
                }
            }

            await LogMemory();
            return finalResult ?? "No result generated.";
        }
        catch (Exception e)
        {
            await Logger.LogAsync("Orchestrator Error", e.ToString());
            await LogMemory();
            throw;
        }
        
    }
    
    private async Task LogMemory()
    {
        var messages = await Memory.GetHistoryAsync();
        
        var requestBody = new
        {
            messages = messages
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        await Logger.LogAsync("Last Memory :", json);
    }
}
