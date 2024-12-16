using System.Security.Claims;
using AzureNamingTool.Helpers;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents the details of an identity provider.
    /// </summary>
    public class IdentityProviderDetails(IConfiguration configuration)
    {

        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        public string CurrentUser { get; set; } = "System";

        /// <summary>
        /// Gets or sets the current identity provider.
        /// </summary>
        public string CurrentIdentityProvider { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the current claims principal.
        /// </summary>
        public ClaimsPrincipal? CurrentClaimsPrincipal { get; set; }

        /// <summary>
        /// Gets a flag indicating whether the current user is an admin.
        /// </summary>
        /// <remarks>
        /// Returns true if the CurrentClaimsPrincipal is not null and the user has the admin claim.
        /// </remarks>
        public bool IsAdmin
        {
            get
            {
                // Get env
                string adminClaimType = Environment.GetEnvironmentVariable("AdminClaimType")
                                        ?? _configuration["AdminClaimType"]
                                        ?? "roles";
                string adminClaimValue = Environment.GetEnvironmentVariable("AdminClaimValue")
                                        ?? _configuration["AdminClaimValue"]
                                        ?? "Admin";
                Console.WriteLine($"DEBUG - AdminClaimType: {adminClaimType}");
                Console.WriteLine($"DEBUG - AdminClaimValue: {adminClaimValue}");
                bool currentClaimsPrincipalIsNull = CurrentClaimsPrincipal == null;
                Console.WriteLine($"DEBUG - CurrentClaimsPrincipal is null: {currentClaimsPrincipalIsNull}");
                bool hasClaim = false;
                if (CurrentClaimsPrincipal != null)
                {
                    // print all claims
                    foreach (Claim claim in CurrentClaimsPrincipal.Claims)
                    {
                        Console.WriteLine($"DEBUG - Stored claim Type: {claim.Type}, Value: {claim.Value}");
                    }
                    hasClaim = CurrentClaimsPrincipal.HasClaim(c => c.Type == adminClaimType && c.Value == adminClaimValue);
                    Console.WriteLine($"DEBUG - CurrentClaimsPrincipal has admin claim: {hasClaim}");
                }


                return hasClaim;
            }
        }
    }
}
