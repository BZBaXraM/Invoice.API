namespace Invoice.Application.DTOs;

/// <summary>
/// Public representation of an invoice.
/// </summary>
public class InvoiceResponse
{
    /// <summary>The invoice's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The customer this invoice is for.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The start of the work period.</summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>The end of the work period.</summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>The invoice line items.</summary>
    public List<InvoiceRowResponse> Rows { get; set; } = [];

    /// <summary>The sum of all row sums.</summary>
    public decimal TotalSum { get; set; }

    /// <summary>The currency the invoice is billed in.</summary>
    public string Currency { get; set; } = "AZN";

    /// <summary>An optional free-text comment.</summary>
    public string? Comment { get; set; }

    /// <summary>The current invoice status.</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>When the invoice was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the invoice was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>When the invoice was archived (soft-deleted), if at all.</summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
