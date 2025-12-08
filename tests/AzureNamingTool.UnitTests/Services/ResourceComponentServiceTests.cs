using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

public class ResourceComponentServiceTests
{
    private readonly Mock<IConfigurationRepository<ResourceComponent>> _mockRepository;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly Mock<IResourceConfigurationCoordinator> _mockCoordinator;
    private readonly ResourceComponentService _service;

    public ResourceComponentServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository<ResourceComponent>>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _mockCoordinator = new Mock<IResourceConfigurationCoordinator>();
        _service = new ResourceComponentService(
            _mockRepository.Object,
            _mockAdminLogService.Object,
            _mockCoordinator.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnEnabledItemsOnly_WhenAdminIsFalse()
    {
        // Arrange
        var items = new List<ResourceComponent>
        {
            new ResourceComponent { Id = 1, Name = "Component1", SortOrder = 1, Enabled = true },
            new ResourceComponent { Id = 2, Name = "Component2", SortOrder = 2, Enabled = false },
            new ResourceComponent { Id = 3, Name = "Component3", SortOrder = 3, Enabled = true }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync(false);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceComponent>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
        returnedItems.All(x => x.Enabled).Should().BeTrue();
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnAllItems_WhenAdminIsTrue()
    {
        // Arrange
        var items = new List<ResourceComponent>
        {
            new ResourceComponent { Id = 1, Name = "Component1", SortOrder = 1, Enabled = true },
            new ResourceComponent { Id = 2, Name = "Component2", SortOrder = 2, Enabled = false }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync(true);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceComponent>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var items = new List<ResourceComponent>
        {
            new ResourceComponent { Id = 1, Name = "Component1" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        var item = result.ResponseObject as ResourceComponent;
        item.Should().NotBeNull();
        item!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnError_WhenItemNotFound()
    {
        // Arrange
        var items = new List<ResourceComponent>
        {
            new ResourceComponent { Id = 1, Name = "Component1" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        ((string)result.ResponseObject!).Should().Be("Resource Component not found!");
    }

    [Fact]
    public async Task GetItemsAsync_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));
        _mockAdminLogService.Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>())).ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Success.Should().BeFalse();
        _mockAdminLogService.Verify(s => s.PostItemAsync(It.Is<AdminLogMessage>(m => m.Title == "ERROR")), Times.Once);
    }
}
