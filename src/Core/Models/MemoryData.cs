namespace DotAgent.Core.Models
{
    /// <summary>
    /// Defines the roles of participants in a conversation for memory purposes.
    /// </summary>
    public enum Memory { System, User, Assistant, Tool }

    /// <summary>
    /// Represents a single piece of data stored in memory, including its role and content.
    /// </summary>
    [Serializable]
    public class MemoryData
    {
        /// <summary>
        /// Gets or sets the role of the entity associated with this memory data (e.g., System, User, Assistant, Tool).
        /// </summary>
        public required Memory Role { get; set; }

        /// <summary>
        /// Gets or sets the content of the memory data.
        /// </summary>
        public required MemoryContent Content { get; set; }
    }

    /// <summary>
    /// Base class for all memory content types.
    /// </summary>
    [Serializable]
    public class MemoryContent
    {
    }

    /// <summary>
    /// Represents image content in memory.
    /// </summary>
    [Serializable]
    public class ImageContent(string imageUrl, string text = "") : MemoryContent
    {
        /// <summary>
        /// The URL of the image.
        /// </summary>
        public string ImageUrl = imageUrl;

        /// <summary>
        /// Optional text description for the image.
        /// </summary>
        public string Text = text;
    }

    /// <summary>
    /// Represents text content in memory.
    /// </summary>
    [Serializable]
    public class TextContent : MemoryContent
    {
        public TextContent(string? text)
        {
            Text = text;
        }
        /// <summary>
        /// The text content.
        /// </summary>
        public string? Text;
    }


    /// <summary>
    /// Represents tool call content in memory.
    /// </summary>
    [Serializable]
    public class ToolCallContent : MemoryContent
    {
        public ToolCallContent(FunctionCall[] functionCall)
        {
            Calls = functionCall;
        }
        /// <summary>
        /// An array of function calls.
        /// </summary>
        public FunctionCall[] Calls;
    }

    /// <summary>
    /// Represents tool result content in memory.
    /// </summary>
    [Serializable]
    public class ToolResultContent : MemoryContent
    {
        public ToolResultContent(string toolCallId, string? result)
        {
            ToolCallId = toolCallId;
            Result = result;
        }
        /// <summary>
        /// The ID of the tool call that produced this result.
        /// </summary>
        public string ToolCallId;

        /// <summary>
        /// The result of the tool execution.
        /// </summary>
        public string? Result;
    }
}
