using MediatR;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(Guid Id) : IRequest;
