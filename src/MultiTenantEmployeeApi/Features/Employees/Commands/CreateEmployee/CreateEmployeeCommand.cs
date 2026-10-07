using System.Text.Json;
using MediatR;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    EmployeeStatus? Status,
    JsonElement? CustomData,
    MoneyDto? Salary) : IRequest<EmployeeDto>;
