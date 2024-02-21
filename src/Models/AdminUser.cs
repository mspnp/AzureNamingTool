namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents an admin user.
    /// </summary>
    public class AdminUser
    {
        /// <summary>
        /// Gets or sets the ID of the admin user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the admin user.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
