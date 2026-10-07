using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common.Exceptions;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly AppDbContext _db;

    public CreateEmployeeCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // query filter already limits this to the current tenant
        var emailTaken = await _db.Employees.AnyAsync(e => e.Email == email, cancellationToken);
        if (emailTaken)
            throw new ConflictException($"Employee with email '{email}' already exists");

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Department = request.Department?.Trim(),
            Status = request.Status ?? EmployeeStatus.Active,
            CustomData = request.CustomData?.GetRawText(),
            Salary = request.Salary == null ? null : new Money(request.Salary.AmountMinor, request.Salary.CurrencyCode)
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(cancellationToken);

        return EmployeeDto.FromEntity(employee);
    }
}
