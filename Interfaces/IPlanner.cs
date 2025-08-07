using DotAgent.Models;

namespace DotAgent.Interfaces;

public interface IPlanner
{
    Task<Plan> CreatePlanAsync(string goal, IReadOnlyList<ITool> tools);
}
