namespace Invoice.Application.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("validation.payment.amountPositive");
        RuleFor(x => x.PaymentDate).NotEqual(default(DateTimeOffset)).WithMessage("validation.payment.dateRequired");
    }
}
