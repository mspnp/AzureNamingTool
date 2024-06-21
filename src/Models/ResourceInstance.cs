using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource instance.
    /// </summary>
    public class ResourceInstance
    {
        /// <summary>
        /// Gets or sets the ID of the resource instance.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource instance.
        /// </summary>
        [Required()]
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the sort order of the resource instance.
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
