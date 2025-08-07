using DotAgent.Core.Tool;
using DotAgent.Interfaces;

namespace DotAgent.Core.Toolkit;

public interface IToolkit
{
    public string ToolPrompt { get; }
    public void AddTool(ITool tool);
    public Task<string> ExecuteToolAsync(string toolName, string parametersJson);
}