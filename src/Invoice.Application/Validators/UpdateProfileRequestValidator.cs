namespace Invoice.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();

        RuleFor(x => x.LastName).NotEmpty();

        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9_.]+$")
            .WithMessage("Username can only contain letters, digits, underscores and dots.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
