using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a policy property.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PolicyProperty"/> class.
    /// </remarks>
    /// <param name="name">The display name of the policy property.</param>
    /// <param name="description">The description of the policy property.</param>
    /// <param name="mode">The mode of the policy property.</param>
    public class PolicyProperty(String name, string description, String mode = "all")
    {

        /// <summary>
        /// Gets or sets the display name of the policy property.
        /// </summary>
        public string DisplayName { get; set; } = name;

        /// <summary>
        /// Gets or sets the policy type of the policy property.
        /// </summary>
        public String PolicyType { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the mode of the policy property.
        /// </summary>
        public String Mode { get; set; } = mode;

        /// <summary>
        /// Gets or sets the description of the policy property.
        /// </summary>
        public String Description { get; set; } = description;

        /// <summary>
        /// Gets or sets the metadata of the policy property.
        /// </summary>
        public PropertyMetadata Metadata { get; set; } = new PropertyMetadata() { Version = "1.0.0", Category = "Azure" };

        /// <summary>
        /// Gets or sets the policy rule of the policy property.
        /// </summary>
        public String PolicyRule { get; set; } = String.Empty;

        /// <summary>
        /// Returns a string that represents the policy property.
        /// </summary>
        /// <returns>A string that represents the policy property.</returns>
        public override string ToString() { return "{" + PolicyRule + "}"; }
    }

    /// <summary>
    /// Represents the metadata of a policy property.
    /// </summary>
    public class PropertyMetadata
    {
        /// <summary>
        /// Gets or sets the version of the property metadata.
        /// </summary>
        public String Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the category of the property metadata.
        /// </summary>
        public String Category { get; set; } = "Name";

        /// <summary>
        /// Returns a string that represents the property metadata.
        /// </summary>
        /// <returns>A string that represents the property metadata.</returns>
        public override string ToString() { return "\"metadata\": { \"version\": \"" + Version + "\", \"category\": \"" + Category + "\" }"; }
    }
}