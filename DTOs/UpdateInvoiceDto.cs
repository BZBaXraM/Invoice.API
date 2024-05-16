using InvoiceManager.API.Models;

namespace InvoiceManager.API.DTOs;

/// <summary>
/// The update invoice DTO.
/// </summary>
public class UpdateInvoiceDto
{
    /// <summary>
    /// The invoice ID.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// The start date.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// The end date.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>
    /// The rows.
    /// </summary>
    public List<UpdateInvoiceRowDto> Rows { get; set; } = [];

    /// <summary>
    /// The comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// The status.
    /// </summary>
    public InvoiceStatus Status { get; set; }
}