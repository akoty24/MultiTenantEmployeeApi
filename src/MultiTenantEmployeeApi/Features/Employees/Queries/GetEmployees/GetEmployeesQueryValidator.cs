using FluentValidation;

namespace MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployees;

public class GetEmployeesQueryValidator : AbstractValidator<GetEmployeesQuery>
{
    public GetEmployeesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum();
    }
}
