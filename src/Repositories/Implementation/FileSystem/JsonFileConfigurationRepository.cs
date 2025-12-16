#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using System.Reflection;
using System.Text.Json;

namespace AzureNamingTool.Repositories.Implementation.FileSystem
{
    /// <summary>
    /// JSON file-based repository implementation (DEFAULT)
    /// Maintains existing file storage behavior while providing abstraction
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class JsonFileConfigurationRepository<T> : IConfigurationRepository<T> where T : class
    {
        private readonly ILogger<JsonFileConfigurationRepository<T>> _logger;
        private readonly string _fileName;
        private readonly string _settingsPath;

        public JsonFileConfigurationRepository(ILogger<JsonFileConfigurationRepository<T>> logger)
        {
            _logger = logger;
            _settingsPath = "settings/";
            _fileName = GetFileNameForType();
            
            _logger.LogDebug("Repository initialized for {Type}, File: {FileName}", 
                typeof(T).Name, _fileName);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Loading all items from {FileName}", _fileName);
                
                // Use existing FileSystemHelper to maintain compatibility
                var json = await FileSystemHelper.ReadFile(_fileName, _settingsPath);
                
                if (string.IsNullOrWhiteSpace(json) || json == "[]")
                {
                    _logger.LogDebug("File {FileName} is empty or contains empty array", _fileName);
                    return Enumerable.Empty<T>();
                }

                // Use case-insensitive deserialization with camelCase policy to support existing user JSON files
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                // Convert enums to their string names for human-readable configuration files
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                
                var items = JsonSerializer.Deserialize<List<T>>(json, options);
                
                _logger.LogDebug("Loaded {Count} items from {FileName}", items?.Count ?? 0, _fileName);
                return items ?? Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from {FileName}", _fileName);
                throw new InvalidOperationException($"Failed to read from {_fileName}", ex);
            }
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                var items = await GetAllAsync();
                var idProperty = typeof(T).GetProperty("Id");
                
                if (idProperty == null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");
                }

                return items.FirstOrDefault(item =>
                {
                    var itemId = idProperty.GetValue(item);
                    return itemId != null && Convert.ToInt64(itemId) == id;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item by ID {Id} from {FileName}", id, _fileName);
                throw;
            }
        }

        public async Task SaveAsync(T item)
        {
            try
            {
                var items = (await GetAllAsync()).ToList();
                var idProperty = typeof(T).GetProperty("Id");
                
                if (idProperty == null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");
                }

                var itemId = Convert.ToInt64(idProperty.GetValue(item));
                
                // Remove existing item with same ID if it exists
                var existingIndex = items.FindIndex(i => 
                    Convert.ToInt64(idProperty.GetValue(i)) == itemId);
                
                if (existingIndex >= 0)
                {
                    items[existingIndex] = item;
                    _logger.LogDebug("Updated existing item with ID {Id} in {FileName}", itemId, _fileName);
                }
                else
                {
                    items.Add(item);
                    _logger.LogDebug("Added new item with ID {Id} to {FileName}", itemId, _fileName);
                }

                await SaveAllAsync(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item to {FileName}", _fileName);
                throw;
            }
        }

        public async Task SaveAllAsync(IEnumerable<T> items)
        {
            try
            {
                // Use case-insensitive options to match GetAllAsync behavior
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                // Convert enums to their string names for human-readable configuration files
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

                var json = JsonSerializer.Serialize(items, options);
                
                // Use existing FileSystemHelper to maintain compatibility
                await FileSystemHelper.WriteFile(_fileName, json, _settingsPath);
                
                _logger.LogInformation("Saved {Count} items to {FileName}", items.Count(), _fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to {FileName}", _fileName);
                throw new InvalidOperationException($"Failed to write to {_fileName}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var items = (await GetAllAsync()).ToList();
                var idProperty = typeof(T).GetProperty("Id");
                
                if (idProperty == null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");
                }

                var itemToRemove = items.FirstOrDefault(item =>
                    Convert.ToInt64(idProperty.GetValue(item)) == id);

                if (itemToRemove != null)
                {
                    items.Remove(itemToRemove);
                    await SaveAllAsync(items);
                    _logger.LogInformation("Deleted item with ID {Id} from {FileName}", id, _fileName);
                }
                else
                {
                    _logger.LogWarning("Item with ID {Id} not found in {FileName}", id, _fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item with ID {Id} from {FileName}", id, _fileName);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var item = await GetByIdAsync(id);
                return item != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if item exists with ID {Id} in {FileName}", id, _fileName);
                return false;
            }
        }

        /// <summary>
        /// Maps entity type to corresponding JSON file name
        /// Maintains exact file naming convention from existing implementation
        /// </summary>
        private string GetFileNameForType()
        {
            var typeName = typeof(T).Name;
            return typeName switch
            {
                nameof(Models.ResourceType) => "resourcetypes.json",
                nameof(Models.ResourceLocation) => "resourcelocations.json",
                nameof(Models.ResourceEnvironment) => "resourceenvironments.json",
                nameof(Models.ResourceOrg) => "resourceorgs.json",
                nameof(Models.ResourceProjAppSvc) => "resourceprojappsvcs.json",
                nameof(Models.ResourceUnitDept) => "resourceunitdepts.json",
                nameof(Models.ResourceFunction) => "resourcefunctions.json",
                nameof(Models.ResourceDelimiter) => "resourcedelimiters.json",
                nameof(Models.ResourceComponent) => "resourcecomponents.json",
                nameof(Models.CustomComponent) => "customcomponents.json",
                nameof(Models.GeneratedName) => "generatednames.json",
                nameof(Models.AdminLogMessage) => "adminlogmessages.json",
                nameof(Models.AdminUser) => "adminusers.json",
                nameof(Models.AzureValidationSettings) => "azurevalidationsettings.json",
                _ => $"{typeName.ToLowerInvariant()}s.json"
            };
        }
    }
}

#pragma warning restore CS1591