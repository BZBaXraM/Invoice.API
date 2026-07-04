namespace Invoice.Application.Validators;

public class ConfirmEmailCodeRequestValidator : AbstractValidator<ConfirmEmailCodeRequest>
{
    public ConfirmEmailCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Confirmation code must contain only uppercase letters and numbers.");
    }
}
