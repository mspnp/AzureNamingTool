using System;
using System.Linq;
using System.Reflection;
using AzureNamingTool.UiTests;
using AzureNamingTool.UiTests.Client;
using AzureNamingTool.UiTests.Contexts;
using LightBDD.Core.Configuration;
using LightBDD.Extensions.DependencyInjection;
using LightBDD.XUnit2;
using Microsoft.Extensions.DependencyInjection;

[assembly: ConfiguredLightBddScope]

namespace AzureNamingTool.UiTests;

public class ConfiguredLightBddScopeAttribute : LightBddScopeAttribute
{
    protected override void OnConfigure(LightBddConfiguration configuration)
    {
        try
        {
            configuration
                .DependencyContainerConfiguration()
                .UseContainer(ConfigureDi(), true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private ServiceProvider ConfigureDi()
    {
        var services = new ServiceCollection();

        services.AddSingleton<AzureNamingToolApplication>();
        
        services.AddSingleton(provider =>
        {
            var application = provider.GetRequiredService<AzureNamingToolApplication>();
            var address = new HostAddressDetails() {Address = application.ServerAddress};
            return address;
        });

        var derivedTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is {IsClass: true, IsAbstract: false} && t.IsSubclassOf(typeof(ContextBase)));

        foreach (var derivedType in derivedTypes)
        {
            services.AddScoped(derivedType);
        }

        return services.BuildServiceProvider();
    }
}