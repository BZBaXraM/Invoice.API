namespace Invoice.Application.DTOs;

/// <summary>
/// Public representation of an invoice line item.
/// </summary>
public class InvoiceRowResponse
{
    /// <summary>The row's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The parent invoice's identifier.</summary>
    public Guid InvoiceId { get; set; }

    /// <summary>The service performed.</summary>
    public required string Service { get; set; }

    /// <summary>The quantity of units performed.</summary>
    public decimal Quantity { get; set; }

    /// <summary>The rate per unit.</summary>
    public decimal Rate { get; set; }

    /// <summary>The computed total for this row (<c>Quantity * Rate</c>).</summary>
    public decimal Sum { get; set; }
}
