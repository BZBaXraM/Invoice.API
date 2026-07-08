namespace Invoice.Infrastructure.Services;

/// <summary>
/// Wire-format DTOs for Groq's OpenAI-compatible chat completions API. Internal to <see cref="GroQChatClient"/>.
/// </summary>
internal sealed class GroqChatRequest
{
    [JsonPropertyName("model")] public required string Model { get; set; }
    [JsonPropertyName("messages")] public required List<GroqMessage> Messages { get; set; }
    [JsonPropertyName("tools")] public List<GroqTool>? Tools { get; set; }
    [JsonPropertyName("tool_choice")] public object? ToolChoice { get; set; }
    [JsonPropertyName("temperature")] public double Temperature { get; set; } = 0.2;
}

internal sealed class GroqToolChoice
{
    [JsonPropertyName("type")] public required string Type { get; set; }
    [JsonPropertyName("function")] public required GroqToolChoiceFunction Function { get; set; }
}

internal sealed class GroqToolChoiceFunction
{
    [JsonPropertyName("name")] public required string Name { get; set; }
}

internal sealed class GroqTool
{
    [JsonPropertyName("type")] public required string Type { get; set; }
    [JsonPropertyName("function")] public required GroqFunctionDefinition Function { get; set; }
}

internal sealed class GroqFunctionDefinition
{
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("description")] public required string Description { get; set; }
    [JsonPropertyName("parameters")] public required object Parameters { get; set; }
}

internal sealed class GroqMessage
{
    [JsonPropertyName("role")] public string? Role { get; set; }
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("tool_call_id")] public string? ToolCallId { get; set; }
    [JsonPropertyName("tool_calls")] public List<GroqToolCall>? ToolCalls { get; set; }
}

internal sealed class GroqToolCall
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("function")] public GroqFunctionCall? Function { get; set; }
}

internal sealed class GroqFunctionCall
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("arguments")] public string? Arguments { get; set; }
}

internal sealed class GroqChatCompletionResponse
{
    [JsonPropertyName("choices")] public List<GroqChoice> Choices { get; set; } = [];
}

internal sealed class GroqChoice
{
    [JsonPropertyName("message")] public GroqMessage? Message { get; set; }
}

/// <summary>
/// Mapping between the provider-agnostic <see cref="Invoice.Application.DTOs"/> chat types and Groq's wire format.
/// </summary>
internal static class GroqWireMapper
{
    public static GroqMessage ToWireMessage(AiMessage message) => new()
    {
        Role = message.Role,
        Content = message.Content,
        ToolCallId = message.ToolCallId,
        ToolCalls = message.ToolCalls?.Select(tc => new GroqToolCall
        {
            Id = tc.Id,
            Type = "function",
            Function = new GroqFunctionCall { Name = tc.FunctionName, Arguments = tc.ArgumentsJson }
        }).ToList()
    };

    public static AiMessage FromWireMessage(GroqMessage message) => new()
    {
        Role = message.Role ?? "assistant",
        Content = message.Content,
        ToolCallId = message.ToolCallId,
        ToolCalls = message.ToolCalls?
            .Where(tc => tc.Function is not null)
            .Select(tc => new AiToolCall
            {
                Id = tc.Id ?? string.Empty,
                FunctionName = tc.Function!.Name ?? string.Empty,
                ArgumentsJson = tc.Function.Arguments ?? "{}"
            }).ToList()
    };

    public static GroqTool ToWireTool(AiTool tool) => new()
    {
        Type = "function",
        Function = new GroqFunctionDefinition
        {
            Name = tool.Name,
            Description = tool.Description,
            Parameters = tool.ParametersSchema
        }
    };

    public static object ToWireToolChoice(string? forceToolName) =>
        forceToolName is null
            ? "auto"
            : new GroqToolChoice
            {
                Type = "function",
                Function = new GroqToolChoiceFunction { Name = forceToolName }
            };
}
