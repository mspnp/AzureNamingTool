using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    public class ResourceType
    {
        public long Id { get; set; }
        [Required()]
        public string Resource { get; set; } = String.Empty;
        public string Optional { get; set; } = String.Empty;
        public string Exclude { get; set; } = String.Empty;
        public string Property { get; set; } = String.Empty;
        private string _ShortName = String.Empty;
        [JsonPropertyName("ShortName")]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value?.ToLower()!;   // set method
        }
        public string Scope { get; set; } = String.Empty;
        public string LengthMin { get; set; } = String.Empty;
        public string LengthMax { get; set; } = String.Empty;
        public string ValidText { get; set; } = String.Empty;
        public string InvalidText { get; set; } = String.Empty;
        public string InvalidCharacters { get; set; } = String.Empty;
        public string InvalidCharactersStart { get; set; } = String.Empty;
        public string InvalidCharactersEnd { get; set; } = String.Empty;
        public string InvalidCharactersConsecutive { get; set; } = String.Empty;
        public string Regx { get; set; } = String.Empty;
        public string StaticValues { get; set; } = String.Empty;
        public bool Enabled { get; set; } = true;
        public bool ApplyDelimiter { get; set; } = true;
    }
}
