using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Tenancy;

namespace MultiTenantEmployeeApi.Tests.Helpers;

// EF InMemory is only used for fast handler unit tests.
// The real PostgreSQL behaviour is covered by the integration tests.
public static class TestDbContextFactory
{
    public static AppDbContext Create(string databaseName, Guid tenantId)
    {
        var tenantProvider = new TenantProvider();
        tenantProvider.SetTenant(tenantId);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options, tenantProvider);
    }

    public static async Task AddEmployeesAsync(AppDbContext db, Guid tenantId, int count, string department = "Engineering")
    {
        for (var i = 0; i < count; i++)
        {
            db.Employees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                Email = $"employee{i}.{Guid.NewGuid():N}@test.com",
                Department = department
            });
        }

        await db.SaveChangesAsync();
    }
}
