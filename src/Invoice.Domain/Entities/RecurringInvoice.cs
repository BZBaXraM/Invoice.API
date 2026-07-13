using Invoice.Domain.Enums;

namespace Invoice.Domain.Entities;

public class RecurringInvoice : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CustomerId { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateTimeOffset NextRunDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int DueInDays { get; set; } = 14;
    public decimal VatRate { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? Comment { get; set; }
    public ICollection<RecurringInvoiceRow> Rows { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
