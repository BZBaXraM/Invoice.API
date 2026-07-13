namespace Invoice.Application.Helpers;

/// <summary>
/// Single source of truth for invoice money math and invoice-number formatting.
/// Total = Subtotal − Discount + VAT, where VAT is charged on the discounted base.
/// All amounts are rounded to 2 decimals, away from zero. The client-side live
/// preview (invoice-form) mirrors this math — keep them in sync.
/// </summary>
public static class InvoiceTotalsCalculator
{
    public static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static decimal RowSum(decimal quantity, decimal rate) => Round2(quantity * rate);

    public static (decimal Subtotal, decimal DiscountAmount, decimal VatAmount, decimal Total) Calculate(
        decimal subtotal,
        DiscountType discountType,
        decimal discountValue,
        decimal vatRate)
    {
        var discountAmount = discountType switch
        {
            DiscountType.Percent => Round2(subtotal * discountValue / 100m),
            DiscountType.Fixed => Round2(Math.Min(discountValue, subtotal)),
            _ => 0m
        };

        var vatAmount = Round2((subtotal - discountAmount) * vatRate / 100m);
        var total = subtotal - discountAmount + vatAmount;

        return (subtotal, discountAmount, vatAmount, total);
    }

    /// <summary>
    /// Recomputes row sums and all invoice totals in place.
    /// </summary>
    public static void ApplyTotals(Domain.Entities.Invoice invoice)
    {
        foreach (var row in invoice.Rows)
        {
            row.Sum = RowSum(row.Quantity, row.Rate);
        }

        var subtotal = invoice.Rows.Sum(r => r.Sum);
        var (_, discountAmount, vatAmount, total) =
            Calculate(subtotal, invoice.DiscountType, invoice.DiscountValue, invoice.VatRate);

        invoice.Subtotal = subtotal;
        invoice.DiscountAmount = discountAmount;
        invoice.VatAmount = vatAmount;
        invoice.TotalSum = total;
    }

    public static string FormatInvoiceNumber(int invoiceNumber) => $"INV-{invoiceNumber:D4}";
}
