using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing generated names.
    /// </summary>
    public class GeneratedNamesService
    {
        /// <summary>
        /// Retrieves a list of items.
        /// </summary>
        /// <returns>Task&lt;ServiceResponse&gt; - The response containing the list of items or an error message.</returns>
        public static async Task<ServiceResponse> GetItems()
        {
            ServiceResponse serviceResponse = new();
            List<GeneratedName> lstGeneratedNames = [];
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<GeneratedName>();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.OrderByDescending(x => x.CreatedOn).ToList();
                    serviceResponse.Success = true;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Retrieves an item with the specified ID.
        /// </summary>
        /// <param name="id">int - The ID of the item to retrieve.</param>
        /// <returns>Task&lt;ServiceResponse&gt; - The response containing the retrieved item or an error message.</returns>
        public static async Task<ServiceResponse> GetItem(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<GeneratedName>();
                if (GeneralHelper.IsNotNull(items))
                {
                    var item = items.Find(x => x.Id == id);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        serviceResponse.ResponseObject = item;
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Generated Name not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Generated Names not found!";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Posts an item to the list of generated names.
        /// </summary>
        /// <param name="generatedName">GeneratedName - The generated name to be added.</param>
        /// <returns>Task&lt;ServiceResponse&gt; - The response indicating the success or failure of the operation.</returns>
        public static async Task<ServiceResponse> PostItem(GeneratedName generatedName)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get the previously generated names
                var items = await ConfigurationHelper.GetList<GeneratedName>();
                if (GeneralHelper.IsNotNull(items))
                {
                    if (items.Count > 0)
                    {
                        generatedName.Id = items.Max(x => x.Id) + 1;
                    }
                    else
                    {
                        generatedName.Id = 1;
                    }

                    items.Add(generatedName);

                    // Write items to file
                    await ConfigurationHelper.WriteList<GeneratedName>(items);

                    CacheHelper.InvalidateCacheObject("generatednames.json");

                    serviceResponse.Success = true;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Deletes an item with the specified ID.
        /// </summary>
        /// <param name="id">int - The ID of the item to delete.</param>
        /// <returns>Task&lt;ServiceResponse&gt; - The response indicating the success or failure of the operation.</returns>
        public static async Task<ServiceResponse> DeleteItem(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<GeneratedName>();
                if (GeneralHelper.IsNotNull(items))
                {
                    // Get the specified item
                    var item = items.Find(x => x.Id == id);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        // Remove the item from the collection
                        items.Remove(item);

                        // Write items to file
                        await ConfigurationHelper.WriteList<GeneratedName>(items);
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Generated Name not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Generated Name not found!";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function deletes all the items.
        /// </summary>
        /// <returns>ServiceResponse - The response indicating the success or failure of the operation.</returns>
        public static async Task<ServiceResponse> DeleteAllItems()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                List<GeneratedName> items = [];
                await ConfigurationHelper.WriteList<GeneratedName>(items);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage { Title = "Error", Message = ex.Message });
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function posts the configuration items.
        /// </summary>
        /// <param name="items">List of GeneratedName - The configuration items to be posted.</param>
        /// <returns>ServiceResponse - The response indicating the success or failure of the operation.</returns>
        public static async Task<ServiceResponse> PostConfig(List<GeneratedName> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var newitems = new List<GeneratedName>();
                int i = 1;

                // Determine new item id
                foreach (GeneratedName item in items)
                {
                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<GeneratedName>(newitems);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }
    }
}