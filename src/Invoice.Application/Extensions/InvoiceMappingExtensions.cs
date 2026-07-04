namespace Invoice.Application.Extensions;

public static class InvoiceMappingExtensions
{
    public static InvoiceRowResponse ToInvoiceRowResponse(this InvoiceRow row) => new()
    {
        Id = row.Id,
        InvoiceId = row.InvoiceId,
        Service = row.Service,
        Quantity = row.Quantity,
        Rate = row.Rate,
        Sum = row.Sum
    };

    public static InvoiceResponse ToInvoiceResponse(this Domain.Entities.Invoice invoice) => new()
    {
        Id = invoice.Id,
        CustomerId = invoice.CustomerId,
        StartDate = invoice.StartDate,
        EndDate = invoice.EndDate,
        Rows = invoice.Rows.Select(r => r.ToInvoiceRowResponse()).ToList(),
        TotalSum = invoice.TotalSum,
        Comment = invoice.Comment,
        Status = invoice.Status,
        CreatedAt = invoice.CreatedAt,
        UpdatedAt = invoice.UpdatedAt,
        DeletedAt = invoice.DeletedAt
    };
}
