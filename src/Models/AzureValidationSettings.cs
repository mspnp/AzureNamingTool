namespace AzureNamingTool.Models
{
    /// <summary>
    /// Configuration settings for Azure tenant name validation
    /// </summary>
    public class AzureValidationSettings
    {
        /// <summary>
        /// Gets or sets the unique identifier for this settings record
        /// Always 1 since this is a singleton settings object
        /// </summary>
        public long Id { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether Azure tenant validation is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the authentication mode for Azure
        /// </summary>
        public AuthenticationMode AuthMode { get; set; } = AuthenticationMode.ManagedIdentity;

        /// <summary>
        /// Gets or sets the Azure tenant ID
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of subscription IDs to query for name validation
        /// </summary>
        public List<string> SubscriptionIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the management group ID for querying (optional)
        /// </summary>
        public string? ManagementGroupId { get; set; }

        /// <summary>
        /// Gets or sets the service principal settings
        /// </summary>
        public ServicePrincipalSettings? ServicePrincipal { get; set; }

        /// <summary>
        /// Gets or sets the Azure Key Vault settings for credential storage
        /// </summary>
        public KeyVaultSettings? KeyVault { get; set; }

        /// <summary>
        /// Gets or sets the conflict resolution strategy settings
        /// </summary>
        public ConflictResolutionSettings ConflictResolution { get; set; } = new();

        /// <summary>
        /// Gets or sets the cache settings for validation results
        /// </summary>
        public CacheSettings Cache { get; set; } = new();
    }

    /// <summary>
    /// Service principal authentication settings
    /// </summary>
    public class ServicePrincipalSettings
    {
        /// <summary>
        /// Gets or sets the Azure AD application (client) ID
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client secret (encrypted or direct)
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the name of the secret in Key Vault (if using Key Vault)
        /// </summary>
        public string? ClientSecretKeyVaultName { get; set; }
    }

    /// <summary>
    /// Azure Key Vault settings for credential storage
    /// </summary>
    public class KeyVaultSettings
    {
        /// <summary>
        /// Gets or sets the Key Vault URI
        /// </summary>
        public string KeyVaultUri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the client secret in Key Vault
        /// </summary>
        public string ClientSecretName { get; set; } = "naming-tool-client-secret";
    }

    /// <summary>
    /// Settings for resolving name conflicts when a name already exists in Azure
    /// </summary>
    public class ConflictResolutionSettings
    {
        /// <summary>
        /// Gets or sets the conflict resolution strategy
        /// Default is NotifyOnly for maximum compatibility with all naming conventions
        /// </summary>
        public ConflictStrategy Strategy { get; set; } = ConflictStrategy.NotifyOnly;

        /// <summary>
        /// Gets or sets the maximum number of auto-increment attempts
        /// </summary>
        public int MaxAttempts { get; set; } = 100;

        /// <summary>
        /// Gets or sets the padding for incremented instance numbers (e.g., 3 = 001, 002, etc.)
        /// </summary>
        public int IncrementPadding { get; set; } = 3;

        /// <summary>
        /// Gets or sets a value indicating whether to include warnings in the response
        /// </summary>
        public bool IncludeWarnings { get; set; } = true;
    }

    /// <summary>
    /// Cache settings for validation results
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the cache duration in minutes
        /// </summary>
        public int DurationMinutes { get; set; } = 5;
    }

    /// <summary>
    /// Azure authentication mode
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// Use Azure Managed Identity (recommended when hosting in Azure)
        /// </summary>
        ManagedIdentity,

        /// <summary>
        /// Use Service Principal with client ID and secret
        /// </summary>
        ServicePrincipal
    }

    /// <summary>
    /// Strategy for resolving name conflicts
    /// </summary>
    public enum ConflictStrategy
    {
        /// <summary>
        /// Automatically increment the instance number until a unique name is found
        /// </summary>
        AutoIncrement,

        /// <summary>
        /// Return the name with a warning that it exists, but don't modify it
        /// </summary>
        NotifyOnly,

        /// <summary>
        /// Fail the request and return an error
        /// </summary>
        Fail,

        /// <summary>
        /// Add a random suffix to make the name unique
        /// </summary>
        SuffixRandom
    }

    /// <summary>
    /// Metadata about Azure validation performed on a generated name
    /// </summary>
    public class AzureValidationMetadata
    {
        /// <summary>
        /// Gets or sets a value indicating whether Azure validation was performed
        /// </summary>
        public bool ValidationPerformed { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the name exists in Azure
        /// </summary>
        public bool ExistsInAzure { get; set; } = false;

        /// <summary>
        /// Gets or sets the original name before any auto-increment modifications
        /// </summary>
        public string? OriginalName { get; set; }

        /// <summary>
        /// Gets or sets the number of increment attempts made to find a unique name
        /// </summary>
        public int? IncrementAttempts { get; set; }

        /// <summary>
        /// Gets or sets the list of conflicting resource IDs found in Azure
        /// </summary>
        public List<string>? ConflictingResources { get; set; }

        /// <summary>
        /// Gets or sets the validation warning message (if any)
        /// </summary>
        public string? ValidationWarning { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when validation was performed
        /// </summary>
        public DateTime? ValidationTimestamp { get; set; }
    }
}
