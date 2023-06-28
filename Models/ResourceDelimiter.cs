using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class ResourceDelimiter
    {
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; } =  String.Empty;
        public string Delimiter { get; set; } =  String.Empty;
        [Required()]
        public bool Enabled { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }
}
