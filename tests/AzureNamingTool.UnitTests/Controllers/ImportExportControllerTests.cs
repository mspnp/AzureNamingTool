using AzureNamingTool.Controllers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Controllers;

public class ImportExportControllerTests
{
    private readonly Mock<IImportExportService> _mockService;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly ImportExportController _controller;

    public ImportExportControllerTests()
    {
        _mockService = new Mock<IImportExportService>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _controller = new ImportExportController(_mockService.Object, _mockAdminLogService.Object);
    }

    [Fact]
    public async Task ExportConfiguration_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var configData = new ConfigurationData();
        var serviceResponse = new ServiceResponse { Success = true, ResponseObject = configData };
        _mockService.Setup(s => s.ExportConfigAsync(It.IsAny<bool>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.ExportConfiguration();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(configData);
    }

    [Fact]
    public async Task ExportConfiguration_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var serviceResponse = new ServiceResponse { Success = false, ResponseObject = "Export failed" };
        _mockService.Setup(s => s.ExportConfigAsync(It.IsAny<bool>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.ExportConfiguration();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ExportConfiguration_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        _mockService.Setup(s => s.ExportConfigAsync(It.IsAny<bool>())).ThrowsAsync(new Exception("Test error"));
        _mockAdminLogService.Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>())).ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _controller.ExportConfiguration();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockAdminLogService.Verify(s => s.PostItemAsync(It.Is<AdminLogMessage>(m => m.Title == "ERROR")), Times.Once);
    }

    [Fact]
    public async Task ImportConfiguration_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var configData = new ConfigurationData();
        var serviceResponse = new ServiceResponse { Success = true, ResponseObject = "Import successful" };
        _mockService.Setup(s => s.PostConfigAsync(It.IsAny<ConfigurationData>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.ImportConfiguration(configData);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ImportConfiguration_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var configData = new ConfigurationData();
        var serviceResponse = new ServiceResponse { Success = false, ResponseObject = "Import failed" };
        _mockService.Setup(s => s.PostConfigAsync(It.IsAny<ConfigurationData>())).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.ImportConfiguration(configData);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ImportConfiguration_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var configData = new ConfigurationData();
        _mockService.Setup(s => s.PostConfigAsync(It.IsAny<ConfigurationData>())).ThrowsAsync(new Exception("Test error"));
        _mockAdminLogService.Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>())).ReturnsAsync(new ServiceResponse { Success = true });

        // Act
        var result = await _controller.ImportConfiguration(configData);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockAdminLogService.Verify(s => s.PostItemAsync(It.Is<AdminLogMessage>(m => m.Title == "ERROR")), Times.Once);
    }
}
