namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents an admin log message.
    /// </summary>
    public class AdminLogMessage
    {
        /// <summary>
        /// Gets or sets the ID of the log message.
        /// </summary>
        public long? Id { get; set; } = 0;

        /// <summary>
        /// Gets or sets the date and time when the log message was created.
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the title of the log message.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content of the log message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source of the log message.
        /// </summary>
        public string Source { get; set; } = "System";
    }
}