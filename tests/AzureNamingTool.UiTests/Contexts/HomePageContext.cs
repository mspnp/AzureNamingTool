using System.Threading.Tasks;

namespace AzureNamingTool.UiTests.Contexts;

public class HomePageContext : ContextBase
{
    public HomePageContext(HostAddressDetails addressDetails) : base(addressDetails)
    {
    }

    public async Task Given_request_home_page()
    {
        await The_route_is_requested("home");
    }

    public async Task A_route_that_doesnt_exist_is_requested()
    {
        await The_route_is_requested("i-do-not-exist");
    }
}