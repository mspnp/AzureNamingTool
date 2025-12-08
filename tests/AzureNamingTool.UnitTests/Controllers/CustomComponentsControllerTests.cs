using AzureNamingTool.Controllers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Controllers;

public class CustomComponentsControllerTests
{
    private readonly Mock<ICustomComponentService> _mockCustomComponentService;
    private readonly Mock<IResourceComponentService> _mockResourceComponentService;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly CustomComponentsController _controller;

    public CustomComponentsControllerTests()
    {
        _mockCustomComponentService = new Mock<ICustomComponentService>();
        _mockResourceComponentService = new Mock<IResourceComponentService>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _controller = new CustomComponentsController(
            _mockCustomComponentService.Object,
            _mockResourceComponentService.Object,
            _mockAdminLogService.Object);
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var customComponents = new List<CustomComponent>
        {
            new CustomComponent { Id = 1, Name = "Component1", ParentComponent = "ResourceType" },
            new CustomComponent { Id = 2, Name = "Component2", ParentComponent = "ResourceType" }
        };

        var serviceResponse = new ServiceResponse
        {
            Success = true,
            ResponseObject = customComponents
        };

        _mockCustomComponentService
            .Setup(s => s.GetItemsAsync(It.IsAny<bool>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Get();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(customComponents);

        _mockCustomComponentService.Verify(
            s => s.GetItemsAsync(It.IsAny<bool>()),
            Times.Once,
            "GetItemsAsync should be called once");
    }

    [Fact]
    public async Task Get_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var serviceResponse = new ServiceResponse
        {
            Success = false,
            ResponseObject = "Error retrieving components"
        };

        _mockCustomComponentService
            .Setup(s => s.GetItemsAsync(It.IsAny<bool>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Error retrieving components");
    }

    [Fact]
    public async Task Get_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        _mockCustomComponentService
            .Setup(s => s.GetItemsAsync(It.IsAny<bool>()))
            .ThrowsAsync(new System.Exception("Database error"));

        _mockAdminLogService
            .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
            .ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _controller.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();

        _mockAdminLogService.Verify(
            s => s.PostItemAsync(It.Is<AdminLogMessage>(m =>
                m.Title == "ERROR" &&
                m.Message == "Database error")),
            Times.Once,
            "Error should be logged when exception occurs");
    }

    [Fact]
    public async Task GetByParentComponentId_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        int parentComponentId = 1;
        var customComponents = new List<CustomComponent>
        {
            new CustomComponent { Id = 1, Name = "Component1", ParentComponent = "ResourceType" },
            new CustomComponent { Id = 2, Name = "Component2", ParentComponent = "ResourceType" }
        };

        var serviceResponse = new ServiceResponse
        {
            Success = true,
            ResponseObject = customComponents
        };

        _mockCustomComponentService
            .Setup(s => s.GetItemsByParentComponentIdAsync(parentComponentId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetByParentComponentId(parentComponentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(customComponents);

        _mockCustomComponentService.Verify(
            s => s.GetItemsByParentComponentIdAsync(parentComponentId),
            Times.Once);
    }

    [Fact]
    public async Task GetByParentComponentId_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        int parentComponentId = 1;
        var serviceResponse = new ServiceResponse
        {
            Success = false,
            ResponseObject = "Error retrieving components by parent ID"
        };

        _mockCustomComponentService
            .Setup(s => s.GetItemsByParentComponentIdAsync(parentComponentId))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetByParentComponentId(parentComponentId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Error retrieving components by parent ID");
    }

    [Fact]
    public async Task GetByParentComponentId_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        int parentComponentId = 1;
        _mockCustomComponentService
            .Setup(s => s.GetItemsByParentComponentIdAsync(parentComponentId))
            .ThrowsAsync(new System.Exception("Service error"));

        _mockAdminLogService
            .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
            .ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _controller.GetByParentComponentId(parentComponentId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();

        _mockAdminLogService.Verify(
            s => s.PostItemAsync(It.Is<AdminLogMessage>(m =>
                m.Title == "ERROR" &&
                m.Message == "Service error")),
            Times.Once);
    }

    [Fact]
    public async Task GetByParentType_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        string parentType = "ResourceType";
        var customComponents = new List<CustomComponent>
        {
            new CustomComponent { Id = 1, Name = "Component1", ParentComponent = "resourcetype" }
        };

        var serviceResponse = new ServiceResponse
        {
            Success = true,
            ResponseObject = customComponents
        };

        _mockCustomComponentService
            .Setup(s => s.GetItemsByParentTypeAsync(It.IsAny<string>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetByParentType(parentType);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(customComponents);

        _mockCustomComponentService.Verify(
            s => s.GetItemsByParentTypeAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByParentType_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        string parentType = "ResourceType";
        var serviceResponse = new ServiceResponse
        {
            Success = false,
            ResponseObject = "Error retrieving components by parent type"
        };

        _mockCustomComponentService
            .Setup(s => s.GetItemsByParentTypeAsync(It.IsAny<string>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetByParentType(parentType);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Error retrieving components by parent type");
    }

    [Fact]
    public async Task GetByParentType_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        string parentType = "ResourceType";
        _mockCustomComponentService
            .Setup(s => s.GetItemsByParentTypeAsync(It.IsAny<string>()))
            .ThrowsAsync(new System.Exception("Query error"));

        _mockAdminLogService
            .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
            .ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _controller.GetByParentType(parentType);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();

        _mockAdminLogService.Verify(
            s => s.PostItemAsync(It.Is<AdminLogMessage>(m =>
                m.Title == "ERROR" &&
                m.Message == "Query error")),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidDependencies()
    {
        // Arrange & Act
        var controller = new CustomComponentsController(
            _mockCustomComponentService.Object,
            _mockResourceComponentService.Object,
            _mockAdminLogService.Object);

        // Assert
        controller.Should().NotBeNull();
    }
}
