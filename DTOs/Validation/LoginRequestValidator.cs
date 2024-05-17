using FluentValidation;
using Microsoft.AspNetCore.Identity.Data;

namespace InvoiceManager.API.DTOs.Validation;

/// <inheritdoc />
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <inheritdoc />
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10);
    }
}