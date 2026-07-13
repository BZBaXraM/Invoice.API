namespace Invoice.Application.DTOs;

/// <summary>
/// Request to record a payment against an invoice.
/// </summary>
public class CreatePaymentRequest
{
    /// <summary>The paid amount. Must be positive and must not exceed the outstanding balance.</summary>
    public decimal Amount { get; set; }

    /// <summary>When the payment was made.</summary>
    public DateTimeOffset PaymentDate { get; set; }

    /// <summary>An optional free-text note.</summary>
    public string? Note { get; set; }
}
