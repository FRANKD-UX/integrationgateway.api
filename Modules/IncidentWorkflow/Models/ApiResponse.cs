namespace IntegrationGateway.Api.Modules.Dashboard.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    public string? Message { get; set; }

    public static ApiResponse<T> Ok(T data) => new()
    {
        Data = data,
        Success = true
    };

    public static ApiResponse<T> Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}