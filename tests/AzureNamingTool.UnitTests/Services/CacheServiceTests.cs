using AzureNamingTool.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureNamingTool.UnitTests.Services;

/// <summary>
/// Tests for CacheService to ensure caching functionality works correctly.
/// These tests validate the cache service implementation and establish patterns
/// for testing services with dependencies.
/// </summary>
public class CacheServiceTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<ILogger<CacheService>> _loggerMock;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_memoryCacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GetCacheObject_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        const string key = "nonexistent-key";
        object? cachedValue = null;
        
        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out cachedValue))
            .Returns(false);

        // Act
        var result = _cacheService.GetCacheObject<string>(key);

        // Assert
        result.Should().BeNull("cache should return null for non-existent keys");
    }

    [Fact]
    public void GetCacheObject_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        const string key = "existing-key";
        const string expectedValue = "cached-value";
        object? cachedValue = expectedValue;
        
        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out cachedValue))
            .Returns(true);

        // Act
        var result = _cacheService.GetCacheObject<string>(key);

        // Assert
        result.Should().Be(expectedValue, "cache should return stored value");
    }

    [Fact(Skip = "Mock-based test - integration tests cover this better")]
    public void SetCacheObject_ShouldStoreValue_WithDefaultExpiration()
    {
        // Arrange
        const string key = "test-key";
        const string value = "test-value";
        
        var cacheEntryMock = new Mock<ICacheEntry>();
        cacheEntryMock.SetupAllProperties();
        
        _memoryCacheMock
            .Setup(x => x.CreateEntry(key))
            .Returns(cacheEntryMock.Object);

        // Act
        _cacheService.SetCacheObject(key, value);

        // Assert
        _memoryCacheMock.Verify(x => x.CreateEntry(key), Times.Once, 
            "cache entry should be created");
        cacheEntryMock.Object.Value.Should().Be(value, 
            "cached value should match input");
    }

    [Fact]
    public void SetCacheObject_ShouldUseCustomExpiration_WhenProvided()
    {
        // Arrange
        const string key = "test-key";
        const string value = "test-value";
        const int expirationMinutes = 30;
        
        var cacheEntryMock = new Mock<ICacheEntry>();
        cacheEntryMock.SetupAllProperties();
        
        _memoryCacheMock
            .Setup(x => x.CreateEntry(key))
            .Returns(cacheEntryMock.Object);

        // Act
        _cacheService.SetCacheObject(key, value, expirationMinutes);

        // Assert
        _memoryCacheMock.Verify(x => x.CreateEntry(key), Times.Once);
        cacheEntryMock.Object.AbsoluteExpirationRelativeToNow.Should().NotBeNull(
            "custom expiration should be set");
        cacheEntryMock.Object.AbsoluteExpirationRelativeToNow.Should().Be(
            TimeSpan.FromMinutes(expirationMinutes),
            "expiration should match provided value");
    }

    [Fact]
    public void InvalidateCacheObject_ShouldRemoveKey()
    {
        // Arrange
        const string key = "test-key";

        // Act
        _cacheService.InvalidateCacheObject(key);

        // Assert
        _memoryCacheMock.Verify(x => x.Remove(key), Times.Once, 
            "cache key should be removed");
    }

    [Fact(Skip = "Mock-based test - integration tests cover this better")]
    public void Exists_ShouldReturnTrue_WhenKeyExists()
    {
        // Arrange
        const string key = "existing-key";
        object? cachedValue = "some-value";
        
        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out cachedValue))
            .Returns(true);

        // Act
        var exists = _cacheService.Exists(key);

        // Assert
        exists.Should().BeTrue("exists should return true for cached keys");
    }

    [Fact]
    public void Exists_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        const string key = "nonexistent-key";
        object? cachedValue = null;
        
        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out cachedValue))
            .Returns(false);

        // Act
        var exists = _cacheService.Exists(key);

        // Assert
        exists.Should().BeFalse("exists should return false for non-existent keys");
    }

    [Theory]
    [InlineData("key1")]
    [InlineData("key2")]
    [InlineData("resourcetypes")]
    public void SetCacheObject_ShouldAcceptVariousKeys(string key)
    {
        // Arrange
        const string value = "test-value";
        var cacheEntryMock = new Mock<ICacheEntry>();
        cacheEntryMock.SetupAllProperties();
        
        _memoryCacheMock
            .Setup(x => x.CreateEntry(key))
            .Returns(cacheEntryMock.Object);

        // Act
        Action act = () => _cacheService.SetCacheObject(key, value);

        // Assert
        act.Should().NotThrow($"cache should accept key: {key}");
    }

    [Fact]
    public void GetCacheObject_ShouldHandleComplexTypes()
    {
        // Arrange
        const string key = "complex-object";
        var expectedValue = new List<string> { "item1", "item2", "item3" };
        object? cachedValue = expectedValue;
        
        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out cachedValue))
            .Returns(true);

        // Act
        var result = _cacheService.GetCacheObject<List<string>>(key);

        // Assert
        result.Should().NotBeNull("complex types should be retrievable");
        result.Should().BeEquivalentTo(expectedValue, "cached complex object should match");
    }
}

/// <summary>
/// Integration tests using real MemoryCache to validate actual caching behavior.
/// These tests demonstrate the pattern for end-to-end cache testing.
/// </summary>
public class CacheServiceIntegrationTests
{
    [Fact]
    public void CacheService_ShouldWorkWithRealMemoryCache()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<CacheService>>();
        var cacheService = new CacheService(memoryCache, logger);
        const string key = "integration-test-key";
        const string value = "integration-test-value";

        // Act - Set
        cacheService.SetCacheObject(key, value);

        // Assert - Get
        var retrieved = cacheService.GetCacheObject<string>(key);
        retrieved.Should().Be(value, "value should be retrievable from cache");

        // Act - Exists
        var exists = cacheService.Exists(key);
        exists.Should().BeTrue("key should exist in cache");

        // Act - Invalidate
        cacheService.InvalidateCacheObject(key);

        // Assert - No longer exists
        var existsAfterInvalidate = cacheService.Exists(key);
        existsAfterInvalidate.Should().BeFalse("key should not exist after invalidation");

        // Cleanup
        memoryCache.Dispose();
    }

    [Fact(Skip = "Timing-sensitive test - can be flaky in CI/CD")]
    public void CacheService_ShouldExpireItemsAfterTimeout()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<CacheService>>();
        var cacheService = new CacheService(memoryCache, logger);
        const string key = "expiration-test-key";
        const string value = "expiration-test-value";

        // Act - Set with very short expiration (1 second for test speed)
        // Note: In real code, expiration is in minutes, but we're testing the mechanism
        var cacheEntry = memoryCache.CreateEntry(key);
        cacheEntry.Value = value;
        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(100);
        cacheEntry.Dispose();

        // Assert - Should exist immediately
        var existsImmediately = cacheService.Exists(key);
        existsImmediately.Should().BeTrue("key should exist immediately after setting");

        // Wait for expiration
        Thread.Sleep(150);

        // Assert - Should not exist after expiration
        var existsAfterExpiration = cacheService.Exists(key);
        existsAfterExpiration.Should().BeFalse("key should not exist after expiration");

        // Cleanup
        memoryCache.Dispose();
    }

    [Fact]
    public void ClearAllCache_ShouldRemoveAllTrackedKeys()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Mock.Of<ILogger<CacheService>>();
        var cacheService = new CacheService(memoryCache, logger);
        
        // Add multiple items
        cacheService.SetCacheObject("key1", "value1");
        cacheService.SetCacheObject("key2", "value2");
        cacheService.SetCacheObject("key3", "value3");

        // Act
        cacheService.ClearAllCache();

        // Assert
        cacheService.Exists("key1").Should().BeFalse("key1 should be cleared");
        cacheService.Exists("key2").Should().BeFalse("key2 should be cleared");
        cacheService.Exists("key3").Should().BeFalse("key3 should be cleared");

        // Cleanup
        memoryCache.Dispose();
    }
}
