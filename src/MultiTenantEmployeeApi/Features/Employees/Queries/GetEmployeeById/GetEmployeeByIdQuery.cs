using MediatR;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;
