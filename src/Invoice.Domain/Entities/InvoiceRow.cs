namespace Invoice.Domain.Entities;

public class InvoiceRow : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public required string Service { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Sum { get; set; }
}
