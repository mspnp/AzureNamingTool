[Overview](/README.md) | [Installation](/docs/INSTALLATION.md) | [Updating](/docs/UPDATING.md) | [Using the API](/docs/USINGTHEAPI.md) | [Release Notes](/RELEASENOTES.md) | [Version History](/docs/VERSIONHISTORY.md) | [FAQ](/docs/FAQ.md) | [Contributors](/docs/CONTRIBUTORS.md)

# Run as a Docker image with podman
* [Choosing an Installation Option](/docs/INSTALLATION.md)
* [Overview]($overview)
* [Steps](#steps)

## Overview
This process will allow you to deploy the Azure Naming Tool using Docker in your local environment.
## Steps
1. Scroll up to the top, left corner of this page.
2. Click on the **AzureNamingTool** link to open the root of this repository.
3. Click the green **<>Code** button and select **Download ZIP**.
4. Open your Downloads folder using File Explorer.
5. Extract the contents of the ZIP archive.

> <br />**NOTE:**<br />
> Validate the project files extracted successfully and match the contents in the GitHub repository.<br /><br />
6. Open a **Command Prompt**
7. Change the directory to the **AzNamingTool** folder. For example:

```cmd
cd .\Downloads\AzureNamingTool
```

8. Run the following **Docker command** to build the image:

```cmd
podman build -t azurenamingtool .
```

> <br />**NOTE:**<br />
> Ensure the '.' is included in the command<br /><br />
9. Run the following **Docker command** to create a new container and mount a new volume:

```cmd
podman run -d -p 8081:8081 --mount type=volume,source=azurenamingtoolvol,target=/app/settings -e "ASPNETCORE_URLS=http://+:8081" azurenamingtool:latest
```

> <br />**NOTES:**<br />  
> * Substitute 8081 for any port not in use on your machine
> * You may see warnings in the command prompt regarding DataProtection and keys. These indicate that the keys are not persisted and are only local to the container instances.<br /><br />
10. Access the site using the following URL: *http://localhost:8081*

> <br />**NOTE:**<br />
> Substitute 8081 for the port you used in the docker run command<br /><br />
