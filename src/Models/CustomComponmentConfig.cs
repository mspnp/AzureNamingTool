namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the configuration for custom components.
    /// </summary>
    public class CustomComponmentConfig
    {
        /// <summary>
        /// Gets or sets the parent components.
        /// </summary>
        public List<ResourceComponent>? ParentComponents { get; set; }

        /// <summary>
        /// Gets or sets the custom components.
        /// </summary>
        public List<CustomComponent>? CustomComponents { get; set; }
    }
}
