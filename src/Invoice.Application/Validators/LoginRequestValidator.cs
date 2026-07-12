namespace Invoice.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UsernameOrEmail).NotEmpty().WithMessage("validation.usernameOrEmail.required");

        RuleFor(x => x.Password).NotEmpty().WithMessage("validation.password.required");
    }
}
