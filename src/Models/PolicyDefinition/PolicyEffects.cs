using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the possible effects of a policy.
    /// </summary>
    public enum PolicyEffects
    {
        /// <summary>
        /// Specifies that the policy will only audit the action without denying it.
        /// </summary>
        Audit,

        /// <summary>
        /// Specifies that the policy will deny the action.
        /// </summary>
        Deny
    }
}
