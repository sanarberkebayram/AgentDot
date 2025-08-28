using DotAgent.Core.Tool;

namespace DotAgent.Core.Toolkit
{
    /// <summary>
    /// Manages a collection of tools that an agent can utilize.
    /// </summary>
    public class Toolkit : IToolkit
    {
        /// <summary>
        /// Gets a prompt string describing the available tools.
        /// </summary>
        public string ToolPrompt { get; private set; }

        /// <summary>
        /// Gets the type definition for the tools, typically in a format suitable for code generation or API consumption.
        /// </summary>
        public string ToolTypeDef => GetTypeDef();

        private readonly List<ITool> _tools = new();
        private readonly Dictionary<string, ITool> _lookUp = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Toolkit"/> class with no tools.
        /// </summary>
        public Toolkit()
        {
            ToolPrompt = "No tools available.";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Toolkit"/> class with a predefined list of tools.
        /// </summary>
        /// <param name="tools">A read-only list of tools to include in the toolkit.</param>
        public Toolkit(IReadOnlyList<ITool> tools)
        {
            foreach (var tool in tools)
            {
                AddTool(tool);
            }

            ToolPrompt = BuildToolPrompt();
        }

        /// <summary>
        /// Adds a tool to the toolkit.
        /// </summary>
        /// <param name="tool">The tool to add.</param>
        public void AddTool(ITool tool)
        {
            _lookUp.Add(tool.Name, tool);
            _tools.Add(tool);
            ToolPrompt = BuildToolPrompt();
        }

        /// <summary>
        /// Builds the tool prompt string based on the currently available tools.
        /// </summary>
        /// <returns>A string describing the available tools and their schemas.</returns>
        private string BuildToolPrompt()
        {
            return _tools.Aggregate("Available Tools: \n", (current, tool) => current + $"- {tool.Name}: {tool.Description}\n Schema:\n{tool.InputFormat}");
        }

        /// <summary>
        /// Executes a tool asynchronously by its name and provided parameters.
        /// </summary>
        /// <param name="toolName">The name of the tool to execute.</param>
        /// <param name="parametersJson">The JSON string representing the parameters for the tool.</param>
        /// <returns>A task that represents the asynchronous operation, returning the result of the tool execution, or "Tool not found." if the tool does not exist.</returns>
        public async Task<string?> ExecuteToolAsync(string toolName, string parametersJson)
        {
            if (_lookUp.TryGetValue(toolName, out var tool))
                return await tool.ExecuteAsync(parametersJson);
            return "Tool not found.";
        }

        /// <summary>
        /// Generates the type definition string for all tools in the toolkit.
        /// </summary>
        /// <returns>A JSON string representing the type definitions of the tools.</returns>
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
}

