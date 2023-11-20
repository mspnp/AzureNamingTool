using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class ResourceComponent
    {
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; } = String.Empty;
        [Required()]
        public string DisplayName { get; set; } = String.Empty;
        [Required()]
        public bool Enabled { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsCustom { get; set; } = false;
        public bool IsFreeText { get; set; } = false;
        public string MinLength { get; set; } = "1";
        public string MaxLength { get; set; } = "10";
        public bool EnforceRandom { get; set; } = false;
        public bool Alphanumeric { get; set; } = true;
        public bool ApplyDelimiterBefore { get; set; } = true;
        public bool ApplyDelimiterAfter { get; set; } = true;
    }
}
