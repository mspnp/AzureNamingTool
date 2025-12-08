using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

public class ResourceTypeServiceTests
{
    private readonly Mock<IConfigurationRepository<ResourceType>> _mockRepository;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly Mock<IResourceConfigurationCoordinator> _mockCoordinator;
    private readonly Mock<IResourceDelimiterService> _mockDelimiterService;
    private readonly ResourceTypeService _service;

    public ResourceTypeServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository<ResourceType>>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _mockCoordinator = new Mock<IResourceConfigurationCoordinator>();
        _mockDelimiterService = new Mock<IResourceDelimiterService>();
        _service = new ResourceTypeService(
            _mockRepository.Object,
            _mockAdminLogService.Object,
            _mockCoordinator.Object,
            _mockDelimiterService.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnOrderedItems_WhenAdminIsFalse()
    {
        // Arrange
        var items = new List<ResourceType>
        {
            new ResourceType { Id = 2, Resource = "Type2", Enabled = true },
            new ResourceType { Id = 1, Resource = "Type1", Enabled = true }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync(false);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceType>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
        returnedItems.All(x => x.Enabled).Should().BeTrue();
    }

    [Fact]
    public async Task GetItemsAsync_ShouldIncludeDisabledItems_WhenAdminIsTrue()
    {
        // Arrange
        var items = new List<ResourceType>
        {
            new ResourceType { Id = 1, Resource = "Type1", Enabled = true },
            new ResourceType { Id = 2, Resource = "Type2", Enabled = false }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync(true);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceType>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var items = new List<ResourceType>
        {
            new ResourceType { Id = 1, Resource = "Type1" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        var item = result.ResponseObject as ResourceType;
        item.Should().NotBeNull();
        item!.Id.Should().Be(1);
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
