namespace Invoice.Application.DTOs;

/// <summary>
/// A single turn of client-supplied chat history ("user" or "assistant").
/// </summary>
public class ChatMessageDto
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}
