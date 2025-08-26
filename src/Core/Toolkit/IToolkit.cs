using DotAgent.Core.Tool;

namespace DotAgent.Core.Toolkit;

public interface IToolkit
{
    public string ToolPrompt { get; }
    public string ToolTypeDef { get; }
    public void AddTool(ITool tool);
    public Task<string?> ExecuteToolAsync(string toolName, string parametersJson);
}