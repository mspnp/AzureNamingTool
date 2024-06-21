using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource organization.
    /// </summary>
    public class ResourceOrg
    {
        /// <summary>
        /// Gets or sets the ID of the resource organization.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource organization.
        /// </summary>
        [Required()]
        public string Name { get; set; } = String.Empty;

        private string _ShortName = String.Empty;

        /// <summary>
        /// Gets or sets the short name of the resource organization.
        /// </summary>
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value!;   // set method
        }

        /// <summary>
        /// Gets or sets the sort order of the resource organization.
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
