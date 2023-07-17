using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class ResourceInstance
    {
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; } =  String.Empty;
        public int SortOrder { get; set; } = 0;
    }
}
