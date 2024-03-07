using AzureNamingTool.Models;
using AzureNamingTool.Components.Pages;
using System.Collections.Generic;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the configuration data for the application.
    /// </summary>
    public class ConfigurationData
    {
        /// <summary>
        /// Gets or sets the list of resource components.
        /// </summary>
        public List<ResourceComponent> ResourceComponents { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource delimiters.
        /// </summary>
        public List<ResourceDelimiter> ResourceDelimiters { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource environments.
        /// </summary>
        public List<ResourceEnvironment> ResourceEnvironments { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource locations.
        /// </summary>
        public List<ResourceLocation> ResourceLocations { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource organizations.
        /// </summary>
        public List<ResourceOrg> ResourceOrgs { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource project application services.
        /// </summary>
        public List<ResourceProjAppSvc> ResourceProjAppSvcs { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource types.
        /// </summary>
        public List<ResourceType> ResourceTypes { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource unit departments.
        /// </summary>
        public List<ResourceUnitDept> ResourceUnitDepts { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of resource functions.
        /// </summary>
        public List<ResourceFunction> ResourceFunctions { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of custom components.
        /// </summary>
        public List<CustomComponent> CustomComponents { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of generated names.
        /// </summary>
        public List<GeneratedName> GeneratedNames { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of admin log messages.
        /// </summary>
        public List<AdminLogMessage>? AdminLogs { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of admin users.
        /// </summary>
        public List<AdminUser> AdminUsers { get; set; } = [];

        /// <summary>
        /// Gets or sets the SALT key.
        /// </summary>
        public string? SALTKey { get; set; }

        /// <summary>
        /// Gets or sets the admin password.
        /// </summary>
        public string? AdminPassword { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        public string? APIKey { get; set; }

        /// <summary>
        /// Gets or sets the read-only API key.
        /// </summary>
        public string? ReadOnlyAPIKey { get; set; }

        /// <summary>
        /// Gets or sets the dismissed alerts.
        /// </summary>
        public string? DismissedAlerts { get; set; }

        /// <summary>
        /// Gets or sets whether duplicate names are allowed.
        /// </summary>
        public string? DuplicateNamesAllowed { get; set; } = "False";

        /// <summary>
        /// Gets or sets the generation webhook.
        /// </summary>
        public string? GenerationWebhook { get; set; }

        /// <summary>
        /// Gets or sets whether connectivity check is enabled.
        /// </summary>
        public string? ConnectivityCheckEnabled { get; set; } = "True";

        /// <summary>
        /// Gets or sets the identity header name.
        /// </summary>
        public string? IdentityHeaderName { get; set; }

        /// <summary>
        /// Gets or sets whether resource type editing is allowed.
        /// </summary>
        public string? ResourceTypeEditingAllowed { get; set; } = "False";

        /// <summary>
        /// Gets or sets whether auto-increment resource instance is enabled.
        /// </summary>
        public string? AutoIncrementResourceInstance { get; set; } = "False";

        /// <summary>
        /// Gets or sets whether instructions are enabled.
        /// </summary>
        public string? InstructionsEnabled { get; set; } = "True";

        /// <summary>
        /// Gets or sets whether generated names log is enabled.
        /// </summary>
        public string? GeneratedNamesLogEnabled { get; set; } = "True";

        /// <summary>
        /// Gets or sets whether latest news is enabled.
        /// </summary>
        public string? LatestNewsEnabled { get; set; } = "False";

        /// <summary>
        /// Gets or sets whether retaining of Generate opage selections is enabled.
        /// </summary>
        public string? RetainGenerateSelections { get; set; } = "False";
    }
}
