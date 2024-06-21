using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a resource type.
    /// </summary>
    public class ResourceType
    {
        /// <summary>
        /// Gets or sets the ID of the resource type.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        [Required()]
        public string Resource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional value.
        /// </summary>
        public string Optional { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exclude value.
        /// </summary>
        public string Exclude { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Property { get; set; } = string.Empty;

        private string _ShortName = string.Empty;

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        [JsonPropertyName("ShortName")]
        public string ShortName
        {
            get { return _ShortName; }
            set => _ShortName = value;
        }

        /// <summary>
        /// Gets or sets the scope value.
        /// </summary>
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the minimum length value.
        /// </summary>
        public string LengthMin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum length value.
        /// </summary>
        public string LengthMax { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the valid text value.
        /// </summary>
        public string ValidText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invalid text value.
        /// </summary>
        public string InvalidText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invalid characters value.
        /// </summary>
        public string InvalidCharacters { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invalid characters start value.
        /// </summary>
        public string InvalidCharactersStart { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invalid characters end value.
        /// </summary>
        public string InvalidCharactersEnd { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invalid consecutive characters value.
        /// </summary>
        public string InvalidCharactersConsecutive { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the regular expression value.
        /// </summary>
        public string Regx { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the static values.
        /// </summary>
        public string StaticValues { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the resource type is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to apply delimiter.
        /// </summary>
        public bool ApplyDelimiter { get; set; } = true;
    }
}
