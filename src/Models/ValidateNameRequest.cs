namespace AzureNamingTool.Models
{
    public class ValidateNameRequest
    {
        public long? ResourceTypeId { get; set;} 
        public string? ResourceType { get; set; }
        public string? Name { get; set; }
    }
}
