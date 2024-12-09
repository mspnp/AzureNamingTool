using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;

// https://learn.microsoft.com/en-us/azure/app-service/configure-authentication-user-identities#decoding-the-client-principal-header
/// <summary>
/// Provides methods to parse HttpRequest and create ClaimsPrincipal.
/// </summary>
public static class ClaimsPrincipalParser
{
    private class ClientPrincipalClaim
    {
        [JsonPropertyName("typ")]
        public required string Type { get; set; }
        [JsonPropertyName("val")]
        public required string Value { get; set; }
    }

    private class ClientPrincipal
    {
        [JsonPropertyName("auth_typ")]
        public string? IdentityProvider { get; set; }
        [JsonPropertyName("name_typ")]
        public string? NameClaimType { get; set; }
        [JsonPropertyName("role_typ")]
        public string? RoleClaimType { get; set; }
        [JsonPropertyName("claims")]
        public IEnumerable<ClientPrincipalClaim>? Claims { get; set; }
    }

    /// <summary>
    /// Parses the HttpRequest to extract and create a ClaimsPrincipal.
    /// </summary>
    /// <param name="req">The HttpRequest containing the client principal header.</param>
    /// <returns>A ClaimsPrincipal object.</returns>
    public static ClaimsPrincipal? Parse(HttpRequest req)
    {
        if (req.Headers.TryGetValue(ConfigurationHelper.GetAppSetting("IdentityHeaderFull", true), out var header))
        {
            var principal = new ClientPrincipal();


            var data = header[0];
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }
            try
            {
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                Console.WriteLine($"DEBUG - IdentityHeaderFull JSON: {json}");
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                /*
                 *  At this point, the code can iterate through `principal.Claims` to
                 *  check claims as part of validation. Alternatively, we can convert
                 *  it into a standard object with which to perform those checks later
                 *  in the request pipeline. That object can also be leveraged for 
                 *  associating user data, etc. The rest of this function performs such
                 *  a conversion to create a `ClaimsPrincipal` as might be used in 
                 *  other .NET code.
                 */
                if (principal == null)
                {
                    return null;
                }
                var identity = new ClaimsIdentity(principal.IdentityProvider, principal.NameClaimType, principal.RoleClaimType);
                if (principal.Claims != null)
                {
                    identity.AddClaims(principal.Claims.Select(c => new Claim(c.Type, c.Value)));
                }
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                // Log the exception
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return null;
            }
        }
        return null;
    }
}