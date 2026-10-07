using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common.Exceptions;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Commands.DeleteEmployee;
using MultiTenantEmployeeApi.Features.Employees.Commands.UpdateEmployee;
using MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployeeById;
using MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployees;
using MultiTenantEmployeeApi.Tests.Helpers;

namespace MultiTenantEmployeeApi.Tests.Unit;

public class TenantIsolationTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    private async Task<Guid> SeedTwoTenantsAsync()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantAId, 3);
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantBId, 2);

        return await db.Employees.IgnoreQueryFilters()
            .Where(e => e.TenantId == SeedData.TenantBId)
            .Select(e => e.Id)
            .FirstAsync();
    }

    [Fact]
    public async Task List_ReturnsOnlyCurrentTenantEmployees()
    {
        await SeedTwoTenantsAsync();

        await using var dbA = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        var resultA = await new GetEmployeesQueryHandler(dbA).Handle(new GetEmployeesQuery(), CancellationToken.None);

        await using var dbB = TestDbContextFactory.Create(_dbName, SeedData.TenantBId);
        var resultB = await new GetEmployeesQueryHandler(dbB).Handle(new GetEmployeesQuery(), CancellationToken.None);

        Assert.Equal(3, resultA.Pagination.TotalCount);
        Assert.Equal(2, resultB.Pagination.TotalCount);
    }

    [Fact]
    public async Task GetById_EmployeeOfAnotherTenant_ThrowsNotFound()
    {
        var tenantBEmployeeId = await SeedTwoTenantsAsync();

        await using var dbA = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        var handler = new GetEmployeeByIdQueryHandler(dbA);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetEmployeeByIdQuery(tenantBEmployeeId), CancellationToken.None));
    }

    [Fact]
    public async Task Update_EmployeeOfAnotherTenant_ThrowsNotFound()
    {
        var tenantBEmployeeId = await SeedTwoTenantsAsync();

        await using var dbA = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        var command = new UpdateEmployeeCommand("Hacked", "Name", "hacked@test.com", null, EmployeeStatus.Active, null, null)
        {
            Id = tenantBEmployeeId
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new UpdateEmployeeCommandHandler(dbA).Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_EmployeeOfAnotherTenant_ThrowsNotFoundAndKeepsRecord()
    {
        var tenantBEmployeeId = await SeedTwoTenantsAsync();

        await using var dbA = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new DeleteEmployeeCommandHandler(dbA).Handle(new DeleteEmployeeCommand(tenantBEmployeeId), CancellationToken.None));

        var employee = await dbA.Employees.IgnoreQueryFilters().SingleAsync(e => e.Id == tenantBEmployeeId);
        Assert.Null(employee.DeletedAt);
    }

    [Fact]
    public async Task SoftDeletedEmployee_IsNotReturned()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantAId, 2);
        var id = await db.Employees.Select(e => e.Id).FirstAsync();

        await new DeleteEmployeeCommandHandler(db).Handle(new DeleteEmployeeCommand(id), CancellationToken.None);
        var result = await new GetEmployeesQueryHandler(db).Handle(new GetEmployeesQuery(), CancellationToken.None);

        Assert.Equal(1, result.Pagination.TotalCount);
        Assert.DoesNotContain(result.Items, e => e.Id == id);
    }
}
