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
        returnedItems[0].Name.Should().Be("Component1"); // Sorted by SortOrder
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
        returnedItems.All(x => x.ParentComponent == "location").Should().BeTrue();
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
}
