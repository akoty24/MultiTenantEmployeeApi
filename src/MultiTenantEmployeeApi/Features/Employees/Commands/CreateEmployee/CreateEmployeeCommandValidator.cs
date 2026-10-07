using FluentValidation;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.FirstName).ValidName();
        RuleFor(x => x.LastName).ValidName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Department).MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.CustomData).ValidCustomData();
        RuleFor(x => x.Salary).ValidSalary();
    }
}
