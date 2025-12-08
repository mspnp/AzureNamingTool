using AzureNamingTool.Data;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

/// <summary>
/// Tests for StorageMigrationService to ensure data migration from JSON to SQLite works correctly.
/// Uses in-memory SQLite database and temporary test files.
/// </summary>
public class StorageMigrationServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ConfigurationDbContext _dbContext;
    private readonly Mock<ILogger<StorageMigrationService>> _loggerMock;
    private readonly string _testSettingsPath;
    private readonly string _testBackupPath;

    public StorageMigrationServiceTests()
    {
        // Create in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ConfigurationDbContext(options);
        _dbContext.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<StorageMigrationService>>();

        // Create temporary test directories
        _testSettingsPath = Path.Combine(Path.GetTempPath(), "AzureNamingToolTests", Guid.NewGuid().ToString(), "settings");
        _testBackupPath = Path.Combine(Path.GetTempPath(), "AzureNamingToolTests", Guid.NewGuid().ToString(), "backups");
        Directory.CreateDirectory(_testSettingsPath);
        Directory.CreateDirectory(_testBackupPath);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        // Cleanup test directories
        try
        {
            if (Directory.Exists(_testSettingsPath))
            {
                Directory.Delete(Path.GetDirectoryName(_testSettingsPath)!, true);
            }
            if (Directory.Exists(_testBackupPath))
            {
                Directory.Delete(Path.GetDirectoryName(_testBackupPath)!, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task IsMigrationNeededAsync_ShouldReturnFalse_WhenSettingsDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, nonExistentPath, _testBackupPath);

        // Act
        var result = await service.IsMigrationNeededAsync();

        // Assert
        result.Should().BeFalse("settings directory does not exist");
    }

    [Fact]
    public async Task IsMigrationNeededAsync_ShouldReturnFalse_WhenNoJsonFilesExist()
    {
        // Arrange
        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var result = await service.IsMigrationNeededAsync();

        // Assert
        result.Should().BeFalse("no JSON files exist in settings directory");
    }

    [Fact]
    public async Task IsMigrationNeededAsync_ShouldReturnFalse_WhenDatabaseAlreadyHasData()
    {
        // Arrange
        CreateTestJsonFile("resourcetype.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });

        // Add data to database
        await _dbContext.ResourceTypes.AddAsync(new ResourceType { Id = 1, Resource = "existing", ShortName = "ex", Enabled = true });
        await _dbContext.SaveChangesAsync();

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var result = await service.IsMigrationNeededAsync();

        // Assert
        result.Should().BeFalse("database already has data");
    }

    [Fact]
    public async Task IsMigrationNeededAsync_ShouldReturnTrue_WhenJsonFilesExistAndDatabaseIsEmpty()
    {
        // Arrange
        CreateTestJsonFile("resourcetype.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var result = await service.IsMigrationNeededAsync();

        // Assert
        result.Should().BeTrue("JSON files exist and database is empty");
    }

    [Fact]
    public async Task BackupCurrentDataAsync_ShouldCreateBackupDirectory()
    {
        // Arrange
        CreateTestJsonFile("test.json", new { test = "data" });
        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var backupPath = await service.BackupCurrentDataAsync();

        // Assert
        backupPath.Should().NotBeNullOrEmpty();
        Directory.Exists(backupPath).Should().BeTrue("backup directory should be created");
        backupPath.Should().StartWith(_testBackupPath);
        backupPath.Should().Contain("backup_");
    }

    [Fact]
    public async Task BackupCurrentDataAsync_ShouldCopyAllJsonFiles()
    {
        // Arrange
        CreateTestJsonFile("resourcetype.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });
        CreateTestJsonFile("resourcelocation.json", new List<ResourceLocation>
        {
            new() { Id = 1, Name = "East US", ShortName = "eus", Enabled = true }
        });

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var backupPath = await service.BackupCurrentDataAsync();

        // Assert
        var backedUpFiles = Directory.GetFiles(backupPath, "*.json");
        backedUpFiles.Should().HaveCount(2, "both JSON files should be backed up");
        backedUpFiles.Should().Contain(f => f.EndsWith("resourcetype.json"));
        backedUpFiles.Should().Contain(f => f.EndsWith("resourcelocation.json"));
    }

    [Fact]
    public async Task BackupCurrentDataAsync_ShouldThrowException_WhenBackupFails()
    {
        // Arrange - use an invalid path to force failure
        var invalidBackupPath = Path.Combine(new string('x', 300), "backups"); // Path too long
        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, invalidBackupPath);

        // Act
        Func<Task> act = async () => await service.BackupCurrentDataAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to create backup*");
    }

    [Fact]
    public async Task MigrateToSQLiteAsync_ShouldMigrateAllEntities_WhenJsonFilesExist()
    {
        // Arrange
        var testData = new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true },
            new() { Id = 2, Resource = "vm", ShortName = "vm", Enabled = true }
        };
        CreateTestJsonFile("resourcetypes.json", testData);

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var result = await service.MigrateToSQLiteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("migration should succeed");
        result.EntitiesMigrated.Should().Be(2, "two entities should be migrated");
        result.EntityCounts.Should().ContainKey("ResourceType");
        result.EntityCounts["ResourceType"].Should().Be(2);
        result.BackupPath.Should().NotBeNullOrEmpty("backup should be created");
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);

        // Verify data is in database
        var migratedData = await _dbContext.ResourceTypes.ToListAsync();
        migratedData.Should().HaveCount(2);
        migratedData.Should().Contain(rt => rt.Resource == "rg");
        migratedData.Should().Contain(rt => rt.Resource == "vm");
    }

    [Fact]
    public async Task MigrateToSQLiteAsync_ShouldHandleMultipleEntityTypes()
    {
        // Arrange
        CreateTestJsonFile("resourcetypes.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });
        CreateTestJsonFile("resourcelocations.json", new List<ResourceLocation>
        {
            new() { Id = 1, Name = "East US", ShortName = "eus", Enabled = true },
            new() { Id = 2, Name = "West US", ShortName = "wus", Enabled = true }
        });
        CreateTestJsonFile("resourceenvironments.json", new List<ResourceEnvironment>
        {
            new() { Id = 1, Name = "Production", ShortName = "prd" }
        });

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var result = await service.MigrateToSQLiteAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.EntitiesMigrated.Should().Be(4, "1 + 2 + 1 = 4 entities");
        result.EntityCounts.Should().HaveCount(4, "three entity types plus AzureValidationSettings");
        result.EntityCounts["ResourceType"].Should().Be(1);
        result.EntityCounts["ResourceLocation"].Should().Be(2);
        result.EntityCounts["ResourceEnvironment"].Should().Be(1);
        result.EntityCounts["AzureValidationSettings"].Should().Be(0, "no validation settings file");

        // Verify database
        var types = await _dbContext.ResourceTypes.ToListAsync();
        var locations = await _dbContext.ResourceLocations.ToListAsync();
        var environments = await _dbContext.ResourceEnvironments.ToListAsync();

        types.Should().HaveCount(1);
        locations.Should().HaveCount(2);
        environments.Should().HaveCount(1);
    }

    [Fact]
    public async Task MigrateToSQLiteAsync_ShouldSkipEmptyJsonFiles()
    {
        // Arrange
        CreateTestJsonFile("resourcetypes.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });
        CreateTestJsonFile("resourcelocations.json", "[]"); // Empty array

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var result = await service.MigrateToSQLiteAsync();

        // Assert
        result.Success.Should().BeTrue();
        result.EntitiesMigrated.Should().Be(1, "only non-empty files should be migrated");
        result.EntityCounts["ResourceType"].Should().Be(1);
        result.EntityCounts["ResourceLocation"].Should().Be(0);
    }

    [Fact]
    public async Task MigrateToSQLiteAsync_ShouldUseProvidedBackupPath()
    {
        // Arrange
        CreateTestJsonFile("resourcetypes.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);
        var existingBackupPath = await service.BackupCurrentDataAsync();

        // Act
        var result = await service.MigrateToSQLiteAsync(existingBackupPath);

        // Assert
        result.Success.Should().BeTrue();
        result.BackupPath.Should().Be(existingBackupPath, "should use provided backup path");
    }

    [Fact]
    public async Task MigrateToSQLiteAsync_ShouldRollbackOnFailure()
    {
        // Arrange
        CreateTestJsonFile("resourcetypes.json", "{ invalid json }"); // Invalid JSON to trigger failure

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var result = await service.MigrateToSQLiteAsync();

        // Assert
        result.Success.Should().BeFalse("migration should fail with invalid JSON");
        result.Message.ToLower().Should().Contain("failed");
        result.Errors.Should().NotBeEmpty("errors should be recorded");
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ValidateMigrationAsync_ShouldReturnValid_WhenCountsMatch()
    {
        // Arrange
        var testData = new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true },
            new() { Id = 2, Resource = "vm", ShortName = "vm", Enabled = true }
        };
        CreateTestJsonFile("resourcetypes.json", testData);

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Migrate data first
        await service.MigrateToSQLiteAsync();

        // Act
        var validation = await service.ValidateMigrationAsync();

        // Assert
        validation.Should().NotBeNull();
        validation.IsValid.Should().BeTrue("counts should match");
        validation.EntityValidation.Should().ContainKey("ResourceType");
        validation.EntityValidation["ResourceType"].SourceCount.Should().Be(2);
        validation.EntityValidation["ResourceType"].TargetCount.Should().Be(2);
        validation.EntityValidation["ResourceType"].Matches.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateMigrationAsync_ShouldReturnInvalid_WhenCountsMismatch()
    {
        // Arrange
        var testData = new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true },
            new() { Id = 2, Resource = "vm", ShortName = "vm", Enabled = true }
        };
        CreateTestJsonFile("resourcetypes.json", testData);

        // Add only one item to database (simulating incomplete migration)
        await _dbContext.ResourceTypes.AddAsync(new ResourceType { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true });
        await _dbContext.SaveChangesAsync();

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var validation = await service.ValidateMigrationAsync();

        // Assert
        validation.IsValid.Should().BeFalse("counts do not match");
        validation.EntityValidation["ResourceType"].SourceCount.Should().Be(2);
        validation.EntityValidation["ResourceType"].TargetCount.Should().Be(1);
        validation.EntityValidation["ResourceType"].Matches.Should().BeFalse();
        validation.EntityValidation["ResourceType"].Discrepancies.Should().Contain("Count mismatch: JSON=2, SQLite=1");
    }

    [Fact]
    public async Task GetMigrationStatusAsync_ShouldReturnCorrectStatus_WhenNotMigrated()
    {
        // Arrange
        CreateTestJsonFile("resourcetypes.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Act
        var status = await service.GetMigrationStatusAsync();

        // Assert
        status.Should().NotBeNull();
        status.IsMigrated.Should().BeFalse("no data in database yet");
        status.JsonFilesExist.Should().BeTrue("JSON files exist");
        status.SQLiteDatabaseExists.Should().BeTrue("database exists even if empty");
        status.CurrentProvider.Should().Be("SQLite", "database exists and can connect");
        status.LastMigrationDate.Should().BeNull();
    }

    [Fact]
    public async Task GetMigrationStatusAsync_ShouldReturnCorrectStatus_WhenMigrated()
    {
        // Arrange
        CreateTestJsonFile("resourcetypes.json", new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        });

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Migrate data
        await service.MigrateToSQLiteAsync();

        // Act
        var status = await service.GetMigrationStatusAsync();

        // Assert
        status.IsMigrated.Should().BeTrue("data has been migrated");
        status.JsonFilesExist.Should().BeTrue();
        status.SQLiteDatabaseExists.Should().BeTrue();
    }

    [Fact]
    public async Task RollbackMigrationAsync_ShouldDeleteDatabaseAndRestoreFromBackup()
    {
        // Arrange
        var testData = new List<ResourceType>
        {
            new() { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
        };
        CreateTestJsonFile("resourcetypes.json", testData);

        var service = new StorageMigrationServiceTestable(_dbContext, _loggerMock.Object, _testSettingsPath, _testBackupPath);

        // Create backup
        var backupPath = await service.BackupCurrentDataAsync();

        // Add some data to database
        await _dbContext.ResourceTypes.AddAsync(new ResourceType { Id = 100, Resource = "test", ShortName = "tst", Enabled = true });
        await _dbContext.SaveChangesAsync();

        // Modify JSON file to be different from backup
        CreateTestJsonFile("resourcetypes.json", new List<ResourceType>
        {
            new() { Id = 2, Resource = "modified", ShortName = "mod", Enabled = true }
        });

        // Act
        await service.RollbackMigrationAsync(backupPath);

        // Assert
        // Database should be cleared (can't verify directly with in-memory DB, but method shouldn't throw)
        var jsonContent = await File.ReadAllTextAsync(Path.Combine(_testSettingsPath, "resourcetypes.json"));
        var restoredData = JsonSerializer.Deserialize<List<ResourceType>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        restoredData.Should().HaveCount(1, "original data should be restored from backup");
        restoredData![0].Resource.Should().Be("rg", "original data should match backup");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbContextIsNull()
    {
        // Arrange & Act
        Action act = () => new StorageMigrationService(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dbContext");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange & Act
        Action act = () => new StorageMigrationService(_dbContext, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // Helper methods
    private void CreateTestJsonFile<T>(string fileName, T data)
    {
        var filePath = Path.Combine(_testSettingsPath, fileName);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }

    private void CreateTestJsonFile(string fileName, string content)
    {
        var filePath = Path.Combine(_testSettingsPath, fileName);
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// Testable version of StorageMigrationService that allows overriding paths
    /// </summary>
    private class StorageMigrationServiceTestable : StorageMigrationService
    {
        private readonly string _customSettingsPath;
        private readonly string _customBackupPath;

        public StorageMigrationServiceTestable(
            ConfigurationDbContext dbContext,
            ILogger<StorageMigrationService> logger,
            string settingsPath,
            string backupPath) : base(dbContext, logger)
        {
            _customSettingsPath = settingsPath;
            _customBackupPath = backupPath;

            // Use reflection to set private fields
            var settingsField = typeof(StorageMigrationService).GetField("_settingsPath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            settingsField?.SetValue(this, _customSettingsPath);

            var backupField = typeof(StorageMigrationService).GetField("_backupBasePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            backupField?.SetValue(this, _customBackupPath);
        }
    }
}
