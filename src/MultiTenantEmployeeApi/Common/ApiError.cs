namespace MultiTenantEmployeeApi.Common;

public record ApiError(string Code, string Message, IDictionary<string, string[]>? Details = null);
