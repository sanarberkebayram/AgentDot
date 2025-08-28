using DotAgent.Core.Tool;

namespace DotAgent.Core.Toolkit
{
    /// <summary>
    /// Defines the contract for a toolkit that manages and executes tools.
    /// </summary>
    public interface IToolkit
    {
        /// <summary>
        /// Gets a prompt string describing the available tools.
        /// </summary>
        public string ToolPrompt { get; }

        /// <summary>
        /// Gets the type definition for the tools, typically in a format suitable for code generation or API consumption.
        /// </summary>
        public string ToolTypeDef { get; }

        /// <summary>
        /// Adds a tool to the toolkit.
        /// </summary>
        /// <param name="tool">The tool to add.</param>
        public void AddTool(ITool tool);

        /// <summary>
        /// Executes a tool asynchronously by its name and provided parameters.
        /// </summary>
        /// <param name="toolName">The name of the tool to execute.</param>
        /// <param name="parametersJson">The JSON string representing the parameters for the tool.</param>
        /// <returns>A task that represents the asynchronous operation, returning the result of the tool execution.</returns>
        public Task<string?> ExecuteToolAsync(string toolName, string parametersJson);
    }
}

