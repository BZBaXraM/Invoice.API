namespace Invoice.Domain.Entities;

public class RecurringInvoiceRow : BaseEntity
{
    public Guid RecurringInvoiceId { get; set; }
    public required string Service { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
}
