using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotAgent.Core.Tool;

namespace DotAgent.Implementations;

public class Orchestrator : IOrchestrator
{
    public string Id { get; private set; }
    public string SystemPrompt { get; private set; }
    public IMemory Memory { get; private set; }
    public IReadOnlyList<ITool> Tools { get; private set; }
    private IConnector? _connector;
    private IPlanner? _planner;
    private readonly List<IAgent> _agents;

    public Orchestrator(string id, string systemPrompt, IMemory? memory, IEmbeddingGenerator embeddingGenerator,
        IReadOnlyList<IAgent>? agents = null, IPlanner? planner = null)
    {
        Id = id;
        SystemPrompt = systemPrompt;
        Memory = memory ?? new VectorMemory(embeddingGenerator);

        _agents = agents?.ToList() ?? new List<IAgent>();
        Tools = _agents.Select(a => new AgentTool(a)).ToList();
        Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.System, Content = $"{SystemPrompt}\n Available Tools: \n{string.Join("\n", Tools.Select(t => $"- {t.Name}: {t.Description}"))}" });
        _planner = planner;
    }

    public void Connect(IConnector connector)
    {
        _connector = connector;
        _planner ??= new OrchestrationPlanner(_connector);
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

            Memory.AddMessage(new ChatMessage
            {
                Role = ChatMessageRole.Assistant,
                Content = $"Goal: {plan.Goal}\n\nSteps:\n" + string.Join("\n",
                    plan.Steps.Select((s, i) => $"  {i + 1}. {"123"}: {s.Rationale}"))
            });
            await Logger.LogAsync($"Orchestrator Planner Decision for {Id}",
                $"Goal: {plan.Goal}\n\nSteps:\n" + string.Join("\n",
                    plan.Steps.Select((s, i) => $"  {i + 1}. {s.GetType().Name}: {s.Rationale}")));

            string? finalResult = null;
            foreach (var step in plan.Steps)
            {
                var stepResult = await ExecuteStep(step);
                Memory.AddMessage(new ChatMessage { Role = ChatMessageRole.Assistant, Content = stepResult });

                if (IsToolCalling(stepResult))
                {
                    var toolCalls = GetToolCalls(stepResult);

                    foreach (var call in toolCalls)
                    {
                        var tool = Tools.FirstOrDefault(t => t.Name == call.ToolName);
                        if (tool == null)
                        {
                            await Logger.LogAsync($"Orchestrator Tool Not Found for {Id}",
                                $"Attempted to use tool '{call.ToolName}' but it was not found.");
                            Memory.AddMessage(new ChatMessage
                            {
                                Role = ChatMessageRole.User, Content = $"Error: Tool '{call.ToolName}' not found."
                            });
                            continue;
                        }
                        
                        using var paramDoc = JsonDocument.Parse(call.ParametersJson);
                        var paramElements = paramDoc.RootElement;
                        if (!paramElements.TryGetProperty("input", out var inputElement) ||
                            inputElement.ValueKind != JsonValueKind.String)
                        {
                            await Logger.LogAsync($"Orchestrator Tool Validation Failure for {Id}",
                                $"Tool: {tool.Name}\nInvalid Parameters: {call.ParametersJson}\nError: Missing or invalid 'input' parameter.");
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
                            $"Tool: {tool.Name}\nParameters: {call.ParametersJson}");
                        finalResult = await tool.ExecuteAsync(paramElements);
                    }
                }
                else
                {
                    finalResult = stepResult;
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

    private ToolCall[] GetToolCalls(string stepResult)
    {
        var toolCalls = new List<ToolCall>();

        using var doc = JsonDocument.Parse(stepResult);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
            {
                if (!element.TryGetProperty("toolName", out var _)) continue;

                var toolCall = new ToolCall
                {
                    ToolName = element.GetProperty("toolName").GetString(),
                    ParametersJson = element.GetProperty("parameters").GetRawText(),
                    Rationale = element.TryGetProperty("rationale", out var rationaleProp) 
                        ? rationaleProp.GetString() 
                        : null
                };
                toolCalls.Add(toolCall);
            }
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("toolName", out var _))
            {
                var toolCall = new ToolCall
                {
                    ToolName = root.GetProperty("toolName").GetString(),
                    ParametersJson = root.GetProperty("parameters").GetRawText(),
                    Rationale = root.TryGetProperty("rationale", out var rationaleProp) 
                        ? rationaleProp.GetString() 
                        : null
                };
                toolCalls.Add(toolCall);
            }
        }

        return toolCalls.ToArray();
    }

    private bool IsToolCalling(string stepResult)
    {
        try
        {
            using var doc = JsonDocument.Parse(stepResult);
            var root = doc.RootElement;

            return root.ValueKind switch
            {
                JsonValueKind.Array => root.EnumerateArray().Any(element => element.TryGetProperty("toolName", out _)),
                JsonValueKind.Object => root.TryGetProperty("toolName", out _),
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<string> ExecuteStep(PlanStep step)
    {
        var toolDescriptions = Tools.Select(t => $"- Name: {t.Name}\n  Description: {t.Description}\n  Parameters: {JsonSerializer.Serialize(t.GetParameters())}");
        
        Memory.AddMessage(
            new ChatMessage 
                { Role = ChatMessageRole.User,
                    Content = $"Current Step Goal: {step.Prompt}\n" +
                              $" Rationale: {step.Rationale}\n" +
                              $"If you want to use a tool, provide the tool name and parameters to the tool. You can only use provided tools in the system prompt.\n" +
                              $"You have the following tools available:\n{string.Join("\n", toolDescriptions)}\n\n" +
                              $"If you do not want to use a tool, respond with plain text about the goal and rationale\n" +
                              $"Multiple tool usage is allowed.\n" +
                              $"Example tool step: {{ \"toolName\": \"SearchTool\", \"parameters\": {{ \"query\": \"search term\" }}}}\n" +
                              "Call tool step as a JSON array. Do not use ```` or ```json` tags. "
                });
        var generatedText = await _connector!.GenerateTextAsync(string.Empty, await Memory.GetHistoryAsync());
        return generatedText;
    }
    private async Task LogMemory()
    {
        var messages = await Memory.GetHistoryAsync();
        
        var requestBody = new
        {
             messages
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        await Logger.LogAsync("Last Memory :", json);
    }
}
