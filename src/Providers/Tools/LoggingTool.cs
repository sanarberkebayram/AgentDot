using System.ComponentModel;
using DotAgent.Core.Tool;

namespace DotAgent.Providers.Tools
{
    /// <summary>
    /// Represents data for logging messages.
    /// </summary>
    [Serializable]
    public class LogData
    {
        /// <summary>
        /// Gets or sets the message to be logged.
        /// </summary>
        [Description("Message to log")]
        public string LogMessage { get; set; }
    }

    /// <summary>
    /// Provides a tool for logging messages to the console.
    /// </summary>
    public class LoggingTool : ToolBase<LogData>
    {
        public LoggingTool() : base("log_tool", "logs given message to console")
        {
        }

        /// <summary>
        /// Executes the logging operation.
        /// </summary>
        /// <param name="parameters">The log data containing the message to be logged.</param>
        /// <returns>A task that represents the asynchronous operation, returning "Log Success" on success or an error message.</returns>
        protected override Task<string?> Execute(LogData? parameters)
        {
            if (parameters == null)
                return Task.FromResult("Error: No Log Provided");

            Console.WriteLine(parameters.LogMessage);
            return Task.FromResult("Log Success");
        }
    }
}

