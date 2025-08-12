using System.Text.Json;
using DotAgent.Core.Util;

namespace DotAgent.Core.Tool;
public abstract class ToolBase<TInputClass> : ITool
{
    public string Name { get; }
    public string Description { get; }
    public string InputFormat { get; }

    protected ToolBase(string name, string description)
    {
        Name = name;
        Description = description;
        InputFormat = JsonUtilities.GenerateSchema(typeof(TInputClass), name, description);
    }


    public bool ValidateInput(string parametersJson)
    {
        return JsonUtilities.ValidateJsonAgainstType<TInputClass>(parametersJson);
    }

    public Task<string> ExecuteAsync(string parametersJson)
    {
        if (!ValidateInput(parametersJson))
           return Task.FromResult("Invalid parameters for tool.");
        
        var deserialized = JsonSerializer.Deserialize<TInputClass>(parametersJson);
        if (deserialized == null)
            return Task.FromResult("Invalid parameters for tool.");
            
        return Execute(deserialized);
    }

    protected abstract Task<string> Execute(TInputClass? parameters);
}