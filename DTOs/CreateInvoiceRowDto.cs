namespace InvoiceManager.API.DTOs;

/// <summary>
/// Represents a data transfer object for creating an invoice row.
/// </summary>
public class CreateInvoiceRowDto
{
    /// <summary>
    /// Gets or sets the service of the invoice row.
    /// </summary>
    public string Service { get; set; } = default!;
    /// <summary>
    /// Gets or sets the quantity of the invoice row.
    /// </summary>
    public decimal Quantity { get; set; }
    /// <summary>
    /// Gets or sets the rate of the invoice row.
    /// </summary>
    public decimal Rate { get; set; }
}