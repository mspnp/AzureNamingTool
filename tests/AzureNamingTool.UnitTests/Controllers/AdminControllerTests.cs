using AzureNamingTool.Controllers;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace AzureNamingTool.UnitTests.Controllers
{
    /// <summary>
    /// Unit tests for AdminController (API v1.0)
    /// Note: Many methods require admin password authentication which complicates testing
    /// Testing focuses on methods that don't require ConfigurationHelper access
    /// </summary>
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly Mock<IAdminLogService> _mockAdminLogService;
        private readonly Mock<IGeneratedNamesService> _mockGeneratedNamesService;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _mockAdminLogService = new Mock<IAdminLogService>();
            _mockGeneratedNamesService = new Mock<IGeneratedNamesService>();

            _controller = new AdminController(
                _mockAdminService.Object,
                _mockAdminLogService.Object,
                _mockGeneratedNamesService.Object);
        }

        #region GetGeneratedNamesLog Tests

        [Fact]
        public async Task GetGeneratedNamesLog_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var generatedNames = new List<GeneratedName>
            {
                new GeneratedName
                {
                    Id = 1,
                    ResourceName = "rg-test-001",
                    ResourceTypeName = "Resource Group",
                    CreatedOn = System.DateTime.Now,
                    User = "TestUser"
                },
                new GeneratedName
                {
                    Id = 2,
                    ResourceName = "vm-test-001",
                    ResourceTypeName = "Virtual Machine",
                    CreatedOn = System.DateTime.Now,
                    User = "TestUser"
                }
            };

            var serviceResponse = new ServiceResponse
            {
                Success = true,
                ResponseObject = generatedNames
            };

            _mockGeneratedNamesService
                .Setup(s => s.GetItemsAsync(It.IsAny<bool>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetGeneratedNamesLog();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(generatedNames);

            _mockGeneratedNamesService.Verify(
                s => s.GetItemsAsync(It.IsAny<bool>()),
                Times.Once,
                "GetItems should be called once");
        }

        [Fact]
        public async Task GetGeneratedNamesLog_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            var serviceResponse = new ServiceResponse
            {
                Success = false
            };

            _mockGeneratedNamesService
                .Setup(s => s.GetItemsAsync(It.IsAny<bool>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetGeneratedNamesLog();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetGeneratedNamesLog_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            _mockGeneratedNamesService
                .Setup(s => s.GetItemsAsync(It.IsAny<bool>()))
                .ThrowsAsync(new System.Exception("Database error"));

            _mockAdminLogService
                .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
                .ReturnsAsync(new ServiceResponse { Success = true });

            // Act
            var result = await _controller.GetGeneratedNamesLog();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            _mockAdminLogService.Verify(
                s => s.PostItemAsync(It.Is<AdminLogMessage>(
                    msg => msg.Title == "ERROR" && msg.Message == "Database error")),
                Times.Once,
                "error should be logged");
        }

        #endregion

        #region GetGeneratedName Tests

        [Fact]
        public async Task GetGeneratedName_ShouldReturnOk_WhenNameExists()
        {
            // Arrange
            int testId = 1;
            var generatedName = new GeneratedName
            {
                Id = testId,
                ResourceName = "rg-test-001",
                ResourceTypeName = "Resource Group",
                CreatedOn = System.DateTime.Now,
                User = "TestUser"
            };

            var serviceResponse = new ServiceResponse
            {
                Success = true,
                ResponseObject = generatedName
            };

            _mockGeneratedNamesService
                .Setup(s => s.GetItemAsync(testId))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetGeneratedName(testId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(generatedName);
        }

        [Fact]
        public async Task GetGeneratedName_ShouldReturnBadRequest_WhenNameNotFound()
        {
            // Arrange
            int testId = 999;
            var serviceResponse = new ServiceResponse
            {
                Success = false
            };

            _mockGeneratedNamesService
                .Setup(s => s.GetItemAsync(testId))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetGeneratedName(testId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetGeneratedName_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            int testId = 1;
            _mockGeneratedNamesService
                .Setup(s => s.GetItemAsync(testId))
                .ThrowsAsync(new System.Exception("Query error"));

            _mockAdminLogService
                .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
                .ReturnsAsync(new ServiceResponse { Success = true });

            // Act
            var result = await _controller.GetGeneratedName(testId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            _mockAdminLogService.Verify(
                s => s.PostItemAsync(It.Is<AdminLogMessage>(
                    msg => msg.Title == "ERROR")),
                Times.Once);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WithValidDependencies()
        {
            // Arrange & Act
            var controller = new AdminController(
                _mockAdminService.Object,
                _mockAdminLogService.Object,
                _mockGeneratedNamesService.Object);

            // Assert
            controller.Should().NotBeNull("controller should be created with valid dependencies");
        }

        #endregion

        #region Service Interaction Tests

        [Fact]
        public async Task GetGeneratedNamesLog_ShouldReturnEmptyList_WhenNoNamesGenerated()
        {
            // Arrange
            var emptyList = new List<GeneratedName>();
            var serviceResponse = new ServiceResponse
            {
                Success = true,
                ResponseObject = emptyList
            };

            _mockGeneratedNamesService
                .Setup(s => s.GetItemsAsync(It.IsAny<bool>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.GetGeneratedNamesLog();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedList = okResult!.Value as List<GeneratedName>;
            returnedList.Should().BeEmpty("no names have been generated");
        }

        #endregion
    }
}
