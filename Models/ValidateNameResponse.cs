namespace AzureNamingTool.Models
{
    public class ValidateNameResponse
    {
        public bool Valid { get; set; } = true;
        public string? Name { get; set; }
        public string? Message { get; set; }

    }
}
