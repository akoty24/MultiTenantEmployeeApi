using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployees;
using MultiTenantEmployeeApi.Tests.Helpers;

namespace MultiTenantEmployeeApi.Tests.Unit;

public class GetEmployeesQueryHandlerTests
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Handle_ReturnsRequestedPageAndPaginationInfo()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantAId, 25);
        var handler = new GetEmployeesQueryHandler(db);

        var result = await handler.Handle(new GetEmployeesQuery(Page: 2, PageSize: 10), CancellationToken.None);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(2, result.Pagination.Page);
        Assert.Equal(10, result.Pagination.PageSize);
        Assert.Equal(25, result.Pagination.TotalCount);
        Assert.Equal(3, result.Pagination.TotalPages);
    }

    [Fact]
    public async Task Handle_LastPage_ReturnsRemainingItems()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantAId, 25);
        var handler = new GetEmployeesQueryHandler(db);

        var result = await handler.Handle(new GetEmployeesQuery(Page: 3, PageSize: 10), CancellationToken.None);

        Assert.Equal(5, result.Items.Count);
    }

    [Fact]
    public async Task Handle_PagesDoNotOverlap()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantAId, 15);
        var handler = new GetEmployeesQueryHandler(db);

        var page1 = await handler.Handle(new GetEmployeesQuery(Page: 1, PageSize: 10), CancellationToken.None);
        var page2 = await handler.Handle(new GetEmployeesQuery(Page: 2, PageSize: 10), CancellationToken.None);

        var allIds = page1.Items.Select(e => e.Id).Concat(page2.Items.Select(e => e.Id)).ToList();
        Assert.Equal(15, allIds.Distinct().Count());
    }

    [Fact]
    public async Task Handle_FilterByDepartment_ReturnsOnlyMatchingEmployees()
    {
        await using var db = TestDbContextFactory.Create(_dbName, SeedData.TenantAId);
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantAId, 3, "Engineering");
        await TestDbContextFactory.AddEmployeesAsync(db, SeedData.TenantAId, 2, "Sales");
        var handler = new GetEmployeesQueryHandler(db);

        var result = await handler.Handle(new GetEmployeesQuery(Department: "Sales"), CancellationToken.None);

        Assert.Equal(2, result.Pagination.TotalCount);
        Assert.All(result.Items, e => Assert.Equal("Sales", e.Department));
    }
}
