namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents a data transfer object for creating an invoice.
/// </summary>
public class CreateInvoiceDto
{
    /// <summary>
    /// Gets or sets the ID of the customer.
    /// </summary>
    public int CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the start date of the invoice.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }
    /// <summary>
    /// Gets or sets the end date of the invoice.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }
    /// <summary>
    /// Gets or sets the rows of the invoice.
    /// </summary>
    public List<CreateInvoiceRowDto> Rows { get; set; } = [];
    /// <summary>
    /// Gets or sets the comment of the invoice.
    /// </summary>
    public string? Comment { get; set; }
}