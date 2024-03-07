using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource project application service.
    /// </summary>
    public class ResourceProjAppSvc
    {
        /// <summary>
        /// Gets or sets the ID of the resource project application service.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource project application service.
        /// </summary>
        [Required()]
        public string Name { get; set; } = string.Empty;

        private string _ShortName = string.Empty;

        /// <summary>
        /// Gets or sets the short name of the resource project application service.
        /// </summary>
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }
            set => _ShortName = value;
        }

        /// <summary>
        /// Gets or sets the sort order of the resource project application service.
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
