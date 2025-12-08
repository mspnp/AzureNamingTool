namespace AzureNamingTool.Models
{
    /// <summary>
    /// Standardized API response wrapper for consistent response format.
    /// This is an optional model for future API versions to maintain consistency.
    /// </summary>
    /// <typeparam name="T">The type of data being returned.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The response data (null if request failed).
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error information (null if request succeeded).
        /// </summary>
        public ApiError? Error { get; set; }

        /// <summary>
        /// Metadata about the request/response.
        /// </summary>
        public ApiMetadata Metadata { get; set; } = new ApiMetadata();

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Message = message
                }
            };
        }

        /// <summary>
        /// Creates an error response.
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string code, string message, string? target = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = new ApiError
                {
                    Code = code,
                    Message = message,
                    Target = target
                },
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// API error information following Microsoft REST API Guidelines.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Error code (e.g., "InvalidInput", "ResourceNotFound").
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The target of the error (e.g., field name).
        /// </summary>
        public string? Target { get; set; }

        /// <summary>
        /// Detailed error information.
        /// </summary>
        public List<ApiError>? Details { get; set; }

        /// <summary>
        /// Inner error with additional debug information.
        /// </summary>
        public ApiInnerError? InnerError { get; set; }
    }

    /// <summary>
    /// Inner error details for debugging purposes.
    /// </summary>
    public class ApiInnerError
    {
        /// <summary>
        /// More specific error code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Nested inner error.
        /// </summary>
        public ApiInnerError? InnerError { get; set; }
    }

    /// <summary>
    /// Metadata about the API request/response.
    /// </summary>
    public class ApiMetadata
    {
        /// <summary>
        /// Correlation ID for request tracking (set by middleware).
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Timestamp when the response was generated (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// API version (optional).
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Optional message about the response.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Pagination information (for list endpoints).
        /// </summary>
        public PaginationMetadata? Pagination { get; set; }
    }

    /// <summary>
    /// Pagination information for list responses.
    /// </summary>
    public class PaginationMetadata
    {
        /// <summary>
        /// Current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates if there is a previous page.
        /// </summary>
        public bool HasPrevious => Page > 1;

        /// <summary>
        /// Indicates if there is a next page.
        /// </summary>
        public bool HasNext => Page < TotalPages;
    }
}
