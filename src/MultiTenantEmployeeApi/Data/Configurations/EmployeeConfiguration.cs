using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiTenantEmployeeApi.Entities;

namespace MultiTenantEmployeeApi.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Department).HasMaxLength(100);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.CustomData).HasColumnType("jsonb");

        builder.OwnsOne(e => e.Salary, salary =>
        {
            salary.Property(s => s.AmountMinor).HasColumnName("SalaryAmountMinor");
            salary.Property(s => s.CurrencyCode).HasColumnName("SalaryCurrencyCode").HasMaxLength(3);
        });

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // email is unique inside a tenant, deleted employees don't count
        builder.HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");
    }
}
