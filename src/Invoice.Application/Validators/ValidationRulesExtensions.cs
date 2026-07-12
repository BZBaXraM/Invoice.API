namespace Invoice.Application.Validators;

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
            options = ruleBuilder.Must(pass => UppercaseRegex().IsMatch(pass))
                .WithMessage("validation.password.uppercase");
        }

        if (mustContainLowercase)
        {
            options = ruleBuilder.Must(pass => LowercaseRegex().IsMatch(pass))
                .WithMessage("validation.password.lowercase");
        }

        if (mustContainDigit)
        {
            options = ruleBuilder.Must(pass => DigitRegex().IsMatch(pass))
                .WithMessage("validation.password.digit");
        }

        return options!;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex DigitRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowercaseRegex();
}
