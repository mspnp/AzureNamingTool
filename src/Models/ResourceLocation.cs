using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource location.
    /// </summary>
    public class ResourceLocation
    {
        /// <summary>
        /// Gets or sets the ID of the resource location.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource location.
        /// </summary>
        [Required()]
        public string Name { get; set; } = String.Empty;

        private string _ShortName = String.Empty;

        /// <summary>
        /// Gets or sets the short name of the resource location.
        /// </summary>
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value;   // set method
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resource location is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
