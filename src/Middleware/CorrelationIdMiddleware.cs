namespace AzureNamingTool.Middleware
{
    /// <summary>
    /// Middleware that adds a correlation ID to each request for tracking purposes.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the HTTP request and adds a correlation ID to the response.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if correlation ID already exists in request headers
            string correlationId;
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingCorrelationId))
            {
                correlationId = existingCorrelationId.ToString();
            }
            else
            {
                // Generate new correlation ID
                correlationId = Guid.NewGuid().ToString();
            }

            // Add correlation ID to response headers
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

            // Add correlation ID to HttpContext.Items for easy access in controllers
            context.Items["CorrelationId"] = correlationId;

            // Add correlation ID to logging scope
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                await _next(context);
            }
        }
    }

    /// <summary>
    /// Extension methods for registering the correlation ID middleware.
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        /// <summary>
        /// Adds correlation ID middleware to the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
