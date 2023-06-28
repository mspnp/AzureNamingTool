using System.ComponentModel.DataAnnotations;

namespace AzureNamingTool.Models
{
    public class ResourceNameRequestWithComponents
    {
        public ResourceDelimiter ResourceDelimiter { get; set; } = new();
        public ResourceEnvironment ResourceEnvironment { get; set; } = new();
        public ResourceFunction ResourceFunction { get; set; } = new();
        public string ResourceInstance { get; set; } =  String.Empty;
        public ResourceLocation ResourceLocation { get; set; } = new();
        public ResourceOrg ResourceOrg { get; set; } = new();
        public ResourceProjAppSvc ResourceProjAppSvc { get; set; } = new();
        public ResourceType ResourceType { get; set; } = new();
        public ResourceUnitDept ResourceUnitDept { get; set; } = new();
    }
}