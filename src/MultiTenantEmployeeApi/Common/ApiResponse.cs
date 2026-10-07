namespace MultiTenantEmployeeApi.Common;

public class ApiResponse<T>
{
    public T? Data { get; init; }
    public Pagination? Pagination { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Success(T data, Pagination? pagination = null) =>
        new() { Data = data, Pagination = pagination };

    public static ApiResponse<T> Fail(ApiError error) =>
        new() { Error = error };
}
