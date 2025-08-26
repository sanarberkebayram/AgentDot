# DotAgent

## Description
DotAgent is a modular C# AI agent framework designed to facilitate the creation and orchestration of AI agents. It provides a foundational structure for building intelligent systems with support for various components like connectors to Large Language Models (LLMs), memory management, and tool usage.

## Features
-   **Modular Design:** Components are loosely coupled and replaceable, promoting flexibility and extensibility.
-   **Extensible:** Easily add new tools, agents, and generators.
-   **Intuitive API:** Simple and clear API for developing and running agents.
-   **Single & Multi-Agent Support:** Supports both standalone agents and complex multi-agent orchestrations.
-   **Tool Calling:** Agents can utilize external tools to perform actions.
-   **Memory Management:** Includes capabilities for conversational history.
-   **Logging:** Detailed markdown logs of agent actions and decisions.

## Getting Started

### Prerequisites
-   .NET 8 SDK (or newer stable version)
-   An OpenAI API Key

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
You can pass the API key directly to the `OpenAiCompletion` constructor.

## Usage Example

The following example demonstrates how to configure and run an agent.

```csharp
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
```

## Core Components

### `IAgent`
Represents an autonomous entity capable of executing tasks, maintaining memory, and interacting with an LLM.

### `ITool`
Defines a capability that an agent can use, such as interacting with a file system or making web requests. Each tool has a name, description, and a structured way to declare input parameters.

### `IGenerator`
Provides the interface for connecting to and interacting with Large Language Models (LLMs), supporting both text generation and tool-calling functionalities.

### `IMemory`
Handles the storage and retrieval of conversational history.

### `IOrchestration`
A specialized `IAgent` that manages and coordinates the activities of multiple other agents to accomplish complex tasks.

### `IToolkit`
Manages the tools available to an agent.

## Documentation

For more detailed documentation on the framework and its components, please see [Documentation.md](Documentation.md).