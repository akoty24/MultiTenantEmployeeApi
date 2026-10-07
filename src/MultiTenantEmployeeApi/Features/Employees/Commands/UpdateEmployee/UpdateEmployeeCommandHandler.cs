using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common.Exceptions;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly AppDbContext _db;

    public UpdateEmployeeCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (employee == null)
            throw new NotFoundException($"Employee '{request.Id}' was not found");

        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _db.Employees.AnyAsync(e => e.Email == email && e.Id != request.Id, cancellationToken);
        if (emailTaken)
            throw new ConflictException($"Employee with email '{email}' already exists");

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = email;
        employee.Department = request.Department?.Trim();
        employee.Status = request.Status;
        employee.CustomData = request.CustomData?.GetRawText();
        employee.Salary = request.Salary == null ? null : new Money(request.Salary.AmountMinor, request.Salary.CurrencyCode);

        await _db.SaveChangesAsync(cancellationToken);

        return EmployeeDto.FromEntity(employee);
    }
}
