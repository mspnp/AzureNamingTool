using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces
{
    /// <summary>
    /// Service interface for validating resource names against Azure tenant
    /// </summary>
    public interface IAzureValidationService
    {
        /// <summary>
        /// Validates a single resource name against Azure tenant
        /// </summary>
        /// <param name="resourceName">The resource name to validate</param>
        /// <param name="resourceType">The Azure resource type (e.g., Microsoft.Network/virtualNetworks)</param>
        /// <returns>Validation metadata indicating if the name exists in Azure</returns>
        Task<AzureValidationMetadata> ValidateNameAsync(string resourceName, string resourceType);

        /// <summary>
        /// Validates multiple resource names in a single batch operation
        /// </summary>
        /// <param name="validationRequests">List of names and types to validate</param>
        /// <returns>Dictionary mapping resource names to their validation metadata</returns>
        Task<Dictionary<string, AzureValidationMetadata>> ValidateBatchAsync(
            List<(string resourceName, string resourceType)> validationRequests);

        /// <summary>
        /// Tests the Azure connection and authentication
        /// </summary>
        /// <returns>Connection test result with details</returns>
        Task<AzureConnectionTestResult> TestConnectionAsync();

        /// <summary>
        /// Gets the current validation settings
        /// </summary>
        /// <returns>Azure validation settings</returns>
        Task<AzureValidationSettings> GetSettingsAsync();

        /// <summary>
        /// Updates the validation settings
        /// </summary>
        /// <param name="settings">New validation settings</param>
        /// <returns>Success result</returns>
        Task<ServiceResponse> UpdateSettingsAsync(AzureValidationSettings settings);

        /// <summary>
        /// Checks if Azure validation is currently enabled
        /// </summary>
        /// <returns>True if enabled, false otherwise</returns>
        Task<bool> IsValidationEnabledAsync();
    }

    /// <summary>
    /// Result of testing Azure connection
    /// </summary>
    public class AzureConnectionTestResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether authentication succeeded
        /// </summary>
        public bool Authenticated { get; set; } = false;

        /// <summary>
        /// Gets or sets the authentication mode used
        /// </summary>
        public string AuthenticationMode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tenant ID
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the list of accessible subscriptions
        /// </summary>
        public List<SubscriptionAccess> AccessibleSubscriptions { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether Resource Graph access is available
        /// </summary>
        public bool ResourceGraphAccess { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether a test query succeeded
        /// </summary>
        public bool TestQuerySucceeded { get; set; } = false;

        /// <summary>
        /// Gets or sets the error message if connection failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets additional details about the connection test
        /// </summary>
        public string? Message { get; set; }
    }

    /// <summary>
    /// Information about subscription access
    /// </summary>
    public class SubscriptionAccess
    {
        /// <summary>
        /// Gets or sets the subscription ID
        /// </summary>
        public string SubscriptionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subscription display name
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether read access is available
        /// </summary>
        public bool HasReadAccess { get; set; } = false;

        /// <summary>
        /// Gets or sets the subscription state
        /// </summary>
        public string? State { get; set; }
    }
}
