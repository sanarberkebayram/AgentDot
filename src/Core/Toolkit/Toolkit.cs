using DotAgent.Core.Tool;

namespace DotAgent.Core.Toolkit;

public class Toolkit : IToolkit
{
    public string ToolPrompt { get; private set; }
    public string ToolTypeDef => GetTypeDef();
    
    private readonly List<ITool> _tools = new();
    private readonly Dictionary<string, ITool> _lookUp = new();

    public Toolkit()
    {
        ToolPrompt = "No tools available.";
    }

    public Toolkit(IReadOnlyList<ITool> tools)
    {
        foreach (var tool in tools)
        {
            AddTool(tool);
        }

        ToolPrompt = BuildToolPrompt();
    }
    
    public void AddTool(ITool tool)
    {
        _lookUp.Add(tool.Name, tool);
        _tools.Add(tool);
        ToolPrompt = BuildToolPrompt();
    }

    private string BuildToolPrompt()
    {
        return _tools.Aggregate("Available Tools: \n", (current, tool) => current + $"- {tool.Name}: {tool.Description}\n Schema:\n{tool.InputFormat}");
    }

    public async Task<string?> ExecuteToolAsync(string toolName, string parametersJson)
    {
        if (_lookUp.TryGetValue(toolName, out var tool))
            return await tool.ExecuteAsync(parametersJson);
        return "Tool not found.";
    }

    private string GetTypeDef()
    {
        if (_tools.Count == 0)
            return "";
        else
        {
            var res = "\"tools\" : [\n";
            for (var i = 0; i < _tools.Count; i++)
            {
                var tool = _tools[i];
                res += tool.InputFormat;
                if (i != _tools.Count - 1)
                    res += ",\n";
            }

            res += "\n]";
            return res;
        }
    }
}
