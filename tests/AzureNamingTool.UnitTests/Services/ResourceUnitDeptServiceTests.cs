using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

public class ResourceUnitDeptServiceTests
{
    private readonly Mock<IConfigurationRepository<ResourceUnitDept>> _mockRepository;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly ResourceUnitDeptService _service;

    public ResourceUnitDeptServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository<ResourceUnitDept>>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _service = new ResourceUnitDeptService(_mockRepository.Object, _mockAdminLogService.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnOrderedItems_WhenItemsExist()
    {
        // Arrange
        var items = new List<ResourceUnitDept>
        {
            new ResourceUnitDept { Id = 2, Name = "UnitDept2", SortOrder = 2 },
            new ResourceUnitDept { Id = 1, Name = "UnitDept1", SortOrder = 1 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceUnitDept>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
        returnedItems![0].Name.Should().Be("UnitDept1");
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var items = new List<ResourceUnitDept>
        {
            new ResourceUnitDept { Id = 1, Name = "UnitDept1" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        var item = result.ResponseObject as ResourceUnitDept;
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
