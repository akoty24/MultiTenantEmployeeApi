using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common.Exceptions;
using MultiTenantEmployeeApi.Data;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand>
{
    private readonly AppDbContext _db;

    public DeleteEmployeeCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (employee == null)
            throw new NotFoundException($"Employee '{request.Id}' was not found");

        // soft delete, the query filter will hide it from now on
        employee.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
