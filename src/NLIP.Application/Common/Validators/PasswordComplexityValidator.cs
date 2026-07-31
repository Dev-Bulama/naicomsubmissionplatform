using System.Text.RegularExpressions;
using FluentValidation;

namespace NLIP.Application.Common.Validators;

/// <summary>Shared password-complexity rule: 12+ chars, upper, lower, digit, symbol. Applied
/// wherever a password is set/changed (see Features/Users/ChangePasswordCommandValidator).</summary>
public static class PasswordComplexityValidator
{
    public static IRuleBuilderOptions<T, string> MustBeAComplexPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MinimumLength(12).WithMessage("Password must be at least 12 characters long.")
            .Must(p => Regex.IsMatch(p, "[A-Z]")).WithMessage("Password must contain an uppercase letter.")
            .Must(p => Regex.IsMatch(p, "[a-z]")).WithMessage("Password must contain a lowercase letter.")
            .Must(p => Regex.IsMatch(p, "[0-9]")).WithMessage("Password must contain a digit.")
            .Must(p => Regex.IsMatch(p, "[^a-zA-Z0-9]")).WithMessage("Password must contain a special character.");
    }
}
