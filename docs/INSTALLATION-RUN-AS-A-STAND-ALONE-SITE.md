[Overview](/README.md) | [Installation](/docs/INSTALLATION.md) | [Updating](/docs/UPDATING.md) | [Using the API](/docs/USINGTHEAPI.md) | [Release Notes](/RELEASENOTES.md) | [Version History](/docs/VERSIONHISTORY.md) | [FAQ](/docs/FAQ.md) | [Contributors](/docs/CONTRIBUTORS.md)

# Run as a Stand-Alone Site
* [Choosing an Installation Option](/docs/INSTALLATION.md)
* [Overview]($overview)
* [Steps](#steps)

## Overview
The Azure Naming Tool can be installed as a stand-alone .NET Core application. The installation process will vary, depending on your hosting environment.

## Steps
1. Scroll up to the top, left corner of this page.
2. Click on the **AzureNamingTool** link to open the root of this repository.
3. Click the green **<>Code** button and select **Download ZIP**.
4. Open your Downloads folder using File Explorer.
5. Extract the contents of the ZIP archive.

> <br />**NOTE:**<br />
> Validate the project files extracted successfully and match the contents in the GitHub repository.<br /><br />

6. Build/Publish the .NET Core application to a deployment directory. [Publish an ASP.NET Core app to IIS](https://learn.microsoft.com/en-us/aspnet/core/tutorials/publish-to-iis?view=aspnetcore-6.0&tabs=visual-studio)
7. In your IIS/Apache environment, create a new .NET application with the source as the deployment directory.
