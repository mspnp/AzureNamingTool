namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents information about a theme.
    /// </summary>
    public class SiteConfiguration
    {
        /// <summary>
        /// Gets or sets the SALT key used for secure operations.
        /// </summary>
        public string? SALTKey { get; set; }

        /// <summary>
        /// Gets or sets the password for the admin user.
        /// </summary>
        public string? AdminPassword { get; set; }

        /// <summary>
        /// Gets or sets the API key used for authentication.
        /// </summary>
        public string? APIKey { get; set; }

        /// <summary>
        /// Gets or sets the read-only API key.
        /// </summary>
        public string? ReadOnlyAPIKey { get; set; }

        /// <summary>
        /// Gets or sets the theme of the application.
        /// </summary>
        public string? AppTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application is running in development mode.
        /// </summary>
        public bool? DevMode { get; set; } = false;

        /// <summary>
        /// Gets or sets the dismissed alerts in the application.
        /// </summary>
        public string? DismissedAlerts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether duplicate names are allowed.
        /// </summary>
        public string? DuplicateNamesAllowed { get; set; } = "False";

        /// <summary>
        /// Gets or sets the webhook for generating names.
        /// </summary>
        public string? GenerationWebhook { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether connectivity check is enabled.
        /// </summary>
        public string? ConnectivityCheckEnabled { get; set; } = "True";

        /// <summary>
        /// Gets or sets the name of the identity header.
        /// </summary>
        public string? IdentityHeaderName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether resource type editing is allowed.
        /// </summary>
        public string? ResourceTypeEditingAllowed { get; set; } = "False";

        /// <summary>
        /// Gets or sets a value indicating whether resource instances are auto-incremented.
        /// </summary>
        public string? AutoIncrementResourceInstance { get; set; } = "False";

        /// <summary>
        /// Gets or sets a value indicating whether instructions are enabled.
        /// </summary>
        public string? InstructionsEnabled { get; set; } = "True";

        /// <summary>
        /// Gets or sets a value indicating whether generated names logging is enabled.
        /// </summary>
        public string? GeneratedNamesLogEnabled { get; set; } = "True";

        /// <summary>
        /// Gets or sets a value indicating whether the latest news feature is enabled.
        /// </summary>
        public string? LatestNewsEnabled { get; set; } = "False";

        /// <summary>
        /// Gets or sets a value indicating whether the Generate oage will retain selected vlues after generation
        /// </summary>
        public string? RetainGenerateSelections { get; set; } = "False";
    }
}
