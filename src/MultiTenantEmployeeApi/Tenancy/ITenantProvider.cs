namespace MultiTenantEmployeeApi.Tenancy;

public interface ITenantProvider
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}
