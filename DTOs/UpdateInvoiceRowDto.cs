namespace InvoiceManager.API.DTOs;

/// <summary>
/// The update invoice row DTO.
/// </summary>
public class UpdateInvoiceRowDto
{
    /// <summary>
    /// The service.
    /// </summary>
    public string Service { get; set; } = default!;
    /// <summary>
    /// The quantity.
    /// </summary>
    public decimal Quantity { get; set; }
    /// <summary>
    /// The rate.
    /// </summary>
    public decimal Rate { get; set; }
}