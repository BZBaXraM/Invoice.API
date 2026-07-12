namespace Invoice.Application.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("validation.oldPassword.required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("validation.newPassword.required")
            .MinimumLength(10).WithMessage("validation.password.minLength")
            .Password();
    }
}
