namespace AzureNamingTool.Models
{
    public class SiteConfiguration
    {
        public string? SALTKey { get; set; }
        public string? AdminPassword { get; set; }
        public string? APIKey { get; set; }
        public string? ReadOnlyAPIKey { get; set; }
        public string? AppTheme { get; set; }
        public bool? DevMode { get; set; } = false;
        public string? DismissedAlerts { get; set; }
        public string? DuplicateNamesAllowed { get; set; } = "False";
        public string? GenerationWebhook { get; set; }
        public string? ConnectivityCheckEnabled { get; set; } = "True";
        public string? IdentityHeaderName { get; set; }
        public string? ResourceTypeEditingAllowed { get; set; } = "False";
        public string? AutoIncrementResourceInstance { get; set; } = "False";
        public string? DisableNews { get; set; } = "False";
        public string? DisableGeneratedNamesLog { get; set; } = "False";
        public string? DisableInstructions { get; set; } = "False";
    }
}
