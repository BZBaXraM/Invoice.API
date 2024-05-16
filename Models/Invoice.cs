namespace InvoiceManager.API.Models;

public class Invoice
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public ICollection<InvoiceRow> Rows { get; set; } = new List<InvoiceRow>();
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
}