using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using System.Text.RegularExpressions;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for resolving resource name conflicts when a name already exists in Azure
    /// </summary>
    public class ConflictResolutionService
    {
        private readonly ILogger<ConflictResolutionService> _logger;
        private readonly IAzureValidationService _validationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictResolutionService"/> class
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="validationService">Azure validation service</param>
        public ConflictResolutionService(
            ILogger<ConflictResolutionService> logger,
            IAzureValidationService validationService)
        {
            _logger = logger;
            _validationService = validationService;
        }

        /// <summary>
        /// Resolves a name conflict based on the configured strategy
        /// </summary>
        /// <param name="originalName">The original generated name</param>
        /// <param name="resourceType">The Azure resource type</param>
        /// <param name="settings">Azure validation settings with conflict resolution strategy</param>
        /// <returns>Resolution result with final name and metadata</returns>
        public async Task<ConflictResolutionResult> ResolveConflictAsync(
            string originalName,
            string resourceType,
            AzureValidationSettings settings)
        {
            var result = new ConflictResolutionResult
            {
                OriginalName = originalName,
                FinalName = originalName,
                Strategy = settings.ConflictResolution.Strategy
            };

            try
            {
                switch (settings.ConflictResolution.Strategy)
                {
                    case ConflictStrategy.AutoIncrement:
                        result = await ResolveWithAutoIncrementAsync(originalName, resourceType, settings);
                        break;

                    case ConflictStrategy.NotifyOnly:
                        result = await ResolveWithNotifyOnlyAsync(originalName, resourceType);
                        break;

                    case ConflictStrategy.Fail:
                        result = ResolveWithFail(originalName);
                        break;

                    case ConflictStrategy.SuffixRandom:
                        result = await ResolveWithRandomSuffixAsync(originalName, resourceType, settings);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported conflict strategy: {settings.ConflictResolution.Strategy}");
                }

                _logger.LogInformation(
                    "Conflict resolved using {Strategy}: {OriginalName} -> {FinalName} (Attempts: {Attempts})",
                    result.Strategy, result.OriginalName, result.FinalName, result.Attempts);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving conflict for {OriginalName}", originalName);
                result.Success = false;
                result.ErrorMessage = $"Conflict resolution failed: {ex.Message}";
                return result;
            }
        }

        #region Strategy Implementations

        /// <summary>
        /// Auto-increment strategy: Increments the instance number until a unique name is found
        /// </summary>
        private async Task<ConflictResolutionResult> ResolveWithAutoIncrementAsync(
            string originalName,
            string resourceType,
            AzureValidationSettings settings)
        {
            var result = new ConflictResolutionResult
            {
                OriginalName = originalName,
                FinalName = originalName,
                Strategy = ConflictStrategy.AutoIncrement,
                Success = false
            };

            // Extract the instance number pattern from the end of the name
            // Expected patterns: -001, -01, -1, 001, 01, 1, etc.
            // Try with hyphen first, then without
            var instancePatternWithHyphen = @"-(\d+)$";
            var instancePatternNoHyphen = @"(\d+)$";
            var match = Regex.Match(originalName, instancePatternWithHyphen);
            var hasHyphen = true;

            if (!match.Success)
            {
                // Try without hyphen
                match = Regex.Match(originalName, instancePatternNoHyphen);
                hasHyphen = false;

                if (!match.Success)
                {
                    // No instance number found, cannot auto-increment
                    result.ErrorMessage = "Cannot auto-increment: No instance number pattern found in name";
                    result.Warning = "Original name does not contain an instance number (e.g., -001 or 001). Cannot auto-increment.";
                    return result;
                }
            }

            var prefix = originalName[..match.Index]; // Everything before the instance number
            var instanceStr = match.Groups[1].Value;
            var instanceNum = int.Parse(instanceStr);
            var padding = instanceStr.Length; // Preserve original padding (e.g., 001 = 3 digits)
            var delimiter = hasHyphen ? "-" : ""; // Preserve original delimiter

            // Try incrementing until we find a unique name or hit max attempts
            var maxAttempts = settings.ConflictResolution.MaxAttempts;
            var attempts = 0;

            for (int i = instanceNum + 1; attempts < maxAttempts; i++, attempts++)
            {
                var newInstanceStr = i.ToString().PadLeft(padding, '0');
                var candidateName = $"{prefix}{delimiter}{newInstanceStr}";

                // Check if this name exists in Azure
                var validation = await _validationService.ValidateNameAsync(candidateName, resourceType);

                if (!validation.ExistsInAzure)
                {
                    // Found a unique name!
                    result.FinalName = candidateName;
                    result.Success = true;
                    result.Attempts = attempts + 1;
                    result.Warning = settings.ConflictResolution.IncludeWarnings
                        ? $"Original name '{originalName}' exists in Azure. Auto-incremented to '{candidateName}'."
                        : null;
                    return result;
                }
            }

            // Exceeded max attempts
            result.ErrorMessage = $"Could not find unique name after {maxAttempts} attempts";
            result.Attempts = maxAttempts;
            result.Warning = $"Exceeded maximum auto-increment attempts ({maxAttempts}). Last tried: {result.FinalName}";

            return result;
        }

        /// <summary>
        /// Notify-only strategy: Returns the name with a warning but doesn't modify it
        /// </summary>
        private async Task<ConflictResolutionResult> ResolveWithNotifyOnlyAsync(
            string originalName,
            string resourceType)
        {
            var result = new ConflictResolutionResult
            {
                OriginalName = originalName,
                FinalName = originalName,
                Strategy = ConflictStrategy.NotifyOnly,
                Success = true,
                Attempts = 1
            };

            // Validate to get conflict details
            var validation = await _validationService.ValidateNameAsync(originalName, resourceType);

            if (validation.ExistsInAzure)
            {
                var resourceCount = validation.ConflictingResources?.Count ?? 0;
                result.Warning = resourceCount > 0
                    ? $"Warning: Name '{originalName}' already exists in Azure ({resourceCount} conflicting resource(s) found)."
                    : $"Warning: Name '{originalName}' already exists in Azure.";
            }

            return result;
        }

        /// <summary>
        /// Fail strategy: Returns an error and stops processing
        /// </summary>
        private ConflictResolutionResult ResolveWithFail(string originalName)
        {
            return new ConflictResolutionResult
            {
                OriginalName = originalName,
                FinalName = originalName,
                Strategy = ConflictStrategy.Fail,
                Success = false,
                Attempts = 0,
                ErrorMessage = $"Name conflict: '{originalName}' already exists in Azure and conflict strategy is set to Fail.",
                Warning = "Conflict resolution strategy is set to 'Fail'. Resource name must be unique."
            };
        }

        /// <summary>
        /// Random suffix strategy: Adds a random suffix to make the name unique
        /// </summary>
        private async Task<ConflictResolutionResult> ResolveWithRandomSuffixAsync(
            string originalName,
            string resourceType,
            AzureValidationSettings settings)
        {
            var result = new ConflictResolutionResult
            {
                OriginalName = originalName,
                Strategy = ConflictStrategy.SuffixRandom,
                Success = false
            };

            var maxAttempts = Math.Min(settings.ConflictResolution.MaxAttempts, 50); // Limit random suffix attempts
            var attempts = 0;

            for (int i = 0; i < maxAttempts; i++, attempts++)
            {
                // Generate random 6-character suffix (alphanumeric)
                var suffix = GenerateRandomSuffix(6);
                var candidateName = $"{originalName}-{suffix}";

                // Check if this name exists in Azure
                var validation = await _validationService.ValidateNameAsync(candidateName, resourceType);

                if (!validation.ExistsInAzure)
                {
                    // Found a unique name!
                    result.FinalName = candidateName;
                    result.Success = true;
                    result.Attempts = attempts + 1;
                    result.Warning = settings.ConflictResolution.IncludeWarnings
                        ? $"Original name '{originalName}' exists in Azure. Added random suffix: '{candidateName}'."
                        : null;
                    return result;
                }
            }

            // Exceeded max attempts (very unlikely with random suffixes)
            result.FinalName = originalName;
            result.ErrorMessage = $"Could not generate unique name with random suffix after {maxAttempts} attempts";
            result.Attempts = maxAttempts;

            return result;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates a random alphanumeric suffix
        /// </summary>
        private string GenerateRandomSuffix(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var suffix = new char[length];

            for (int i = 0; i < length; i++)
            {
                suffix[i] = chars[random.Next(chars.Length)];
            }

            return new string(suffix);
        }

        #endregion
    }

    /// <summary>
    /// Result of conflict resolution
    /// </summary>
    public class ConflictResolutionResult
    {
        /// <summary>
        /// Gets or sets the original name before conflict resolution
        /// </summary>
        public string OriginalName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the final resolved name
        /// </summary>
        public string FinalName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the strategy used for resolution
        /// </summary>
        public ConflictStrategy Strategy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether resolution was successful
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of attempts made to find a unique name
        /// </summary>
        public int Attempts { get; set; } = 0;

        /// <summary>
        /// Gets or sets a warning message (if applicable)
        /// </summary>
        public string? Warning { get; set; }

        /// <summary>
        /// Gets or sets an error message if resolution failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
