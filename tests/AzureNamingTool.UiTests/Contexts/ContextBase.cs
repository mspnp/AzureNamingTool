using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AzureNamingTool.UiTests.Contexts;

public abstract class ContextBase : PageTest, IDisposable
{
    private readonly HostAddressDetails _addressDetails;
    private IPage? _page;

    protected ContextBase(HostAddressDetails addressDetails)
    {
        _addressDetails = addressDetails;
    }

    public void Dispose()
    {
        _page?.CloseAsync();
    }
    
    public async Task The_browser_is_ready()
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        _page = await browser.NewPageAsync();
    }

    protected async Task The_route_is_requested(string route)
    {
        await _page?.GotoAsync(_addressDetails.Address + "/" + route);
    }

    public async Task The_expected_content_should_be_found(string locator, string content)
    {
        await Expect(_page.Locator(locator)).ToHaveTextAsync(content);
    }
    
    public async Task The_expected_content_should_be_found(string content)
    {
        await Expect(_page
                .GetByText(new Regex(content, RegexOptions.IgnoreCase)))
            .ToBeVisibleAsync();
    }
}