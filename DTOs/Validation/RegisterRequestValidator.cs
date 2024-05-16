using FluentValidation;
using Microsoft.AspNetCore.Identity.Data;

namespace InvoiceManager.API.DTOs.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10);
    }
}