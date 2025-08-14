namespace DotAgent.Core.Models;

[Serializable]
public class FunctionCall
{
    public string Name { get; set; }
    public string Parameters { get; set; }
    public string Id { get; set; }
}
[Serializable]
public class GenerationResponse
{
    public ResponseType Type { get; set; }
    public FunctionCall[] FunctionCalls { get; set; }
    public string Message { get; set; } = "";
    public string Error { get; set; } = "";
}


public enum ResponseType
{
    ToolCalling = 0,
    Text = 1,
    Image = 2,
    Audio = 3,
    Video = 4,
}