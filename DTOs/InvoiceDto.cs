using InvoiceManager.API.Models;

namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents a data transfer object for an invoice.
/// </summary>
public class InvoiceDto
{
    /// <summary>
    /// Gets or sets the invoice identifier.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public int CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }
    /// <summary>
    /// Gets or sets the customer address.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }
    /// <summary>
    /// Gets or sets the invoice rows.
    /// </summary>
    public List<InvoiceRowDto> Rows { get; set; } = [];
    /// <summary>
    /// Gets or sets the total sum of the invoice.
    /// </summary>
    public decimal TotalSum { get; set; }
    /// <summary>
    /// Gets or sets the comment for the invoice.
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// Gets or sets the status of the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the invoice was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the invoice was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}