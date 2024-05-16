using InvoiceManager.API.DTOs;

public class CreateInvoiceDto
{
    public int CustomerId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public List<CreateInvoiceRowDto> Rows { get; set; } = [];
    public string? Comment { get; set; }
}