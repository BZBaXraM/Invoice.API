using Invoice.Domain.Enums;

namespace Invoice.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public ICollection<InvoiceRow> Rows { get; set; } = [];
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
