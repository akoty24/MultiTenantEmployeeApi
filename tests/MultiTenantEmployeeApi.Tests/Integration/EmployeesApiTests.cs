using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantEmployeeApi.Common;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Features.Employees.Dtos;
using MultiTenantEmployeeApi.Tenancy;

namespace MultiTenantEmployeeApi.Tests.Integration;

public class EmployeesApiTests : IClassFixture<PostgresApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgresApiFactory _factory;

    public EmployeesApiTests(PostgresApiFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(Guid tenantId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TenantMiddleware.TenantHeader, tenantId.ToString());
        return client;
    }

    private static object NewEmployee(string email) => new
    {
        firstName = "Sara",
        lastName = "Hassan",
        email,
        department = "HR",
        customData = new { level = "Mid" },
        salary = new { amountMinor = 1200000, currencyCode = "EGP" }
    };

    private static async Task<EmployeeDto> CreateEmployeeAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/v1/employees", NewEmployee(email));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>(JsonOptions);
        return body!.Data!;
    }

    [Fact]
    public async Task EmployeeOfTenantA_IsNotVisibleToTenantB()
    {
        var clientA = CreateClient(SeedData.TenantAId);
        var clientB = CreateClient(SeedData.TenantBId);
        var created = await CreateEmployeeAsync(clientA, $"sara.{Guid.NewGuid():N}@test.com");

        // tenant A can read it
        var getA = await clientA.GetAsync($"/api/v1/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getA.StatusCode);

        // tenant B gets 404 for the same id
        var getB = await clientB.GetAsync($"/api/v1/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getB.StatusCode);

        // and it is not in tenant B list
        var listB = await clientB.GetFromJsonAsync<ApiResponse<List<EmployeeDto>>>("/api/v1/employees?pageSize=100", JsonOptions);
        Assert.DoesNotContain(listB!.Data!, e => e.Id == created.Id);

        // tenant B can't delete it either
        var deleteB = await clientB.DeleteAsync($"/api/v1/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteB.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsEnvelopeAndStoresDataInPostgres()
    {
        var client = CreateClient(SeedData.TenantAId);
        var created = await CreateEmployeeAsync(client, $"envelope.{Guid.NewGuid():N}@test.com");

        Assert.Equal("Mid", created.CustomData!.Value.GetProperty("level").GetString());
        Assert.Equal(1200000, created.Salary!.AmountMinor);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Employees.IgnoreQueryFilters().SingleAsync(e => e.Id == created.Id);
        Assert.Equal(SeedData.TenantAId, saved.TenantId);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        var client = CreateClient(SeedData.TenantAId);
        var email = $"dup.{Guid.NewGuid():N}@test.com";
        await CreateEmployeeAsync(client, email);

        var response = await client.PostAsJsonAsync("/api/v1/employees", NewEmployee(email));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        Assert.Equal("Conflict", body!.Error!.Code);
    }

    [Fact]
    public async Task Create_InvalidBody_Returns400WithValidationErrors()
    {
        var client = CreateClient(SeedData.TenantAId);

        var response = await client.PostAsJsonAsync("/api/v1/employees", new { firstName = "", lastName = "X", email = "bad" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        Assert.Equal("ValidationFailed", body!.Error!.Code);
        Assert.True(body.Error.Details!.ContainsKey("Email"));
        Assert.True(body.Error.Details!.ContainsKey("FirstName"));
    }

    [Fact]
    public async Task Delete_SoftDeletesEmployee()
    {
        var client = CreateClient(SeedData.TenantAId);
        var created = await CreateEmployeeAsync(client, $"delete.{Guid.NewGuid():N}@test.com");

        var delete = await client.DeleteAsync($"/api/v1/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await client.GetAsync($"/api/v1/employees/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        // row is still in the database, just marked as deleted
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Employees.IgnoreQueryFilters().SingleAsync(e => e.Id == created.Id);
        Assert.NotNull(saved.DeletedAt);
    }

    [Fact]
    public async Task Request_WithoutTenantHeader_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/employees");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithUnknownTenant_Returns403()
    {
        var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/v1/employees");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
