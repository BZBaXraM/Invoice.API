namespace Invoice.Application.Validators;

public class CreateRecurringInvoiceRequestValidator : AbstractValidator<CreateRecurringInvoiceRequest>
{
    public CreateRecurringInvoiceRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty).WithMessage("validation.recurring.customerRequired");
        RuleFor(x => x.NextRunDate).NotEqual(default(DateTimeOffset)).WithMessage("validation.recurring.nextRunDateRequired");
        RuleFor(x => x.EndDate).GreaterThan(x => x.NextRunDate)
            .WithMessage("validation.recurring.endDateBeforeNextRun")
            .When(x => x.EndDate.HasValue);
        RuleFor(x => x.DueInDays).InclusiveBetween(0, 365).WithMessage("validation.recurring.dueInDaysRange");
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

public class UpdateRecurringInvoiceRequestValidator : AbstractValidator<UpdateRecurringInvoiceRequest>
{
    public UpdateRecurringInvoiceRequestValidator(CreateRecurringInvoiceRequestValidator createValidator)
    {
        Include(createValidator);
    }
}
