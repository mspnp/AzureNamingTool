using AzureNamingTool.Models;
using AzureNamingTool.Pages;
using System.Collections.Generic;

namespace AzureNamingTool.Models
{
    public class ConfigurationData
    {
        public List<ResourceComponent> ResourceComponents { get; set; } = new();
        public List<ResourceDelimiter> ResourceDelimiters { get; set; } = new();
        public List<ResourceEnvironment> ResourceEnvironments { get; set; } = new();
        public List<ResourceLocation> ResourceLocations { get; set; } = new();
        public List<ResourceOrg> ResourceOrgs { get; set; } = new();
        public List<ResourceProjAppSvc> ResourceProjAppSvcs { get; set; } = new();
        public List<ResourceType> ResourceTypes { get; set; } = new();
        public List<ResourceUnitDept> ResourceUnitDepts { get; set; } = new();
        public List<ResourceFunction> ResourceFunctions { get; set; } = new();
        public List<CustomComponent> CustomComponents { get; set; } = new();
        public List<GeneratedName> GeneratedNames { get; set; } = new();
        public List<AdminLogMessage>? AdminLogs { get; set; } = new();
        public List<AdminUser> AdminUsers { get; set; } = new();

        public string? SALTKey { get; set; }
        public string? AdminPassword { get; set; }
        public string? APIKey { get; set; }
        public string? DismissedAlerts { get; set; }
        public string? DuplicateNamesAllowed { get; set; } = "False";
        public string? GenerationWebhook { get; set; }
        public string? ConnectivityCheckEnabled { get; set; } = "True";
        public string? IdentityHeaderName { get; set; }
        public string? ResourceTypeEditingAllowed { get; set; } = "False";
        public string? AutoIncrementResourceInstance { get; set; } = "False";
    }
}
