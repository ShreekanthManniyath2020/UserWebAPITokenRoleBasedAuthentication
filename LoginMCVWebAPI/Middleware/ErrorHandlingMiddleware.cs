using LoginMCVWebAPI.Services;
using System.Text.Json;

namespace LoginMCVWebAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
         private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly bool _isDevelopment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _isDevelopment = environment.IsDevelopment();
        }

        public async Task InvokeAsync(HttpContext context)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception
            LogException(exception, context);

            // Create error response
            var errorResponse = ErrorResponse.FromException(
                exception,
                context,
                _isDevelopment);

            // Set response status code
            var statusCode = GetStatusCode(exception);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Serialize and write response
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _isDevelopment
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                CustomAppException appEx => appEx.StatusCode,
               // ValidationAppException => 400,
                UnauthorizedAccessException => 401,
                KeyNotFoundException => 404,
                NotImplementedException => 501,
                _ => 500
            };
        }

        private void LogException(Exception exception, HttpContext context)
        {
            var logLevel = GetLogLevel(exception);
            var logTemplate = "HTTP {Method} {Path} responded {StatusCode} - {ErrorType}: {ErrorMessage}";

            _logger.Log(logLevel, exception, logTemplate,
                context.Request.Method,
                context.Request.Path,
                GetStatusCode(exception),
                exception.GetType().Name,
                exception.Message);
        }

        private LogLevel GetLogLevel(Exception exception)
        {
            return exception switch
            {
                ValidationAppException => LogLevel.Warning,
                CustomAppException appEx when appEx.StatusCode < 500 => LogLevel.Warning,
                _ => LogLevel.Error
            };
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder app)
        {
            // Ensure it's placed early in the pipeline
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }

        public static IServiceCollection AddCustomErrorHandling(this IServiceCollection services)
        {
            // Register any dependencies if needed
            return services;
        }
    }
}
