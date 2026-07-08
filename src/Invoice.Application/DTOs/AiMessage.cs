namespace Invoice.Application.DTOs;

/// <summary>
/// A single message in a provider-agnostic chat completion exchange (system/user/assistant/tool).
/// </summary>
public class AiMessage
{
    public required string Role { get; set; }
    public string? Content { get; set; }
    public List<AiToolCall>? ToolCalls { get; set; }
    public string? ToolCallId { get; set; }

    public static AiMessage System(string content) => new() { Role = "system", Content = content };
    public static AiMessage User(string content) => new() { Role = "user", Content = content };
    public static AiMessage Assistant(string content) => new() { Role = "assistant", Content = content };

    public static AiMessage ToolResult(string toolCallId, string content) =>
        new() { Role = "tool", ToolCallId = toolCallId, Content = content };
}
