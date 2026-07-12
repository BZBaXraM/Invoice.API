namespace Invoice.Application.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("validation.firstName.required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("validation.lastName.required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("validation.email.required")
            .EmailAddress().WithMessage("validation.email.invalid");
    }
}
