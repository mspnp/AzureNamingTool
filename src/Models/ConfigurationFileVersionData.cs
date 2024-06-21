namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the data for the configuration file version.
    /// </summary>
    public class ConfigurationFileVersionData
    {
        /// <summary>
        /// Gets or sets the resource types.
        /// </summary>
        public string resourcetypes { get; set; } = "0.0.0";

        /// <summary>
        /// Gets or sets the resource locations.
        /// </summary>
        public string resourcelocations { get; set; } = "0.0.0";
    }
}
