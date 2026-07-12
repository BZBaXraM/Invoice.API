namespace Invoice.Application.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("validation.email.required")
            .EmailAddress().WithMessage("validation.email.invalid");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("validation.code.required")
            .Length(6).WithMessage("validation.code.length")
            .Matches("^[A-Z0-9]+$").WithMessage("validation.code.pattern");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("validation.newPassword.required")
            .MinimumLength(10).WithMessage("validation.password.minLength")
            .Password();
    }
}
