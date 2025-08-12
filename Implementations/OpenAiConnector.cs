using DotAgent.Interfaces;
using DotAgent.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DotAgent.Core.Tool;
using DotAgent.Logging;

namespace DotAgent.Implementations;

public class OpenAiConnector : IConnector
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiConnector(string? apiKey = null, string model = "gpt-4o-mini-2024-07-18")
    {
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new ArgumentNullException(nameof(apiKey), "OpenAI API Key is not provided and OPENAI_API_KEY environment variable is not set.");
        _model = model;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GenerateTextAsync(string prompt, IReadOnlyList<ChatMessage> history)
    {
        var messages = history.Select(m => new OpenAiChatMessage { Role = m.Role.ToString().ToLower(), Content = m.Content }).ToList();
        messages.Add(new OpenAiChatMessage { Role = "user", Content = prompt });

        var requestBody = new
        {
            model = _model,
            messages = messages
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        // response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        Logger.LogAsync("OpenAI Response", responseBody);
        using (JsonDocument doc = JsonDocument.Parse(responseBody))
        {
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("choices", out JsonElement choicesArray) && choicesArray.EnumerateArray().Any())
            {
                JsonElement firstChoice = choicesArray.EnumerateArray().First();
                if (firstChoice.TryGetProperty("message", out JsonElement messageElement) && messageElement.TryGetProperty("content", out JsonElement contentElement))
                {
                    return contentElement.GetString() ?? string.Empty;
                }
            }
        }
        throw new InvalidOperationException("Failed to retrieve text generation from OpenAI API response.");
    }

    public async Task<ToolCallResult> InvokeToolCallingAsync(string prompt, IReadOnlyList<ITool> tools, IReadOnlyList<ChatMessage> history)
    {
        var messages = history.Select(m => new OpenAiChatMessage { Role = m.Role.ToString().ToLower(), Content = m.Content }).ToList();
        messages.Add(new OpenAiChatMessage { Role = "user", Content = prompt });

        var openAiTools = tools.Select(t => new OpenAiTool
        {
            Type = "function",
            Function = new OpenAiToolFunction
            {
                Name = t.Name,
                Description = t.Description,
                Parameters = new OpenAiToolFunctionParameters
                {
                    Properties = t.GetParameters().Where(p => p.Name != null).ToDictionary(p => p.Name!,
                        p => new OpenAiToolFunctionProperty { Type = p.Type, Description = p.Description }),
                    Required = t.GetParameters().Where(p => p.IsRequired && p.Name != null).Select(p => p.Name!).ToList()
                }
            }
        }).ToList();

        var requestBody = new
        {
            model = _model,
            messages = messages,
            tools = openAiTools,
            tool_choice = "auto"
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;
        
        if (!root.TryGetProperty("choices", out JsonElement choicesArray) || !choicesArray.EnumerateArray().Any())
            throw new InvalidOperationException("Failed to retrieve tool call from OpenAI API response.");
        
        var firstChoice = choicesArray.EnumerateArray().First();
        if (!firstChoice.TryGetProperty("message", out JsonElement messageElement) ||
            !messageElement.TryGetProperty("tool_calls", out JsonElement toolCallsArray) ||
            !toolCallsArray.EnumerateArray().Any())
            throw new InvalidOperationException("Failed to retrieve tool call from OpenAI API response.");
        
        var firstToolCall = toolCallsArray.EnumerateArray().First();
        if (!firstToolCall.TryGetProperty("function", out JsonElement functionElement))
            throw new InvalidOperationException("Failed to retrieve tool call from OpenAI API response.");
        
        string? toolName = null;
        JsonElement parameters = default;

        if (functionElement.TryGetProperty("name", out JsonElement nameElement))
        {
            toolName = nameElement.GetString();
        }
        if (functionElement.TryGetProperty("arguments", out JsonElement argumentsElement))
        {
            parameters = JsonDocument.Parse(argumentsElement.GetString() ?? "{}").RootElement;
        }

        return new ToolCallResult
        {
            ToolName = toolName,
            Parameters = parameters,
            ThoughtProcess = "Tool call generated by OpenAI."
        };

    }

    private class OpenAiChatMessage
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private class OpenAiTool
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("function")]
        public OpenAiToolFunction? Function { get; set; }
    }

    private class OpenAiToolFunction
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("parameters")]
        public OpenAiToolFunctionParameters? Parameters { get; set; }
    }

    private class OpenAiToolFunctionParameters
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";
        [JsonPropertyName("properties")]
        public Dictionary<string, OpenAiToolFunctionProperty>? Properties { get; set; }
        [JsonPropertyName("required")]
        public List<string>? Required { get; set; }
    }

    private class OpenAiToolFunctionProperty
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
