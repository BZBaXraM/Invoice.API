namespace Invoice.Application.DTOs;

/// <summary>
/// A request to the AI report chat. History is kept client-side and resent every turn.
/// </summary>
public class ChatRequest
{
    public required string Message { get; set; }
    public List<ChatMessageDto> History { get; set; } = [];
}
