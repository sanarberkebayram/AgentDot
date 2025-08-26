using System.ComponentModel;
using DotAgent.Core.Tool;

namespace DotAgent.Providers.Tools;

[Serializable]
public class LogData
{
    [Description("Message to log")]
    public string LogMessage { get; set; }
}
public class LoggingTool() : ToolBase<LogData>("log_tool", "Logs message to console.")
{
    protected override Task<string?> Execute(LogData? parameters)
    {
        if (parameters == null)
            return Task.FromResult("Error: No Log Provided");
        
        Console.WriteLine(parameters.LogMessage);
        return Task.FromResult("Log Success");
    }
}