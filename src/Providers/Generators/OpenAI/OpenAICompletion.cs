using DotAgent.Core.Generator;
using DotAgent.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DotAgent.Providers.Generators.OpenAI;

/// <summary>
/// Provides completion services using the OpenAI API.
/// </summary>
public class OpenAiCompletion(string apiKey, string model = "gpt-5-nano") : GeneratorBase
{
    private static readonly HttpClient Http = new();

    /// <summary>
    /// Generates a response from the OpenAI API based on the provided request.
    /// </summary>
    /// <param name="request">The generator request containing memory and toolkit information.</param>
    /// <returns>A <see cref="GenerationResponse"/> containing the generated message or function calls.</returns>
    protected override async Task<GenerationResponse> GenerateResponse(GeneratorRequest request)
    {
        try
        {
            if (request.Memory == null)
                throw new Exception("Memory is not set.");
            
            var msg = await request.Memory.GetHistoryAsync();
            var messages = msg.Select<MemoryData, object>(m =>
            {
                if (m.Content is ToolResultContent resContent)
                {
                    return new
                    {
                        role = m.Role.ToString().ToLower(), // system, user, assistant, tool
                        content = ConvertContent(m.Content),
                        tool_call_id = resContent.ToolCallId,
                    };
                }

                if (m.Content is ToolCallContent callContent)
                {
                    return new
                    {
                        role = m.Role.ToString().ToLower(), // system, user, assistant, tool
                        content = ConvertContent(m.Content),
                        tool_calls = ConvertToolCalls(callContent)
                    };
                }

                return new
                {
                    role = m.Role.ToString().ToLower(), // system, user, assistant, tool
                    content = ConvertContent(m.Content)
                };
            }).ToList();

            object payload = new
            {
                model ,
                messages,
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            if (request.Toolkit != null)
                json = ReplaceLastClosingBrace(json, request.Toolkit.ToolTypeDef);

            
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Http.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            var root = doc.RootElement;
            if (!root.TryGetProperty("error", out var err))
            {
                return new GenerationResponse
                {
                    Error = !string.IsNullOrWhiteSpace(err.GetString()) ? "OpenAICompletion Error: "+err.GetString():"OpenAICompletion Error:  Unexpected Error."
                };
            }
            var choice = root.GetProperty("choices")[0].GetProperty("message");

            var genResponse = new GenerationResponse();
            // Case 1: Function/Tool calls
            if (choice.TryGetProperty("tool_calls", out var toolCalls))
            {
                genResponse.FunctionCalls = toolCalls.EnumerateArray()
                    .Select(tc => new FunctionCall
                    {
                        Id = tc.GetProperty("id").GetString() ?? "",
                        Name = tc.GetProperty("function").GetProperty("name").GetString() ?? "",
                        Parameters = tc.GetProperty("function").GetProperty("arguments").GetString() ?? ""
                    })
                    .ToArray();
            }
            // Case 2: Normal text response
            else if (choice.TryGetProperty("content", out var contentProp))
            {
                genResponse.Message = contentProp.GetString() ?? "";
            }

            return genResponse;
        }
        catch (Exception ex)
        {
            return new GenerationResponse
            {
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Converts a <see cref="ToolCallContent"/> object into an anonymous object suitable for OpenAI API.
    /// </summary>
    /// <param name="callContent">The tool call content to convert.</param>
    /// <returns>An anonymous object representing the tool call.</returns>
    private object ConvertToolCalls(ToolCallContent callContent)
    {
        return callContent.Calls.Select(content => new
        {
            type = "function",
            id = content.Id,
            function = new
            {
                name = content.Name,
                arguments = content.Parameters
            }
        }).ToArray();
    }

    /// <summary>
    /// Converts a <see cref="MemoryContent"/> object into an anonymous object suitable for OpenAI API.
    /// </summary>
    /// <param name="content">The memory content to convert.</param>
    /// <returns>An anonymous object representing the content.</returns>
    private static object ConvertContent(MemoryContent content) =>
        (content switch
        {
            TextContent t => new[]
            {
                new { type = "text", text = t.Text }
            },
            ImageContent i => new object[]
            {
                new { type = "text", text = i.Text },
                new { type = "image_url", image_url = new { url = i.ImageUrl } }
            },
            ToolCallContent _ => null,
            ToolResultContent tr => new[]
            {
                new { type = "text",  text = tr.Result } 
            },
            _ => new object[] { }
        })!;
    
    /// <summary>
    /// Replaces the last closing brace in a string with a specified replacement string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="replacement">The string to replace the last closing brace with.</param>
    /// <returns>The modified string.</returns>
    private static string ReplaceLastClosingBrace(string input, string replacement)
    {
        var index = input.LastIndexOf('}');
        return index == -1 ? input : string.Concat(input.AsSpan(0, index),
",", replacement, "}");
    }
}
