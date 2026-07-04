namespace Invoice.Application.DTOs;

/// <summary>
/// A single line item on a newly created invoice.
/// </summary>
public class CreateInvoiceRowRequest
{
    /// <summary>The service performed.</summary>
    public required string Service { get; set; }

    /// <summary>The quantity of units performed.</summary>
    public decimal Quantity { get; set; }

    /// <summary>The rate per unit.</summary>
    public decimal Rate { get; set; }
}
