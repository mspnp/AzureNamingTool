namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the response of name validation.
    /// </summary>
    public class ValidateNameResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the name is valid.
        /// </summary>
        public bool Valid { get; set; } = true;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the name validation.
        /// </summary>
        public string? Message { get; set; }
    }
}
