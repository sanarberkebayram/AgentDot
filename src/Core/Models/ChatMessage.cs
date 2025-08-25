namespace DotAgent.Models;

public enum ChatMessageRole { System, User, Assistant, Tool }

[Serializable]
public class ChatMessage
{
    public ChatMessageRole Role { get; set; }
    public string? Content { get; set; }
    public float[]? Vector { get; set; }
}

[Serializable]
public class ToolMessage : ChatMessage
{
    public string? ToolCallId;
}
