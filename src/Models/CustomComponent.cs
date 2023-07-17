using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class CustomComponent
    {
        public long Id { get; set; }
        [Required()]
        public string ParentComponent { get; set; } =  String.Empty;
        [Required()]
        public string Name { get; set; } =  String.Empty;
        private string _ShortName = String.Empty;
        [Required()]
        public string ShortName
        {
            get { return _ShortName; }   // get method
            set => _ShortName = value?.ToLower()!;   // set method
        }
        public int SortOrder { get; set; } = 0;
        public string MinLength { get; set; } = "1";
        public string MaxLength { get; set; } = "10";
    }
}
