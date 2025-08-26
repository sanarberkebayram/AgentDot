# DotAgent Documentation

This document provides a detailed overview of the DotAgent framework, its core components, and how to use them.

## Core Components

### 1. Agent (`IAgent`, `AgentBase`, `AgentDefault`)

The agent is the fundamental entity in the framework, responsible for processing messages, managing memory, and utilizing tools.

-   **`IAgent`**: The interface defining the essential properties and methods of an agent, including `Id`, `SystemPrompt`, `Memory`, `Toolkit`, and the `ProcessMessageAsync` method.

-   **`AgentBase`**: An abstract class that provides a base implementation for `IAgent`. It handles the construction of the agent's system prompt by combining the user-provided prompt with the tool prompts from the toolkit.

-   **`AgentDefault`**: A concrete implementation of `AgentBase`. It orchestrates the interaction between the generator, memory, and toolkit. When `ProcessMessageAsync` is called, it sends the message to the generator, handles the generator's response (which can be a message or a tool call), and manages the conversation history.

### 2. Generator (`IGenerator`, `GeneratorBase`, `OpenAICompletion`)

The generator is responsible for interacting with a Large Language Model (LLM) to generate responses.

-   **`IGenerator`**: The interface for all generators, defining the `GenerateAsync` method.

-   **`GeneratorBase`**: An abstract class that provides a base implementation for `IGenerator`, including error handling and logging.

-   **`OpenAICompletion`**: A concrete implementation of `GeneratorBase` that connects to the OpenAI API. It takes an API key and a model name in its constructor. It serializes the conversation history and tool definitions into the format expected by the OpenAI API.

### 3. Memory (`IMemory`, `MemoryBase`)

The memory component stores and retrieves the conversation history.

-   **`IMemory`**: The interface for memory components, defining methods to add messages, get the history, and change the system prompt.

-   **`MemoryBase`**: A concrete implementation of `IMemory` that stores the conversation history in a list.

### 4. Tool (`ITool`, `ToolBase`, `LoggingTool`, `AgentTool`)

Tools are capabilities that an agent can use to interact with the external world.

-   **`ITool`**: The interface for all tools, defining properties for `Name`, `Description`, and `InputFormat`, and methods for `ValidateInput` and `ExecuteAsync`.

-   **`ToolBase<TInputClass>`**: An abstract class that simplifies the creation of new tools. It handles the JSON schema generation for the tool's input, as well as the deserialization and validation of the input parameters.

-   **`LoggingTool`**: A simple tool that logs a message to the console.

-   **`AgentTool`**: A special tool that allows an agent to be used as a tool by another agent (typically an orchestrator).

### 5. Toolkit (`IToolkit`, `Toolkit`)

The toolkit manages the tools available to an agent.

-   **`IToolkit`**: The interface for toolkits, defining properties for `ToolPrompt` and `ToolTypeDef`, and methods to `AddTool` and `ExecuteToolAsync`.

-   **`Toolkit`**: A concrete implementation of `IToolkit`. It builds the `ToolPrompt` that is injected into the agent's system prompt and the `ToolTypeDef` that is sent to the LLM for function calling.

### 6. Orchestration (`IOrchestration`, `OrchestrationBase`)

Orchestration allows for the coordination of multiple agents to accomplish complex tasks.

-   **`IOrchestration`**: The interface for orchestrators, which extends `IAgent` and adds a method to `AddAgent`.

-   **`OrchestrationBase`**: A concrete implementation of `IOrchestration`. It uses the `AgentTool` to treat sub-agents as tools.

### 7. Models

The framework uses several models to structure data:

-   **`GenerationResponse`**: Represents the response from the generator, which can contain a message and/or function calls.
-   **`GeneratorRequest`**: Represents the request sent to the generator, containing the memory and toolkit.
-   **`MemoryData`**: Represents a single message in the conversation history, including the role (system, user, assistant, or tool) and the content.

### 8. Utilities (`JsonUtilities`)

-   **`JsonUtilities`**: A static class that provides helper methods for working with JSON, including generating JSON schemas from C# classes and validating JSON strings against a given type.

### 9. Logging (`Logger`)

-   **`Logger`**: A static class for logging messages to the console and to a session-specific log file in the `logs` directory.

## How It Works

1.  An **`Agent`** is created with a **`Generator`**, **`Memory`**, and **`Toolkit`**.
2.  The `Toolkit` contains a list of **`Tools`**.
3.  When the agent's `ProcessMessageAsync` method is called, the message is added to the `Memory`.
4.  The `Agent` then calls the `Generator`'s `GenerateAsync` method, passing the `Memory` and `Toolkit`.
5.  The `Generator` sends the conversation history and tool definitions to the LLM.
6.  The LLM can either return a text message or a request to call one or more tools.
7.  If the LLM returns a message, the `Agent` returns the message to the user.
8.  If the LLM requests a tool call, the `Agent` uses the `Toolkit` to execute the tool.
9.  The result of the tool execution is added to the `Memory`, and the `Generator` is called again. This loop continues until the LLM returns a message.

## Implementation Examples

### Creating a Custom Tool

To create a custom tool, you need to create a class that inherits from `ToolBase<TInputClass>` and provide an implementation for the `Execute` method. `TInputClass` is a class that defines the input parameters for your tool.

```csharp
using System.ComponentModel;
using DotAgent.Core.Tool;

// 1. Define the input parameters for the tool
[Serializable]
public class FileWriteData
{
    [Description("Path to the file")]
    public string FilePath { get; set; } // Parameters must be defined with getter&setters

    [Description("Content to write to the file")]
    public string Content { get; set; }
}

// 2. Create the tool by inheriting from ToolBase
public class FileWriteTool() : ToolBase<FileWriteData>("file_write_tool", "Writes content to a file.")
{
    protected override Task<string?> Execute(FileWriteData? parameters)
    {
        if (parameters == null)
            return Task.FromResult("Error: No parameters provided");

        try
        {
            File.WriteAllText(parameters.FilePath, parameters.Content);
            return Task.FromResult("File written successfully.");
        }
        catch (Exception ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }
    }
}
```

### Creating a Custom Agent

You can create a custom agent by inheriting from `AgentBase` and implementing the `ProcessMessageAsync` and `HandleGeneratorResponse` methods.

```csharp
using DotAgent.Core.Agent;
using DotAgent.Core.Generator;
using DotAgent.Core.Models;

public class MyCustomAgent : AgentBase
{
    public MyCustomAgent(string id, string? systemPrompt, IGenerator generator) : base(id, systemPrompt, new MemoryBase(systemPrompt), new Toolkit(), generator)
    {
    }

    public override async Task<string?> ProcessMessageAsync(string? message)
    {
        Memory.AddMessage(new MemoryData() { Role = Models.Memory.User, Content = new TextContent(message) });
        var response = await Generator.GenerateAsync(new GeneratorRequest() { Memory = Memory, Toolkit = Toolkit });
        return await HandleGeneratorResponse(response);
    }

    protected override Task<string?> HandleGeneratorResponse(GenerationResponse response)
    {
        if (!string.IsNullOrWhiteSpace(response.Error))
            return Task.FromResult(response.Error);

        if (response.FunctionCalls is { Length: > 0 })
        {
            // Handle function calls if needed
        }

        return Task.FromResult(response.Message);
    }
}
```

### Using the Orchestrator

The orchestrator can be used to manage multiple agents.

```csharp
using DotAgent.Core.Generator;
using DotAgent.Core.Orchestration;
using DotAgent.Providers.Generators.OpenAI;

// 1. Create a generator
IGenerator generator = new OpenAiCompletion("open-ai-api-key");

// 2. Create an orchestrator
var orchestrator = new OrchestrationBase(generator);

// 3. Create and add agents to the orchestrator
var agent1 = new AgentDefault("researcher", "You are a researcher.", generator);
var agent2 = new AgentDefault("writer", "You are a writer.", generator);

orchestrator.AddAgent(agent1);
orchestrator.AddAgent(agent2);

// 4. Process a message with the orchestrator
var result = await orchestrator.ProcessMessageAsync("Research the latest AI trends and write a blog post about them.");
Console.WriteLine(result);
```
