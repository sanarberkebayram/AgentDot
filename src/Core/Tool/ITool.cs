namespace DotAgent.Core.Tool
{
    /// <summary>
    /// Defines the contract for a tool that can be executed by the agent.
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Gets the name of the tool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the tool.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the JSON schema defining the input format for the tool.
        /// </summary>
        string InputFormat { get; }

        /// <summary>
        /// Validates the provided JSON parameters against the tool's input format.
        /// </summary>
        /// <param name="parametersJson">The JSON string representing the input parameters.</param>
        /// <returns>True if the parameters are valid, otherwise false.</returns>
        protected bool ValidateInput(string parametersJson);

        /// <summary>
        /// Executes the tool asynchronously with the provided JSON parameters.
        /// </summary>
        /// <param name="parametersJson">The JSON string representing the input parameters.</param>
        /// <returns>A task that represents the asynchronous operation, returning the result of the tool execution.</returns>
        Task<string?> ExecuteAsync(string parametersJson);
    }
}
