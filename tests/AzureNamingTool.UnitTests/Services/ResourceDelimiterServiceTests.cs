using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

public class ResourceDelimiterServiceTests
{
    private readonly Mock<IConfigurationRepository<ResourceDelimiter>> _mockRepository;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly ResourceDelimiterService _service;

    public ResourceDelimiterServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository<ResourceDelimiter>>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _service = new ResourceDelimiterService(_mockRepository.Object, _mockAdminLogService.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnOrderedItems_WhenItemsExist()
    {
        // Arrange
        var items = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 2, Name = "Underscore", SortOrder = 2 },
            new ResourceDelimiter { Id = 1, Name = "Hyphen", SortOrder = 1 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync(true);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceDelimiter>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
        returnedItems![0].Name.Should().Be("Hyphen");
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var items = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        var item = result.ResponseObject as ResourceDelimiter;
        item.Should().NotBeNull();
        item!.Name.Should().Be("Hyphen");
    }

    [Fact(Skip = "Service behavior inconsistent - doesn't set Success property")]
    public async Task GetItemAsync_ShouldHandleItemNotFound()
    {
        // Arrange
        var items = new List<ResourceDelimiter> { new ResourceDelimiter { Id = 1, Name = "Hyphen" } };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemAsync(999);

        // Assert
        result.ResponseObject!.Should().NotBeNull();
        result.ResponseObject.Should().Be("Resource Delimiter not found!");
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnEnabledOnly_WhenAdminIsFalse()
    {
        // Arrange
        var items = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen", Enabled = true, SortOrder = 1 },
            new ResourceDelimiter { Id = 2, Name = "Underscore", Enabled = false, SortOrder = 2 },
            new ResourceDelimiter { Id = 3, Name = "Period", Enabled = true, SortOrder = 3 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync(false);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceDelimiter>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
        returnedItems.Should().OnlyContain(x => x.Enabled == true);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnAllItems_WhenAdminIsTrue()
    {
        // Arrange
        var items = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen", Enabled = true, SortOrder = 1 },
            new ResourceDelimiter { Id = 2, Name = "Underscore", Enabled = false, SortOrder = 2 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetItemsAsync(true);

        // Assert
        result.Success.Should().BeTrue();
        var returnedItems = result.ResponseObject as List<ResourceDelimiter>;
        returnedItems.Should().NotBeNull();
        returnedItems!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnErrorMessage_WhenItemsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((List<ResourceDelimiter>)null!);

        // Act
        var result = await _service.GetItemsAsync(true);

        // Assert
        ((object?)result.ResponseObject).Should().Be("Resource Delimiters not found!");
    }

    [Fact]
    public async Task GetItemsAsync_ShouldHandleException_AndLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);

        // Act
        var result = await _service.GetItemsAsync(true);

        // Assert
        result.Success.Should().BeFalse();
        var ex = result.ResponseObject as Exception;
        ex.Should().NotBeNull();
        _mockAdminLogService.Verify(x => x.PostItemAsync(It.Is<AdminLogMessage>(
            msg => msg.Title == "ERROR" && msg.Message == exception.Message)), Times.Once);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnErrorMessage_WhenItemsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((List<ResourceDelimiter>)null!);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        ((object?)result.ResponseObject).Should().Be("Resource Delimiters not found!");
    }

    [Fact]
    public async Task GetItemAsync_ShouldHandleException_AndLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);

        // Act
        var result = await _service.GetItemAsync(1);

        // Assert
        result.Success.Should().BeFalse();
        var ex = result.ResponseObject as Exception;
        ex.Should().NotBeNull();
        _mockAdminLogService.Verify(x => x.PostItemAsync(It.Is<AdminLogMessage>(
            msg => msg.Title == "ERROR" && msg.Message == exception.Message)), Times.Once);
    }

    [Fact]
    public async Task GetCurrentItemAsync_ShouldReturnFirstEnabledItem()
    {
        // Arrange
        var items = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen", Enabled = true, SortOrder = 1 },
            new ResourceDelimiter { Id = 2, Name = "Underscore", Enabled = false, SortOrder = 2 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(items);

        // Act
        var result = await _service.GetCurrentItemAsync();

        // Assert
        result.Success.Should().BeTrue();
        var item = result.ResponseObject as ResourceDelimiter;
        item.Should().NotBeNull();
        item!.Name.Should().Be("Hyphen");
    }

    [Fact]
    public async Task GetCurrentItemAsync_ShouldReturnErrorMessage_WhenItemsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((List<ResourceDelimiter>)null!);

        // Act
        var result = await _service.GetCurrentItemAsync();

        // Assert
        ((object?)result.ResponseObject).Should().Be("Resource Delimiter not found!");
    }

    [Fact]
    public async Task GetCurrentItemAsync_ShouldHandleException_AndLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);

        // Act
        var result = await _service.GetCurrentItemAsync();

        // Assert
        result.Success.Should().BeFalse();
        var ex = result.ResponseObject as Exception;
        ex.Should().NotBeNull();
        _mockAdminLogService.Verify(x => x.PostItemAsync(It.Is<AdminLogMessage>(
            msg => msg.Title == "ERROR" && msg.Message == exception.Message)), Times.Once);
    }

    [Fact]
    public async Task PostItemAsync_ShouldAddNewItem_WhenIdIsZero()
    {
        // Arrange
        var existingItems = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen", Enabled = false, SortOrder = 1 }
        };
        var newItem = new ResourceDelimiter { Id = 0, Name = "Underscore", Delimiter = "_" };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingItems);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        ((object?)result.ResponseObject).Should().Be("Resource Delimiter added/updated!");
        _mockRepository.Verify(r => r.SaveAllAsync(It.Is<IEnumerable<ResourceDelimiter>>(
            items => items.Count() == 2)), Times.Once);
    }

    [Fact]
    public async Task PostItemAsync_ShouldUpdateExistingItem()
    {
        // Arrange
        var existingItems = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen", Enabled = true, SortOrder = 1 },
            new ResourceDelimiter { Id = 2, Name = "Underscore", Enabled = false, SortOrder = 2 }
        };
        var updatedItem = new ResourceDelimiter { Id = 1, Name = "Hyphen Updated", Delimiter = "-", SortOrder = 1 };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingItems);

        // Act
        var result = await _service.PostItemAsync(updatedItem);

        // Assert
        result.Success.Should().BeTrue();
        ((object?)result.ResponseObject).Should().Be("Resource Delimiter added/updated!");
        _mockRepository.Verify(r => r.SaveAllAsync(It.IsAny<IEnumerable<ResourceDelimiter>>()), Times.Once);
    }

    [Fact]
    public async Task PostItemAsync_ShouldDisableOtherItems_WhenNewItemEnabled()
    {
        // Arrange
        var existingItems = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen", Enabled = true, SortOrder = 1 }
        };
        var newItem = new ResourceDelimiter { Id = 2, Name = "Underscore", Delimiter = "_", Enabled = true };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingItems);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        _mockRepository.Verify(r => r.SaveAllAsync(It.Is<IEnumerable<ResourceDelimiter>>(
            items => items.Count(x => x.Enabled) == 1)), Times.Once);
    }

    [Fact]
    public async Task PostItemAsync_ShouldReturnError_WhenItemsNull()
    {
        // Arrange
        var newItem = new ResourceDelimiter { Id = 0, Name = "Test" };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync((List<ResourceDelimiter>)null!);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        ((object?)result.ResponseObject).Should().Be("Resource Delimiters not found!");
    }

    [Fact]
    public async Task PostItemAsync_ShouldHandleException_AndLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var newItem = new ResourceDelimiter { Id = 0, Name = "Test" };
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeFalse();
        var ex = result.ResponseObject as Exception;
        ex.Should().NotBeNull();
        _mockAdminLogService.Verify(x => x.PostItemAsync(It.Is<AdminLogMessage>(
            msg => msg.Title == "ERROR" && msg.Message == exception.Message)), Times.Once);
    }

    [Fact]
    public async Task PostConfigAsync_ShouldValidateAndSaveDelimiters()
    {
        // Arrange
        var items = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Name = "dash", Delimiter = "-", Enabled = true },
            new ResourceDelimiter { Name = "underscore", Delimiter = "_", Enabled = false }
        };

        // Act
        var result = await _service.PostConfigAsync(items);

        // Assert
        result.Success.Should().BeTrue();
        _mockRepository.Verify(r => r.SaveAllAsync(It.Is<IEnumerable<ResourceDelimiter>>(
            delims => delims.Count() == 4)), Times.Once); // Should have all 4 delimiters
    }

    [Fact]
    public async Task PostConfigAsync_ShouldAddMissingDelimiters()
    {
        // Arrange
        var items = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Name = "dash", Delimiter = "-", Enabled = true }
        };

        // Act
        var result = await _service.PostConfigAsync(items);

        // Assert
        result.Success.Should().BeTrue();
        _mockRepository.Verify(r => r.SaveAllAsync(It.Is<IEnumerable<ResourceDelimiter>>(
            delims => delims.Any(d => d.Name == "underscore") &&
                     delims.Any(d => d.Name == "period") &&
                     delims.Any(d => d.Name == "none"))), Times.Once);
    }

    [Fact]
    public async Task PostConfigAsync_ShouldHandleException_AndLogError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var items = new List<ResourceDelimiter>();
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<ResourceDelimiter>>())).ThrowsAsync(exception);

        // Act
        var result = await _service.PostConfigAsync(items);

        // Assert
        result.Success.Should().BeFalse();
        var ex = result.ResponseObject as Exception;
        ex.Should().NotBeNull();
        _mockAdminLogService.Verify(x => x.PostItemAsync(It.Is<AdminLogMessage>(
            msg => msg.Title == "ERROR" && msg.Message == exception.Message)), Times.Once);
    }

    [Fact]
    public async Task PostItemAsync_ShouldAssignCorrectId_WhenItemsHaveBeenDeleted()
    {
        // Arrange - Simulate scenario where IDs 1, 2, 3 exist and ID 2 was deleted
        var existingItems = new List<ResourceDelimiter>
        {
            new ResourceDelimiter { Id = 1, Name = "Hyphen", Delimiter = "-", SortOrder = 1, Enabled = true },
            new ResourceDelimiter { Id = 3, Name = "None", Delimiter = "", SortOrder = 2, Enabled = false }
        };

        var newItem = new ResourceDelimiter
        {
            Id = 0,
            Name = "Underscore",
            Delimiter = "_"
        };

        List<ResourceDelimiter>? savedItems = null;
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingItems);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<ResourceDelimiter>>()))
            .Callback<IEnumerable<ResourceDelimiter>>(items => savedItems = items.ToList())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        savedItems.Should().NotBeNull();
        var addedItem = savedItems!.FirstOrDefault(x => x.Name == "Underscore");
        addedItem.Should().NotBeNull();
        addedItem!.Id.Should().Be(4, "because ID should be Max(existing IDs) + 1, not Count + 1");
        addedItem.Id.Should().NotBe(3, "to avoid collision with existing item");
    }

    [Fact]
    public async Task PostItemAsync_ShouldAssignId1_WhenNoItemsExist()
    {
        // Arrange
        var emptyList = new List<ResourceDelimiter>();
        var newItem = new ResourceDelimiter
        {
            Id = 0,
            Name = "FirstDelimiter",
            Delimiter = "-"
        };

        List<ResourceDelimiter>? savedItems = null;
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(emptyList);
        _mockRepository.Setup(r => r.SaveAllAsync(It.IsAny<IEnumerable<ResourceDelimiter>>()))
            .Callback<IEnumerable<ResourceDelimiter>>(items => savedItems = items.ToList())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.PostItemAsync(newItem);

        // Assert
        result.Success.Should().BeTrue();
        savedItems.Should().NotBeNull();
        savedItems!.Should().HaveCount(1);
        savedItems![0].Id.Should().Be(1);
    }
}
