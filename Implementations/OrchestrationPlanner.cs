using DotAgent.Interfaces;
using DotAgent.Models;
using System.Text.Json;
using DotAgent.Core.Tool;

namespace DotAgent.Implementations;

public class OrchestrationPlanner(IConnector connector) : IPlanner
{
    public async Task<Plan> CreatePlanAsync(string goal, IReadOnlyList<ITool> tools)
    {
        var toolDescriptions = tools.Select(t => $"- Name: {t.Name}\n  Description: {t.Description}\n  Parameters: {JsonSerializer.Serialize(t.GetParameters())}");
        var prompt = $"You are a planner. Your goal is to create a step-by-step plan to achieve the user's goal. For an easy goal, one step is enough. Decide step count based on the complexity of the goal." +
                     $"You have the following tools available:\n{string.Join("\n", toolDescriptions)}\n\n" +
                     "Your plan should be a JSON array of steps. Each step must have a 'prompt' and a 'rationale'.\n" +
                     "If step includes some of the tools, it should provide hints in the prompt. For example, use write file tool" +
                     "Example step: {{ \"prompt\": \"Summarize the findings\", \"rationale\": \"To present the results\" }}\n\n" +
                     $"User's Goal: {goal}\n\n" +
                     "Please provide the plan as a JSON array. Do not use ```` or ```json` tags. :";

        var planJson = await connector.GenerateTextAsync(prompt, new List<ChatMessage>());

        var planSteps = new List<PlanStep>();
        using (var doc = JsonDocument.Parse(planJson))
        {
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (!element.TryGetProperty("prompt", out var _)) continue;
                planSteps.Add(new PlanStep
                {
                    Prompt = element.GetProperty("prompt").GetString(),
                    Rationale = element.GetProperty("rationale").GetString()
                });
            }
        }

        return new Plan { Goal = goal, Steps = planSteps };
    }
}
