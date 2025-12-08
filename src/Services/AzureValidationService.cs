using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Secrets;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services.Interfaces;
using System.Text.Json;
using System.Net.Http.Headers;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for validating resource names against Azure tenant using Azure Resource Graph
    /// and CheckNameAvailability API for globally unique resources
    /// </summary>
    public class AzureValidationService : IAzureValidationService
    {
        private readonly ILogger<AzureValidationService> _logger;
        private readonly IConfiguration _config;
        private readonly IResourceTypeService _resourceTypeService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationRepository<AzureValidationSettings> _settingsRepository;
        private ArmClient? _armClient;
        private TokenCredential? _credential;
        private const string SETTINGS_FILE = "azurevalidationsettings.json"; // Legacy file for migration
        private const string CACHE_KEY_PREFIX = "azure-validation:";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureValidationService"/> class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="config">Configuration instance</param>
        /// <param name="resourceTypeService">Resource type service for accessing resource metadata</param>
        /// <param name="httpClientFactory">HTTP client factory for API calls</param>
        /// <param name="settingsRepository">Repository for Azure validation settings</param>
        public AzureValidationService(
            ILogger<AzureValidationService> logger, 
            IConfiguration config,
            IResourceTypeService resourceTypeService,
            IHttpClientFactory httpClientFactory,
            IConfigurationRepository<AzureValidationSettings> settingsRepository)
        {
            _logger = logger;
            _config = config;
            _resourceTypeService = resourceTypeService;
            _httpClientFactory = httpClientFactory;
            _settingsRepository = settingsRepository;
        }

        /// <summary>
        /// Sanitizes user input for safe logging by removing newlines and control characters.
        /// </summary>
        private static string SanitizeForLog(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Remove newlines and carriage returns
            return input.Replace("\r", "").Replace("\n", "");
        }

        /// <summary>
        /// Validates a single resource name against Azure tenant
        /// </summary>
        public async Task<AzureValidationMetadata> ValidateNameAsync(string resourceName, string resourceType)
        {
            var metadata = new AzureValidationMetadata
            {
                ValidationPerformed = false,
                ExistsInAzure = false,
                ValidationTimestamp = DateTime.UtcNow
            };

            try
            {
                // Check if validation is enabled
                if (!await IsValidationEnabledAsync())
                {
                    return metadata;
                }

                var settings = await GetSettingsAsync();

                // Check cache first if enabled
                if (settings.Cache.Enabled)
                {
                    var cacheKey = $"{CACHE_KEY_PREFIX}{resourceType}:{resourceName}";
                    var cachedResult = CacheHelper.GetCacheObject(cacheKey);
                    if (cachedResult != null && cachedResult is AzureValidationMetadata cachedMetadata)
                    {
                        _logger.LogInformation("Azure validation cache hit for {ResourceName}", resourceName);
                        return cachedMetadata;
                    }
                }

                // Ensure authenticated
                await EnsureAuthenticatedAsync(settings);

                // Get resource type metadata to determine validation approach
                var resourceTypeInfo = await GetResourceTypeInfoAsync(resourceType);
                bool isGlobalScope = resourceTypeInfo?.Scope?.Equals("global", StringComparison.OrdinalIgnoreCase) ?? false;

                (bool exists, List<string> resourceIds) validationResult;

                // Use appropriate validation method based on scope
                if (isGlobalScope)
                {
                    _logger.LogInformation("Using CheckNameAvailability API for globally unique resource: {ResourceType}", SanitizeForLog(resourceType));
                    validationResult = await CheckNameAvailabilityAsync(resourceName, resourceType, settings);
                }
                else
                {
                    _logger.LogInformation("Using Resource Graph query for scoped resource: {ResourceType}", SanitizeForLog(resourceType));
                    validationResult = await CheckResourceExistsAsync(resourceName, resourceType, settings);
                }

                metadata.ValidationPerformed = true;
                metadata.ExistsInAzure = validationResult.exists;
                metadata.ConflictingResources = validationResult.resourceIds;

                // Cache the result
                if (settings.Cache.Enabled)
                {
                    var cacheKey = $"{CACHE_KEY_PREFIX}{resourceType}:{resourceName}";
                    CacheHelper.SetCacheObject(cacheKey, metadata);
                }

                _logger.LogInformation("Azure validation completed for {ResourceName}: exists={Exists}, scope={Scope}", 
                    resourceName, validationResult.exists, isGlobalScope ? "global" : "scoped");

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating resource name {ResourceName} against Azure", resourceName);
                metadata.ValidationWarning = $"Validation error: {ex.Message}";
                return metadata;
            }
        }

        /// <summary>
        /// Validates multiple resource names in a batch
        /// </summary>
        public async Task<Dictionary<string, AzureValidationMetadata>> ValidateBatchAsync(
            List<(string resourceName, string resourceType)> validationRequests)
        {
            var results = new Dictionary<string, AzureValidationMetadata>();

            if (!await IsValidationEnabledAsync())
            {
                foreach (var request in validationRequests)
                {
                    results[request.resourceName] = new AzureValidationMetadata
                    {
                        ValidationPerformed = false,
                        ValidationTimestamp = DateTime.UtcNow
                    };
                }
                return results;
            }

            var settings = await GetSettingsAsync();

            // Check cache for all requests
            var uncachedRequests = new List<(string resourceName, string resourceType)>();
            
            foreach (var request in validationRequests)
            {
                if (settings.Cache.Enabled)
                {
                    var cacheKey = $"{CACHE_KEY_PREFIX}{request.resourceType}:{request.resourceName}";
                    var cachedResult = CacheHelper.GetCacheObject(cacheKey);
                    if (cachedResult != null && cachedResult is AzureValidationMetadata cachedMetadata)
                    {
                        results[request.resourceName] = cachedMetadata;
                        continue;
                    }
                }
                uncachedRequests.Add(request);
            }

            // Query uncached items in batch
            if (uncachedRequests.Any())
            {
                try
                {
                    await EnsureAuthenticatedAsync(settings);

                    foreach (var request in uncachedRequests)
                    {
                        var metadata = await ValidateNameAsync(request.resourceName, request.resourceType);
                        results[request.resourceName] = metadata;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in batch validation");
                    foreach (var request in uncachedRequests)
                    {
                        if (!results.ContainsKey(request.resourceName))
                        {
                            results[request.resourceName] = new AzureValidationMetadata
                            {
                                ValidationPerformed = false,
                                ValidationWarning = $"Batch validation error: {ex.Message}",
                                ValidationTimestamp = DateTime.UtcNow
                            };
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Tests the Azure connection
        /// </summary>
        public async Task<AzureConnectionTestResult> TestConnectionAsync()
        {
            var result = new AzureConnectionTestResult();

            try
            {
                // Check global toggle first
                var globalEnabled = Convert.ToBoolean(ConfigurationHelper.GetAppSetting("AzureTenantNameValidationEnabled"));
                if (!globalEnabled)
                {
                    result.ErrorMessage = "Azure tenant name validation is not enabled in Site Settings";
                    return result;
                }

                var settings = await GetSettingsAsync();

                // For test connection, we don't require settings.Enabled to be true
                // The global site setting being enabled is sufficient for testing
                // This allows users to test before saving settings

                result.AuthenticationMode = settings.AuthMode.ToString();
                result.TenantId = settings.TenantId;

                // Test authentication
                await EnsureAuthenticatedAsync(settings);

                if (_armClient == null)
                {
                    result.ErrorMessage = "Failed to create ARM client";
                    return result;
                }

                result.Authenticated = true;

                // Test subscription access
                await foreach (var subscription in _armClient.GetSubscriptions().GetAllAsync())
                {
                    var subAccess = new SubscriptionAccess
                    {
                        SubscriptionId = subscription.Data.SubscriptionId ?? string.Empty,
                        DisplayName = subscription.Data.DisplayName ?? string.Empty,
                        State = subscription.Data.State?.ToString(),
                        HasReadAccess = true
                    };
                    result.AccessibleSubscriptions.Add(subAccess);

                    // Only check configured subscriptions if specified
                    if (settings.SubscriptionIds.Any() && 
                        settings.SubscriptionIds.Contains(subscription.Data.SubscriptionId ?? string.Empty))
                    {
                        // Found a configured subscription
                    }
                }

                // Test Resource Graph query
                try
                {
                    var testQuery = "Resources | where type =~ 'microsoft.resources/subscriptions' | limit 1";
                    var queryResult = await ExecuteResourceGraphQueryAsync(testQuery, settings);
                    result.ResourceGraphAccess = true;
                    result.TestQuerySucceeded = queryResult != null;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Resource Graph test query failed");
                    result.ResourceGraphAccess = false;
                    result.ErrorMessage = $"Resource Graph query failed: {ex.Message}";
                }

                result.Message = result.TestQuerySucceeded 
                    ? "Successfully connected to Azure" 
                    : "Authenticated but Resource Graph query failed";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Azure connection");
                result.ErrorMessage = ex.Message;
                result.Message = "Connection test failed";
                return result;
            }
        }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public async Task<AzureValidationSettings> GetSettingsAsync()
        {
            try
            {
                // Try to get settings from repository (SQLite or JSON depending on storage provider)
                var settings = await _settingsRepository.GetByIdAsync(1);
                
                if (settings != null)
                {
                    return settings;
                }
            }
            catch (Exception ex)
            {
                // Table might not exist yet or other database error
                _logger.LogWarning(ex, "Could not load Azure validation settings from repository, trying legacy migration");
            }

            try
            {
                // If no settings exist, try to migrate from legacy JSON file
                var migrated = await MigrateLegacySettingsAsync();
                if (migrated != null)
                {
                    return migrated;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not migrate legacy Azure validation settings");
            }

            // Return default settings
            return new AzureValidationSettings { Id = 1 };
        }

        /// <summary>
        /// Migrates legacy JSON file settings to repository if they exist
        /// </summary>
        private async Task<AzureValidationSettings?> MigrateLegacySettingsAsync()
        {
            try
            {
                var settingsPath = Path.Combine("settings", SETTINGS_FILE);
                if (File.Exists(settingsPath))
                {
                    _logger.LogInformation("Migrating legacy Azure validation settings from JSON file");
                    
                    var settingsContent = await FileSystemHelper.ReadFile(SETTINGS_FILE, "settings/");
                    
                    if (!string.IsNullOrEmpty(settingsContent))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        
                        AzureValidationSettings? settings = null;
                        
                        // Try array format first (legacy format: [{...}])
                        try
                        {
                            var settingsArray = JsonSerializer.Deserialize<List<AzureValidationSettings>>(settingsContent, options);
                            if (settingsArray != null && settingsArray.Count > 0)
                            {
                                settings = settingsArray[0];
                            }
                        }
                        catch
                        {
                            // If array deserialization fails, try single object format
                            settings = JsonSerializer.Deserialize<AzureValidationSettings>(settingsContent, options);
                        }
                        
                        if (settings != null)
                        {
                            settings.Id = 1; // Ensure ID is set
                            await _settingsRepository.SaveAsync(settings);
                            
                            _logger.LogInformation("Legacy Azure validation settings migrated successfully. Strategy: {Strategy}, SubscriptionIds: {Count}", 
                                settings.ConflictResolution?.Strategy, settings.SubscriptionIds?.Count ?? 0);
                            
                            // Optionally rename the old file to indicate migration
                            try
                            {
                                File.Move(settingsPath, settingsPath + ".migrated");
                            }
                            catch { /* Ignore errors renaming file */ }
                            
                            return settings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error migrating legacy Azure validation settings, will use defaults");
            }

            return null;
        }

        /// <summary>
        /// Updates the settings
        /// </summary>
        public async Task<ServiceResponse> UpdateSettingsAsync(AzureValidationSettings settings)
        {
            var response = new ServiceResponse();

            try
            {
                // Ensure ID is always 1 (singleton pattern)
                settings.Id = 1;
                
                await _settingsRepository.SaveAsync(settings);

                // Clear ARM client to force re-authentication with new settings
                _armClient = null;
                _credential = null;

                // Clear validation cache
                CacheHelper.InvalidateCacheObject($"{CACHE_KEY_PREFIX}*");

                response.Success = true;
                response.ResponseMessage = "Azure validation settings updated successfully";
                
                _logger.LogInformation("Azure validation settings updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Azure validation settings");
                response.Success = false;
                response.ResponseMessage = $"Error saving settings: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Checks if validation is enabled
        /// </summary>
        public async Task<bool> IsValidationEnabledAsync()
        {
            // Check global toggle first
            var globalEnabled = Convert.ToBoolean(ConfigurationHelper.GetAppSetting("AzureTenantNameValidationEnabled"));
            if (!globalEnabled)
            {
                return false;
            }

            // Check settings
            var settings = await GetSettingsAsync();
            return settings.Enabled;
        }

        #region Private Helper Methods

        /// <summary>
        /// Ensures ARM client is authenticated
        /// </summary>
        private async Task EnsureAuthenticatedAsync(AzureValidationSettings settings)
        {
            if (_armClient != null && _credential != null)
            {
                return; // Already authenticated
            }

            try
            {
                _credential = await GetCredentialAsync(settings);
                _armClient = new ArmClient(_credential);
                
                _logger.LogInformation("Azure ARM client authenticated using {AuthMode}", settings.AuthMode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate to Azure");
                throw new InvalidOperationException("Failed to authenticate to Azure. Check your credentials and permissions.", ex);
            }
        }

        /// <summary>
        /// Gets the appropriate credential based on settings
        /// </summary>
        private async Task<TokenCredential> GetCredentialAsync(AzureValidationSettings settings)
        {
            switch (settings.AuthMode)
            {
                case AuthenticationMode.ManagedIdentity:
                    _logger.LogInformation("Using Managed Identity for Azure authentication");
                    return new DefaultAzureCredential();

                case AuthenticationMode.ServicePrincipal:
                    if (settings.ServicePrincipal == null)
                    {
                        throw new InvalidOperationException("Service Principal settings are required");
                    }

                    var clientSecret = await GetClientSecretAsync(settings);
                    
                    _logger.LogInformation("Using Service Principal for Azure authentication");
                    return new ClientSecretCredential(
                        settings.TenantId,
                        settings.ServicePrincipal.ClientId,
                        clientSecret);

                default:
                    throw new InvalidOperationException($"Unsupported authentication mode: {settings.AuthMode}");
            }
        }

        /// <summary>
        /// Gets the client secret from Key Vault or configuration
        /// </summary>
        private async Task<string> GetClientSecretAsync(AzureValidationSettings settings)
        {
            if (settings.ServicePrincipal == null)
            {
                throw new InvalidOperationException("Service Principal settings are required");
            }

            // Try Key Vault first if configured
            if (settings.KeyVault != null && !string.IsNullOrEmpty(settings.KeyVault.KeyVaultUri))
            {
                try
                {
                    _logger.LogInformation("Retrieving client secret from Key Vault");
                    
                    var kvCredential = new DefaultAzureCredential();
                    var kvClient = new SecretClient(new Uri(settings.KeyVault.KeyVaultUri), kvCredential);
                    
                    var secretName = settings.ServicePrincipal.ClientSecretKeyVaultName 
                        ?? settings.KeyVault.ClientSecretName;
                    
                    var secret = await kvClient.GetSecretAsync(secretName);
                    return secret.Value.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve secret from Key Vault");
                    throw new InvalidOperationException("Failed to retrieve client secret from Key Vault", ex);
                }
            }

            // Fallback to configuration (encrypted or plain)
            if (!string.IsNullOrEmpty(settings.ServicePrincipal.ClientSecret))
            {
                // Check if it's encrypted
                if (settings.ServicePrincipal.ClientSecret.StartsWith("encrypted:"))
                {
                    try
                    {
                        var encryptedValue = settings.ServicePrincipal.ClientSecret["encrypted:".Length..];
                        var config = ConfigurationHelper.GetConfigurationData();
                        return GeneralHelper.DecryptString(encryptedValue, config.SALTKey!);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to decrypt client secret");
                        throw new InvalidOperationException("Failed to decrypt client secret", ex);
                    }
                }
                
                return settings.ServicePrincipal.ClientSecret;
            }

            throw new InvalidOperationException("Client secret not found in Key Vault or configuration");
        }

        /// <summary>
        /// Checks if a resource exists using Azure Resource Graph
        /// </summary>
        private async Task<(bool Exists, List<string> ResourceIds)> CheckResourceExistsAsync(
            string resourceName, 
            string resourceType, 
            AzureValidationSettings settings)
        {
            try
            {
                // Get the full Azure resource type from shortname
                var resourceTypeInfo = await GetResourceTypeInfoAsync(resourceType);
                if (resourceTypeInfo == null)
                {
                    _logger.LogWarning("Could not find resource type info for {ResourceType}", SanitizeForLog(resourceType));
                    return (false, new List<string>());
                }

                // Convert to Azure Resource Manager format (e.g., "Microsoft.Web/serverfarms")
                var azureResourceType = ConvertToAzureResourceType(resourceTypeInfo.Resource);

                // Build Resource Graph query using the full Azure resource type
                var query = $"Resources | where name =~ '{resourceName.Replace("'", "\\'")}' | where type =~ '{azureResourceType.Replace("'", "\\'")}' | project id, name, type, resourceGroup";

                var result = await ExecuteResourceGraphQueryAsync(query, settings);

                if (result != null && result.Data != null)
                {
                    var dataJson = result.Data.ToString();
                    if (!string.IsNullOrEmpty(dataJson))
                    {
                        var dataElement = JsonSerializer.Deserialize<JsonElement>(dataJson);
                        if (dataElement.ValueKind == JsonValueKind.Array)
                        {
                            var resourceIds = new List<string>();
                            
                            foreach (var item in dataElement.EnumerateArray())
                            {
                                if (item.TryGetProperty("id", out var idProp))
                                {
                                    var id = idProp.GetString();
                                    if (!string.IsNullOrEmpty(id))
                                    {
                                        resourceIds.Add(id);
                                    }
                                }
                            }

                            return (resourceIds.Any(), resourceIds);
                        }
                    }
                }

                return (false, new List<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Azure Resource Graph for {ResourceName}", resourceName);
                throw;
            }
        }

        /// <summary>
        /// Gets resource type information from the repository
        /// </summary>
        private async Task<Models.ResourceType?> GetResourceTypeInfoAsync(string resourceTypeShortName)
        {
            try
            {
                var response = await _resourceTypeService.GetItemsAsync();
                if (!response.Success || response.ResponseObject == null)
                {
                    return null;
                }

                if (response.ResponseObject is not List<Models.ResourceType> resourceTypes)
                {
                    return null;
                }
                
                // Try to match by ShortName first (e.g., "st" for storage)
                var match = resourceTypes.FirstOrDefault(rt => 
                    rt.ShortName.Equals(resourceTypeShortName, StringComparison.OrdinalIgnoreCase));

                // If not found, try matching by Resource field (e.g., "Storage/storageAccounts")
                if (match == null)
                {
                    match = resourceTypes.FirstOrDefault(rt => 
                        rt.Resource.Equals(resourceTypeShortName, StringComparison.OrdinalIgnoreCase));
                }

                // If still not found, try partial match on Resource field
                if (match == null)
                {
                    match = resourceTypes.FirstOrDefault(rt => 
                        rt.Resource.Contains(resourceTypeShortName, StringComparison.OrdinalIgnoreCase));
                }

                return match;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting resource type info for {ResourceType}", SanitizeForLog(resourceTypeShortName));
                return null;
            }
        }

        /// <summary>
        /// Checks name availability using Azure Resource Manager CheckNameAvailability API
        /// </summary>
        private async Task<(bool Exists, List<string> ResourceIds)> CheckNameAvailabilityAsync(
            string resourceName,
            string resourceType,
            AzureValidationSettings settings)
        {
            try
            {
                // Get resource type info to build proper API request
                var resourceTypeInfo = await GetResourceTypeInfoAsync(resourceType);
                if (resourceTypeInfo == null)
                {
                    _logger.LogWarning("Could not find resource type info for {ResourceType}, falling back to Resource Graph", SanitizeForLog(resourceType));
                    return await CheckResourceExistsAsync(resourceName, resourceType, settings);
                }

                // Map the resource type to provider namespace and type
                var (providerNamespace, resourceTypeName) = ParseResourceType(resourceTypeInfo.Resource);
                
                // Get subscription ID (required for CheckNameAvailability API)
                var subscriptionId = settings.SubscriptionIds.FirstOrDefault();
                if (string.IsNullOrEmpty(subscriptionId))
                {
                    // Try to get default subscription from ARM client
                    await foreach (var sub in _armClient!.GetSubscriptions().GetAllAsync())
                    {
                        subscriptionId = sub.Data.SubscriptionId;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(subscriptionId))
                {
                    _logger.LogWarning("No subscription ID available for CheckNameAvailability API, falling back to Resource Graph");
                    return await CheckResourceExistsAsync(resourceName, resourceType, settings);
                }

                // Build CheckNameAvailability API endpoint
                var apiVersion = GetCheckNameAvailabilityApiVersion(providerNamespace);
                var endpoint = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/{providerNamespace}/checkNameAvailability?api-version={apiVersion}";

                // Build request body
                var requestBody = new
                {
                    name = resourceName,
                    type = $"{providerNamespace}/{resourceTypeName}"
                };

                // Get access token
                var token = await GetAccessTokenAsync();

                // Create HTTP client
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Send POST request
                var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);
                response.EnsureSuccessStatusCode();

                // Parse response
                var result = await response.Content.ReadFromJsonAsync<CheckNameAvailabilityResponse>();

                if (result == null)
                {
                    _logger.LogWarning("CheckNameAvailability API returned null response");
                    return (false, new List<string>());
                }

                // nameAvailable = true means name is NOT taken (available for use)
                // nameAvailable = false means name IS taken (already exists)
                bool exists = !result.NameAvailable;

                _logger.LogInformation(
                    "CheckNameAvailability result for {ResourceName}: available={Available}, reason={Reason}, message={Message}",
                    resourceName, result.NameAvailable, result.Reason, result.Message);

                return (exists, exists ? new List<string> { result.Message ?? "Name already exists globally" } : new List<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CheckNameAvailability API for {ResourceName}, falling back to Resource Graph", resourceName);
                // Fall back to Resource Graph if CheckNameAvailability fails
                return await CheckResourceExistsAsync(resourceName, resourceType, settings);
            }
        }

        /// <summary>
        /// Parses a resource type string into provider namespace and resource type name
        /// </summary>
        private (string providerNamespace, string resourceTypeName) ParseResourceType(string resourceType)
        {
            // Handle formats like "Storage/storageAccounts" or "Microsoft.Storage/storageAccounts"
            var parts = resourceType.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length >= 2)
            {
                // If first part doesn't start with "Microsoft.", add it
                var providerNamespace = parts[0].StartsWith("Microsoft.") 
                    ? parts[0] 
                    : $"Microsoft.{parts[0]}";
                
                var resourceTypeName = parts[1];
                
                return (providerNamespace, resourceTypeName);
            }

            // Default fallback
            return ("Microsoft.Resources", resourceType);
        }

        /// <summary>
        /// Converts a resource type from shorthand format to Azure Resource Manager format
        /// Examples: "Storage/storageAccounts" -> "microsoft.storage/storageaccounts"
        ///           "Web/serverfarms" -> "microsoft.web/serverfarms"
        /// </summary>
        private string ConvertToAzureResourceType(string resourceType)
        {
            var (providerNamespace, resourceTypeName) = ParseResourceType(resourceType);
            // Azure Resource Graph uses lowercase for resource types
            return $"{providerNamespace.ToLowerInvariant()}/{resourceTypeName.ToLowerInvariant()}";
        }

        /// <summary>
        /// Gets the appropriate API version for CheckNameAvailability based on provider namespace
        /// </summary>
        private string GetCheckNameAvailabilityApiVersion(string providerNamespace)
        {
            // Map of provider namespaces to their CheckNameAvailability API versions
            return providerNamespace switch
            {
                "Microsoft.Storage" => "2023-01-01",
                "Microsoft.Web" => "2023-01-01",
                "Microsoft.KeyVault" => "2023-07-01",
                "Microsoft.ContainerRegistry" => "2023-07-01",
                "Microsoft.CognitiveServices" => "2023-05-01",
                "Microsoft.Cache" => "2023-08-01",
                "Microsoft.DocumentDB" => "2023-11-15",
                "Microsoft.ServiceBus" => "2022-10-01-preview",
                "Microsoft.EventHub" => "2022-10-01-preview",
                "Microsoft.Devices" => "2023-06-30",
                "Microsoft.ApiManagement" => "2023-05-01-preview",
                "Microsoft.DataFactory" => "2018-06-01",
                "Microsoft.Search" => "2023-11-01",
                "Microsoft.Communication" => "2023-04-01",
                "Microsoft.SignalRService" => "2023-02-01",
                "Microsoft.Sql" => "2023-08-01-preview",
                "Microsoft.DBforMySQL" => "2023-12-30",
                "Microsoft.DBforPostgreSQL" => "2023-12-01-preview",
                "Microsoft.DBforMariaDB" => "2020-01-01",
                _ => "2021-04-01" // Default fallback version
            };
        }

        /// <summary>
        /// Gets an access token for Azure Resource Manager API calls
        /// </summary>
        private async Task<string> GetAccessTokenAsync()
        {
            if (_credential == null)
            {
                throw new InvalidOperationException("Azure credential is not initialized");
            }

            var tokenContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var token = await _credential.GetTokenAsync(tokenContext, CancellationToken.None);
            return token.Token;
        }

        /// <summary>
        /// Response model for CheckNameAvailability API
        /// </summary>
        private class CheckNameAvailabilityResponse
        {
            public bool NameAvailable { get; set; }
            public string? Reason { get; set; }
            public string? Message { get; set; }
        }

        /// <summary>
        /// Executes a Resource Graph query
        /// </summary>
        private async Task<ResourceQueryResult?> ExecuteResourceGraphQueryAsync(
            string query, 
            AzureValidationSettings settings)
        {
            if (_armClient == null)
            {
                throw new InvalidOperationException("ARM client is not authenticated");
            }

            try
            {
                // Get tenant resource
                string? tenantId = settings.TenantId;
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    // Get first available tenant
                    await foreach (var t in _armClient.GetTenants().GetAllAsync())
                    {
                        tenantId = t.Data.TenantId?.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new InvalidOperationException("Could not determine tenant ID");
                }

                TenantResource? tenant = null;
                await foreach (var t in _armClient.GetTenants().GetAllAsync())
                {
                    if (t.Data.TenantId?.ToString() == tenantId)
                    {
                        tenant = t;
                        break;
                    }
                }

                if (tenant == null)
                {
                    throw new InvalidOperationException($"Could not access tenant {tenantId}");
                }

                // Build query request
                var queryRequest = new ResourceQueryContent(query);

                // Add subscription scope if configured
                if (settings.SubscriptionIds.Any())
                {
                    foreach (var subId in settings.SubscriptionIds)
                    {
                        queryRequest.Subscriptions.Add(subId);
                    }
                }

                // Set query options
                queryRequest.Options = new ResourceQueryRequestOptions
                {
                    ResultFormat = ResultFormat.ObjectArray
                };

                // Execute query with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await tenant.GetResourcesAsync(queryRequest, cts.Token);

                return response.Value;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Resource Graph query timed out after 5 seconds");
                throw new TimeoutException("Azure Resource Graph query timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Resource Graph query");
                throw;
            }
        }

        #endregion
    }
}
