using System.Text.Json;
using FluentValidation;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees;

// Rules shared between create and update validators.
public static class EmployeeValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(100);

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(255).EmailAddress();

    public static IRuleBuilderOptions<T, JsonElement?> ValidCustomData<T>(this IRuleBuilder<T, JsonElement?> rule) =>
        rule.Must(x => x == null || x.Value.ValueKind == JsonValueKind.Object)
            .WithMessage("CustomData must be a JSON object");

    public static void ValidSalary<T>(this IRuleBuilderInitial<T, MoneyDto?> rule) =>
        rule.ChildRules(salary =>
        {
            salary.RuleFor(s => s!.AmountMinor).GreaterThanOrEqualTo(0);
            salary.RuleFor(s => s!.CurrencyCode)
                .NotEmpty()
                .Matches("^[A-Z]{3}$").WithMessage("CurrencyCode must be a 3 letter ISO code (e.g. USD)");
        });
}
