namespace FASTSURVEY.Services.Result;

public record ServiceError(string Code, string Message);

public class ServiceResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public List<ServiceError> Errors { get; } = new();

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static ServiceResult<T> Fail(string code, string message)
        => new() { Success = false, Errors = { new ServiceError(code, message) } };
}
