using DotAgent.Core.Tool;
using DotAgent.Implementations;
using DotAgent.Interfaces;

public class Program
{
    public static async Task Main(string[] args)
    {
        // IMPORTANT: Replace "YOUR_OPENAI_API_KEY" with your actual OpenAI API Key,
        // or set it as an environment variable named OPENAI_API_KEY.
        // For example: Environment.SetEnvironmentVariable("OPENAI_API_KEY", "sk-...");
        string? openAiApiKey =
            "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";

        if (string.IsNullOrEmpty(openAiApiKey))
        {
            Console.WriteLine("OpenAI API Key not found. Please set the OPENAI_API_KEY environment variable or replace 'YOUR_OPENAI_API_KEY' in Program.cs.");
            return;
        }

        // 1. Configure connector and tools
        var llmConnector = new OpenAiConnector(openAiApiKey);
        var embeddingGenerator = new ChatGPTEmbeddingGenerator(openAiApiKey);
        var fileManagerTool = new FileManager();

        // 2. Create and configure individual agents
        var writerAgent = new Agent(
            "file_manager_tool",
            "you are a file manager tool that can create, read, update, and delete files. Also you can log messages to the console.",
            null, // Let agent create its own VectorMemory
            embeddingGenerator,
            new List<ITool> { fileManagerTool, new ConsoleLogger() }
        );

        // 3. Connect agents to the LLM
        writerAgent.Connect(llmConnector);

        // 4. Create and configure the orchestrator
        var orchestrator = new Orchestrator(
            "orchestrator",
            "You are a multi agent system that manages multiple agents and orchestrates their tasks. Your job is to coordinate the agents and execute complex tasks.",
            null, // Let orchestrator create its own VectorMemory
            embeddingGenerator
        );
        orchestrator.AddAgent(writerAgent);
        orchestrator.Connect(llmConnector);

        // 5. Execute a complex task
        string userRequest = "write a txt file 'berke.txt' with the contents: Hello World!" ;
        Console.WriteLine($"Executing task: {userRequest}\n");
        string result = await orchestrator.ExecuteAsync(userRequest);

        Console.WriteLine($"\nTask completed. Final result: {result}");
    }
}
