using System.Text.Json;
using DotAgent.Core.Util;

namespace DotAgent.Core.Tool
{
    /// <summary>
    /// Provides a base implementation for tools that can be executed by the agent.
    /// </summary>
    /// <typeparam name="TInputClass">The type of the input parameters for the tool.</typeparam>
    public abstract class ToolBase<TInputClass> : ITool
    {
        public ToolBase(string name, string description)
        {
            Name = name;
            Description = description;
            InputFormat = JsonUtilities.GenerateSchema(typeof(TInputClass), name, description);
        }
        /// <summary>
        /// Gets the name of the tool.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the tool.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the JSON schema defining the input format for the tool.
        /// </summary>
        public string InputFormat { get; protected set; }


        /// <summary>
        /// Validates the provided JSON parameters against the tool's input format.
        /// </summary>
        /// <param name="parametersJson">The JSON string representing the input parameters.</param>
        /// <returns>True if the parameters are valid, otherwise false.</returns>
        public bool ValidateInput(string parametersJson)
        {
            return JsonUtilities.ValidateJsonAgainstType<TInputClass>(parametersJson);
        }

        /// <summary>
        /// Executes the tool asynchronously with the provided JSON parameters.
        /// </summary>
        /// <param name="parametersJson">The JSON string representing the input parameters.</param>
        /// <returns>A task that represents the asynchronous operation, returning the result of the tool execution.</returns>
        public Task<string?> ExecuteAsync(string parametersJson)
        {
            if (!ValidateInput(parametersJson))
                return Task.FromResult("Invalid parameters for tool.");

            var deserialized = JsonSerializer.Deserialize<TInputClass>(parametersJson);
            if (deserialized == null)
                return Task.FromResult("Invalid parameters for tool.");

            return Execute(deserialized);
        }

        /// <summary>
        /// Executes the tool with the deserialized input parameters.
        /// </summary>
        /// <param name="parameters">The deserialized input parameters.</param>
        /// <returns>A task that represents the asynchronous operation, returning the result of the tool execution.</returns>
        protected abstract Task<string?> Execute(TInputClass? parameters);
    }
}

