using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Components.Pages;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing the Admin log.
    /// </summary>
    public class AdminLogService
    {
        /// <summary>
        /// This function retrieves the items from the Admin log.
        /// </summary>
        /// <returns>ServiceResponse - The response containing the retrieved items.</returns>
        public static async Task<ServiceResponse> GetItems()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminLogMessage>();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.OrderByDescending(x => x.CreatedOn).ToList();
                    serviceResponse.Success = true;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage { Title = "Error", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function posts an AdminLogMessage item to the Admin log.
        /// </summary>
        /// <param name="adminlogMessage">The AdminLogMessage item to be posted.</param>
        public static async void PostItem(AdminLogMessage adminlogMessage)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Log the created name
                var items = await ConfigurationHelper.GetList<AdminLogMessage>();
                if (GeneralHelper.IsNotNull(items))
                {
                    if (items.Count > 0)
                    {
                        adminlogMessage.Id = items.Max(x => x.Id) + 1;
                    }
                    items.Add(adminlogMessage);
                    // Write items to file
                    await ConfigurationHelper.WriteList<AdminLogMessage>(items);
                }
            }
            catch (Exception)
            {
                // No exception is logged due to this function being the function that would complete the action.
            }
        }

        /// <summary>
        /// This function deletes all items from the Admin log.
        /// </summary>
        /// <returns>ServiceResponse - The response indicating the success or failure of the operation.</returns>
        public static async Task<ServiceResponse> DeleteAllItems()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                List<AdminLogMessage> lstAdminLogMessages = [];
                await ConfigurationHelper.WriteList<AdminLogMessage>(lstAdminLogMessages);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function posts the configuration items to the Admin log.
        /// </summary>
        /// <param name="items">The list of AdminLogMessage items to be posted.</param>
        /// <returns>ServiceResponse - The response indicating the success or failure of the operation.</returns>
        public static async Task<ServiceResponse> PostConfig(List<AdminLogMessage> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var newitems = new List<AdminLogMessage>();
                int i = 1;

                // Determine new item id
                foreach (AdminLogMessage item in items)
                {
                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<AdminLogMessage>(newitems);
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
