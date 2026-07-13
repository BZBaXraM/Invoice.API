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

    public static PaymentResponse ToPaymentResponse(this Payment payment) => new()
    {
        Id = payment.Id,
        InvoiceId = payment.InvoiceId,
        Amount = payment.Amount,
        PaymentDate = payment.PaymentDate,
        Note = payment.Note,
        CreatedAt = payment.CreatedAt
    };

    public static InvoiceResponse ToInvoiceResponse(this Domain.Entities.Invoice invoice)
    {
        var paidAmount = invoice.Payments.Sum(p => p.Amount);

        return new InvoiceResponse
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            InvoiceNumber = invoice.InvoiceNumber,
            Number = InvoiceTotalsCalculator.FormatInvoiceNumber(invoice.InvoiceNumber),
            StartDate = invoice.StartDate,
            EndDate = invoice.EndDate,
            DueDate = invoice.DueDate,
            Rows = invoice.Rows.Select(r => r.ToInvoiceRowResponse()).ToList(),
            VatRate = invoice.VatRate,
            DiscountType = invoice.DiscountType,
            DiscountValue = invoice.DiscountValue,
            Subtotal = invoice.Subtotal,
            DiscountAmount = invoice.DiscountAmount,
            VatAmount = invoice.VatAmount,
            TotalSum = invoice.TotalSum,
            PaidAmount = paidAmount,
            BalanceDue = invoice.TotalSum - paidAmount,
            Payments = invoice.Payments
                .OrderBy(p => p.PaymentDate)
                .Select(p => p.ToPaymentResponse())
                .ToList(),
            Comment = invoice.Comment,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,
            DeletedAt = invoice.DeletedAt
        };
    }
}
