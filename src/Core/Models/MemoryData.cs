namespace DotAgent.Core.Models;

public enum Memory { System, User, Assistant, Tool }

[Serializable]
public class MemoryData
{
    public required Memory Role { get; set; }
    public required MemoryContent Content { get; set; }
}

[Serializable]
public class MemoryContent
{
}

[Serializable]
public class ImageContent(string imageUrl,string text = "") : MemoryContent
{ public string ImageUrl = imageUrl; public string Text = text; }

[Serializable]
public class TextContent(string? text) : MemoryContent
{ public string? Text = text; }


[Serializable]
public class ToolCallContent(FunctionCall[] functionCall) : MemoryContent
{ public FunctionCall[] Calls = functionCall; }

[Serializable]
public class ToolResultContent(string toolCallId, string? result) : MemoryContent
{
    public string ToolCallId = toolCallId;
    public string? Result = result;
}
