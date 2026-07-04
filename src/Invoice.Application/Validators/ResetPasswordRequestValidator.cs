namespace Invoice.Application.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Reset code must contain only uppercase letters and numbers.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10)
            .Password();
    }
}
