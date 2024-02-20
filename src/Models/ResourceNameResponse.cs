namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the response containing the resource name information.
    /// </summary>
    public class ResourceNameResponse
    {
        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        public string ResourceName { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Gets or sets the details of the generated resource name.
        /// </summary>
        public GeneratedName ResourceNameDetails { get; set; } = new();
    }
}
