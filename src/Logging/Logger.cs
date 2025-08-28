namespace DotAgent.Logging
{
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

        public static async Task LogAsync(LogType logType, string title, string content)
        {
            var logContent = $"#[{logType.ToString().ToUpper()}] {title}:\n{content}{LogSeparator}";
            Console.WriteLine(logContent);
            await File.AppendAllTextAsync(SessionLogFileName, logContent);
        }

        public enum LogType
        {
            Info,
            Warning,
            Error
        }
    }
}
