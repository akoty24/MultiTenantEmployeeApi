namespace MultiTenantEmployeeApi.Tenancy;

// Registered as scoped, so every request has its own tenant.
public class TenantProvider : ITenantProvider
{
    public Guid TenantId { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
