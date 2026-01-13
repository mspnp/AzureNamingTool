using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services.Interfaces;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing resource delimiters.
    /// </summary>
    public class ResourceDelimiterService : IResourceDelimiterService
    {
        private readonly IConfigurationRepository<ResourceDelimiter> _repository;
        private readonly IAdminLogService _adminLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceDelimiterService"/> class.
        /// </summary>
        /// <param name="repository">The configuration repository for resource delimiters.</param>
        /// <param name="adminLogService">The admin log service.</param>
        public ResourceDelimiterService(
            IConfigurationRepository<ResourceDelimiter> repository,
            IAdminLogService adminLogService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _adminLogService = adminLogService ?? throw new ArgumentNullException(nameof(adminLogService));
        }

        /// <summary>
        /// Retrieves a list of items based on the specified criteria.
        /// </summary>
        /// <param name="admin">A boolean value indicating whether the user is an admin.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation. The task result contains the service response.</returns>
        public async Task<ServiceResponse> GetItemsAsync(bool admin)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await _repository.GetAllAsync();
                if (GeneralHelper.IsNotNull(items))
                {
                    if (!admin)
                    {
                        serviceResponse.ResponseObject = items.Where(x => x.Enabled == true).OrderBy(y => y.SortOrder).ToList();
                    }
                    else
                    {
                        serviceResponse.ResponseObject = items.OrderBy(y => y.SortOrder).OrderByDescending(y => y.Enabled).ToList();
                    }
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Delimiters not found!";
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
        /// Gets the item with the specified ID from the list of resource delimiters.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation. The task result contains the service response.</returns>
        public async Task<ServiceResponse> GetItemAsync(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await _repository.GetAllAsync();
                if (GeneralHelper.IsNotNull(items))
                {
                    var item = items.FirstOrDefault(x => x.Id == id);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        serviceResponse.ResponseObject = item;
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Resource Delimiter not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Delimiters not found!";
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
        /// Gets the current item from the list of resource delimiters.
        /// </summary>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation. The task result contains the service response.</returns>
        public async Task<ServiceResponse> GetCurrentItemAsync()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await _repository.GetAllAsync();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.OrderBy(y => y.SortOrder).OrderByDescending(y => y.Enabled).First();
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Delimiter not found!";
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
        /// Posts an item of type ResourceDelimiter.
        /// </summary>
        /// <param name="item">The item to be posted.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation. The task result contains the service response.</returns>
        public async Task<ServiceResponse> PostItemAsync(ResourceDelimiter item)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var itemsEnumerable = await _repository.GetAllAsync();
                if (itemsEnumerable == null)
                {
                    serviceResponse.ResponseObject = "Resource Delimiters not found!";
                    return serviceResponse;
                }
                var items = itemsEnumerable.ToList();
                if (GeneralHelper.IsNotNull(items))
                {
                    // Set the new id
                    if (item.Id == 0)
                    {
                        // Use max ID + 1 instead of count + 1 to avoid ID collisions after deletions
                        item.Id = items.Count > 0 ? items.Max(x => x.Id) + 1 : 1;
                    }
                    item.Enabled = true;
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
                        if (items.Any(x => x.Id == item.Id))
                        {
                            // Remove the updated item from the list
                            var existingitem = items.FirstOrDefault(x => x.Id == item.Id);
                            if (GeneralHelper.IsNotNull(existingitem))
                            {
                                items.Remove(existingitem);
                            }

                            // Reset the sort order of the list
                            foreach (ResourceDelimiter thisitem in items.OrderBy(x => x.SortOrder).ToList())
                            {
                                if (item.Enabled && thisitem.Id != item.Id)
                                {
                                    thisitem.Enabled = false;
                                }
                                thisitem.SortOrder = position;
                                position += 1;
                            }

                            // Check for the new sort order
                            var existingAtPosition = items.FirstOrDefault(x => x.SortOrder == item.SortOrder);
                            if (existingAtPosition != null)
                            {
                                // Insert at the specified position
                                items.Insert(items.IndexOf(existingAtPosition), item);
                            }
                            else
                            {
                                // Put the item at the end
                                items.Add(item);
                            }
                        }
                        else
                        {
                            // Add new item - ID was already set above
                            item.SortOrder = items.Count + 1;
                            
                            // Disable other items if this new item is enabled
                            if (item.Enabled)
                            {
                                foreach (var existingItem in items)
                                {
                                    existingItem.Enabled = false;
                                }
                            }
                            
                            items.Add(item);
                        }
                    }
                    else
                    {
                        // Add first item to empty list
                        item.SortOrder = 1;
                        items.Add(item);
                    }

                    // Normalize sort order
                    position = 1;
                    foreach (ResourceDelimiter thisitem in items.OrderBy(x => x.SortOrder).ToList())
                    {
                        thisitem.SortOrder = position;
                        position += 1;
                    }

                    // Write items to file
                    await _repository.SaveAllAsync(items);
                    serviceResponse.ResponseObject = "Resource Delimiter added/updated!";
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Delimiters not found!";
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
        /// Posts the configuration for resource delimiters.
        /// </summary>
        /// <param name="items">The list of resource delimiters to be configured.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation. The task result contains the service response.</returns>
        public async Task<ServiceResponse> PostConfigAsync(List<ResourceDelimiter> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                string[,] delimiters = new string[4, 2] { { "dash", "-" }, { "underscore", "_" }, { "period", "." }, { "none", "" } };
                var newitems = new List<ResourceDelimiter>();

                // Examine the current items
                foreach (ResourceDelimiter item in items)
                {
                    // Check if the item is valid
                    for (int j = 0; j <= delimiters.GetUpperBound(0); j++)
                    {
                        if ((item.Name == delimiters[j, 0]) && (item.Delimiter == delimiters[j, 1]))
                        {
                            // Add the item to the update list
                            newitems.Add(item);
                            break;
                        }
                    }
                }

                // Make sure all the delimiters are present
                for (int k = 0; k <= delimiters.GetUpperBound(0); k++)
                {
                    if (!newitems.Any(x => x.Name == delimiters[k, 0] && x.Delimiter == delimiters[k, 1]))
                    {
                        // Create a delimiter object 
                        ResourceDelimiter newitem = new()
                        {
                            Name = delimiters[k, 0],
                            Delimiter = delimiters[k, 1],
                            Enabled = false
                        };
                        newitems.Add(newitem);
                    }
                }

                // Determine new item ids/order
                int i = 1;
                var sorteditems = newitems.OrderBy(x => x.SortOrder).OrderByDescending(x => x.Enabled);
                foreach (ResourceDelimiter item in sorteditems)
                {
                    item.Id = i;
                    item.SortOrder = i;
                    i += 1;
                }

                // Write items to file
                await _repository.SaveAllAsync(newitems);
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
