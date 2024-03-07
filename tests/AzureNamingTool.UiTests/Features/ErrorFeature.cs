using System.Threading.Tasks;
using AzureNamingTool.UiTests.Contexts;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;


namespace AzureNamingTool.UiTests.Features;

[FeatureDescription(
    """
    As an azure naming tool engineer
            I want errors to be handled gracefully
            So users understand when something has gone wrong in the system
    """)]
public class ErrorFeature : FeatureFixture
{
    [Scenario]
    public async Task ReturnsNotFoundView_ForUnknownRoute()
    {
        await Runner
            .WithContext<HomePageContext>()
            .AddAsyncSteps(
                given => given.The_browser_is_ready(),
                when => when.A_route_that_doesnt_exist_is_requested(),
                then => then.The_expected_content_should_be_found("Whoa, it looks like that page went and r-u-n-n-o-f-t! Try again!")
            ).RunAsync();
    }
}