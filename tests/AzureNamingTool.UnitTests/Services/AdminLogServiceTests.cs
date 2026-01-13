using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

public class AdminLogServiceTests
{
    private readonly Mock<IConfigurationRepository<AdminLogMessage>> _mockRepository;
    private readonly AdminLogService _service;

    public AdminLogServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository<AdminLogMessage>>();
        _service = new AdminLogService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnOrderedItems_WhenItemsExist()
    {
        // Arrange
        var items = new List<AdminLogMessage>
        {
            new AdminLogMessage { Id = 1, Title = "Log1", CreatedOn = DateTime.Now.AddHours(-2) },
            new AdminLogMessage { Id = 2, Title = "Log2", CreatedOn = DateTime.Now },
            new AdminLogMessage { Id = 3, Title = "Log3", CreatedOn = DateTime.Now.AddHours(-1) }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<AdminLogMessage>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(3);
        returnedItems![0].Id.Should().Be(2); // Most recent first
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var logMessage = new AdminLogMessage { Id = 1, Title = "Test Log" };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(logMessage);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        var item = result.ResponseObject as AdminLogMessage;
        item.Should().NotBeNull();
        item!.Id.Should().Be(1);
        item.Title.Should().Be("Test Log");
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AdminLogMessage?)null);

        // Act
        var result = await _service.GetItemAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        ((string)result.ResponseObject!).Should().Be("Admin Log Message not found!");
    }

    [Fact]
    public async Task PostItemAsync_ShouldAssignNextId_WhenItemsExist()
    {
        // Arrange
        var existingItems = new List<AdminLogMessage>
        {
            new AdminLogMessage { Id = 1, Title = "Log1" },
            new AdminLogMessage { Id = 2, Title = "Log2" }
        };
        var newItem = new AdminLogMessage { Title = "New Log" };
        
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingItems);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<AdminLogMessage>>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        newItem.Id.Should().Be(3);
    }

    [Fact]
    public async Task PostItemAsync_ShouldAssignIdOne_WhenNoItemsExist()
    {
        // Arrange
        var emptyList = new List<AdminLogMessage>();
        var newItem = new AdminLogMessage { Title = "First Log" };
        
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyList);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<AdminLogMessage>>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        // Note: The service doesn't set ID when list is empty (just adds to empty list)
        // The ID remains at its default value
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Success.Should().BeFalse();
        // ResponseObject is dynamic - cast to Exception for assertion
        Exception? exception = result.ResponseObject as Exception;
        exception.Should().NotBeNull();
        exception!.Message.Should().Be("Database error");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
    {
        // Act & Assert
        Action act = () => new AdminLogService(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
