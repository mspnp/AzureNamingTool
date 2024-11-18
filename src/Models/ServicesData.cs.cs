namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the data for services.
    /// </summary>
    public class ServicesData
    {
        /// <summary>
        /// Gets or sets the list of resource components.
        /// </summary>
        public List<ResourceComponent>? ResourceComponents { get; set; }

        /// <summary>
        /// Gets or sets the list of resource delimiters.
        /// </summary>
        public List<ResourceDelimiter>? ResourceDelimiters { get; set; }

        /// <summary>
        /// Gets or sets the list of resource environments.
        /// </summary>
        public List<ResourceEnvironment>? ResourceEnvironments { get; set; }

        /// <summary>
        /// Gets or sets the list of resource locations.
        /// </summary>
        public List<ResourceLocation>? ResourceLocations { get; set; }

        /// <summary>
        /// Gets or sets the list of resource organizations.
        /// </summary>
        public List<ResourceOrg>? ResourceOrgs { get; set; }

        /// <summary>
        /// Gets or sets the list of resource project application services.
        /// </summary>
        public List<ResourceProjAppSvc>? ResourceProjAppSvcs { get; set; }

        /// <summary>
        /// Gets or sets the list of resource types.
        /// </summary>
        public List<ResourceType>? ResourceTypes { get; set; }

        /// <summary>
        /// Gets or sets the list of resource unit departments.
        /// </summary>
        public List<ResourceUnitDept>? ResourceUnitDepts { get; set; }

        /// <summary>
        /// Gets or sets the list of resource functions.
        /// </summary>
        public List<ResourceFunction>? ResourceFunctions { get; set; }

        /// <summary>
        /// Gets or sets the list of custom components.
        /// </summary>
        public List<CustomComponent>? CustomComponents { get; set; }

        /// <summary>
        /// Gets or sets the list of generated names.
        /// </summary>
        public List<GeneratedName>? GeneratedNames { get; set; }

        /// <summary>
        /// Gets or sets the list of admin log messages.
        /// </summary>
        public List<AdminLogMessage>? AdminLogMessages { get; set; }

        /// <summary>
        /// Gets or sets the list of admin users.
        /// </summary>
        public List<AdminUser>? AdminUsers { get; set; }
    }
}
