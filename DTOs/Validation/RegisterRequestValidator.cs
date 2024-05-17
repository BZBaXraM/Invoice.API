using FluentValidation;
using Microsoft.AspNetCore.Identity.Data;

namespace InvoiceManager.API.DTOs.Validation;

/// <inheritdoc />
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <inheritdoc />
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