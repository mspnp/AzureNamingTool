using AzureNamingTool.Attributes;
using AzureNamingTool.Components;
using AzureNamingTool.Data;
using AzureNamingTool.Helpers;
using AzureNamingTool.Middleware;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories;
using AzureNamingTool.Repositories.Implementation.FileSystem;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using BlazorDownloadFile;
using Blazored.Modal;
using Blazored.Toast;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add Response Compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        // Optimize for better responsiveness in Azure App Service
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);  // Increased for slower networks
        options.EnableDetailedErrors = builder.Configuration.GetValue<bool>("DetailedErrors", false);
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        options.KeepAliveInterval = TimeSpan.FromSeconds(10);      // More frequent keep-alive
        options.MaximumParallelInvocationsPerClient = 2;           // Allow parallel operations
        options.MaximumReceiveMessageSize = 102400000;
        options.StreamBufferCapacity = 10;
    })
    .AddCircuitOptions(options =>
    {
        // Configure circuit behavior for Azure App Service
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    });


builder.Services.AddHealthChecks();
builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<StateContainer>();

builder.Services.AddEndpointsApiExplorer();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
        new Asp.Versioning.UrlSegmentApiVersionReader(),
        new Asp.Versioning.HeaderApiVersionReader("X-Api-Version")
    );
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure JSON serialization options globally
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Convert enums to their string names instead of numeric values
    // This makes JSON configuration files human-readable
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<CustomHeaderSwaggerAttribute>();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Azure Naming Tool API v1",
        Description = "An ASP.NET Core Web API for managing the Azure Naming tool configuration. All API requests require the configured API Keys (found in the site Admin configuration). You can find more details in the <a href=\"https://github.com/mspnp/AzureNamingTool/wiki/Using-the-API\" target=\"_new\">Azure Naming Tool API documentation</a>.<br/><br/><strong>Version 1.0:</strong> Current stable API with backward compatibility."
    });
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2.0",
        Title = "Azure Naming Tool API v2",
        Description = "An ASP.NET Core Web API for managing the Azure Naming tool configuration. All API requests require the configured API Keys (found in the site Admin configuration). You can find more details in the <a href=\"https://github.com/mspnp/AzureNamingTool/wiki/Using-the-API\" target=\"_new\">Azure Naming Tool API documentation</a>.<br/><br/><strong>Version 2.0:</strong> Enhanced API with standardized responses, improved error handling, and additional features."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add services to the container
builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddMemoryCache();
builder.Services.AddMvcCore().AddApiExplorer();

// Register HttpClient for Azure validation services
builder.Services.AddHttpClient();

// Register Cache Service (Singleton since it wraps IMemoryCache)
builder.Services.AddSingleton<ICacheService, CacheService>();

// Register Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<AzureNamingTool.HealthChecks.StorageHealthCheck>(
        "storage",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "storage" })
    .AddCheck<AzureNamingTool.HealthChecks.CacheHealthCheck>(
        "cache",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "ready", "cache" });

// Always register DbContext (needed by StorageMigrationService even when using FileSystem)
var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/azurenamingtool.db");

// Ensure the database directory exists (e.g., /settings for container persistence)
var dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

// Check for pending database restore (before DbContext initialization)
var flagPath = Path.Combine(dbDirectory!, "restore-pending.flag");
var pendingRestorePath = Path.Combine(dbDirectory!, "azurenamingtool-pending-restore.db");

if (File.Exists(flagPath) && File.Exists(pendingRestorePath))
{
    try
    {
        Console.WriteLine("=== DATABASE RESTORE DETECTED ===");
        Console.WriteLine($"Flag file: {flagPath}");
        Console.WriteLine($"Restore file: {pendingRestorePath}");
        Console.WriteLine($"Target database: {dbPath}");
        
        // Read flag file for logging
        var flagContent = File.ReadAllText(flagPath);
        Console.WriteLine($"Restore details:\n{flagContent}");
        
        // Delete the current database (if it exists)
        if (File.Exists(dbPath))
        {
            Console.WriteLine("Deleting current database...");
            File.Delete(dbPath);
        }
        
        // Move the pending restore file to the active database location
        Console.WriteLine("Restoring database from backup...");
        File.Move(pendingRestorePath, dbPath);
        
        // Delete the flag file
        Console.WriteLine("Cleaning up restore flag...");
        File.Delete(flagPath);
        
        // Clean up old pre-restore backup files (keep only the 5 most recent)
        Console.WriteLine("Cleaning up old pre-restore backup files...");
        try
        {
            var backupFiles = Directory.GetFiles(dbDirectory!, "azurenamingtool-prerestore-*.db")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();
            
            if (backupFiles.Count > 5)
            {
                var filesToDelete = backupFiles.Skip(5);
                foreach (var fileToDelete in filesToDelete)
                {
                    Console.WriteLine($"Deleting old backup: {fileToDelete.Name}");
                    fileToDelete.Delete();
                }
                Console.WriteLine($"Deleted {backupFiles.Count - 5} old backup file(s).");
            }
            else
            {
                Console.WriteLine($"Found {backupFiles.Count} backup file(s). No cleanup needed (keeping max 5).");
            }
        }
        catch (Exception cleanupEx)
        {
            Console.WriteLine($"Warning: Failed to clean up old backups: {cleanupEx.Message}");
            // Don't fail the restore if cleanup fails
        }
        
        // Clean up any temporary backup files left from export operations
        Console.WriteLine("Cleaning up temporary backup files...");
        try
        {
            var tempBackupFiles = Directory.GetFiles(dbDirectory!, "temp_backup_*.db");
            if (tempBackupFiles.Length > 0)
            {
                foreach (var tempFile in tempBackupFiles)
                {
                    Console.WriteLine($"Deleting temp backup: {Path.GetFileName(tempFile)}");
                    File.Delete(tempFile);
                }
                Console.WriteLine($"Deleted {tempBackupFiles.Length} temporary backup file(s).");
            }
            else
            {
                Console.WriteLine("No temporary backup files found.");
            }
        }
        catch (Exception cleanupEx)
        {
            Console.WriteLine($"Warning: Failed to clean up temp backups: {cleanupEx.Message}");
            // Don't fail the restore if cleanup fails
        }
        
        Console.WriteLine("=== DATABASE RESTORE COMPLETED SUCCESSFULLY ===");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR during database restore: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        // Don't delete flag file so restore can be retried
    }
}

var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
    options.UseSqlite(connectionString));

// Always register Migration Service (needed by MainLayout and Admin page)
builder.Services.AddScoped<IStorageMigrationService, StorageMigrationService>();

// Storage Provider Detection Logic
// Default for v5.0.0: SQLite (set in repository/appsettings.json)
// Only override if upgrading from < v5.0.0 (settings file exists but missing StorageProvider field)
string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
string settingsAppsettingsPath = Path.Combine(settingsPath, "appsettings.json");

// Check if settings/appsettings.json exists BEFORE VerifyConfiguration runs
if (File.Exists(settingsAppsettingsPath))
{
    Console.WriteLine("[Storage] Existing settings/appsettings.json detected - checking for StorageProvider field");
    
    try
    {
        // Read the existing settings/appsettings.json
        var settingsJson = File.ReadAllText(settingsAppsettingsPath);
        var settingsConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(settingsJson);
        
        if (settingsConfig != null && !settingsConfig.ContainsKey("StorageProvider"))
        {
            // < v5.0.0 file - missing StorageProvider field = upgrade from v4.x
            // v4.x always used JSON files (FileSystem provider)
            Console.WriteLine("[Storage] Upgrade from < v5.0.0 detected (missing StorageProvider field)");
            
            // Add StorageProvider field to the existing file
            try
            {
                settingsConfig.Add("StorageProvider", System.Text.Json.JsonSerializer.SerializeToElement("FileSystem"));
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var updatedJson = System.Text.Json.JsonSerializer.Serialize(settingsConfig, options);
                File.WriteAllText(settingsAppsettingsPath, updatedJson);
                Console.WriteLine("[Storage] Updated settings/appsettings.json with StorageProvider=FileSystem");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Storage] Warning: Could not update settings/appsettings.json: {ex.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Storage] Warning: Could not read settings/appsettings.json: {ex.Message}");
    }
}
else
{
    // No settings/appsettings.json = Fresh install
    // VerifyConfiguration will copy repository/appsettings.json with StorageProvider=FileSystem (default)
    Console.WriteLine("[Storage] Fresh install detected - will use default (FileSystem)");
}

// Run VerifyConfiguration to ensure all config files exist
// For fresh installs, this copies repository/appsettings.json → settings/appsettings.json (with StorageProvider=FileSystem)
ConfigurationHelper.VerifyConfiguration(new StateContainer());

// Read the configuration and use the StorageProvider value
var siteConfig = ConfigurationHelper.GetConfigurationData();
var provider = siteConfig.StorageProvider?.ToLower() ?? "filesystem";

if (provider == "sqlite")
{
    // SQLite Configuration
    
    // Register SQLite Storage Provider (Scoped because it depends on DbContext)
    builder.Services.AddScoped<IStorageProvider>(sp =>
    {
        var dbContext = sp.GetRequiredService<ConfigurationDbContext>();
        return new SQLiteStorageProvider(dbContext, dbPath);
    });
    
    // Register SQLite Repositories
    builder.Services.AddScoped(typeof(IConfigurationRepository<>), typeof(SQLiteConfigurationRepository<>));
    
    builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
    Console.WriteLine($"[Storage] Using SQLite provider: {dbPath}");
}
else
{
    // FileSystem (JSON) Configuration - DEFAULT
    builder.Services.AddScoped<IStorageProvider, FileSystemStorageProvider>();
    
    // Register JSON File Repositories
    builder.Services.AddScoped(typeof(IConfigurationRepository<>), typeof(JsonFileConfigurationRepository<>));
    builder.Services.AddScoped<IConfigurationRepository<AdminLogMessage>, JsonFileConfigurationRepository<AdminLogMessage>>();
    builder.Services.AddScoped<IConfigurationRepository<ResourceDelimiter>, JsonFileConfigurationRepository<ResourceDelimiter>>();
    
    Console.WriteLine("[Storage] Using FileSystem provider (JSON files)");
}

// Register Application Services (Scoped for per-request lifetime)
builder.Services.AddScoped<IAdminLogService, AdminLogService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<ICustomComponentService, CustomComponentService>();
builder.Services.AddScoped<IGeneratedNamesService, GeneratedNamesService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IResourceComponentService, ResourceComponentService>();
builder.Services.AddScoped<IResourceDelimiterService, ResourceDelimiterService>();
builder.Services.AddScoped<IResourceEnvironmentService, ResourceEnvironmentService>();
builder.Services.AddScoped<IResourceFunctionService, ResourceFunctionService>();
builder.Services.AddScoped<IResourceLocationService, ResourceLocationService>();
builder.Services.AddScoped<IResourceNamingRequestService, ResourceNamingRequestService>();
builder.Services.AddScoped<IResourceOrgService, ResourceOrgService>();
builder.Services.AddScoped<IResourceProjAppSvcService, ResourceProjAppSvcService>();
builder.Services.AddScoped<IResourceTypeService, ResourceTypeService>();
builder.Services.AddScoped<IResourceUnitDeptService, ResourceUnitDeptService>();
builder.Services.AddScoped<IAzureValidationService, AzureValidationService>();
builder.Services.AddScoped<ConflictResolutionService>();

// Register coordinator to break circular dependencies between ResourceComponent and ResourceType
builder.Services.AddScoped<IResourceConfigurationCoordinator, ResourceConfigurationCoordinator>();

// Register Helpers
builder.Services.AddScoped<ServicesHelper>();

var app = builder.Build();

// Initialize SQLite database for fresh installs
if (provider == "sqlite")
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        
        // Create database and tables if they don't exist
        bool isNewDatabase = !File.Exists(dbPath);
        dbContext.Database.EnsureCreated();
        
        if (isNewDatabase)
        {
            Console.WriteLine("[Storage] New SQLite database created - seeding with default data from repository...");
            
            // For fresh installs, seed the database with default data from repository JSON files
            var migrationService = scope.ServiceProvider.GetRequiredService<IStorageMigrationService>();
            try
            {
                var result = await migrationService.LoadRepositoryDataIntoSQLiteAsync();
                Console.WriteLine($"[Storage] Database seeded successfully: {result.EntitiesMigrated} entities migrated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Storage] Warning: Could not seed database with default data: {ex.Message}");
                Console.WriteLine("[Storage] Application will start with empty database");
            }
        }
        else
        {
            Console.WriteLine("[Storage] SQLite database exists and is ready");
        }
    }
}

// Note: Automatic migration has been removed in favor of user-initiated migration
// Users can migrate via the modal prompt (first run) or Admin page (ongoing management)
// The StorageProvider setting in SiteConfiguration.json determines which provider is active

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Naming Tool API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "Azure Naming Tool API v2");
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
});

app.UseHttpsRedirection();

// Enable Response Compression
app.UseResponseCompression();

// Add Correlation ID to all requests/responses
app.UseCorrelationId();

// Add API request/response logging
app.UseApiLogging();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseStatusCodePagesWithRedirects("/404");

app.MapControllers();

// Map Health Check Endpoints
// Basic ping endpoint (backward compatibility)
app.MapHealthChecks("/healthcheck/ping");

// Liveness probe - checks if application is running
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Don't run any checks, just return 200 if app is running
});

// Readiness probe - checks if application is ready to serve requests
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"), // Run all checks tagged with "ready"
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();


/// <summary>
/// Exists so can be used as reference for WebApplicationFactory in tests project
/// </summary>
public partial class Program
{
}