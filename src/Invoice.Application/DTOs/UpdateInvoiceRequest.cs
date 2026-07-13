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

    /// <summary>The payment due date, if any.</summary>
    public DateTimeOffset? DueDate { get; set; }

    /// <summary>The invoice line items.</summary>
    public List<UpdateInvoiceRowRequest> Rows { get; set; } = [];

    /// <summary>The VAT rate in percent (0–100).</summary>
    public decimal VatRate { get; set; }

    /// <summary>The kind of discount applied to the invoice.</summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>The discount value: a percentage for Percent, an amount for Fixed.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>An optional free-text comment.</summary>
    public string? Comment { get; set; }
}
