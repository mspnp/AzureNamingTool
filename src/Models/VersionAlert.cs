namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a version alert.
    /// </summary>
    public class VersionAlert
    {
        /// <summary>
        /// Gets or sets the ID of the version alert.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the version string.
        /// </summary>
        public string Version { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the alert message.
        /// </summary>
        public string Alert { get; set; } = String.Empty;
    }
}
