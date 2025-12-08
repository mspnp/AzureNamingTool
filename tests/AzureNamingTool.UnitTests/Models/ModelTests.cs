using AzureNamingTool.Models;
using FluentAssertions;
using Xunit;

namespace AzureNamingTool.UnitTests.Models;

public class ServiceResponseTests
{
    [Fact]
    public void ServiceResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ServiceResponse();

        // Assert
        response.Success.Should().BeFalse();
        // ResponseObject is dynamic and null - can't use FluentAssertions on it directly
        Assert.Null(response.ResponseObject);
    }

    [Fact]
    public void ServiceResponse_ShouldSetSuccessProperty()
    {
        // Arrange
        var response = new ServiceResponse();

        // Act
        response.Success = true;

        // Assert
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void ServiceResponse_ShouldSetResponseObjectProperty()
    {
        // Arrange
        var response = new ServiceResponse();
        var data = "Test data";

        // Act
        response.ResponseObject = data;

        // Assert
        // Cast dynamic to string for assertion
        string actualData = response.ResponseObject;
        actualData.Should().Be(data);
    }
}

public class AdminLogMessageTests
{
    [Fact]
    public void AdminLogMessage_ShouldInitializeWithDefaultValues()
    {
        // Act
        var message = new AdminLogMessage();

        // Assert
        message.Id.Should().Be(0);
        message.Title.Should().BeEmpty();
        message.Message.Should().BeEmpty();
        message.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AdminLogMessage_ShouldSetProperties()
    {
        // Arrange
        var message = new AdminLogMessage();
        var now = DateTime.Now;

        // Act
        message.Id = 1;
        message.Title = "ERROR";
        message.Message = "Test error message";
        message.CreatedOn = now;

        // Assert
        message.Id.Should().Be(1);
        message.Title.Should().Be("ERROR");
        message.Message.Should().Be("Test error message");
        message.CreatedOn.Should().Be(now);
    }
}

public class ResourceNameResponseTests
{
    [Fact]
    public void ResourceNameResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ResourceNameResponse();

        // Assert
        response.Success.Should().BeFalse();
        response.ResourceName.Should().BeEmpty();
        response.Message.Should().BeEmpty();
    }

    [Fact]
    public void ResourceNameResponse_ShouldSetProperties()
    {
        // Arrange
        var response = new ResourceNameResponse();

        // Act
        response.Success = true;
        response.ResourceName = "st-test-dev-001";
        response.Message = "Name generated successfully";

        // Assert
        response.Success.Should().BeTrue();
        response.ResourceName.Should().Be("st-test-dev-001");
        response.Message.Should().Be("Name generated successfully");
    }
}

public class ValidateNameResponseTests
{
    [Fact]
    public void ValidateNameResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ValidateNameResponse();

        // Assert
        response.Valid.Should().BeTrue(); // Default is true according to the model
        response.Name.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void ValidateNameResponse_ShouldSetProperties()
    {
        // Arrange
        var response = new ValidateNameResponse();

        // Act
        response.Valid = true;
        response.Name = "st-test-dev-001";
        response.Message = "Name is valid";

        // Assert
        response.Valid.Should().BeTrue();
        response.Name.Should().Be("st-test-dev-001");
        response.Message.Should().Be("Name is valid");
    }
}

public class AdminUserTests
{
    [Fact]
    public void AdminUser_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new AdminUser();

        // Assert
        user.Id.Should().Be(0);
        user.Name.Should().BeEmpty();
    }

    [Fact]
    public void AdminUser_ShouldSetIdProperty()
    {
        // Arrange
        var user = new AdminUser();

        // Act
        user.Id = 42;

        // Assert
        user.Id.Should().Be(42);
    }

    [Fact]
    public void AdminUser_ShouldSetNameProperty()
    {
        // Arrange
        var user = new AdminUser();

        // Act
        user.Name = "TestUser";

        // Assert
        user.Name.Should().Be("TestUser");
    }
}

public class ResourceNameRequestTests
{
    [Fact]
    public void ResourceNameRequest_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new ResourceNameRequest();

        // Assert
        request.ResourceEnvironment.Should().BeEmpty();
        request.ResourceFunction.Should().BeEmpty();
        request.ResourceInstance.Should().BeEmpty();
        request.ResourceLocation.Should().BeEmpty();
        request.ResourceOrg.Should().BeEmpty();
        request.ResourceProjAppSvc.Should().BeEmpty();
        request.ResourceType.Should().BeEmpty();
        request.ResourceUnitDept.Should().BeEmpty();
    }

    [Fact]
    public void ResourceNameRequest_ShouldSetAllProperties()
    {
        // Arrange
        var request = new ResourceNameRequest();

        // Act
        request.ResourceEnvironment = "dev";
        request.ResourceFunction = "web";
        request.ResourceInstance = "001";
        request.ResourceLocation = "eastus";
        request.ResourceOrg = "contoso";
        request.ResourceProjAppSvc = "myapp";
        request.ResourceType = "Microsoft.Storage/storageAccounts";
        request.ResourceUnitDept = "marketing";

        // Assert
        request.ResourceEnvironment.Should().Be("dev");
        request.ResourceFunction.Should().Be("web");
        request.ResourceInstance.Should().Be("001");
        request.ResourceLocation.Should().Be("eastus");
        request.ResourceOrg.Should().Be("contoso");
        request.ResourceProjAppSvc.Should().Be("myapp");
        request.ResourceType.Should().Be("Microsoft.Storage/storageAccounts");
        request.ResourceUnitDept.Should().Be("marketing");
    }
}

public class CustomComponentTests
{
    [Fact]
    public void CustomComponent_ShouldInitializeWithDefaultValues()
    {
        // Act
        var component = new CustomComponent();

        // Assert
        component.Id.Should().Be(0);
        component.ParentComponent.Should().BeEmpty();
        component.Name.Should().BeEmpty();
        component.ShortName.Should().BeEmpty();
        component.SortOrder.Should().Be(0);
        component.MinLength.Should().Be("1");
    }

    [Fact]
    public void CustomComponent_ShouldSetShortNameProperty()
    {
        // Arrange
        var component = new CustomComponent();

        // Act
        component.ShortName = "tst";

        // Assert
        component.ShortName.Should().Be("tst");
    }

    [Fact]
    public void CustomComponent_ShouldSetAllProperties()
    {
        // Arrange
        var component = new CustomComponent();

        // Act
        component.Id = 1;
        component.ParentComponent = "ResourceEnvironment";
        component.Name = "Testing";
        component.ShortName = "tst";
        component.SortOrder = 5;
        component.MinLength = "2";

        // Assert
        component.Id.Should().Be(1);
        component.ParentComponent.Should().Be("ResourceEnvironment");
        component.Name.Should().Be("Testing");
        component.ShortName.Should().Be("tst");
        component.SortOrder.Should().Be(5);
        component.MinLength.Should().Be("2");
    }
}

public class GeneratedNameTests
{
    [Fact]
    public void GeneratedName_ShouldInitializeWithDefaultValues()
    {
        // Act
        var generatedName = new GeneratedName();

        // Assert
        generatedName.Id.Should().Be(0);
        generatedName.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        generatedName.ResourceName.Should().BeEmpty();
        generatedName.ResourceTypeName.Should().BeEmpty();
        generatedName.Components.Should().BeEmpty();
        generatedName.User.Should().Be("General");
        generatedName.Message.Should().BeNull();
    }

    [Fact]
    public void GeneratedName_ShouldSetAllProperties()
    {
        // Arrange
        var generatedName = new GeneratedName();
        var testDate = DateTime.Now.AddDays(-1);
        var components = new List<string[]> { new[] { "env", "dev" }, new[] { "location", "eastus" } };

        // Act
        generatedName.Id = 42;
        generatedName.CreatedOn = testDate;
        generatedName.ResourceName = "st-test-dev-001";
        generatedName.ResourceTypeName = "Storage Account";
        generatedName.Components = components;
        generatedName.User = "admin@contoso.com";
        generatedName.Message = "Test message";

        // Assert
        generatedName.Id.Should().Be(42);
        generatedName.CreatedOn.Should().Be(testDate);
        generatedName.ResourceName.Should().Be("st-test-dev-001");
        generatedName.ResourceTypeName.Should().Be("Storage Account");
        generatedName.Components.Should().HaveCount(2);
        generatedName.User.Should().Be("admin@contoso.com");
        generatedName.Message.Should().Be("Test message");
    }
}

public class ResourceTypeTests
{
    [Fact]
    public void ResourceType_ShouldInitializeWithDefaultValues()
    {
        // Act
        var resourceType = new ResourceType();

        // Assert
        resourceType.Id.Should().Be(0);
        resourceType.Resource.Should().BeEmpty();
        resourceType.Optional.Should().BeEmpty();
        resourceType.Exclude.Should().BeEmpty();
        resourceType.Property.Should().BeEmpty();
        resourceType.ShortName.Should().BeEmpty();
        resourceType.Enabled.Should().BeTrue();
        resourceType.ApplyDelimiter.Should().BeTrue();
    }

    [Fact]
    public void ResourceType_ShouldSetShortNameProperty()
    {
        // Arrange
        var resourceType = new ResourceType();

        // Act
        resourceType.ShortName = "st";

        // Assert
        resourceType.ShortName.Should().Be("st");
    }

    [Fact]
    public void ResourceType_ShouldSetAllProperties()
    {
        // Arrange
        var resourceType = new ResourceType();

        // Act
        resourceType.Id = 1;
        resourceType.Resource = "Microsoft.Storage/storageAccounts";
        resourceType.Optional = "ResourceOrg";
        resourceType.Exclude = "ResourceInstance";
        resourceType.Property = "Storage";
        resourceType.ShortName = "st";
        resourceType.Scope = "global";
        resourceType.LengthMin = "3";
        resourceType.LengthMax = "24";
        resourceType.ValidText = "Lowercase letters and numbers";
        resourceType.InvalidCharacters = "-_";
        resourceType.Regx = "^[a-z0-9]+$";
        resourceType.StaticValues = "none";
        resourceType.Enabled = true;
        resourceType.ApplyDelimiter = false;

        // Assert
        resourceType.Id.Should().Be(1);
        resourceType.Resource.Should().Be("Microsoft.Storage/storageAccounts");
        resourceType.Optional.Should().Be("ResourceOrg");
        resourceType.Exclude.Should().Be("ResourceInstance");
        resourceType.Property.Should().Be("Storage");
        resourceType.ShortName.Should().Be("st");
        resourceType.Scope.Should().Be("global");
        resourceType.LengthMin.Should().Be("3");
        resourceType.LengthMax.Should().Be("24");
        resourceType.Enabled.Should().BeTrue();
        resourceType.ApplyDelimiter.Should().BeFalse();
    }
}

public class ResourceComponentTests
{
    [Fact]
    public void ResourceComponent_ShouldInitializeWithDefaultValues()
    {
        // Act
        var component = new ResourceComponent();

        // Assert
        component.Id.Should().Be(0);
        component.Name.Should().BeEmpty();
        component.DisplayName.Should().BeEmpty();
        component.Enabled.Should().BeFalse();
        component.SortOrder.Should().Be(0);
        component.IsCustom.Should().BeFalse();
        component.IsFreeText.Should().BeFalse();
        component.MinLength.Should().Be("1");
        component.MaxLength.Should().Be("10");
    }

    [Fact]
    public void ResourceComponent_ShouldSetEnabledProperty()
    {
        // Arrange
        var component = new ResourceComponent();

        // Act
        component.Enabled = true;

        // Assert
        component.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ResourceComponent_ShouldSetAllProperties()
    {
        // Arrange
        var component = new ResourceComponent();

        // Act
        component.Id = 1;
        component.Name = "ResourceEnvironment";
        component.DisplayName = "Environment";
        component.Enabled = true;
        component.SortOrder = 5;
        component.IsCustom = false;
        component.IsFreeText = false;
        component.MinLength = "2";
        component.MaxLength = "5";

        // Assert
        component.Id.Should().Be(1);
        component.Name.Should().Be("ResourceEnvironment");
        component.DisplayName.Should().Be("Environment");
        component.Enabled.Should().BeTrue();
        component.SortOrder.Should().Be(5);
        component.IsCustom.Should().BeFalse();
        component.IsFreeText.Should().BeFalse();
        component.MinLength.Should().Be("2");
        component.MaxLength.Should().Be("5");
    }
}

public class ResourceEnvironmentTests
{
    [Fact]
    public void ResourceEnvironment_ShouldInitializeWithDefaultValues()
    {
        // Act
        var environment = new ResourceEnvironment();

        // Assert
        environment.Id.Should().Be(0);
        environment.Name.Should().BeEmpty();
        environment.ShortName.Should().BeEmpty();
        environment.SortOrder.Should().Be(0);
    }

    [Fact]
    public void ResourceEnvironment_ShouldSetShortNameProperty()
    {
        // Arrange
        var environment = new ResourceEnvironment();

        // Act
        environment.ShortName = "dev";

        // Assert
        environment.ShortName.Should().Be("dev");
    }

    [Fact]
    public void ResourceEnvironment_ShouldSetAllProperties()
    {
        // Arrange
        var environment = new ResourceEnvironment();

        // Act
        environment.Id = 1;
        environment.Name = "Development";
        environment.ShortName = "dev";
        environment.SortOrder = 1;

        // Assert
        environment.Id.Should().Be(1);
        environment.Name.Should().Be("Development");
        environment.ShortName.Should().Be("dev");
        environment.SortOrder.Should().Be(1);
    }
}

public class ResourceLocationTests
{
    [Fact]
    public void ResourceLocation_ShouldInitializeWithDefaultValues()
    {
        // Act
        var location = new ResourceLocation();

        // Assert
        location.Id.Should().Be(0);
        location.Name.Should().BeEmpty();
        location.ShortName.Should().BeEmpty();
        location.Enabled.Should().BeTrue(); // Default is true
    }

    [Fact]
    public void ResourceLocation_ShouldSetShortNameProperty()
    {
        // Arrange
        var location = new ResourceLocation();

        // Act
        location.ShortName = "eus";

        // Assert
        location.ShortName.Should().Be("eus");
    }

    [Fact]
    public void ResourceLocation_ShouldSetAllProperties()
    {
        // Arrange
        var location = new ResourceLocation();

        // Act
        location.Id = 1;
        location.Name = "East US";
        location.ShortName = "eus";
        location.Enabled = true;

        // Assert
        location.Id.Should().Be(1);
        location.Name.Should().Be("East US");
        location.ShortName.Should().Be("eus");
        location.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ResourceLocation_ShouldAllowDisabling()
    {
        // Arrange
        var location = new ResourceLocation { Enabled = false };

        // Assert
        location.Enabled.Should().BeFalse();
    }
}

public class ResourceDelimiterTests
{
    [Fact]
    public void ResourceDelimiter_ShouldInitializeWithDefaultValues()
    {
        // Act
        var delimiter = new ResourceDelimiter();

        // Assert
        delimiter.Id.Should().Be(0);
        delimiter.Name.Should().BeEmpty();
        delimiter.Delimiter.Should().BeEmpty();
        delimiter.Enabled.Should().BeTrue(); // Default is true
        delimiter.SortOrder.Should().Be(0);
    }

    [Fact]
    public void ResourceDelimiter_ShouldSetAllProperties()
    {
        // Arrange
        var delimiter = new ResourceDelimiter();

        // Act
        delimiter.Id = 1;
        delimiter.Name = "Dash";
        delimiter.Delimiter = "-";
        delimiter.Enabled = true;
        delimiter.SortOrder = 1;

        // Assert
        delimiter.Id.Should().Be(1);
        delimiter.Name.Should().Be("Dash");
        delimiter.Delimiter.Should().Be("-");
        delimiter.Enabled.Should().BeTrue();
        delimiter.SortOrder.Should().Be(1);
    }

    [Fact]
    public void ResourceDelimiter_ShouldSupportVariousDelimiters()
    {
        // Arrange & Act
        var dash = new ResourceDelimiter { Delimiter = "-" };
        var underscore = new ResourceDelimiter { Delimiter = "_" };
        var none = new ResourceDelimiter { Delimiter = "" };

        // Assert
        dash.Delimiter.Should().Be("-");
        underscore.Delimiter.Should().Be("_");
        none.Delimiter.Should().BeEmpty();
    }
}

public class ResourceOrgTests
{
    [Fact]
    public void ResourceOrg_ShouldInitializeWithDefaultValues()
    {
        // Act
        var resourceOrg = new ResourceOrg();

        // Assert
        resourceOrg.Id.Should().Be(0);
        resourceOrg.Name.Should().BeEmpty();
        resourceOrg.ShortName.Should().BeEmpty();
        resourceOrg.SortOrder.Should().Be(0);
    }

    [Fact]
    public void ResourceOrg_ShouldSetShortNameProperty()
    {
        // Arrange
        var resourceOrg = new ResourceOrg();

        // Act
        resourceOrg.ShortName = "org";

        // Assert
        resourceOrg.ShortName.Should().Be("org");
    }

    [Fact]
    public void ResourceOrg_ShouldSetAllProperties()
    {
        // Arrange
        var resourceOrg = new ResourceOrg();

        // Act
        resourceOrg.Id = 1;
        resourceOrg.Name = "Engineering";
        resourceOrg.ShortName = "eng";
        resourceOrg.SortOrder = 5;

        // Assert
        resourceOrg.Id.Should().Be(1);
        resourceOrg.Name.Should().Be("Engineering");
        resourceOrg.ShortName.Should().Be("eng");
        resourceOrg.SortOrder.Should().Be(5);
    }
}

public class ResourceFunctionTests
{
    [Fact]
    public void ResourceFunction_ShouldInitializeWithDefaultValues()
    {
        // Act
        var resourceFunction = new ResourceFunction();

        // Assert
        resourceFunction.Id.Should().Be(0);
        resourceFunction.Name.Should().BeEmpty();
        resourceFunction.ShortName.Should().BeEmpty();
        resourceFunction.SortOrder.Should().Be(0);
    }

    [Fact]
    public void ResourceFunction_ShouldSetShortNameProperty()
    {
        // Arrange
        var resourceFunction = new ResourceFunction();

        // Act
        resourceFunction.ShortName = "web";

        // Assert
        resourceFunction.ShortName.Should().Be("web");
    }

    [Fact]
    public void ResourceFunction_ShouldSetAllProperties()
    {
        // Arrange
        var resourceFunction = new ResourceFunction();

        // Act
        resourceFunction.Id = 1;
        resourceFunction.Name = "Web Application";
        resourceFunction.ShortName = "webapp";
        resourceFunction.SortOrder = 10;

        // Assert
        resourceFunction.Id.Should().Be(1);
        resourceFunction.Name.Should().Be("Web Application");
        resourceFunction.ShortName.Should().Be("webapp");
        resourceFunction.SortOrder.Should().Be(10);
    }
}

public class ResourceProjAppSvcTests
{
    [Fact]
    public void ResourceProjAppSvc_ShouldInitializeWithDefaultValues()
    {
        // Act
        var resourceProjAppSvc = new ResourceProjAppSvc();

        // Assert
        resourceProjAppSvc.Id.Should().Be(0);
        resourceProjAppSvc.Name.Should().BeEmpty();
        resourceProjAppSvc.ShortName.Should().BeEmpty();
        resourceProjAppSvc.SortOrder.Should().Be(0);
    }

    [Fact]
    public void ResourceProjAppSvc_ShouldSetShortNameProperty()
    {
        // Arrange
        var resourceProjAppSvc = new ResourceProjAppSvc();

        // Act
        resourceProjAppSvc.ShortName = "app";

        // Assert
        resourceProjAppSvc.ShortName.Should().Be("app");
    }

    [Fact]
    public void ResourceProjAppSvc_ShouldSetAllProperties()
    {
        // Arrange
        var resourceProjAppSvc = new ResourceProjAppSvc();

        // Act
        resourceProjAppSvc.Id = 1;
        resourceProjAppSvc.Name = "Customer Portal";
        resourceProjAppSvc.ShortName = "cust";
        resourceProjAppSvc.SortOrder = 15;

        // Assert
        resourceProjAppSvc.Id.Should().Be(1);
        resourceProjAppSvc.Name.Should().Be("Customer Portal");
        resourceProjAppSvc.ShortName.Should().Be("cust");
        resourceProjAppSvc.SortOrder.Should().Be(15);
    }
}

public class ResourceUnitDeptTests
{
    [Fact]
    public void ResourceUnitDept_ShouldInitializeWithDefaultValues()
    {
        // Act
        var resourceUnitDept = new ResourceUnitDept();

        // Assert
        resourceUnitDept.Id.Should().Be(0);
        resourceUnitDept.Name.Should().BeEmpty();
        resourceUnitDept.ShortName.Should().BeEmpty();
        resourceUnitDept.SortOrder.Should().Be(0);
    }

    [Fact]
    public void ResourceUnitDept_ShouldSetShortNameProperty()
    {
        // Arrange
        var resourceUnitDept = new ResourceUnitDept();

        // Act
        resourceUnitDept.ShortName = "dev";

        // Assert
        resourceUnitDept.ShortName.Should().Be("dev");
    }

    [Fact]
    public void ResourceUnitDept_ShouldSetAllProperties()
    {
        // Arrange
        var resourceUnitDept = new ResourceUnitDept();

        // Act
        resourceUnitDept.Id = 1;
        resourceUnitDept.Name = "Development";
        resourceUnitDept.ShortName = "dev";
        resourceUnitDept.SortOrder = 20;

        // Assert
        resourceUnitDept.Id.Should().Be(1);
        resourceUnitDept.Name.Should().Be("Development");
        resourceUnitDept.ShortName.Should().Be("dev");
        resourceUnitDept.SortOrder.Should().Be(20);
    }
}

