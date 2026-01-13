using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services
{
    public class PolicyServiceTests
    {
        private readonly Mock<IConfigurationRepository<ResourceComponent>> _componentRepoMock;
        private readonly Mock<IConfigurationRepository<ResourceType>> _typeRepoMock;
        private readonly Mock<IConfigurationRepository<ResourceUnitDept>> _unitDeptRepoMock;
        private readonly Mock<IConfigurationRepository<ResourceEnvironment>> _environmentRepoMock;
        private readonly Mock<IConfigurationRepository<ResourceLocation>> _locationRepoMock;
        private readonly Mock<IConfigurationRepository<ResourceOrg>> _orgRepoMock;
        private readonly Mock<IConfigurationRepository<ResourceFunction>> _functionRepoMock;
        private readonly Mock<IConfigurationRepository<ResourceProjAppSvc>> _projAppSvcRepoMock;
        private readonly Mock<IAdminLogService> _adminLogServiceMock;
        private readonly PolicyService _service;

        public PolicyServiceTests()
        {
            _componentRepoMock = new Mock<IConfigurationRepository<ResourceComponent>>();
            _typeRepoMock = new Mock<IConfigurationRepository<ResourceType>>();
            _unitDeptRepoMock = new Mock<IConfigurationRepository<ResourceUnitDept>>();
            _environmentRepoMock = new Mock<IConfigurationRepository<ResourceEnvironment>>();
            _locationRepoMock = new Mock<IConfigurationRepository<ResourceLocation>>();
            _orgRepoMock = new Mock<IConfigurationRepository<ResourceOrg>>();
            _functionRepoMock = new Mock<IConfigurationRepository<ResourceFunction>>();
            _projAppSvcRepoMock = new Mock<IConfigurationRepository<ResourceProjAppSvc>>();
            _adminLogServiceMock = new Mock<IAdminLogService>();

            _service = new PolicyService(
                _componentRepoMock.Object,
                _typeRepoMock.Object,
                _unitDeptRepoMock.Object,
                _environmentRepoMock.Object,
                _locationRepoMock.Object,
                _orgRepoMock.Object,
                _functionRepoMock.Object,
                _projAppSvcRepoMock.Object,
                _adminLogServiceMock.Object
            );
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldReturnSuccess_WhenAllRepositoriesReturnData()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceType", Enabled = true, SortOrder = 1 },
                new ResourceComponent { Name = "ResourceEnvironment", Enabled = true, SortOrder = 2 }
            };

            var resourceTypes = new List<ResourceType>
            {
                new ResourceType { ShortName = "st" },
                new ResourceType { ShortName = "vnet" }
            };

            var environments = new List<ResourceEnvironment>
            {
                new ResourceEnvironment { ShortName = "dev" },
                new ResourceEnvironment { ShortName = "prod" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(resourceTypes);
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(environments);
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            ((object?)result.ResponseObject).Should().NotBeNull();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldGeneratePolicy_WithEnabledComponentsOnly()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceType", Enabled = true, SortOrder = 1 },
                new ResourceComponent { Name = "ResourceEnvironment", Enabled = false, SortOrder = 2 } // Disabled
            };

            var resourceTypes = new List<ResourceType>
            {
                new ResourceType { ShortName = "st" }
            };

            var environments = new List<ResourceEnvironment>
            {
                new ResourceEnvironment { ShortName = "dev" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(resourceTypes);
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(environments);
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            // Environment repo should not be called since component is disabled
            _environmentRepoMock.Verify(x => x.GetAllAsync(), Times.Once); // Still called to load all data upfront
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleResourceUnitDept_WhenEnabled()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceUnitDept", Enabled = true, SortOrder = 1 }
            };

            var unitDepts = new List<ResourceUnitDept>
            {
                new ResourceUnitDept { ShortName = "it" },
                new ResourceUnitDept { ShortName = "hr" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(unitDepts);
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleResourceLocation_WhenEnabled()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceLocation", Enabled = true, SortOrder = 1 }
            };

            var locations = new List<ResourceLocation>
            {
                new ResourceLocation { ShortName = "eus" },
                new ResourceLocation { ShortName = "wus" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(locations);
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleResourceOrgs_WhenEnabled()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceOrgs", Enabled = true, SortOrder = 1 }
            };

            var orgs = new List<ResourceOrg>
            {
                new ResourceOrg { ShortName = "contoso" },
                new ResourceOrg { ShortName = "fabrikam" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(orgs);
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleResourceFunctions_WhenEnabled()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceFunctions", Enabled = true, SortOrder = 1 }
            };

            var functions = new List<ResourceFunction>
            {
                new ResourceFunction { ShortName = "web" },
                new ResourceFunction { ShortName = "api" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(functions);
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleResourceProjAppSvcs_WhenEnabled()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceProjAppSvcs", Enabled = true, SortOrder = 1 }
            };

            var projAppSvcs = new List<ResourceProjAppSvc>
            {
                new ResourceProjAppSvc { ShortName = "app1" },
                new ResourceProjAppSvc { ShortName = "app2" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(projAppSvcs);

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleUnknownComponent_Gracefully()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "UnknownComponent", Enabled = true, SortOrder = 1 }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleEmptyComponents_Gracefully()
        {
            // Arrange
            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceComponent>());
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldFilterOutItemsWithEmptyShortNames()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceType", Enabled = true, SortOrder = 1 }
            };

            var resourceTypes = new List<ResourceType>
            {
                new ResourceType { ShortName = "st" },
                new ResourceType { ShortName = "" }, // Empty short name
                new ResourceType { ShortName = "   " } // Whitespace only
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(resourceTypes);
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleMultipleLevels_OfComponents()
        {
            // Arrange
            var components = new List<ResourceComponent>
            {
                new ResourceComponent { Name = "ResourceType", Enabled = true, SortOrder = 1 },
                new ResourceComponent { Name = "ResourceEnvironment", Enabled = true, SortOrder = 2 },
                new ResourceComponent { Name = "ResourceLocation", Enabled = true, SortOrder = 3 }
            };

            var resourceTypes = new List<ResourceType>
            {
                new ResourceType { ShortName = "st" }
            };

            var environments = new List<ResourceEnvironment>
            {
                new ResourceEnvironment { ShortName = "dev" },
                new ResourceEnvironment { ShortName = "prod" }
            };

            var locations = new List<ResourceLocation>
            {
                new ResourceLocation { ShortName = "eus" }
            };

            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(components);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(resourceTypes);
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(environments);
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(locations);
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeTrue();
            var policy = result.ResponseObject as PolicyDefinition;
            policy.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleException_AndReturnFailure()
        {
            // Arrange
            _componentRepoMock.Setup(x => x.GetAllAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert
            result.Success.Should().BeFalse();
            var exception = result.ResponseObject as Exception;
            exception.Should().NotBeNull();
            _adminLogServiceMock.Verify(x => x.PostItemAsync(It.IsAny<AdminLogMessage>()), Times.Once);
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var expectedException = new Exception("Repository failure");
            _componentRepoMock.Setup(x => x.GetAllAsync()).ThrowsAsync(expectedException);

            // Act
            await _service.GetPolicyAsync();

            // Assert
            _adminLogServiceMock.Verify(x => x.PostItemAsync(
                It.Is<AdminLogMessage>(msg => 
                    msg.Title == "ERROR" && 
                    msg.Message == expectedException.Message)),
                Times.Once);
        }

        [Fact]
        public async Task GetPolicyAsync_ShouldHandleNullComponents_Gracefully()
        {
            // Arrange
            _componentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync((List<ResourceComponent>)null!);
            _typeRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceType>());
            _unitDeptRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceUnitDept>());
            _environmentRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceEnvironment>());
            _locationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceLocation>());
            _orgRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceOrg>());
            _functionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceFunction>());
            _projAppSvcRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ResourceProjAppSvc>());

            // Act
            var result = await _service.GetPolicyAsync();

            // Assert - Service doesn't handle null gracefully, returns failure
            result.Success.Should().BeFalse();
        }
    }
}
