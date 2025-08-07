using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotAgent.Implementations;

public class OrchestrationPlanner : IPlanner
{
    private readonly IConnector _connector;

    public OrchestrationPlanner(IConnector connector)
    {
        _connector = connector;
    }

    public async Task<Plan> CreatePlanAsync(string goal, IReadOnlyList<ITool> tools)
    {
        var toolDescriptions = tools.Select(t => $"- Name: {t.Name}\n  Description: {t.Description}\n  Parameters: {JsonSerializer.Serialize(t.GetParameters())}");
        var prompt = $"You are a planner. Your goal is to create a step-by-step plan to achieve the user's goal. For an easy goal, one step is enough. Decide step count based on the complexity of the goal." +
                     $"You have the following tools available:\n{string.Join("\n", toolDescriptions)}\n\n" +
                     "Your plan should be a JSON array of steps. Each step must have a 'type' (either \"tool\" or \"text\") and a 'rationale'.\n" +
                     "For a \"tool\" step, include 'toolName' and 'parameters' (a JSON object). For a \"text\" step, include 'prompt'.\n" +
                     "You must create \"tool\" steps until the final step. Final step must be \"text\" with a 'prompt' that describes the final result.\n"  +
                     "Example tool step: {{ \"type\": \"tool\", \"toolName\": \"SearchTool\", \"parameters\": {{ \"query\": \"search term\" }}, \"rationale\": \"To find information\" }}\n" +
                     "Example text step: {{ \"type\": \"text\", \"prompt\": \"Summarize the findings\", \"rationale\": \"To present the results\" }}\n\n" +
                     $"User's Goal: {goal}\n\n" +
                     "Please provide the plan as a JSON array. Do not use ```` or ```json` tags. :";

        var planJson = await _connector.GenerateTextAsync(prompt, new List<ChatMessage>());

        var planSteps = new List<PlanStep>();
        using (JsonDocument doc = JsonDocument.Parse(planJson))
        {
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("type", out JsonElement typeElement))
                {
                    var type = typeElement.GetString();
                    if (type == "tool")
                    {
                        planSteps.Add(new ToolStep
                        {
                            ToolName = element.GetProperty("toolName").GetString(),
                            Parameters = JsonDocument.Parse(element.GetProperty("parameters").GetRawText()).RootElement,
                            Rationale = element.GetProperty("rationale").GetString()
                        });
                    }
                    else if (type == "text")
                    {
                        planSteps.Add(new TextGenerationStep
                        {
                            Prompt = element.GetProperty("prompt").GetString(),
                            Rationale = element.GetProperty("rationale").GetString()
                        });
                    }
                }
            }
        }

        return new Plan { Goal = goal, Steps = planSteps };
    }
}
