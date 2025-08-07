using System.IO;
using System.Threading.Tasks;

namespace DotAgent.Implementations;

public static class Logger
{
    private const string LogDirectory = "logs";
    private static readonly string SessionLogFileName;
    private const string LogSeparator = "\n\n---\n\n"; // Separator for log entries

    static Logger()
    {
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }
        // Generate a unique file name for the current session
        SessionLogFileName = Path.Combine(LogDirectory, $"session_log_{DateTime.Now:yyyyMMdd_HHmmss}.md");
    }

    public static async Task LogAsync(string title, string content)
    {
        var logContent = $"# {title}\n\n{content}{LogSeparator}";
        await File.AppendAllTextAsync(SessionLogFileName, logContent);
    }
}
