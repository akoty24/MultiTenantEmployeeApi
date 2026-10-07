using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    EmployeeStatus Status,
    JsonElement? CustomData,
    MoneyDto? Salary) : IRequest<EmployeeDto>
{
    // comes from the route, not the body
    [JsonIgnore]
    public Guid Id { get; init; }
}
