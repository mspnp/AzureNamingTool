using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a custom component.
    /// </summary>
    public class CustomComponent
    {
        /// <summary>
        /// Gets or sets the ID of the custom component.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the parent component of the custom component.
        /// </summary>
        [Required()]
        public string ParentComponent { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the name of the custom component.
        /// </summary>
        [Required()]
        public string Name { get; set; } = String.Empty;

        private string _ShortName = String.Empty;

        /// <summary>
        /// Gets or sets the short name of the custom component.
        /// </summary>
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value;   // set method
        }

        /// <summary>
        /// Gets or sets the sort order of the custom component.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum length of the custom component.
        /// </summary>
        public string MinLength { get; set; } = "1";

        /// <summary>
        /// Gets or sets the maximum length of the custom component.
        /// </summary>
        public string MaxLength { get; set; } = "10";
    }
}
