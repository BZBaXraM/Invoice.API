namespace Invoice.Application.DTOs;

/// <summary>
/// Request to update an existing invoice. Only allowed while the invoice is still <c>Created</c>.
/// </summary>
public class UpdateInvoiceRequest
{
    /// <summary>The customer this invoice is for.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The start of the work period.</summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>The end of the work period.</summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>The invoice line items.</summary>
    public List<UpdateInvoiceRowRequest> Rows { get; set; } = [];

    /// <summary>An optional free-text comment.</summary>
    public string? Comment { get; set; }
}
