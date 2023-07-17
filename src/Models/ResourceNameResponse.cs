namespace AzureNamingTool.Models
{
    public class ResourceNameResponse
    {
        public string ResourceName { get; set; } =  String.Empty;
        public string Message { get; set; } =  String.Empty;
        public bool Success { get; set; } = false;
        public GeneratedName ResourceNameDetails { get; set; } = new();
    }
}
