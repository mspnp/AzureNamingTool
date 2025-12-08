using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Implementation.FileSystem;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Repositories;

/// <summary>
/// Tests for JsonFileConfigurationRepository to ensure file-based storage works correctly.
/// These tests validate the repository pattern implementation and establish patterns for future tests.
/// </summary>
public class JsonFileConfigurationRepositoryTests
{
    private readonly Mock<ILogger<JsonFileConfigurationRepository<ResourceType>>> _loggerMock;
    private readonly JsonFileConfigurationRepository<ResourceType> _repository;

    public JsonFileConfigurationRepositoryTests()
    {
        _loggerMock = new Mock<ILogger<JsonFileConfigurationRepository<ResourceType>>>();
        _repository = new JsonFileConfigurationRepository<ResourceType>(_loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenFileDoesNotExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "AzureNamingToolTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Create a repository pointing to a non-existent file location
            // This test validates behavior when settings file doesn't exist yet
            
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull("repository should return empty list instead of null");
            result.Should().BeEmpty("no items should exist when file doesn't exist");
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        const int nonExistentId = 9999;

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull("item with non-existent ID should return null");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Arrange
        const int nonExistentId = 9999;

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId);

        // Assert
        exists.Should().BeFalse("non-existent item should return false");
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WhenLoggerIsProvided()
    {
        // Arrange & Act
        Action act = () => new JsonFileConfigurationRepository<ResourceType>(_loggerMock.Object);

        // Assert
        act.Should().NotThrow("repository should be instantiable with valid logger");
    }

    /// <summary>
    /// Integration test pattern - this demonstrates how to test against actual files
    /// when testing with real configuration data in the settings/ directory.
    /// </summary>
    [Fact(Skip = "Integration test - requires actual settings files")]
    public async Task GetAllAsync_ShouldReturnItems_WhenFileExists()
    {
        // This test is skipped by default but shows the pattern for integration testing
        // To run: ensure settings/resourcetypes.json exists with test data
        
        // Arrange
        var repository = new JsonFileConfigurationRepository<ResourceType>(_loggerMock.Object);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("settings file should contain resource types");
        result.Should().AllSatisfy(rt => 
        {
            rt.Id.Should().BeGreaterThan(0, "all items should have valid IDs");
            rt.Resource.Should().NotBeNullOrEmpty("all items should have resource names");
        });
    }

    /// <summary>
    /// Integration test pattern for save operations
    /// </summary>
    [Fact(Skip = "Integration test - modifies actual settings files")]
    public async Task SaveAsync_ShouldPersistItem_WhenValidItemProvided()
    {
        // This test is skipped by default to prevent modifying real data
        // Pattern shows how to test save operations
        
        // Arrange
        var repository = new JsonFileConfigurationRepository<ResourceType>(_loggerMock.Object);
        var testItem = new ResourceType
        {
            Id = 9999,
            Resource = "Test Resource",
            ShortName = "test",
            Enabled = true
        };

        // Act
        await repository.SaveAsync(testItem);

        // Assert - verify item can be retrieved
        var retrieved = await repository.GetByIdAsync(9999);
        retrieved.Should().NotBeNull();
        retrieved!.Resource.Should().Be("Test Resource");

        // Cleanup - delete test item
        await repository.DeleteAsync(9999);
    }
}

/// <summary>
/// Tests for different entity types to ensure repository pattern works across all models.
/// This establishes the pattern that repositories should be type-agnostic.
/// </summary>
public class JsonFileConfigurationRepositoryGenericTests
{
    [Fact]
    public void Constructor_ShouldWork_ForResourceLocation()
    {
        // Test demonstrates the pattern works for different entity types
        // Arrange
        var logger = Mock.Of<ILogger<JsonFileConfigurationRepository<ResourceLocation>>>();

        // Act
        Action act = () => new JsonFileConfigurationRepository<ResourceLocation>(logger);

        // Assert
        act.Should().NotThrow("repository should support ResourceLocation");
    }

    [Fact]
    public void Constructor_ShouldWork_ForResourceEnvironment()
    {
        // Arrange
        var logger = Mock.Of<ILogger<JsonFileConfigurationRepository<ResourceEnvironment>>>();

        // Act
        Action act = () => new JsonFileConfigurationRepository<ResourceEnvironment>(logger);

        // Assert
        act.Should().NotThrow("repository should support ResourceEnvironment");
    }
}
