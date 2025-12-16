namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the response from a bulk resource name generation request.
    /// </summary>
    public class BulkResourceNameResponse
    {
        /// <summary>
        /// Gets or sets the list of individual resource name generation results.
        /// </summary>
        public List<BulkResourceNameResult> Results { get; set; } = new();

        /// <summary>
        /// Gets or sets the overall success status.
        /// </summary>
        /// <remarks>
        /// True if all resource names were generated successfully.
        /// False if any resource name generation failed.
        /// </remarks>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Gets or sets the overall message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of resources requested.
        /// </summary>
        public int TotalRequested { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of successfully generated resource names.
        /// </summary>
        public int SuccessCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of failed resource name generations.
        /// </summary>
        public int FailureCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the timestamp when the bulk operation was processed.
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents the result of a single resource name generation in a bulk operation.
    /// </summary>
    public class BulkResourceNameResult
    {
        /// <summary>
        /// Gets or sets the resource type that was processed.
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this resource name was generated successfully.
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Gets or sets the generated resource name.
        /// </summary>
        /// <remarks>
        /// Only populated if Success is true.
        /// </remarks>
        public string? ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the error message if generation failed.
        /// </summary>
        /// <remarks>
        /// Only populated if Success is false.
        /// </remarks>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the details of the generated resource name.
        /// </summary>
        /// <remarks>
        /// Only populated if Success is true and ValidateOnly is false.
        /// </remarks>
        public GeneratedName? ResourceNameDetails { get; set; }

        /// <summary>
        /// Gets or sets validation errors if any.
        /// </summary>
        public List<string>? ValidationErrors { get; set; }

        /// <summary>
        /// Gets or sets the Azure tenant validation metadata (if validation was performed).
        /// </summary>
        public AzureValidationMetadata? ValidationMetadata { get; set; }
    }
}
