namespace Invoice.Application.DTOs;

/// <summary>
/// A line item on a recurring invoice template.
/// </summary>
public class RecurringInvoiceRowRequest
{
    /// <summary>The service description.</summary>
    public required string Service { get; set; }

    /// <summary>The quantity.</summary>
    public decimal Quantity { get; set; }

    /// <summary>The rate per unit.</summary>
    public decimal Rate { get; set; }
}

/// <summary>
/// Request to create a recurring invoice template.
/// </summary>
public class CreateRecurringInvoiceRequest
{
    /// <summary>The customer generated invoices are for.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>How often an invoice is generated.</summary>
    public RecurrenceFrequency Frequency { get; set; }

    /// <summary>When the next invoice should be generated.</summary>
    public DateTimeOffset NextRunDate { get; set; }

    /// <summary>When the template stops generating invoices, if ever.</summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>How many days after generation the invoice is due.</summary>
    public int DueInDays { get; set; } = 14;

    /// <summary>The VAT rate in percent (0–100).</summary>
    public decimal VatRate { get; set; }

    /// <summary>The kind of discount applied to generated invoices.</summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>The discount value: a percentage for Percent, an amount for Fixed.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>An optional free-text comment copied onto generated invoices.</summary>
    public string? Comment { get; set; }

    /// <summary>The template line items.</summary>
    public List<RecurringInvoiceRowRequest> Rows { get; set; } = [];
}

/// <summary>
/// Request to update a recurring invoice template.
/// </summary>
public class UpdateRecurringInvoiceRequest : CreateRecurringInvoiceRequest;

/// <summary>
/// Public representation of a recurring invoice template line item.
/// </summary>
public class RecurringInvoiceRowResponse
{
    /// <summary>The row's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The service description.</summary>
    public required string Service { get; set; }

    /// <summary>The quantity.</summary>
    public decimal Quantity { get; set; }

    /// <summary>The rate per unit.</summary>
    public decimal Rate { get; set; }
}

/// <summary>
/// Public representation of a recurring invoice template.
/// </summary>
public class RecurringInvoiceResponse
{
    /// <summary>The template's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The customer generated invoices are for.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>How often an invoice is generated.</summary>
    public RecurrenceFrequency Frequency { get; set; }

    /// <summary>When the next invoice will be generated.</summary>
    public DateTimeOffset NextRunDate { get; set; }

    /// <summary>When the template stops generating invoices, if ever.</summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>Whether the template is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>How many days after generation the invoice is due.</summary>
    public int DueInDays { get; set; }

    /// <summary>The VAT rate in percent (0–100).</summary>
    public decimal VatRate { get; set; }

    /// <summary>The kind of discount applied to generated invoices.</summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>The discount value: a percentage for Percent, an amount for Fixed.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>An optional free-text comment copied onto generated invoices.</summary>
    public string? Comment { get; set; }

    /// <summary>The template line items.</summary>
    public List<RecurringInvoiceRowResponse> Rows { get; set; } = [];

    /// <summary>When the template was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the template was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
