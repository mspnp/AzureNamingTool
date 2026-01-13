using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services;
using AzureNamingTool.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

public class AdminUserServiceTests
{
    private readonly Mock<IConfigurationRepository<AdminUser>> _mockRepository;
    private readonly Mock<IAdminLogService> _mockAdminLogService;
    private readonly AdminUserService _service;

    public AdminUserServiceTests()
    {
        _mockRepository = new Mock<IConfigurationRepository<AdminUser>>();
        _mockAdminLogService = new Mock<IAdminLogService>();
        _service = new AdminUserService(_mockRepository.Object, _mockAdminLogService.Object);
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnOrderedAdminUsers()
    {
        // Arrange
        var users = new List<AdminUser>
        {
            new() { Id = 1, Name = "Charlie" },
            new() { Id = 2, Name = "Alice" },
            new() { Id = 3, Name = "Bob" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Success.Should().BeTrue();
        var returnedUsers = result.ResponseObject as List<AdminUser>;
        returnedUsers.Should().NotBeNull();
        returnedUsers!.Should().HaveCount(3);
        returnedUsers![0].Name.Should().Be("Alice");
        returnedUsers![1].Name.Should().Be("Bob");
        returnedUsers![2].Name.Should().Be("Charlie");
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnUserByName()
    {
        // Arrange
        var users = new List<AdminUser>
        {
            new() { Id = 1, Name = "Alice" },
            new() { Id = 2, Name = "Bob" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _service.GetItemAsync("Bob");

        // Assert
        result.Success.Should().BeTrue();
        var user = result.ResponseObject as AdminUser;
        user.Should().NotBeNull();
        user!.Name.Should().Be("Bob");
        user.Id.Should().Be(2);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnNullForNonExistentUser()
    {
        // Arrange
        var users = new List<AdminUser>
        {
            new() { Id = 1, Name = "Alice" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _service.GetItemAsync("NonExistent");

        // Assert
        result.Success.Should().BeTrue();
        // ResponseObject is dynamic - cast to AdminUser for assertion
        AdminUser? user = result.ResponseObject as AdminUser;
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetItemsAsync_ShouldHandleException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetItemsAsync();

        // Assert
        result.Success.Should().BeFalse();
        Exception? exception = result.ResponseObject as Exception;
        exception.Should().NotBeNull();
        exception!.Message.Should().Be("Database error");
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDependencies()
    {
        // Act
        var service = new AdminUserService(_mockRepository.Object, _mockAdminLogService.Object);

        // Assert
        service.Should().NotBeNull();
    }
}
