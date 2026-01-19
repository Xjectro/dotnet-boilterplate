namespace Source.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string>? Errors { get; set; }

        public ApiResponse(T data, string message = "", bool success = true)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = null;
        }

        public ApiResponse(string message, List<string>? errors = null, bool success = false)
        {
            Success = success;
            Message = message;
            Data = default!;
            Errors = errors;
        }
    }
}
