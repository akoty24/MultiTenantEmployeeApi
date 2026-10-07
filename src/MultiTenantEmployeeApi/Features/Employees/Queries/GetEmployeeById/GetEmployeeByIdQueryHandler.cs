using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common.Exceptions;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly AppDbContext _db;

    public GetEmployeeByIdQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        // employees of other tenants are filtered out, so they also end up as 404
        if (employee == null)
            throw new NotFoundException($"Employee '{request.Id}' was not found");

        return EmployeeDto.FromEntity(employee);
    }
}
