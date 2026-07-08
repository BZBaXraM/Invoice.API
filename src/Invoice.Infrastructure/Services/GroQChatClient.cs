namespace Invoice.Infrastructure.Services;

/// <summary>
/// <see cref="IAiChatClient"/> implementation over Groq's OpenAI-compatible chat completions API.
/// </summary>
public class GroQChatClient(HttpClient httpClient, GroqConfig config, ILogger<GroQChatClient> logger) : IAiChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<AiMessage> GetCompletionAsync(
        IReadOnlyList<AiMessage> messages,
        IReadOnlyList<AiTool> tools,
        string? forceToolName,
        CancellationToken cancellationToken = default)
    {
        var payload = new GroqChatRequest
        {
            Model = config.Model,
            Messages = messages.Select(GroqWireMapper.ToWireMessage).ToList(),
            Tools = tools.Select(GroqWireMapper.ToWireTool).ToList(),
            ToolChoice = GroqWireMapper.ToWireToolChoice(forceToolName)
        };

        using var response =
            await httpClient.PostAsJsonAsync("chat/completions", payload, JsonOptions, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Groq API error {StatusCode}: {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"Groq API error {(int)response.StatusCode}: {body}");
        }

        GroqChatCompletionResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<GroqChatCompletionResponse>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse Groq API response body: {Body}", body);
            throw new InvalidOperationException("Groq API returned a response body that could not be parsed.", ex);
        }

        var message = parsed?.Choices.FirstOrDefault()?.Message
                      ?? throw new InvalidOperationException("Groq API returned an empty response body.");

        return GroqWireMapper.FromWireMessage(message);
    }
}
