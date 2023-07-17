namespace AzureNamingTool.Models
{
    public class ResourceNameRequest
    {
        public string ResourceEnvironment { get; set; } =  String.Empty;
        public string ResourceFunction { get; set; } =  String.Empty;
        public string ResourceInstance { get; set; } =  String.Empty;
        public string ResourceLocation { get; set; } =  String.Empty;
        public string ResourceOrg { get; set; } =  String.Empty;
        public string ResourceProjAppSvc { get; set; } =  String.Empty;
        public string ResourceType { get; set; } =  String.Empty;
        public string ResourceUnitDept { get; set; } =  String.Empty;
        /// <summary>
        /// Dictionary [Custom Component Type Name],[Custom Component Short Name Value]
        /// </summary>
        public Dictionary<string, string>? CustomComponents { get; set; } = new();
        /// <summary>
        /// long - Resource Id (example: 14)
        /// </summary>
        public long ResourceId { get; set; } = 0;
        public string CreatedBy { get; set; } = "System";
    }
}
