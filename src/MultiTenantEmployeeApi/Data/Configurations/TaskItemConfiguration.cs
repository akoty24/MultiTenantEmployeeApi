using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiTenantEmployeeApi.Entities;

namespace MultiTenantEmployeeApi.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(2000);
    }
}
