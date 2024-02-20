namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents information about a theme.
    /// </summary>
    public class ThemeInfo
    {
        /// <summary>
        /// Gets or sets the name of the theme.
        /// </summary>
        public string ThemeName { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the style of the theme.
        /// </summary>
        public string ThemeStyle { get; set; } = String.Empty;
    }
}
