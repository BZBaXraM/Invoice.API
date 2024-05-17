using System.Text.RegularExpressions;
using FluentValidation;

namespace InvoiceManager.API.DTOs.Validation;

public static partial class ValidationRulesExtensions
{
    public static IRuleBuilderOptions<T, string> Password<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        bool mustContainLowercase = true,
        bool mustContainUppercase = true,
        bool mustContainDigit = true)
    {
        IRuleBuilderOptions<T, string>? options = null;

        if (mustContainUppercase)
        {
            options = ruleBuilder.Must(pass => MyRegex().IsMatch(pass))
                .WithMessage("Password must contain at least 1 uppercase letter");
        }

        if (mustContainLowercase)
        {
            options = ruleBuilder.Must(pass => MyRegex2().IsMatch(pass))
                .WithMessage("Password must contain at least 1 lowercase letter");
        }

        if (mustContainDigit)
        {
            options = ruleBuilder.Must(pass => MyRegex1().IsMatch(pass))
                .WithMessage("Password must contain at least 1 number");
        }

        return options!;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex MyRegex();
    [GeneratedRegex("[0-9]")]
    private static partial Regex MyRegex1();
    [GeneratedRegex("[a-z]")]
    private static partial Regex MyRegex2();
}