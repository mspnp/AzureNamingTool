#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services.Interfaces;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing resource environments.
    /// </summary>
    public class ResourceEnvironmentService : IResourceEnvironmentService
    {
        private readonly IConfigurationRepository<ResourceEnvironment> _repository;
        private readonly IAdminLogService _adminLogService;

        public ResourceEnvironmentService(
            IConfigurationRepository<ResourceEnvironment> repository,
            IAdminLogService adminLogService)
        {
            _repository = repository;
            _adminLogService = adminLogService;
        }
        /// <summary>
        /// Retrieves a list of resource environments.
        /// </summary>
        /// <returns>A <see cref="ServiceResponse"/> containing the list of resource environments if found, or an error message if not found.</returns>
        public async Task<ServiceResponse> GetItemsAsync(bool admin = true)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = (await _repository.GetAllAsync()).ToList();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.OrderBy(x => x.SortOrder).ToList();
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Environments not found!";
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Retrieves an item from the list of resource environments based on the specified ID.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <returns>A <see cref="ServiceResponse"/> containing the retrieved item if found, or an error message if not found.</returns>
        public async Task<ServiceResponse> GetItemAsync(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = (await _repository.GetAllAsync()).ToList();
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
                        serviceResponse.ResponseObject = "Resource Environment not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Environments not found!";
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Posts an item to the list of resource environments.
        /// </summary>
        /// <param name="item">The item to add or update.</param>
        /// <returns>A <see cref="ServiceResponse"/> indicating the success of the operation.</returns>
        public async Task<ServiceResponse> PostItemAsync(ResourceEnvironment item)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Make sure the new item short name only contains letters/numbers
                if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
                {
                    serviceResponse.Success = false;
                    serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                    return serviceResponse;
                }

                // Get list of items
                var items = (await _repository.GetAllAsync()).ToList();
                if (GeneralHelper.IsNotNull(items))
                {
                    // Set the new id
                    if (item.Id == 0)
                    {
                        if (items.Count > 0)
                        {
                            item.Id = items.Max(t => t.Id) + 1;
                        }
                        else
                        {
                            item.Id = 1;
                        }
                    }

                    int position = 1;
                    items = [.. items.OrderBy(x => x.SortOrder)];

                    if (item.SortOrder == 0)
                    {
                        item.SortOrder = items.Count + 1;
                    }

                    // Determine new item id
                    if (items.Count > 0)
                    {
                        // Check if the item already exists
                        if (items.Exists(x => x.Id == item.Id))
                        {
                            // Remove the updated item from the list
                            var existingitem = items.Find(x => x.Id == item.Id);
                            if (GeneralHelper.IsNotNull(existingitem))
                            {
                                int index = items.IndexOf(existingitem);
                                items.RemoveAt(index);
                            }
                        }

                        // Reset the sort order of the list
                        foreach (ResourceEnvironment thisitem in items.OrderBy(x => x.SortOrder).ToList())
                        {
                            thisitem.SortOrder = position;
                            position += 1;
                        }

                        // Check for the new sort order
                        if (items.Exists(x => x.SortOrder == item.SortOrder))
                        {
                            // Remove the updated item from the list
                            items.Insert(items.IndexOf(items.FirstOrDefault(x => x.SortOrder == item.SortOrder)!), item);
                        }
                        else
                        {
                            // Put the item at the end
                            item.SortOrder = position;
                            items.Add(item);
                        }
                    }
                    else
                    {
                        item.Id = 1;
                        item.SortOrder = 1;
                        items.Add(item);
                    }

                    position = 1;
                    foreach (ResourceEnvironment thisitem in items.OrderBy(x => x.SortOrder).ToList())
                    {
                        thisitem.SortOrder = position;
                        position += 1;
                    }

                    // Write items to file
                    await _repository.SaveAllAsync(items);
                    // Get the item
                    var newitem = (await GetItemAsync((int)item.Id)).ResponseObject;
                    serviceResponse.ResponseObject = newitem;
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Environments not found!";
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Deletes an item from the list of resource environments.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <returns>A <see cref="ServiceResponse"/> indicating the success of the operation.</returns>
        public async Task<ServiceResponse> DeleteItemAsync(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = (await _repository.GetAllAsync()).ToList();
                if (GeneralHelper.IsNotNull(items))
                {
                    // Get the specified item
                    var item = items.Find(x => x.Id == id);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        // Remove the item from the collection
                        items.Remove(item);

                        // Update all the sort order values to reflect the removal
                        int position = 1;
                        foreach (ResourceEnvironment thisitem in items.OrderBy(x => x.SortOrder).ToList())
                        {
                            thisitem.SortOrder = position;
                            position += 1;
                        }

                        // Write items to file
                        await _repository.SaveAllAsync(items);
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Resource Environment not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Environments not found!";
                }
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Posts the configuration for a list of resource environments.
        /// </summary>
        /// <param name="items">The list of resource environments to configure.</param>
        /// <returns>A <see cref="ServiceResponse"/> indicating the success of the operation.</returns>
        public async Task<ServiceResponse> PostConfigAsync(List<ResourceEnvironment> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var newitems = new List<ResourceEnvironment>();
                int i = 1;

                // Determine new item id
                foreach (ResourceEnvironment item in items)
                {
                    // Make sure the new item short name only contains letters/numbers
                    if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
                    {
                        serviceResponse.Success = false;
                        serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                        return serviceResponse;
                    }

                    item.Id = i;
                    item.SortOrder = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await _repository.SaveAllAsync(newitems);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Updates the sort order of resource environments without normalization.
        /// This method directly saves the provided items without re-sequencing IDs or sort orders.
        /// </summary>
        /// <param name="items">The list of resource environments with updated sort orders.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public async Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceEnvironment> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Simply save the items as-is without any normalization
                await _repository.SaveAllAsync(items);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                await _adminLogService.PostItemAsync(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }
    }
}

#pragma warning restore CS1591