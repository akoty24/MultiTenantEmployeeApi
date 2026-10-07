using FluentValidation;

namespace MultiTenantEmployeeApi.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).ValidName();
        RuleFor(x => x.LastName).ValidName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Department).MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.CustomData).ValidCustomData();
        RuleFor(x => x.Salary).ValidSalary();
    }
}
