namespace MultiTenantEmployeeApi.Common;

public record PagedResult<T>(List<T> Items, Pagination Pagination);
