using AzureNamingTool.Models;
using System.Text.Json;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for logging operations.
    /// </summary>
    public class LogHelper
    {
        /// <summary>
        /// Helper class for logging operations.
        /// </summary>
        private static readonly JsonSerializerOptions options = new()
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Retrieves the list of generated names.
        /// </summary>
        /// <returns>List of GeneratedName objects.</returns>
        public static async Task<List<GeneratedName>> GetGeneratedNames()
        {
            List<GeneratedName> lstGeneratedNames = [];
            try
            {
                string items = await FileSystemHelper.ReadFile("generatednames.json");
                if (GeneralHelper.IsNotNull(items))
                {
                    lstGeneratedNames = [.. JsonSerializer.Deserialize<List<GeneratedName>>(items, options)!.OrderByDescending(x => x.CreatedOn)];
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return lstGeneratedNames;
        }

        /// <summary>
        /// Logs the generated name.
        /// </summary>
        /// <param name="lstGeneratedName">The generated name to log.</param>
        public static async void LogGeneratedName(GeneratedName lstGeneratedName)
        {
            try
            {
                // Log the created name
                var lstGeneratedNames = new List<GeneratedName>();
                lstGeneratedNames = await GetGeneratedNames();

                if (lstGeneratedNames.Count > 0)
                {
                    lstGeneratedName.Id = lstGeneratedNames.Max(x => x.Id) + 1;
                }
                else
                {
                    lstGeneratedName.Id = 1;
                }

                lstGeneratedNames.Add(lstGeneratedName);
                var jsonGeneratedNames = JsonSerializer.Serialize(lstGeneratedNames);
                await FileSystemHelper.WriteFile("generatednames.json", jsonGeneratedNames);
                CacheHelper.InvalidateCacheObject("GeneratedName");
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }

        /// <summary>
        /// Purges the generated names.
        /// </summary>
        public static async Task PurgeGeneratedNames()
        {
            try
            {
                await FileSystemHelper.WriteFile("generatednames.json", "[]");
                CacheHelper.InvalidateCacheObject("GeneratedName");
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the list of admin log messages.
        /// </summary>
        /// <returns>List of AdminLogMessage objects.</returns>
        public static async Task<List<AdminLogMessage>> GetAdminLog()
        {
            List<AdminLogMessage> lstAdminLogMessages = [];
            try
            {
                string data = await FileSystemHelper.ReadFile("adminlogmessages.json");
                if (GeneralHelper.IsNotNull(data))
                {
                    var items = new List<AdminLogMessage>();
                    lstAdminLogMessages = [.. JsonSerializer.Deserialize<List<AdminLogMessage>>(data, options)!.OrderByDescending(x => x.CreatedOn)];
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
            return lstAdminLogMessages;
        }

        /// <summary>
        /// Logs an admin message.
        /// </summary>
        /// <param name="title">The title of the admin message.</param>
        /// <param name="message">The content of the admin message.</param>
        public static async void LogAdminMessage(string title, string message)
        {
            try
            {
                AdminLogMessage adminmessage = new()
                {
                    Id = 1,
                    Title = title,
                    Message = message
                };

                // Log the created name
                var lstAdminLogMessages = new List<AdminLogMessage>();
                lstAdminLogMessages = await GetAdminLog();

                if (lstAdminLogMessages.Count > 0)
                {
                    adminmessage.Id = lstAdminLogMessages.Max(x => x.Id) + 1;
                }

                lstAdminLogMessages.Add(adminmessage);
                var jsonAdminLogMessages = JsonSerializer.Serialize(lstAdminLogMessages);
                await FileSystemHelper.WriteFile("adminlogmessages.json", jsonAdminLogMessages);
                CacheHelper.InvalidateCacheObject("AdminLogMessage");
            }
            catch (Exception)
            {
                // No exception is logged due to this function being the function that would complete the action.
            }
        }

        /// <summary>
        /// Purges the admin log messages.
        /// </summary>
        public static async Task PurgeAdminLog()
        {
            try
            {
                await FileSystemHelper.WriteFile("adminlogmessages.json", "[]");
                CacheHelper.InvalidateCacheObject("AdminLogMessage");
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
            }
        }
    }
}
