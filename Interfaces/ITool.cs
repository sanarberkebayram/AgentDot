
using System.Text.Json;
using DotAgent.Models;

namespace DotAgent.Interfaces;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    IReadOnlyList<ToolInputParameter> GetParameters();
    Task<string> ExecuteAsync(JsonElement parameters);
}
