using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for identity-related operations.
    /// </summary>
    public class IdentityHelper
    {
        /// <summary>
        /// Checks if the user is an admin user.
        /// </summary>
        /// <param name="state">The state container.</param>
        /// <param name="session">The protected session storage.</param>
        /// <param name="name">The username.</param>
        /// <param name="adminUserService">The admin user service.</param>
        /// <returns>True if the user is an admin user, otherwise false.</returns>
        public static async Task<bool> IsAdminUser(StateContainer state, ProtectedSessionStorage session, string name, IAdminUserService adminUserService)
        {
            bool result = false;
            try
            {
                // Check if the username is in the list of Admin Users
                ServiceResponse serviceResponse = await adminUserService.GetItemsAsync();
                if (serviceResponse.Success)
                {
                    if (GeneralHelper.IsNotNull(serviceResponse.ResponseObject))
                    {
                        List<AdminUser> adminusers = serviceResponse.ResponseObject!;
                        if (adminusers.Exists(x => x.Name.ToLower() == name.ToLower()))
                        {
                            state.SetAdmin(true);
                            await session.SetAsync("admin", true);
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't expose to user
                Console.WriteLine($"Error checking admin user: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <param name="session">The protected session storage.</param>
        /// <returns>The current user.</returns>
        public static async Task<string> GetCurrentUser(ProtectedSessionStorage session)
        {
            string currentuser = "System";
            try
            {
                var currentuservalue = await session.GetAsync<string>("currentuser");
                if (!String.IsNullOrEmpty(currentuservalue.Value))
                {
                    currentuser = currentuservalue.Value;
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return currentuser;
        }
    }
}
