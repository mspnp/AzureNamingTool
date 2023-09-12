namespace AzureNamingTool.Models
{
    public class GeneratedName
    {
        public long Id { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string ResourceName { get; set; } =  String.Empty;
        public string ResourceTypeName { get; set; } =  String.Empty;
        public List<string[]> Components { get; set; } = new();
        public string User { get; set; } = "General";
        public string? Message { get; set; }
    }
}
