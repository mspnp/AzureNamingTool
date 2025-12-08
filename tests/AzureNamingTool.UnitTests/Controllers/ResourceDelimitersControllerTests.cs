using AzureNamingTool.Controllers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Controllers;

public class ResourceDelimitersControllerTests
{
    private readonly Mock<IResourceDelimiterService> _mockService;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly ResourceDelimitersController _controller;

    public ResourceDelimitersControllerTests()
    {
        _mockService = new Mock<IResourceDelimiterService>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _controller = new ResourceDelimitersController(_mockService.Object, _mockAdminLogService.Object);
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var items = new List<ResourceDelimiter> { new ResourceDelimiter { Id = 1, Name = "Hyphen" } };
        var serviceResponse = new ServiceResponse { Success = true, ResponseObject = items };
        _mockService.Setup(s => s.GetItemsAsync(It.IsAny<bool>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Get();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Get_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var serviceResponse = new ServiceResponse { Success = false, ResponseObject = "Error" };
        _mockService.Setup(s => s.GetItemsAsync(It.IsAny<bool>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Get_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        _mockService.Setup(s => s.GetItemsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception("Test error"));
        _mockAdminLogService.Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>())).ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _controller.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockAdminLogService.Verify(s => s.PostItemAsync(It.Is<AdminLogMessage>(m => m.Title == "ERROR")), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenItemExists()
    {
        // Arrange
        var item = new ResourceDelimiter { Id = 1, Name = "Hyphen" };
        var serviceResponse = new ServiceResponse { Success = true, ResponseObject = item };
        _mockService.Setup(s => s.GetItemAsync(1)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Get(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task GetById_ShouldReturnBadRequest_WhenItemNotFound()
    {
        // Arrange
        var serviceResponse = new ServiceResponse { Success = false };
        _mockService.Setup(s => s.GetItemAsync(999)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Get(999);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
