namespace Invoice.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("validation.firstName.required");

        RuleFor(x => x.LastName).NotEmpty().WithMessage("validation.lastName.required");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("validation.username.required")
            .Matches("^[a-zA-Z0-9_.]+$").WithMessage("validation.username.pattern");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("validation.email.required")
            .EmailAddress().WithMessage("validation.email.invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("validation.password.required")
            .MinimumLength(10).WithMessage("validation.password.minLength")
            .Password();

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("validation.phoneNumber.invalid");
    }
}
