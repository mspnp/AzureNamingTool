using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource component.
    /// </summary>
    public class ResourceComponent
    {
        /// <summary>
        /// Gets or sets the ID of the resource component.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource component.
        /// </summary>
        [Required()]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the resource component.
        /// </summary>
        [Required()]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the resource component is enabled.
        /// </summary>
        [Required()]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the resource component.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the resource component is custom.
        /// </summary>
        public bool IsCustom { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the resource component is free text.
        /// </summary>
        public bool IsFreeText { get; set; } = false;

        /// <summary>
        /// Gets or sets the minimum length of the resource component.
        /// </summary>
        public string MinLength { get; set; } = "1";

        /// <summary>
        /// Gets or sets the maximum length of the resource component.
        /// </summary>
        public string MaxLength { get; set; } = "10";

        /// <summary>
        /// Gets or sets a value indicating whether to enforce randomization of the resource component.
        /// </summary>
        public bool EnforceRandom { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the resource component should be alphanumeric.
        /// </summary>
        public bool Alphanumeric { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to apply a delimiter before the resource component.
        /// </summary>
        public bool ApplyDelimiterBefore { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to apply a delimiter after the resource component.
        /// </summary>
        public bool ApplyDelimiterAfter { get; set; } = true;
    }
}
