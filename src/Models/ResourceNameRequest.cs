namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource name request.
    /// </summary>
    public class ResourceNameRequest
    {
        /// <summary>
        /// Gets or sets the resource environment.
        /// </summary>
        public string ResourceEnvironment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource function.
        /// </summary>
        public string ResourceFunction { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource instance.
        /// </summary>
        public string ResourceInstance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource location.
        /// </summary>
        public string ResourceLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource organization.
        /// </summary>
        public string ResourceOrg { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource project or application service.
        /// </summary>
        public string ResourceProjAppSvc { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource unit or department.
        /// </summary>
        public string ResourceUnitDept { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the custom components.
        /// </summary>
        /// <remarks>
        /// Dictionary [Custom Component Type Name],[Custom Component Short Name Value]
        /// </remarks>
        public Dictionary<string, string>? CustomComponents { get; set; } = [];

        /// <summary>
        /// Gets or sets the resource ID.
        /// </summary>
        /// <remarks>
        /// long - Resource Id (example: 14)
        /// </remarks>
        public long ResourceId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the creator of the resource.
        /// </summary>
        public string CreatedBy { get; set; } = "System";
    }
}
