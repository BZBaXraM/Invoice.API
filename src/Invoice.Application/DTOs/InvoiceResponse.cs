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

    /// <summary>The sequential per-user invoice number.</summary>
    public int InvoiceNumber { get; set; }

    /// <summary>The formatted invoice number, e.g. INV-0007.</summary>
    public required string Number { get; set; }

    /// <summary>The start of the work period.</summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>The end of the work period.</summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>The payment due date, if any.</summary>
    public DateTimeOffset? DueDate { get; set; }

    /// <summary>The invoice line items.</summary>
    public List<InvoiceRowResponse> Rows { get; set; } = [];

    /// <summary>The VAT rate in percent (0–100).</summary>
    public decimal VatRate { get; set; }

    /// <summary>The kind of discount applied to the invoice.</summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>The discount value: a percentage for Percent, an amount for Fixed.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>The sum of all row sums, before discount and VAT.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>The computed discount amount.</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>The computed VAT amount (charged on the discounted base).</summary>
    public decimal VatAmount { get; set; }

    /// <summary>The grand total: Subtotal − DiscountAmount + VatAmount.</summary>
    public decimal TotalSum { get; set; }

    /// <summary>The sum of all recorded payments.</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>The outstanding balance: TotalSum − PaidAmount.</summary>
    public decimal BalanceDue { get; set; }

    /// <summary>The recorded payments.</summary>
    public List<PaymentResponse> Payments { get; set; } = [];

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
