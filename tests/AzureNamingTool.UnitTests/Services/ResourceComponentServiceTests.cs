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
        returnedItems!.All(x => x.Enabled).Should().BeTrue();
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

    [Fact]
    public async Task PostItemAsync_ShouldAssignCorrectId_WhenItemsHaveBeenDeleted()
    {
        // Arrange - Simulate scenario where IDs 1, 2, 3 exist and ID 2 was deleted
        var existingItems = new List<ResourceComponent>
        {
            new ResourceComponent { Id = 1, Name = "Component1", SortOrder = 1, IsCustom = true },
            new ResourceComponent { Id = 3, Name = "Component3", SortOrder = 2, IsCustom = true }
        };

        var newItem = new ResourceComponent
        {
            Id = 0,
            Name = "NewComponent",
            DisplayName = "New Component",
            IsCustom = true
        };

        List<ResourceComponent>? savedItems = null;
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingItems);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<ResourceComponent>>()))
            .Callback<IEnumerable<ResourceComponent>>(items => savedItems = items.ToList())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        savedItems.Should().NotBeNull();
        var addedItem = savedItems!.FirstOrDefault(x => x.Name == "NewComponent");
        addedItem.Should().NotBeNull();
        addedItem!.Id.Should().Be(4, "because ID should be Max(existing IDs) + 1, not Count + 1");
        addedItem.Id.Should().NotBe(3, "to avoid collision with existing item");
    }

    [Fact]
    public async Task PostItemAsync_ShouldAssignId1_WhenNoItemsExist()
    {
        // Arrange
        var emptyList = new List<ResourceComponent>();
        var newItem = new ResourceComponent
        {
            Id = 0,
            Name = "FirstComponent",
            DisplayName = "First Component",
            IsCustom = true
        };

        List<ResourceComponent>? savedItems = null;
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyList);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<ResourceComponent>>()))
            .Callback<IEnumerable<ResourceComponent>>(items => savedItems = items.ToList())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        savedItems.Should().NotBeNull();
        savedItems!.Should().HaveCount(1);
        savedItems![0].Id.Should().Be(1);
    }
}
