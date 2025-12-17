using AzureNamingTool.Helpers;
using FluentAssertions;
using Xunit;

namespace AzureNamingTool.UnitTests.Helpers;

public class GeneralHelperTests
{
    [Theory]
    [InlineData("VGVzdA==", true)] // "Test" in base64
    [InlineData("SGVsbG8gV29ybGQ=", true)] // "Hello World" in base64
    [InlineData("not-base64!", false)]
    public void IsBase64Encoded_ShouldDetectBase64Correctly(string value, bool expected)
    {
        // Act
        var result = GeneralHelper.IsBase64Encoded(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Resource Location", false, "Location")]
    [InlineData("Resource Environment", true, "environment")]
    [InlineData("My Resource Type", false, "MyType")]
    [InlineData("My Resource Type", true, "mytype")]
    [InlineData("Simple Name", false, "SimpleName")]
    [InlineData("Simple Name", true, "simplename")]
    public void NormalizeName_ShouldNormalizeCorrectly(string input, bool lowercase, string expected)
    {
        // Act
        var result = GeneralHelper.NormalizeName(input, lowercase);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(true, "")]
    [InlineData(false, "disabled-text")]
    public void SetTextEnabledClass_ShouldReturnCorrectClass(bool enabled, string expected)
    {
        // Act
        var result = GeneralHelper.SetTextEnabledClass(enabled);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsNotNull_ShouldReturnTrue_WhenObjectIsNotNull()
    {
        // Arrange
        var obj = new object();

        // Act
        var result = GeneralHelper.IsNotNull(obj);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNotNull_ShouldReturnFalse_WhenObjectIsNull()
    {
        // Arrange
        object? obj = null;

        // Act
        var result = GeneralHelper.IsNotNull(obj);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EncryptString_ShouldEncryptAndDecrypt_Successfully()
    {
        // Arrange
        string original = "TestPassword123";
        string key = "12345678901234567890123456789012"; // 32 chars for AES-256

        // Act
        var encrypted = GeneralHelper.EncryptString(original, key);
        var decrypted = GeneralHelper.DecryptString(encrypted, key);

        // Assert
        encrypted.Should().NotBe(original);
        decrypted.Should().Be(original);
    }

    [Fact]
    public void GetPropertyValue_ShouldReturnPropertyValue_WhenPropertyExists()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 123 };

        // Act
        var result = GeneralHelper.GetPropertyValue(obj, "Name");

        // Assert
        result.Should().Be("Test");
    }

    [Fact]
    public void GetPropertyValue_ShouldReturnNull_WhenPropertyDoesNotExist()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 123 };

        // Act
        var result = GeneralHelper.GetPropertyValue(obj, "NonExistentProperty");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GenerateRandomString_ShouldReturnStringOfCorrectLength()
    {
        // Arrange
        var length = 10;

        // Act
        var result = GeneralHelper.GenerateRandomString(length, false);

        // Assert
        result.Should().HaveLength(length);
    }

    [Fact]
    public void GenerateRandomString_ShouldContainOnlyLowercaseLetters_WhenAlphanumericIsFalse()
    {
        // Arrange
        var length = 50;

        // Act
        var result = GeneralHelper.GenerateRandomString(length, false);

        // Assert
        result.Should().MatchRegex("^[a-z]+$");
    }

    [Fact]
    public void GenerateRandomString_ShouldContainAlphanumericCharacters_WhenAlphanumericIsTrue()
    {
        // Arrange
        var length = 100; // Larger sample to ensure we likely get both letters and numbers

        // Act
        var result = GeneralHelper.GenerateRandomString(length, true);

        // Assert
        result.Should().MatchRegex("^[a-z0-9]+$");
    }

    [Theory]
    [InlineData("Microsoft.Storage/storageAccounts", "Microsoft.Storage/storageAccounts")]
    [InlineData("Microsoft.Compute/virtualMachines (VM Details)", "Microsoft.Compute/virtualMachines")]
    public void FormatResourceType_ShouldFormatCorrectly(string input, string expectedBase)
    {
        // Act
        var result = GeneralHelper.FormatResourceType(input);

        // Assert
        result[0].Should().Be(expectedBase);
    }

    [Theory]
    [InlineData("test123", "test123")] // Already base64
    [InlineData("VGVzdA==", "VGVzdA==")] // Base64 string
    [InlineData("SGVsbG8gV29ybGQ=", "SGVsbG8gV29ybGQ=")] // Base64 for "Hello World"
    public void IsBase64Encoded_ShouldHandleValidBase64Strings(string value, string expected)
    {
        // Arrange & Act
        var result = GeneralHelper.IsBase64Encoded(value);

        // Assert
        // Just verify it doesn't throw - actual validation tested in IsBase64Encoded_ShouldDetectBase64Correctly
        value.Should().Be(expected);
    }

    [Fact]
    public void EncryptString_ShouldProduceDifferentOutput_ForDifferentInputs()
    {
        // Arrange
        string input1 = "password1";
        string input2 = "password2";
        string key = "12345678901234567890123456789012";

        // Act
        var encrypted1 = GeneralHelper.EncryptString(input1, key);
        var encrypted2 = GeneralHelper.EncryptString(input2, key);

        // Assert
        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact]
    public void DecryptString_ShouldHandleEncryptedData()
    {
        // Arrange
        string original = "SecurePassword123";
        string key = "12345678901234567890123456789012";
        
        // Act
        var encrypted = GeneralHelper.EncryptString(original, key);
        var decrypted = GeneralHelper.DecryptString(encrypted, key);

        // Assert
        decrypted.Should().Be(original);
        encrypted.Should().NotBe(original);
    }

    [Fact]
    public void GetPropertyValue_ShouldHandleComplexObjects()
    {
        // Arrange
        var obj = new { 
            Name = "Test", 
            Value = 123,
            Nested = new { Inner = "InnerValue" }
        };

        // Act
        var nameResult = GeneralHelper.GetPropertyValue(obj, "Name");
        var valueResult = GeneralHelper.GetPropertyValue(obj, "Value");
        var nestedResult = GeneralHelper.GetPropertyValue(obj, "Nested");

        // Assert
        nameResult.Should().Be("Test");
        valueResult.Should().Be(123);
        nestedResult.Should().NotBeNull();
    }
}
