namespace Invoice.Application.Extensions;

public static class RecurringInvoiceMappingExtensions
{
    public static RecurringInvoiceRowResponse ToRecurringInvoiceRowResponse(this RecurringInvoiceRow row) => new()
    {
        Id = row.Id,
        Service = row.Service,
        Quantity = row.Quantity,
        Rate = row.Rate
    };

    public static RecurringInvoiceResponse ToRecurringInvoiceResponse(this RecurringInvoice recurring) => new()
    {
        Id = recurring.Id,
        CustomerId = recurring.CustomerId,
        Frequency = recurring.Frequency,
        NextRunDate = recurring.NextRunDate,
        EndDate = recurring.EndDate,
        IsActive = recurring.IsActive,
        DueInDays = recurring.DueInDays,
        VatRate = recurring.VatRate,
        DiscountType = recurring.DiscountType,
        DiscountValue = recurring.DiscountValue,
        Comment = recurring.Comment,
        Rows = recurring.Rows.Select(r => r.ToRecurringInvoiceRowResponse()).ToList(),
        CreatedAt = recurring.CreatedAt,
        UpdatedAt = recurring.UpdatedAt
    };
}
