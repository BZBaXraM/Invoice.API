namespace Invoice.API.Models;

public class InvoiceRow
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Service { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }

    public decimal Sum => Rate * Quantity;
}