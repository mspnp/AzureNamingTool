using AzureNamingTool.Helpers;
using FluentAssertions;
using Xunit;

namespace AzureNamingTool.UnitTests;

public class GeneralHelperTests
{
    [Fact]
    public void NormalizeName_RemovesResourceFromName_WhenInputContains()
    {
        const string input = "ResourceOrg";

        var result = GeneralHelper.NormalizeName(input, true);

        result.Should().Be("org");
    }
    
    [Theory]
    [InlineData(true, "")]
    [InlineData(false, "disabled-text")]
    public void SetTextEnabledClass_ReturnsExpectedClass(bool status, string expected)
    {
        var result = GeneralHelper.SetTextEnabledClass(status);

        result.Should().Be(expected);
    }
    
    [Fact]
    public void FormatResourceType_SplitsInputIntoArray_WhenInputContains()
    {
        const string input = "Web/sites - Static Web App (stapp)";

        var result = GeneralHelper.FormatResourceType(input);

        result.Length.Should().Be(4);
        result[0].Should().Be("Web/sites - Static Web App");
        result[1].Should().Be("Web/sites");
        result[2].Should().Be("stapp");
        result[3].Should().Be("Static Web App");
    }
}