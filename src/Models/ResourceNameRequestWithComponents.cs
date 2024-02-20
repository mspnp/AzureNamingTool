using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource name request with components.
    /// </summary>
    public class ResourceNameRequestWithComponents
    {
        /// <summary>
        /// Represents the resource delimiter.
        /// </summary>
        public ResourceDelimiter ResourceDelimiter { get; set; } = new();
        /// <summary>
        /// Represents the resource environment.
        /// </summary>
        public ResourceEnvironment ResourceEnvironment { get; set; } = new();
        /// <summary>
        /// Represents the resource function.
        /// </summary>
        public ResourceFunction ResourceFunction { get; set; } = new();
        /// <summary>
        /// Represents the resource instance.
        /// </summary>
        public string ResourceInstance { get; set; } = string.Empty;
        /// <summary>
        /// Represents the resource location.
        /// </summary>
        public ResourceLocation ResourceLocation { get; set; } = new();
        /// <summary>
        /// Represents the resource organization.
        /// </summary>
        public ResourceOrg ResourceOrg { get; set; } = new();
        /// <summary>
        /// Represents the resource project application service.
        /// </summary>
        public ResourceProjAppSvc ResourceProjAppSvc { get; set; } = new();
        /// <summary>
        /// Represents the resource type.
        /// </summary>
        public ResourceType ResourceType { get; set; } = new();
        /// <summary>
        /// Represents the resource unit department.
        /// </summary>
        public ResourceUnitDept ResourceUnitDept { get; set; } = new();
    }
}