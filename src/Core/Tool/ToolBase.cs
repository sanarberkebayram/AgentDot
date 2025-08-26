using System.Text.Json;
using DotAgent.Core.Util;

namespace DotAgent.Core.Tool;
public abstract class ToolBase<TInputClass>(string name, string description) : ITool
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string InputFormat { get; protected set; } = JsonUtilities.GenerateSchema(typeof(TInputClass), name, description);


    public bool ValidateInput(string parametersJson)
    {
        return JsonUtilities.ValidateJsonAgainstType<TInputClass>(parametersJson);
    }

    public Task<string?> ExecuteAsync(string parametersJson)
    {
        if (!ValidateInput(parametersJson))
           return Task.FromResult("Invalid parameters for tool.");
        
        var deserialized = JsonSerializer.Deserialize<TInputClass>(parametersJson);
        if (deserialized == null)
            return Task.FromResult("Invalid parameters for tool.");
            
        return Execute(deserialized);
    }

    protected abstract Task<string?> Execute(TInputClass? parameters);
}