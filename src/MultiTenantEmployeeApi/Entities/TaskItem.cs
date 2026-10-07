namespace MultiTenantEmployeeApi.Entities;

// Named TaskItem to avoid conflict with System.Threading.Tasks.Task, the table is "Tasks".
// Not used by any endpoint yet.
public class TaskItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}
