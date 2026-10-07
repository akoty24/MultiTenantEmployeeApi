using MediatR;
using MultiTenantEmployeeApi.Common;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployees;

public record GetEmployeesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Department = null,
    EmployeeStatus? Status = null,
    string? Search = null) : IRequest<PagedResult<EmployeeDto>>;
