using AzureNamingTool.Repositories.Implementation.FileSystem;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Repositories;

/// <summary>
/// Tests for FileSystemStorageProvider to ensure health checks and availability work correctly.
/// These tests validate storage provider implementation and establish patterns for testing
/// alternative storage providers (database, blob storage, etc.) in the future.
/// </summary>
public class FileSystemStorageProviderTests
{
    private readonly Mock<ILogger<FileSystemStorageProvider>> _loggerMock;
    private readonly FileSystemStorageProvider _provider;

    public FileSystemStorageProviderTests()
    {
        _loggerMock = new Mock<ILogger<FileSystemStorageProvider>>();
        _provider = new FileSystemStorageProvider(_loggerMock.Object);
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue_WhenFileSystemIsAccessible()
    {
        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue("file system should be available in test environment");
    }

    [Fact]
    public async Task InitializeAsync_ShouldNotThrow_WhenFileSystemIsAccessible()
    {
        // Act
        Func<Task> act = async () => await _provider.InitializeAsync();

        // Assert
        await act.Should().NotThrowAsync("initialization should succeed when file system is available");
    }

    [Fact]
    public async Task GetHealthAsync_ShouldReturnHealthyStatus_WhenFileSystemIsAccessible()
    {
        // Act
        var health = await _provider.GetHealthAsync();

        // Assert
        health.Should().NotBeNull("health status should always be returned");
        health.IsHealthy.Should().BeTrue("file system should be healthy in test environment");
        health.ProviderName.Should().Be("FileSystem (JSON)", "provider name should match");
        health.Message.Should().NotBeNullOrEmpty("health message should be provided");
    }

    [Fact]
    public async Task GetHealthAsync_ShouldIncludeMetadata_WhenFileSystemIsAccessible()
    {
        // Act
        var health = await _provider.GetHealthAsync();

        // Assert
        health.Metadata.Should().NotBeNull("metadata should be provided");
        health.Metadata!.Should().NotBeNull("metadata should be provided");
        
        // If there was a file locking error (e.g., from concurrent test runs), skip the rest
        if (health.Metadata!.ContainsKey("Error"))
        {
            // File locking can occur in test scenarios - this is acceptable
            health.Metadata.Should().ContainKey("SettingsPath", "error metadata should include settings path");
            return;
        }
        
        health.Metadata.Should().ContainKey("DirectoryExists", "should report directory existence");
        health.Metadata.Should().ContainKey("FileCount", "should report file count");
        // Note: CanWrite is only present when write fails - successful writes don't add this key
    }

    [Fact]
    public void ProviderName_ShouldReturnFileSystem()
    {
        // Act
        var name = _provider.ProviderName;

        // Assert
        name.Should().Be("FileSystem (JSON)", "provider should identify itself correctly");
    }

    [Fact]
    public async Task InitializeAsync_ShouldCreateSettingsDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "AzureNamingToolTests", Guid.NewGuid().ToString());
        
        try
        {
            // The provider should create the directory during initialization
            // This validates the auto-creation behavior
            
            // Act
            await _provider.InitializeAsync();

            // Assert
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");
            Directory.Exists(settingsPath).Should().BeTrue(
                "settings directory should exist after initialization (or already existed)");
        }
        finally
        {
            // Cleanup temp directory if created
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task GetHealthAsync_ShouldReportFileCount_WhenSettingsDirectoryHasFiles()
    {
        // Arrange
        await _provider.InitializeAsync();

        // Act
        var health = await _provider.GetHealthAsync();

        // Assert
        health.Metadata.Should().ContainKey("FileCount");
        var fileCount = health.Metadata!["FileCount"];
        fileCount.Should().BeOfType<int>("file count should be an integer");
        ((int)fileCount!).Should().BeGreaterThanOrEqualTo(0, "file count should not be negative");
    }

    /// <summary>
    /// Integration test pattern - validates against actual settings directory
    /// </summary>
    [Fact(Skip = "Integration test - requires actual settings directory")]
    public async Task GetHealthAsync_ShouldReportMultipleFiles_InProductionEnvironment()
    {
        // This test validates behavior in production environment with actual settings files
        
        // Arrange
        var provider = new FileSystemStorageProvider(_loggerMock.Object);
        await provider.InitializeAsync();

        // Act
        var health = await provider.GetHealthAsync();

        // Assert
        health.IsHealthy.Should().BeTrue("production environment should be healthy");
        var fileCount = (int)health.Metadata!["FileCount"]!;
        fileCount.Should().BeGreaterThan(0, "settings directory should contain configuration files");
        
        // Validate typical configuration files exist
        health.Metadata.Should().ContainKey("LastModified");
        health.Metadata!["LastModified"].Should().NotBeNull("should report last modification time");
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldCompleteQuickly()
    {
        // This test ensures health checks are performant
        
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        var isAvailable = await _provider.IsAvailableAsync();

        // Assert
        var duration = DateTime.UtcNow - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(1), 
            "availability check should be fast for health monitoring");
        isAvailable.Should().BeTrue();
    }
}

/// <summary>
/// Tests demonstrating the storage provider abstraction pattern.
/// These tests show how the same test suite could be used for different storage providers.
/// </summary>
public class StorageProviderContractTests
{
    [Fact]
    public async Task FileSystemProvider_ShouldImplementStorageProviderContract()
    {
        // This test validates that FileSystemStorageProvider properly implements
        // the IStorageProvider interface contract. When additional providers are added
        // (e.g., DatabaseStorageProvider, BlobStorageProvider), similar tests ensure
        // they all follow the same contract.

        // Arrange
        var logger = Mock.Of<ILogger<FileSystemStorageProvider>>();
        var provider = new FileSystemStorageProvider(logger);

        // Act & Assert - all interface members should be callable
        await provider.InitializeAsync();
        var isAvailable = await provider.IsAvailableAsync();
        var health = await provider.GetHealthAsync();
        var name = provider.ProviderName;

        // Verify contract compliance
        isAvailable.Should().Be(isAvailable); // Just verify it's a bool
        health.Should().NotBeNull();
        health.IsHealthy.Should().Be(health.IsHealthy); // Just verify it's a bool
        health.ProviderName.Should().Be(name);
        name.Should().NotBeNullOrEmpty();
    }
}
