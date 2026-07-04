namespace Invoice.Application.Validators;

public class UpdateInvoiceRequestValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEqual(Guid.Empty);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.Rows).NotEmpty();

        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(r => r.Service).NotEmpty();
            row.RuleFor(r => r.Quantity).GreaterThan(0);
            row.RuleFor(r => r.Rate).GreaterThanOrEqualTo(0);
        });
    }
}
