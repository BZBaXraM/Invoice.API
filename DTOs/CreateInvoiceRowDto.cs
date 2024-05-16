namespace InvoiceManager.API.DTOs;

public class CreateInvoiceRowDto
{
    public string Service { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
}