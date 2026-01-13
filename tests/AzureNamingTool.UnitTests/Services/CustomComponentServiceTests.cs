using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

public class CustomComponentServiceTests
{
    private readonly Mock<IConfigurationRepository<CustomComponent>> _mockRepository;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly Mock<IResourceComponentService> _mockResourceComponentService;
    private readonly CustomComponentService _service;

    public CustomComponentServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository<CustomComponent>>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _mockResourceComponentService = new Mock<IResourceComponentService>();
        _service = new CustomComponentService(
            _mockRepository.Object,
            _mockAdminLogService.Object,
            _mockResourceComponentService.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnOrderedItems_WhenItemsExist()
    {
        // Arrange
        var items = new List<CustomComponent>
        {
            new CustomComponent { Id = 2, Name = "Component2", SortOrder = 2 },
            new CustomComponent { Id = 1, Name = "Component1", SortOrder = 1 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<CustomComponent>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
        returnedItems![0].Name.Should().Be("Component1"); // Sorted by SortOrder
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
    public async Task GetItemsByParentComponentIdAsync_ShouldReturnFilteredItems_WhenParentComponentExists()
    {
        // Arrange
        var parentComponent = new ResourceComponent { Id = 1, Name = "ResourceLocation" };
        var items = new List<CustomComponent>
        {
            new CustomComponent { Id = 1, Name = "Custom1", ParentComponent = "location", SortOrder = 1 },
            new CustomComponent { Id = 2, Name = "Custom2", ParentComponent = "type", SortOrder = 2 },
            new CustomComponent { Id = 3, Name = "Custom3", ParentComponent = "location", SortOrder = 3 }
        };

        _mockResourceComponentService
            .Setup(s => s.GetItemAsync(1))
            .ReturnsAsync(new ServiceResponse { Success = true, ResponseObject = parentComponent });
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsByParentComponentIdAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<CustomComponent>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
        returnedItems!.All(x => x.ParentComponent == "location").Should().BeTrue();
    }

    [Fact]
    public async Task GetItemsByParentComponentIdAsync_ShouldReturnError_WhenParentComponentNotFound()
    {
        // Arrange
        _mockResourceComponentService
            .Setup(s => s.GetItemAsync(999))
            .ReturnsAsync(new ServiceResponse { Success = false });

        // Act
        var result = await _service.GetItemsByParentComponentIdAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        ((string)result.ResponseObject!).Should().Be("Resource Component not found!");
    }

    [Fact]
    public async Task GetItemsByParentComponentIdAsync_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        _mockResourceComponentService.Setup(s => s.GetItemAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Service error"));
        _mockAdminLogService.Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>())).ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _service.GetItemsByParentComponentIdAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        _mockAdminLogService.Verify(s => s.PostItemAsync(It.Is<AdminLogMessage>(m => m.Title == "ERROR")), Times.Once);
    }

    [Fact]
    public async Task PostItemAsync_ShouldAssignCorrectId_WhenItemsHaveBeenDeleted()
    {
        // Arrange - Simulate scenario where IDs 1, 2, 3 exist and ID 2 was deleted
        var existingItems = new List<CustomComponent>
        {
            new CustomComponent { Id = 1, Name = "Component1", ShortName = "c1", ParentComponent = "test", SortOrder = 1 },
            new CustomComponent { Id = 3, Name = "Component3", ShortName = "c3", ParentComponent = "test", SortOrder = 2 }
        };

        var newItem = new CustomComponent
        {
            Id = 0,
            Name = "NewComponent",
            ShortName = "new",
            ParentComponent = "test"
        };

        List<CustomComponent>? savedItems = null;
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingItems);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<CustomComponent>>()))
            .Callback<IEnumerable<CustomComponent>>(items => savedItems = items.ToList())
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
        var emptyList = new List<CustomComponent>();
        var newItem = new CustomComponent
        {
            Id = 0,
            Name = "FirstComponent",
            ShortName = "first",
            ParentComponent = "test"
        };

        List<CustomComponent>? savedItems = null;
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyList);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<CustomComponent>>()))
            .Callback<IEnumerable<CustomComponent>>(items => savedItems = items.ToList())
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
