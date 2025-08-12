namespace DotAgent.Core.Models;

[Serializable]
public class AgentResponse
{
    public string context;
    public AgentAction action;
    public string output;
}

[Serializable]
public class AgentAction
{
    public string type;
    public string name;
    public string parameters;
}

[Serializable]
public class AgentResponseNextStep
{
    public string[] suggestions;
    public bool userInputRequired;
    public bool runAfterToolUsage;
}

[Serializable]
public class AgentResponseError
{
    public string code;
    public string message;   
}