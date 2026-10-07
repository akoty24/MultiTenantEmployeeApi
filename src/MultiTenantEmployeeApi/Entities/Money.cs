namespace MultiTenantEmployeeApi.Entities;

// Amount is stored in minor units (cents) to avoid floating point problems.
public record Money(long AmountMinor, string CurrencyCode);
