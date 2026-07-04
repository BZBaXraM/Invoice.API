namespace Invoice.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UsernameOrEmail).NotEmpty();

        RuleFor(x => x.Password).NotEmpty();
    }
}
