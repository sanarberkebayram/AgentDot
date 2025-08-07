using DotAgent.Interfaces;
using DotAgent.Models;

namespace DotAgent.Implementations;

public class VectorMemory : IMemory
{
    private readonly List<ChatMessage> _messages = new();
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public VectorMemory(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    public async void AddMessage(ChatMessage message)
    {
        if (message.Content != null)
        {
            // message.Vector = await _embeddingGenerator.GenerateEmbeddingAsync(message.Content);
        }
        _messages.Add(message);
    }

    public Task<IReadOnlyList<ChatMessage>> GetHistoryAsync()
    {
        return Task.FromResult<IReadOnlyList<ChatMessage>>(_messages);
    }

    public async Task<IReadOnlyList<ChatMessage>> FindRelevantMessagesAsync(string query, int maxResults)
    {
        var queryVector = await _embeddingGenerator.GenerateEmbeddingAsync(query);

        var results = _messages.Where(m => m.Vector != null)
                               .Select(m => new
                               {
                                   Message = m,
                                   Similarity = CosineSimilarity(queryVector, m.Vector!)
                               })
                               .OrderByDescending(x => x.Similarity)
                               .Take(maxResults)
                               .Select(x => x.Message)
                               .ToList();
        return results;
    }

    private static float CosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        float dotProduct = 0.0f;
        float magnitude1 = 0.0f;
        float magnitude2 = 0.0f;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = (float)Math.Sqrt(magnitude1);
        magnitude2 = (float)Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
        {
            return 0.0f; // Avoid division by zero
        }

        return dotProduct / (magnitude1 * magnitude2);
    }

    public Task SummarizeAsync()
    {
        // Summarization logic here
        return Task.CompletedTask;
    }
}
