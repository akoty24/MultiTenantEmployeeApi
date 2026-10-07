using MediatR;
using Microsoft.AspNetCore.Mvc;
using MultiTenantEmployeeApi.Common;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Features.Employees.Commands.CreateEmployee;
using MultiTenantEmployeeApi.Features.Employees.Commands.DeleteEmployee;
using MultiTenantEmployeeApi.Features.Employees.Commands.UpdateEmployee;
using MultiTenantEmployeeApi.Features.Employees.Dtos;
using MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployeeById;
using MultiTenantEmployeeApi.Features.Employees.Queries.GetEmployees;

namespace MultiTenantEmployeeApi.Controllers;

[ApiController]
[Route("api/v1/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ApiResponse<EmployeeDto>.Success(employee));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? department = null,
        [FromQuery] EmployeeStatus? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmployeesQuery(page, pageSize, department, status, search);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<List<EmployeeDto>>.Success(result.Items, result.Pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Success(employee));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> Update(Guid id, UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await _mediator.Send(command with { Id = id }, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Success(employee));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return NoContent();
    }
}
