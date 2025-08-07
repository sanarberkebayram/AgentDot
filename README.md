# DotAgent

## Description
DotAgent is a modular C# AI agent framework designed to facilitate the creation and orchestration of AI agents. It provides a foundational structure for building intelligent systems with support for various components like connectors to Large Language Models (LLMs), memory management, planning, and tool usage.

## Features
-   **Modular Design:** Components are loosely coupled and replaceable, promoting flexibility and extensibility.
-   **Extensible:** Easily add new tools, agents, and connectors.
-   **Intuitive API:** Simple and clear API for developing and running agents.
-   **Single & Multi-Agent Support:** Supports both standalone agents and complex multi-agent orchestrations.
-   **Tool Calling:** Agents can utilize external tools to perform actions.
-   **Memory Management:** Includes capabilities for conversational history and semantic search.
-   **Planning:** Agents can generate step-by-step plans to achieve complex goals.
-   **Logging:** Detailed markdown logs of agent actions and decisions.

## Getting Started

### Prerequisites
-   .NET 8 SDK (or newer stable version)
-   An OpenAI API Key (for `OpenAiConnector` and `ChatGPTEmbeddingGenerator`)

### Installation
1.  Clone the repository:
    ```bash
    git clone https://github.com/your-repo/DotAgent.git
    cd DotAgent
    ```
2.  Build the project:
    ```bash
    dotnet build
    ```

### Configuration
Set your OpenAI API key as an environment variable:

```bash
export OPENAI_API_KEY="YOUR_API_KEY"
```

Alternatively, you can pass the API key directly to the `OpenAiConnector` and `ChatGPTEmbeddingGenerator` constructors.

## Usage Example

The following example demonstrates how to configure and run a multi-agent orchestration to solve a complex task.

```csharp
using DotAgent.Implementations;
using DotAgent.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ExampleUsage
{
    public static async Task RunAgent()
    {
        // 1. Configure connector and tools
        // Dependencies are instantiated. API keys would come from config/env variables.
        var llmConnector = new OpenAiConnector(); // API key from environment variable
        var embeddingGenerator = new ChatGPTEmbeddingGenerator(); // API key from environment variable
        var fileManagerTool = new FileManager();
        var consoleLoggerTool = new ConsoleLogger();

        // 2. Create and configure individual agents
        // Each agent gets a specific persona (system prompt) and a list of tools.
        var researchAgent = new Agent(
            "researcher",
            "You are a world-class researcher. Your job is to find information.",
            null, // Let agent create its own VectorMemory
            embeddingGenerator,
            new List<ITool> { fileManagerTool, consoleLoggerTool }
        );
        var writerAgent = new Agent(
            "writer",
            "You are a professional technical writer. Your job is to write content to files.",
            null, // Let agent create its own VectorMemory
            embeddingGenerator,
            new List<ITool> { fileManagerTool, consoleLoggerTool }
        );

        // 3. Connect agents to the LLM
        researchAgent.Connect(llmConnector);
        writerAgent.Connect(llmConnector);

        // 4. Create and configure the orchestrator
        // The orchestrator is a specialized agent that manages other agents.
        var orchestrator = new Orchestrator(
            "project_manager",
            "You are a project manager. Your goal is to use your team of agents to solve problems.",
            null, // Let orchestrator create its own VectorMemory
            embeddingGenerator
        );
        orchestrator.AddAgent(researchAgent);
        orchestrator.AddAgent(writerAgent);
        orchestrator.Connect(llmConnector);

        // 5. Execute a complex task
        // The orchestrator's planner will break this down, route sub-tasks to the
        // correct agent, and synthesize the results.
        string userRequest = "Research the key features of .NET 9 and write the findings to a file named 'dotnet9_features.md'.";
        string result = await orchestrator.ExecuteAsync(userRequest);

        Console.WriteLine(result); // e.g., "Task completed. Results written to dotnet9_features.md."
    }
}
```

## Core Components

### `IAgent`
Represents an autonomous entity capable of executing tasks, maintaining memory, and interacting with an LLM.

### `ITool`
Defines a capability that an agent can use, such as interacting with a file system or making web requests. Each tool has a name, description, and a structured way to declare input parameters.

### `IConnector`
Provides the interface for connecting to and interacting with Large Language Models (LLMs), supporting both text generation and tool-calling functionalities.

### `IMemory`
Handles the storage and retrieval of conversational history, including summarization and semantic search capabilities.

### `IPlanner`
The component responsible for generating a step-by-step plan for an agent to achieve a given goal.

### `IOrchestrator`
A specialized `IAgent` that manages and coordinates the activities of multiple other agents to accomplish complex tasks.
