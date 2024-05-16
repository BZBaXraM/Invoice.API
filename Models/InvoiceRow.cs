namespace InvoiceManager.API.Models;

public class InvoiceRow
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string Service { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Sum { get; set; }
}