using AzureNamingTool.Models;

namespace AzureNamingTool.Services.Interfaces;

/// <summary>
/// Interface for import/export configuration operations.
/// </summary>
public interface IImportExportService
{
    /// <summary>
    /// Exports the current configuration.
    /// </summary>
    /// <param name="includeadmin">Flag indicating whether to include admin data in the export.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> ExportConfigAsync(bool includeadmin = false);

    /// <summary>
    /// Posts a configuration for import.
    /// </summary>
    /// <param name="configdata">The configuration data to import.</param>
    /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
    Task<ServiceResponse> PostConfigAsync(ConfigurationData configdata);
}
