namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a bulk resource name generation request.
    /// </summary>
    public class BulkResourceNameRequest
    {
        /// <summary>
        /// Gets or sets the list of resource types to generate names for.
        /// </summary>
        /// <remarks>
        /// Array of resource type short names (e.g., "vm", "st", "kv")
        /// </remarks>
        public List<string> ResourceTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets the shared resource environment for all resources.
        /// </summary>
        public string ResourceEnvironment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shared resource function for all resources.
        /// </summary>
        public string ResourceFunction { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shared resource instance for all resources.
        /// </summary>
        public string ResourceInstance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shared resource location for all resources.
        /// </summary>
        public string ResourceLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shared resource organization for all resources.
        /// </summary>
        public string ResourceOrg { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shared resource project or application service for all resources.
        /// </summary>
        public string ResourceProjAppSvc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shared resource unit or department for all resources.
        /// </summary>
        public string ResourceUnitDept { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shared custom components for all resources.
        /// </summary>
        /// <remarks>
        /// Dictionary [Custom Component Type Name],[Custom Component Short Name Value]
        /// </remarks>
        public Dictionary<string, string>? CustomComponents { get; set; } = new();

        /// <summary>
        /// Gets or sets per-resource type overrides.
        /// </summary>
        /// <remarks>
        /// Dictionary [Resource Type Short Name],[Override Values]
        /// Allows overriding specific values for individual resource types.
        /// </remarks>
        public Dictionary<string, ResourceTypeOverride>? ResourceTypeOverrides { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to continue processing if an individual resource name generation fails.
        /// </summary>
        /// <remarks>
        /// Default is true - continue processing remaining resources even if one fails.
        /// </remarks>
        public bool ContinueOnError { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to only validate the names without saving to the database.
        /// </summary>
        /// <remarks>
        /// When true, validates all names but does not persist them.
        /// </remarks>
        public bool ValidateOnly { get; set; } = false;

        /// <summary>
        /// Gets or sets the creator of the resources.
        /// </summary>
        public string CreatedBy { get; set; } = "System";
    }

    /// <summary>
    /// Represents override values for a specific resource type in a bulk operation.
    /// </summary>
    public class ResourceTypeOverride
    {
        /// <summary>
        /// Gets or sets the resource environment override.
        /// </summary>
        public string? ResourceEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the resource function override.
        /// </summary>
        public string? ResourceFunction { get; set; }

        /// <summary>
        /// Gets or sets the resource instance override.
        /// </summary>
        public string? ResourceInstance { get; set; }

        /// <summary>
        /// Gets or sets the resource location override.
        /// </summary>
        public string? ResourceLocation { get; set; }

        /// <summary>
        /// Gets or sets the resource organization override.
        /// </summary>
        public string? ResourceOrg { get; set; }

        /// <summary>
        /// Gets or sets the resource project or application service override.
        /// </summary>
        public string? ResourceProjAppSvc { get; set; }

        /// <summary>
        /// Gets or sets the resource unit or department override.
        /// </summary>
        public string? ResourceUnitDept { get; set; }

        /// <summary>
        /// Gets or sets the custom components override.
        /// </summary>
        public Dictionary<string, string>? CustomComponents { get; set; }
    }
}
