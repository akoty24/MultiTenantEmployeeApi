using System.Text.Json;
using MultiTenantEmployeeApi.Entities;

namespace MultiTenantEmployeeApi.Features.Employees.Dtos;

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    EmployeeStatus Status,
    JsonElement? CustomData,
    MoneyDto? Salary,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static EmployeeDto FromEntity(Employee employee)
    {
        JsonElement? customData = null;
        if (!string.IsNullOrEmpty(employee.CustomData))
        {
            using var doc = JsonDocument.Parse(employee.CustomData);
            customData = doc.RootElement.Clone();
        }

        var salary = employee.Salary == null
            ? null
            : new MoneyDto(employee.Salary.AmountMinor, employee.Salary.CurrencyCode);

        return new EmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.Department,
            employee.Status,
            customData,
            salary,
            employee.CreatedAt,
            employee.UpdatedAt);
    }
}
