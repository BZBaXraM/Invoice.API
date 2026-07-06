namespace Invoice.Application.DTOs;

/// <summary>
/// Invoice statistics for a single customer over a date range.
/// </summary>
public class CustomerStatResponse
{
    /// <summary>The customer's unique identifier.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The customer's name.</summary>
    public required string CustomerName { get; set; }

    /// <summary>The number of invoices issued to this customer in the range.</summary>
    public int InvoiceCount { get; set; }

    /// <summary>The total sum of those invoices.</summary>
    public decimal TotalSum { get; set; }

    /// <summary>The currency the total sum is billed in.</summary>
    public string Currency { get; set; } = "AZN";
}
