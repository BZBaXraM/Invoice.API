namespace Invoice.Application.Validators;

public class UpdateInvoiceRequestValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty).WithMessage("validation.invoice.customerRequired");
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("validation.invoice.endDateBeforeStartDate");
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.EndDate)
            .WithMessage("validation.invoice.dueDateBeforeEndDate")
            .When(x => x.DueDate.HasValue);
        RuleFor(x => x.VatRate).InclusiveBetween(0, 100).WithMessage("validation.invoice.vatRateRange");
        RuleFor(x => x.DiscountValue).GreaterThanOrEqualTo(0).WithMessage("validation.invoice.discountValueNegative");
        RuleFor(x => x.DiscountValue).LessThanOrEqualTo(100)
            .WithMessage("validation.invoice.discountPercentRange")
            .When(x => x.DiscountType == DiscountType.Percent);
        RuleFor(x => x.Rows).NotEmpty().WithMessage("validation.invoice.rowsRequired");

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.Service).NotEmpty().WithMessage("validation.invoiceRow.serviceRequired");
            row.RuleFor(r => r.Quantity).GreaterThan(0).WithMessage("validation.invoiceRow.quantityPositive");
            row.RuleFor(r => r.Rate).GreaterThanOrEqualTo(0).WithMessage("validation.invoiceRow.rateNegative");
        });
    }
}
