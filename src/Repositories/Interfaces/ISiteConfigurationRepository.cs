using AzureNamingTool.Models;

namespace AzureNamingTool.Repositories.Interfaces
{
    /// <summary>
    /// Repository for site configuration settings
    /// </summary>
    public interface ISiteConfigurationRepository
    {
        /// <summary>
        /// Get the complete configuration data
        /// </summary>
        /// <returns>Configuration data</returns>
        Task<ConfigurationData> GetConfigurationAsync();

        /// <summary>
        /// Update the complete configuration data
        /// </summary>
        /// <param name="config">Configuration data to save</param>
        Task UpdateConfigurationAsync(ConfigurationData config);

        /// <summary>
        /// Get a specific setting value
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <returns>Setting value</returns>
        Task<string> GetSettingAsync(string key);

        /// <summary>
        /// Set a specific setting value
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="value">Setting value</param>
        Task SetSettingAsync(string key, string value);

        /// <summary>
        /// Get all settings as key-value pairs
        /// </summary>
        /// <returns>Dictionary of all settings</returns>
        Task<Dictionary<string, string>> GetAllSettingsAsync();
    }
}
