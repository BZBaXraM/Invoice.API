namespace Invoice.Application.DTOs;

/// <summary>
/// Invoice statistics for a single service (work item) over a date range.
/// </summary>
public class ServiceStatResponse
{
    /// <summary>The name of the service performed.</summary>
    public required string Service { get; set; }

    /// <summary>The number of invoices that included this service in the range.</summary>
    public int InvoiceCount { get; set; }

    /// <summary>The total sum billed for this service.</summary>
    public decimal TotalSum { get; set; }
}
