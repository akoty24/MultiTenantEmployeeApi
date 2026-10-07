using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common.Exceptions;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Commands.CreateEmployee;
using MultiTenantEmployeeApi.Features.Employees.Dtos;
using MultiTenantEmployeeApi.Tests.Helpers;

namespace MultiTenantEmployeeApi.Tests.Unit;

public class CreateEmployeeCommandHandlerTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    private static CreateEmployeeCommand NewCommand(string email = "john.doe@test.com") =>
        new("John", "Doe", email, "Engineering", null,
            JsonDocument.Parse("{\"level\":\"Senior\"}").RootElement,
            new MoneyDto(1500000, "USD"));

    [Fact]
    public async Task Handle_ValidCommand_CreatesEmployeeForCurrentTenant()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        var handler = new CreateEmployeeCommandHandler(db);

        var result = await handler.Handle(NewCommand("John.Doe@Test.com"), CancellationToken.None);

        var saved = await db.Employees.SingleAsync(e => e.Id == result.Id);
        Assert.Equal(SeedData.TenantAId, saved.TenantId);
        Assert.Equal("john.doe@test.com", saved.Email);
        Assert.Equal(EmployeeStatus.Active, saved.Status);
        Assert.Equal(1500000, saved.Salary!.AmountMinor);
        Assert.Equal("USD", saved.Salary.CurrencyCode);
        Assert.NotEqual(default, saved.CreatedAt);
        Assert.Equal("Senior", result.CustomData!.Value.GetProperty("level").GetString());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsConflictException()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        var handler = new CreateEmployeeCommandHandler(db);
        await handler.Handle(NewCommand(), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(NewCommand("JOHN.DOE@test.com"), CancellationToken.None));

        Assert.Equal(1, await db.Employees.CountAsync());
    }

    [Fact]
    public async Task Handle_SameEmailInAnotherTenant_IsAllowed()
    {
        await using (var dbA = TestDbContextFactory.Create(_dbName, SeedData.TenantAId))
        {
            await new CreateEmployeeCommandHandler(dbA).Handle(NewCommand(), CancellationToken.None);
        }

        await using var dbB = TestDbContextFactory.Create(_dbName, SeedData.TenantBId);
        var result = await new CreateEmployeeCommandHandler(dbB).Handle(NewCommand(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
    }
}
