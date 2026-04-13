// Common/ErrorHandlingMiddleware.cs
using System.Net;
using System.Text.Json;

namespace ApiConcilacionFr.Common;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "Ocurrió un error interno en el servidor.";

        if (exception is ApiException apiException)
        {
            statusCode = apiException.StatusCode;
            message = apiException.Message;
        }
        else
        {
            _logger.LogError(exception, "Unhandled Exception");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(ApiResponse<object>.Failure(message));
        return context.Response.WriteAsync(result);
    }
}
