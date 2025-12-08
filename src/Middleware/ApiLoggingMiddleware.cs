using System.Diagnostics;
using System.Text;

namespace AzureNamingTool.Middleware
{
    /// <summary>
    /// Middleware that logs API requests and responses for auditing and troubleshooting.
    /// </summary>
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        /// <summary>
        /// Sanitizes user input for safe logging by removing newlines and control characters.
        /// </summary>
        private static string SanitizeForLog(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Remove newlines and carriage returns
            var sanitized = input.Replace("\r", "").Replace("\n", "");
            
            // Remove other control characters
            sanitized = new string(sanitized.Where(c => !char.IsControl(c) || c == ' ').ToArray());
            
            return sanitized;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the HTTP request and logs request/response information for API endpoints.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Only log API requests (not static files, health checks, etc.)
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;
            var stopwatch = Stopwatch.StartNew();

            // Log request
            await LogRequestAsync(context, correlationId);

            // Capture original response body stream
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                try
                {
                    await _next(context);

                    stopwatch.Stop();

                    // Log response
                    await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    
                    _logger.LogError(ex,
                        "API request failed. Method: {Method}, Path: {Path}, CorrelationId: {CorrelationId}, Duration: {Duration}ms",
                        SanitizeForLog(context.Request.Method),
                        SanitizeForLog(context.Request.Path.ToString()),
                        correlationId,
                        stopwatch.ElapsedMilliseconds);

                    throw;
                }
                finally
                {
                    // Copy response body back to original stream
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }

        private async Task LogRequestAsync(HttpContext context, string correlationId)
        {
            var request = context.Request;

            // Build request details
            var requestDetails = new StringBuilder();
            requestDetails.AppendLine($"API Request:");
            requestDetails.AppendLine($"  Method: {SanitizeForLog(request.Method)}");
            requestDetails.AppendLine($"  Path: {SanitizeForLog(request.Path.ToString())}{SanitizeForLog(request.QueryString.ToString())}");
            requestDetails.AppendLine($"  CorrelationId: {SanitizeForLog(correlationId)}");
            
            // Log API key info (first few characters only for security)
            if (request.Headers.TryGetValue("APIKey", out var apiKey))
            {
                var sanitizedKey = SanitizeForLog(apiKey.ToString());
                var maskedKey = sanitizedKey.Length > 8 
                    ? sanitizedKey.Substring(0, 8) + "..." 
                    : "***";
                requestDetails.AppendLine($"  APIKey: {maskedKey}");
            }

            // Log request body for POST/PUT requests (if not too large)
            if ((request.Method == "POST" || request.Method == "PUT") && request.ContentLength > 0 && request.ContentLength < 10000)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                int totalRead = 0;
                int bytesRead;
                while (totalRead < buffer.Length && (bytesRead = await request.Body.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead))) > 0)
                {
                    totalRead += bytesRead;
                }
                var bodyAsText = Encoding.UTF8.GetString(buffer, 0, totalRead);
                request.Body.Position = 0; // Reset stream position

                requestDetails.AppendLine($"  Body: {SanitizeForLog(bodyAsText)}");
            }

            _logger.LogInformation(requestDetails.ToString());
        }

        private async Task LogResponseAsync(HttpContext context, string correlationId, long durationMs)
        {
            var response = context.Response;

            // Build response details
            var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                          response.StatusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            var responseDetails = new StringBuilder();
            responseDetails.AppendLine($"API Response:");
            responseDetails.AppendLine($"  StatusCode: {response.StatusCode}");
            responseDetails.AppendLine($"  CorrelationId: {correlationId}");
            responseDetails.AppendLine($"  Duration: {durationMs}ms");

            // Log response body for errors or if response is small
            if ((response.StatusCode >= 400 || durationMs > 5000) && response.Body.Length < 10000)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);

                if (!string.IsNullOrWhiteSpace(bodyAsText))
                {
                    responseDetails.AppendLine($"  Body: {bodyAsText}");
                }
            }

            _logger.Log(logLevel, responseDetails.ToString());

            // Log structured data for monitoring/alerting
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Method"] = SanitizeForLog(context.Request.Method),
                ["Path"] = SanitizeForLog(context.Request.Path.ToString()),
                ["StatusCode"] = response.StatusCode,
                ["DurationMs"] = durationMs
            }))
            {
                if (logLevel == LogLevel.Error)
                {
                    _logger.LogError(
                        "API request completed with error. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                        SanitizeForLog(context.Request.Method),
                        SanitizeForLog(context.Request.Path.ToString()),
                        response.StatusCode,
                        durationMs);
                }
                else if (logLevel == LogLevel.Warning)
                {
                    _logger.LogWarning(
                        "API request completed with client error. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                        SanitizeForLog(context.Request.Method),
                        SanitizeForLog(context.Request.Path.ToString()),
                        response.StatusCode,
                        durationMs);
                }
                else if (durationMs > 5000)
                {
                    _logger.LogWarning(
                        "Slow API request detected. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                        SanitizeForLog(context.Request.Method),
                        SanitizeForLog(context.Request.Path.ToString()),
                        response.StatusCode,
                        durationMs);
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for registering the API logging middleware.
    /// </summary>
    public static class ApiLoggingMiddlewareExtensions
    {
        /// <summary>
        /// Adds API logging middleware to the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLoggingMiddleware>();
        }
    }
}
