using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Components.Pages;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services.Interfaces;
using System.Text.Json;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing the Admin log.
    /// </summary>
    public class AdminLogService : IAdminLogService
    {
        private readonly IConfigurationRepository<AdminLogMessage> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminLogService"/> class.
        /// </summary>
        /// <param name="repository">The configuration repository for admin log messages.</param>
        public AdminLogService(IConfigurationRepository<AdminLogMessage> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// This function retrieves the items from the Admin log.
        /// </summary>
        /// <returns>ServiceResponse - The response containing the retrieved items.</returns>
        public async Task<ServiceResponse> GetItemsAsync()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await _repository.GetAllAsync();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.OrderByDescending(x => x.CreatedOn).ToList();
                    serviceResponse.Success = true;
                }
            }
            catch (Exception ex)
            {
                // Avoid recursion - just log to console/diagnostics instead of PostItem
                System.Diagnostics.Debug.WriteLine($"AdminLogService.GetItems Error: {ex.Message}");
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Retrieves a specific administrative log message by ID.
        /// </summary>
        /// <param name="id">The ID of the log message.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public async Task<ServiceResponse> GetItemAsync(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item != null)
                {
                    serviceResponse.ResponseObject = item;
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Admin Log Message not found!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminLogService.GetItemAsync Error: {ex.Message}");
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function posts an AdminLogMessage item to the Admin log.
        /// </summary>
        /// <param name="adminlogMessage">The AdminLogMessage item to be posted.</param>
        public async Task<ServiceResponse> PostItemAsync(AdminLogMessage adminlogMessage)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Log the created name
                var items = (await _repository.GetAllAsync()).ToList();
                if (GeneralHelper.IsNotNull(items))
                {
                    if (items.Count > 0)
                    {
                        adminlogMessage.Id = items.Max(x => x.Id) + 1;
                    }
                    items.Add(adminlogMessage);
                    // Write items to file
                    await _repository.SaveAllAsync(items);
                    serviceResponse.Success = true;
                }
            }
            catch (Exception)
            {
                // No exception is logged due to this function being the function that would complete the action.
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Deletes an administrative log message by ID.
        /// </summary>
        /// <param name="id">The ID of the log message to delete.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public async Task<ServiceResponse> DeleteItemAsync(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                await _repository.DeleteAsync(id);
                serviceResponse.Success = true;
                serviceResponse.ResponseObject = "Admin Log Message deleted!";
                {
                    serviceResponse.ResponseObject = "Admin Log Message not found!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AdminLogService.DeleteItemAsync Error: {ex.Message}");
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function deletes all items from the Admin log.
        /// </summary>
        /// <returns>ServiceResponse - The response indicating the success or failure of the operation.</returns>
        public async Task<ServiceResponse> DeleteAllItemsAsync()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                List<AdminLogMessage> lstAdminLogMessages = [];
                await _repository.SaveAllAsync(lstAdminLogMessages);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                // Avoid recursion - log to diagnostics
                System.Diagnostics.Debug.WriteLine($"AdminLogService.DeleteAllItems Error: {ex.Message}");
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// This function posts the configuration items to the Admin log.
        /// </summary>
        /// <param name="items">The list of AdminLogMessage items to be posted.</param>
        /// <returns>ServiceResponse - The response indicating the success or failure of the operation.</returns>
        public async Task<ServiceResponse> PostConfigAsync(List<AdminLogMessage> items)
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
                await _repository.SaveAllAsync(newitems);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                // Avoid recursion - log to diagnostics
                System.Diagnostics.Debug.WriteLine($"AdminLogService.PostConfig Error: {ex.Message}");
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }
    }
}
