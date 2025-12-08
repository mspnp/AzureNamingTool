using AzureNamingTool.Controllers;
using AzureNamingTool.Models;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AzureNamingTool.UnitTests.Controllers
{
    /// <summary>
    /// Unit tests for ResourceNamingRequestsController (API v1.0)
    /// </summary>
    public class ResourceNamingRequestsControllerTests
    {
        private readonly Mock<IResourceNamingRequestService> _mockNamingService;
        private readonly Mock<IResourceTypeService> _mockResourceTypeService;
        private readonly Mock<IAdminLogService> _mockAdminLogService;
        private readonly ResourceNamingRequestsController _controller;

        public ResourceNamingRequestsControllerTests()
        {
            _mockNamingService = new Mock<IResourceNamingRequestService>();
            _mockResourceTypeService = new Mock<IResourceTypeService>();
            _mockAdminLogService = new Mock<IAdminLogService>();

            _controller = new ResourceNamingRequestsController(
                _mockNamingService.Object,
                _mockResourceTypeService.Object,
                _mockAdminLogService.Object);
        }

        #region RequestNameWithComponents Tests

        [Fact]
        public async Task RequestNameWithComponents_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            var request = new ResourceNameRequestWithComponents
            {
                ResourceType = new ResourceType { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true },
                ResourceEnvironment = new ResourceEnvironment { Id = 1, Name = "Production", ShortName = "prd" },
                ResourceLocation = new ResourceLocation { Id = 1, Name = "East US", ShortName = "eus", Enabled = true },
                ResourceOrg = new ResourceOrg { Id = 1, Name = "Marketing", ShortName = "mkt" },
                ResourceProjAppSvc = new ResourceProjAppSvc { Id = 1, Name = "Project1", ShortName = "proj1" },
                ResourceUnitDept = new ResourceUnitDept { Id = 1, Name = "Finance", ShortName = "fin" },
                ResourceFunction = new ResourceFunction { Id = 1, Name = "Data", ShortName = "data" }
            };

            var expectedResponse = new ResourceNameResponse
            {
                Success = true,
                ResourceName = "rg-mkt-proj1-eus-prd",
                Message = "Name generated successfully"
            };

            _mockNamingService
                .Setup(s => s.RequestNameWithComponentsAsync(It.IsAny<ResourceNameRequestWithComponents>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RequestNameWithComponents(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>("successful request should return 200 OK");
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);

            _mockNamingService.Verify(
                s => s.RequestNameWithComponentsAsync(It.Is<ResourceNameRequestWithComponents>(r => r.ResourceType.Resource == "rg")),
                Times.Once,
                "service should be called once");
        }

        [Fact]
        public async Task RequestNameWithComponents_ShouldReturnBadRequest_WhenRequestFails()
        {
            // Arrange
            var request = new ResourceNameRequestWithComponents
            {
                ResourceType = new ResourceType { Id = 1, Resource = "invalid", ShortName = "inv", Enabled = true }
            };

            var expectedResponse = new ResourceNameResponse
            {
                Success = false,
                Message = "Invalid resource type"
            };

            _mockNamingService
                .Setup(s => s.RequestNameWithComponentsAsync(It.IsAny<ResourceNameRequestWithComponents>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RequestNameWithComponents(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>("failed request should return 400 Bad Request");
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task RequestNameWithComponents_ShouldReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var request = new ResourceNameRequestWithComponents
            {
                ResourceType = new ResourceType { Id = 1, Resource = "rg", ShortName = "rg", Enabled = true }
            };

            _mockNamingService
                .Setup(s => s.RequestNameWithComponentsAsync(It.IsAny<ResourceNameRequestWithComponents>()))
                .ThrowsAsync(new System.Exception("Test exception"));

            _mockAdminLogService
                .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
                .ReturnsAsync(new ServiceResponse { Success = true });

            // Act
            var result = await _controller.RequestNameWithComponents(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>("exception should return 400 Bad Request");
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Test exception");

            _mockAdminLogService.Verify(
                s => s.PostItemAsync(It.Is<AdminLogMessage>(msg => msg.Title == "ERROR")),
                Times.Once,
                "error should be logged");
        }

        #endregion

        #region RequestName Tests

        [Fact]
        public async Task RequestName_ShouldReturnOk_WhenRequestIsSuccessful()
        {
            // Arrange
            var request = new ResourceNameRequest
            {
                ResourceType = "rg",
                ResourceEnvironment = "prd",
                ResourceLocation = "eastus"
            };

            var expectedResponse = new ResourceNameResponse
            {
                Success = true,
                ResourceName = "rg-corp-app1-eus-prd",
                Message = "Name generated successfully"
            };

            _mockNamingService
                .Setup(s => s.RequestNameAsync(It.IsAny<ResourceNameRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RequestName(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);

            _mockNamingService.Verify(
                s => s.RequestNameAsync(It.Is<ResourceNameRequest>(r => r.CreatedBy == "API")),
                Times.Once,
                "CreatedBy should be set to 'API'");
        }

        [Fact]
        public async Task RequestName_ShouldSetCreatedBy_ToAPI()
        {
            // Arrange
            var request = new ResourceNameRequest
            {
                ResourceType = "rg"
            };

            _mockNamingService
                .Setup(s => s.RequestNameAsync(It.IsAny<ResourceNameRequest>()))
                .ReturnsAsync(new ResourceNameResponse { Success = true });

            // Act
            await _controller.RequestName(request);

            // Assert
            request.CreatedBy.Should().Be("API", "controller should set CreatedBy");
        }

        [Fact]
        public async Task RequestName_ShouldReturnBadRequest_WhenRequestFails()
        {
            // Arrange
            var request = new ResourceNameRequest
            {
                ResourceType = "invalid"
            };

            var expectedResponse = new ResourceNameResponse
            {
                Success = false,
                Message = "Invalid resource type"
            };

            _mockNamingService
                .Setup(s => s.RequestNameAsync(It.IsAny<ResourceNameRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RequestName(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RequestName_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new ResourceNameRequest
            {
                ResourceType = "rg"
            };

            _mockNamingService
                .Setup(s => s.RequestNameAsync(It.IsAny<ResourceNameRequest>()))
                .ThrowsAsync(new System.Exception("Service error"));

            _mockAdminLogService
                .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
                .ReturnsAsync(new ServiceResponse { Success = true });

            // Act
            var result = await _controller.RequestName(request);

            // Assert
            _mockAdminLogService.Verify(
                s => s.PostItemAsync(It.Is<AdminLogMessage>(
                    msg => msg.Title == "ERROR" && msg.Message == "Service error")),
                Times.Once);
        }

        #endregion

        #region ValidateName Tests

        [Fact]
        public async Task ValidateName_ShouldReturnOk_WhenValidationSucceeds()
        {
            // Arrange
            var request = new ValidateNameRequest
            {
                ResourceType = "rg",
                Name = "rg-test-001"
            };

            var validateResponse = new ValidateNameResponse
            {
                Valid = true,
                Message = "Name is valid"
            };

            var serviceResponse = new ServiceResponse
            {
                Success = true,
                ResponseObject = validateResponse
            };

            _mockResourceTypeService
                .Setup(s => s.ValidateResourceTypeNameAsync(It.IsAny<ValidateNameRequest>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.ValidateName(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(validateResponse);
        }

        [Fact]
        public async Task ValidateName_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var request = new ValidateNameRequest
            {
                ResourceType = "rg",
                Name = "invalid@name"
            };

            var serviceResponse = new ServiceResponse
            {
                Success = false
            };

            _mockResourceTypeService
                .Setup(s => s.ValidateResourceTypeNameAsync(It.IsAny<ValidateNameRequest>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.ValidateName(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ValidateName_ShouldReturnBadRequest_WhenResponseObjectIsNull()
        {
            // Arrange
            var request = new ValidateNameRequest
            {
                ResourceType = "rg",
                Name = "rg-test-001"
            };

            var serviceResponse = new ServiceResponse
            {
                Success = true,
                ResponseObject = null
            };

            _mockResourceTypeService
                .Setup(s => s.ValidateResourceTypeNameAsync(It.IsAny<ValidateNameRequest>()))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _controller.ValidateName(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("There was a problem validating the name.");
        }

        [Fact]
        public async Task ValidateName_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new ValidateNameRequest
            {
                ResourceType = "rg",
                Name = "rg-test-001"
            };

            _mockResourceTypeService
                .Setup(s => s.ValidateResourceTypeNameAsync(It.IsAny<ValidateNameRequest>()))
                .ThrowsAsync(new System.Exception("Validation error"));

            _mockAdminLogService
                .Setup(s => s.PostItemAsync(It.IsAny<AdminLogMessage>()))
                .ReturnsAsync(new ServiceResponse { Success = true });

            // Act
            var result = await _controller.ValidateName(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            
            _mockAdminLogService.Verify(
                s => s.PostItemAsync(It.Is<AdminLogMessage>(
                    msg => msg.Title == "ERROR" && msg.Message == "Validation error")),
                Times.Once);
        }

        #endregion
    }
}
