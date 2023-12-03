using Invoice.API.Models.Enum;

namespace Invoice.API.Models;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<InvoiceRow> Rows { get; set; } = new();
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsSent { get; set; }
    public bool IsDeleted { get; set; }
}