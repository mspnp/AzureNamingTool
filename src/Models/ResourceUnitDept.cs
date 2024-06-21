using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource unit department.
    /// </summary>
    public class ResourceUnitDept
    {
        /// <summary>
        /// Gets or sets the ID of the resource unit department.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource unit department.
        /// </summary>
        [Required()]
        public string Name { get; set; } = string.Empty;

        private string _ShortName = string.Empty;

        /// <summary>
        /// Gets or sets the short name of the resource unit department.
        /// </summary>
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }
            set
            {
                _ShortName = value;
            }
        }

        /// <summary>
        /// Gets or sets the sort order of the resource unit department.
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
