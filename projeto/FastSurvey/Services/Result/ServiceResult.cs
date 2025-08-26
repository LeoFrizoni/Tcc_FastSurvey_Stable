namespace FASTSURVEY.Services.Result
{
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string? Code { get; private set; }
        public string? Message { get; private set; }
        public List<string> Errors { get; private set; } = new();

        public static ServiceResult<T> Ok(T data) =>
            new ServiceResult<T> { Success = true, Data = data };

        // Mantém compat com versões antigas
        public static ServiceResult<T> Fail(string code, string message) =>
            new ServiceResult<T>
            {
                Success = false,
                Code = code,
                Message = message,
            };

        // Novo overload aceitando lista de erros
        public static ServiceResult<T> Fail(string code, List<string> errors) =>
            new ServiceResult<T>
            {
                Success = false,
                Code = code,
                Errors = errors,
            };
    }
}
