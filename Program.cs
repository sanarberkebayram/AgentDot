using DotAgent.Core.Agent;
using DotAgent.Core.Generator;
using DotAgent.Providers.Generators.OpenAI;
using DotAgent.Providers.Tools;

public class Program
{
    public static async Task Main(string[] args)
    {
            IGenerator generator = new OpenAiCompletion("open-ai-api-key");
            var agent = new AgentDefault("test-agent", "You are an agent",generator);
            agent.Toolkit.AddTool(new LoggingTool());
            var answer = await agent.ProcessMessageAsync("Log the the President of the United States?");
            Console.WriteLine(answer);
    }
}
