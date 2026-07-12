namespace Invoice.Application.Validators;

public class ConfirmEmailCodeRequestValidator : AbstractValidator<ConfirmEmailCodeRequest>
{
    public ConfirmEmailCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("validation.code.required")
            .Length(6).WithMessage("validation.code.length")
            .Matches("^[A-Z0-9]+$").WithMessage("validation.code.pattern");
    }
}
