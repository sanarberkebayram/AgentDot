using DotAgent.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotAgent.Implementations;

public class ChatGPTEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public ChatGPTEmbeddingGenerator(string? apiKey = null, string model = "text-embedding-3-small")
    {
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new ArgumentNullException(nameof(apiKey), "OpenAI API Key is not provided and OPENAI_API_KEY environment variable is not set.");
        _model = model;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            input = text,
            model = _model,
            encoding_format = "float"
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        using (JsonDocument doc = JsonDocument.Parse(responseBody))
        {
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("data", out JsonElement dataArray) && dataArray.EnumerateArray().Any())
            {
                JsonElement firstDataItem = dataArray.EnumerateArray().First();
                if (firstDataItem.TryGetProperty("embedding", out JsonElement embeddingArray))
                {
                    return embeddingArray.EnumerateArray().Select(e => e.GetSingle()).ToArray();
                }
            }
        }
        throw new InvalidOperationException("Failed to retrieve embedding from OpenAI API response.");
    }
}
