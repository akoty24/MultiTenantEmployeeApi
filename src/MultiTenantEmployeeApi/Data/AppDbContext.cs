using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Entities;
using MultiTenantEmployeeApi.Tenancy;

namespace MultiTenantEmployeeApi.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    // used inside the global query filter, EF evaluates it per query
    public Guid CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Every employee query is filtered by the current tenant and hides soft deleted rows.
        modelBuilder.Entity<Employee>()
            .HasQueryFilter(e => e.TenantId == CurrentTenantId && e.DeletedAt == null);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Employee>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                    entry.Entity.TenantId = CurrentTenantId;

                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
