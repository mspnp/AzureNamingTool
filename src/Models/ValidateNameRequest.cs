namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a request to validate a name.
    /// </summary>
    public class ValidateNameRequest
    {
        /// <summary>
        /// Gets or sets the resource type ID.
        /// </summary>
        public long? ResourceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        public string? ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the name to be validated.
        /// </summary>
        public string? Name { get; set; }
    }
}
