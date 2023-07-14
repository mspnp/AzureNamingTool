using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AzureNamingTool.Models
{
    public class ResourceProjAppSvc
    {
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; } =  String.Empty;
        private string _ShortName =  String.Empty;
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value?.ToLower()!;   // set method
        }
        public int SortOrder { get; set; } = 0;
    }
}
