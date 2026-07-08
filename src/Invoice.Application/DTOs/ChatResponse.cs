namespace Invoice.Application.DTOs;

/// <summary>
/// The AI report chat's structured reply: text, clickable follow-up suggestions, and any
/// invoices it referenced (so the client can offer a PDF download for them).
/// </summary>
public class ChatResponse
{
    public required string Reply { get; set; }
    public List<string> Suggestions { get; set; } = [];
    public List<Guid> InvoiceIds { get; set; } = [];
}
