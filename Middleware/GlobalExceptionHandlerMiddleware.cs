using System.Text.Json;

namespace ProductManagement.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Title = "An error occurred",
                Status = GetStatusCode(exception),
                Detail = exception.Message
            };

            // Добавляем дополнительную информацию в development
            if (context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                // Проверяем на null перед добавлением в словарь
                if (exception.StackTrace != null)
                {
                    problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                }
            }

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }

    public class ProblemDetails
    {
        public string Type { get; set; } = "about:blank";
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public Dictionary<string, object> Extensions { get; set; } = new();
    }
}
