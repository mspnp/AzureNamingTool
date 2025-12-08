using AzureNamingTool.Helpers;
using FluentAssertions;
using System;
using System.Runtime.Caching;
using System.Threading;
using Xunit;

namespace AzureNamingTool.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for CacheHelper
    /// </summary>
    public class CacheHelperTests : IDisposable
    {
        public CacheHelperTests()
        {
            // Clear cache before each test
            CacheHelper.ClearAllCache();
        }

        public void Dispose()
        {
            // Clear cache after each test
            CacheHelper.ClearAllCache();
        }

        [Fact]
        public void SetCacheObject_ShouldStoreValue_InCache()
        {
            // Arrange
            var cacheKey = "test_key_1";
            var cacheData = "test_value_1";

            // Act
            CacheHelper.SetCacheObject(cacheKey, cacheData);

            // Assert
            var result = CacheHelper.GetCacheObject(cacheKey);
            result.Should().NotBeNull();
            result.Should().Be(cacheData);
        }

        [Fact]
        public void GetCacheObject_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            // Arrange
            var nonExistentKey = "nonexistent_key";

            // Act
            var result = CacheHelper.GetCacheObject(nonExistentKey);

            // Assert
            result.Should().BeNull("cache should return null for non-existent keys");
        }

        [Fact]
        public void SetCacheObject_ShouldOverwriteExistingValue()
        {
            // Arrange
            var cacheKey = "overwrite_key";
            var initialValue = "initial_value";
            var newValue = "new_value";

            // Act
            CacheHelper.SetCacheObject(cacheKey, initialValue);
            CacheHelper.SetCacheObject(cacheKey, newValue);

            // Assert
            var result = CacheHelper.GetCacheObject(cacheKey);
            result.Should().Be(newValue, "cache should contain the most recent value");
        }

        [Fact]
        public void InvalidateCacheObject_ShouldRemoveValue_FromCache()
        {
            // Arrange
            var cacheKey = "invalidate_key";
            var cacheData = "data_to_invalidate";
            CacheHelper.SetCacheObject(cacheKey, cacheData);

            // Act
            CacheHelper.InvalidateCacheObject(cacheKey);

            // Assert
            var result = CacheHelper.GetCacheObject(cacheKey);
            result.Should().BeNull("cache should not contain invalidated keys");
        }

        [Fact]
        public void InvalidateCacheObject_ShouldHandleNonExistentKey_Gracefully()
        {
            // Arrange
            var nonExistentKey = "key_that_doesnt_exist";

            // Act & Assert - should not throw
            var act = () => CacheHelper.InvalidateCacheObject(nonExistentKey);
            act.Should().NotThrow("invalidating non-existent keys should be safe");
        }

        [Fact]
        public void ClearAllCache_ShouldRemoveAllCachedItems()
        {
            // Arrange
            CacheHelper.SetCacheObject("key1", "value1");
            CacheHelper.SetCacheObject("key2", "value2");
            CacheHelper.SetCacheObject("key3", "value3");

            // Act
            CacheHelper.ClearAllCache();

            // Assert
            CacheHelper.GetCacheObject("key1").Should().BeNull();
            CacheHelper.GetCacheObject("key2").Should().BeNull();
            CacheHelper.GetCacheObject("key3").Should().BeNull();
        }

        [Fact]
        public void SetCacheObject_ShouldHandleComplexObjects()
        {
            // Arrange
            var cacheKey = "complex_object";
            var complexObject = new { Name = "Test", Id = 123, Items = new[] { "A", "B", "C" } };

            // Act
            CacheHelper.SetCacheObject(cacheKey, complexObject);

            // Assert
            var result = CacheHelper.GetCacheObject(cacheKey);
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(complexObject);
        }

        [Fact]
        public void SetCacheObject_ShouldHandleNullValues()
        {
            // Arrange
            var cacheKey = "null_value_key";
            object? nullValue = null;

            // Act
            CacheHelper.SetCacheObject(cacheKey, nullValue!);

            // Assert
            var result = CacheHelper.GetCacheObject(cacheKey);
            result.Should().BeNull("cache should store null values");
        }

    [Fact]
    public void GetAllCacheData_ShouldReturnEmptyString_WhenCacheIsEmpty()
    {
        // Arrange
        CacheHelper.ClearAllCache();

        // Act
        var result = CacheHelper.GetAllCacheData();

        // Assert
        result.Should().BeEmpty();
    }        [Fact]
        public void GetAllCacheData_ShouldReturnCachedData_WhenItemsExist()
        {
            // Arrange
            CacheHelper.SetCacheObject("data_key_1", "data_value_1");
            CacheHelper.SetCacheObject("data_key_2", "data_value_2");

            // Act
            var result = CacheHelper.GetAllCacheData();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("data_key_1", "should contain first key");
            result.Should().Contain("data_value_1", "should contain first value");
            result.Should().Contain("data_key_2", "should contain second key");
            result.Should().Contain("data_value_2", "should contain second value");
        }

        [Fact]
        public void GetAllCacheData_ShouldReturnSortedKeys()
        {
            // Arrange
            CacheHelper.SetCacheObject("zebra", "value_z");
            CacheHelper.SetCacheObject("apple", "value_a");
            CacheHelper.SetCacheObject("middle", "value_m");

            // Act
            var result = CacheHelper.GetAllCacheData();

            // Assert
            result.Should().NotBeNullOrEmpty();
            var indexApple = result.IndexOf("apple");
            var indexMiddle = result.IndexOf("middle");
            var indexZebra = result.IndexOf("zebra");

            indexApple.Should().BeLessThan(indexMiddle, "keys should be sorted alphabetically");
            indexMiddle.Should().BeLessThan(indexZebra, "keys should be sorted alphabetically");
        }

        [Fact]
        public void SetCacheObject_ShouldStoreMultipleItems_Independently()
        {
            // Arrange & Act
            CacheHelper.SetCacheObject("key_a", "value_a");
            CacheHelper.SetCacheObject("key_b", 12345);
            CacheHelper.SetCacheObject("key_c", true);

            // Assert
            CacheHelper.GetCacheObject("key_a").Should().Be("value_a");
            CacheHelper.GetCacheObject("key_b").Should().Be(12345);
            CacheHelper.GetCacheObject("key_c").Should().Be(true);
        }

        [Fact]
        public void GetCacheObject_ShouldHandleEmptyKey()
        {
            // Arrange
            var emptyKey = "";

            // Act
            var result = CacheHelper.GetCacheObject(emptyKey);

            // Assert
            result.Should().BeNull("empty key should not exist in cache");
        }

        [Fact]
        public void SetCacheObject_ShouldHandleSpecialCharactersInKey()
        {
            // Arrange
            var specialKey = "key@#$%^&*()_+-=[]{}|;':\",./<>?";
            var value = "special_value";

            // Act
            CacheHelper.SetCacheObject(specialKey, value);

            // Assert
            var result = CacheHelper.GetCacheObject(specialKey);
            result.Should().Be(value, "should handle special characters in keys");
        }

        [Fact]
        public void ClearAllCache_ShouldWorkWhenCacheIsEmpty()
        {
            // Arrange
            CacheHelper.ClearAllCache();

            // Act & Assert - should not throw
            var act = () => CacheHelper.ClearAllCache();
            act.Should().NotThrow("clearing empty cache should be safe");
        }

        [Fact]
        public void InvalidateCacheObject_ShouldOnlyRemoveSpecificKey()
        {
            // Arrange
            CacheHelper.SetCacheObject("keep_key_1", "keep_value_1");
            CacheHelper.SetCacheObject("remove_key", "remove_value");
            CacheHelper.SetCacheObject("keep_key_2", "keep_value_2");

            // Act
            CacheHelper.InvalidateCacheObject("remove_key");

            // Assert
            CacheHelper.GetCacheObject("keep_key_1").Should().Be("keep_value_1");
            CacheHelper.GetCacheObject("remove_key").Should().BeNull();
            CacheHelper.GetCacheObject("keep_key_2").Should().Be("keep_value_2");
        }

        [Fact]
        public void SetCacheObject_ShouldHandleLargeStrings()
        {
            // Arrange
            var cacheKey = "large_string_key";
            var largeString = new string('X', 10000); // 10KB string

            // Act
            CacheHelper.SetCacheObject(cacheKey, largeString);

            // Assert
            var result = CacheHelper.GetCacheObject(cacheKey);
            result.Should().Be(largeString);
        }

        [Fact]
        public void GetAllCacheData_ShouldFormatOutput_WithHTML()
        {
            // Arrange
            CacheHelper.SetCacheObject("html_test_key", "html_test_value");

            // Act
            var result = CacheHelper.GetAllCacheData();

            // Assert
            result.Should().Contain("<p>", "should contain HTML paragraph tags");
            result.Should().Contain("fw-bold", "should contain CSS class");
            result.Should().Contain("alert alert-secondary", "should contain alert styling");
        }
    }
}
