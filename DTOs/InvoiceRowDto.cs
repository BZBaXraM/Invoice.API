namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents a row in an invoice.
/// </summary>
public class InvoiceRowDto
{
    /// <summary>
    /// The unique identifier of the invoice row.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The unique identifier of the invoice.
    /// </summary>
    public int InvoiceId { get; set; }

    /// <summary>
    /// The service provided.
    /// </summary>
    public string Service { get; set; } = default!;

    /// <summary>
    /// The quantity of the service provided.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// The rate of the service provided.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// The total sum of the service provided.
    /// </summary>
    public decimal Sum { get; set; }
}