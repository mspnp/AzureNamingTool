using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a policy definition.
    /// </summary>
    public class PolicyDefinition
    {
        /// <summary>
        /// Gets or sets the list of policy properties.
        /// </summary>
        public List<PolicyProperty> Properties { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDefinition"/> class with a single policy property.
        /// </summary>
        /// <param name="property">The policy property to add.</param>
        public PolicyDefinition(PolicyProperty property)
        {
            Properties.Add(property);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDefinition"/> class with a list of policy properties.
        /// </summary>
        /// <param name="property">The list of policy properties to add.</param>
        public PolicyDefinition(List<PolicyProperty> property)
        {
            Properties.AddRange(property);
        }

        /// <summary>
        /// Returns a string representation of the policy definition.
        /// </summary>
        /// <returns>A string representation of the policy definition.</returns>
        public override string ToString() { return "{ \"properties\": " + String.Join(",", Properties) + "}"; }
    }
}