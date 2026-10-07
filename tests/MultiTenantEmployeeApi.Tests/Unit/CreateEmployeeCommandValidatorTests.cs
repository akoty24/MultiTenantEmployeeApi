using MultiTenantEmployeeApi.Features.Employees.Commands.CreateEmployee;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Tests.Unit;

public class CreateEmployeeCommandValidatorTests
{
    private readonly CreateEmployeeCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_HasNoErrors()
    {
        var command = new CreateEmployeeCommand("John", "Doe", "john@test.com", "IT", null, null, new MoneyDto(1000, "USD"));

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidEmail_HasError(string email)
    {
        var command = new CreateEmployeeCommand("John", "Doe", email, null, null, null, null);

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeCommand.Email));
    }

    [Fact]
    public void MissingFirstName_HasError()
    {
        var command = new CreateEmployeeCommand("", "Doe", "john@test.com", null, null, null, null);

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeCommand.FirstName));
    }

    [Fact]
    public void InvalidCurrencyCode_HasError()
    {
        var command = new CreateEmployeeCommand("John", "Doe", "john@test.com", null, null, null, new MoneyDto(1000, "usd1"));

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
