using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Features.Employees.Dtos;

namespace MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployees;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly AppDbContext _db;

    public GetEmployeesQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Department))
            query = query.Where(e => e.Department == request.Department);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(search) ||
                e.LastName.ToLower().Contains(search) ||
                e.Email.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var employees = await query
            .OrderByDescending(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pagination = new Pagination
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return new PagedResult<EmployeeDto>(employees.Select(EmployeeDto.FromEntity).ToList(), pagination);
    }
}
