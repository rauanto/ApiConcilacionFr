// Common/ApiResponse.cs
namespace ApiConcilacionFr.Common;

using System.Text.Json.Serialization;

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IEnumerable<string>? Errors { get; init; }
    public PaginationMeta? Pagination { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    

    // ── Factories ──────────────────────────────────────────────────────────────

    /// <summary>Respuesta exitosa con datos</summary>
    public static ApiResponse<T> Success(T? data, string message = "Operación exitosa")
        => new()
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };

    /// <summary>Respuesta exitosa con paginación</summary>
    public static ApiResponse<T> SuccessPaged(
        T? data,
        PaginationMeta pagination,
        string message = "Operación exitosa")
        => new()
        {
            IsSuccess = true,
            Message = message,
            Data = data,
            Pagination = pagination
        };

    /// <summary>Respuesta de error</summary>
    public static ApiResponse<T> Failure(string message, IEnumerable<string>? errors = null)
        => new()
        {
            IsSuccess = false,
            Message = message,
            Errors = errors
        };

    /// <summary>Respuesta de validación fallida</summary>
    public static ApiResponse<T> ValidationError(IEnumerable<string> errors)
        => new()
        {
            IsSuccess = false,
            Message = "Error de validación",
            Errors = errors
        };
}

// Metadata de paginación
public record PaginationMeta(
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);