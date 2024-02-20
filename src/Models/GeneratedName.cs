namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a generated name.
    /// </summary>
    public class GeneratedName
    {
        /// <summary>
        /// Gets or sets the ID of the generated name.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the generated name was created.
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        public string ResourceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource type name.
        /// </summary>
        public string ResourceTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the components of the generated name.
        /// </summary>
        public List<string[]> Components { get; set; } = [];

        /// <summary>
        /// Gets or sets the user who generated the name.
        /// </summary>
        public string User { get; set; } = "General";

        /// <summary>
        /// Gets or sets the optional message associated with the generated name.
        /// </summary>
        public string? Message { get; set; }
    }
}
