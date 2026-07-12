namespace Invoice.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("validation.firstName.required");

        RuleFor(x => x.LastName).NotEmpty().WithMessage("validation.lastName.required");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("validation.username.required")
            .Matches("^[a-zA-Z0-9_.]+$").WithMessage("validation.username.pattern");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("validation.email.required")
            .EmailAddress().WithMessage("validation.email.invalid");
    }
}
