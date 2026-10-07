namespace MultiTenantEmployeeApi.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    // raw json, mapped to a jsonb column
    public string? CustomData { get; set; }

    public Money? Salary { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
