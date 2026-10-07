using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiTenantEmployeeApi.Entities;

namespace MultiTenantEmployeeApi.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();

        builder.HasData(
            new Tenant { Id = SeedData.TenantAId, Name = "Tenant A" },
            new Tenant { Id = SeedData.TenantBId, Name = "Tenant B" });
    }
}
