namespace Invoice.Application.Validators;

public class UpsertCompanyProfileRequestValidator : AbstractValidator<UpsertCompanyProfileRequest>
{
    public UpsertCompanyProfileRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("validation.companyProfile.companyNameRequired")
            .MaximumLength(200).WithMessage("validation.companyProfile.companyNameTooLong");

        RuleFor(x => x.Voen)
            .NotEmpty().WithMessage("validation.companyProfile.voenRequired")
            .Matches("^[0-9]{10}$").WithMessage("validation.companyProfile.voenInvalid");

        RuleFor(x => x.BankVoen)
            .Matches("^[0-9]{10}$").WithMessage("validation.companyProfile.bankVoenInvalid")
            .When(x => !string.IsNullOrWhiteSpace(x.BankVoen));

        RuleFor(x => x.Iban)
            .Matches("^[A-Za-z0-9]{15,34}$").WithMessage("validation.companyProfile.ibanInvalid")
            .When(x => !string.IsNullOrWhiteSpace(x.Iban));
    }
}
