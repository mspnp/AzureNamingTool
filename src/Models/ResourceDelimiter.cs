using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource delimiter.
    /// </summary>
    public class ResourceDelimiter
    {
        /// <summary>
        /// Gets or sets the ID of the resource delimiter.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource delimiter.
        /// </summary>
        [Required()]
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the delimiter of the resource delimiter.
        /// </summary>
        public string Delimiter { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the resource delimiter is enabled.
        /// </summary>
        [Required()]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the sort order of the resource delimiter.
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
