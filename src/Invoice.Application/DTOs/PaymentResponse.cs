namespace Invoice.Application.DTOs;

/// <summary>
/// Public representation of a recorded invoice payment.
/// </summary>
public class PaymentResponse
{
    /// <summary>The payment's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The invoice this payment belongs to.</summary>
    public Guid InvoiceId { get; set; }

    /// <summary>The paid amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>When the payment was made.</summary>
    public DateTimeOffset PaymentDate { get; set; }

    /// <summary>An optional free-text note.</summary>
    public string? Note { get; set; }

    /// <summary>When the payment was recorded.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
