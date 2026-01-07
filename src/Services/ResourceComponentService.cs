#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Repositories;
using AzureNamingTool.Repositories.Interfaces;
using AzureNamingTool.Services.Interfaces;
using System.Net.WebSockets;
using System.Security.AccessControl;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing resource components.
    /// </summary>
    public class ResourceComponentService : IResourceComponentService
    {
        private readonly IConfigurationRepository<ResourceComponent> _repository;
        private readonly IAdminLogService _adminLogService;
        private readonly IResourceConfigurationCoordinator _coordinator;

        public ResourceComponentService(
            IConfigurationRepository<ResourceComponent> repository,
            IAdminLogService adminLogService,
            IResourceConfigurationCoordinator coordinator)
        {
            _repository = repository;
            _adminLogService = adminLogService;
            _coordinator = coordinator;
        }

        /// <summary>
        /// Retrieves a list of resource components.
        /// </summary>
        /// <param name="admin">A boolean value indicating whether the user is an admin.</param>
        /// <returns>A service response containing the list of resource components.</returns>
        public async Task<ServiceResponse> GetItemsAsync(bool admin = true)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                var items = (await _repository.GetAllAsync()).ToList();
                if (GeneralHelper.IsNotNull(items))
                {
                    if (!admin)
                    {
                        serviceResponse.ResponseObject = items.Where(x => x.Enabled == true).OrderBy(y => y.SortOrder).ToList();
                    }
                    else
                    {
                        // Admin view: show all items in their exact SortOrder
                        serviceResponse.ResponseObject = items.OrderBy(y => y.SortOrder).ToList();
                    }
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Components not found!";
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
        /// Retrieves a resource component item by its ID.
        /// </summary>
        /// <param name="id">The ID of the resource component item.</param>
        /// <returns>A service response containing the resource component item.</returns>
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
                        serviceResponse.ResponseObject = "Resource Component not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Component not found!";
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
        /// Posts a resource component item.
        /// </summary>
        /// <param name="item">The resource component item to post.</param>
        /// <returns>A service response indicating the success of the operation.</returns>
        public async Task<ServiceResponse> PostItemAsync(ResourceComponent item)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = (await _repository.GetAllAsync()).ToList();
                if (GeneralHelper.IsNotNull(items))
                {
                    if (GeneralHelper.IsNotNull(items))
                    {
                        // Set the new id
                        if (item.Id == 0)
                        {
                            // Use max ID + 1 instead of count + 1 to avoid ID collisions after deletions
                            item.Id = items.Count > 0 ? items.Max(x => x.Id) + 1 : 1;
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
                            foreach (ResourceComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                            {
                                thisitem.SortOrder = position;
                                position += 1;
                            }

                            // Check for the new sort order
                            if (items.Exists(x => x.SortOrder == item.SortOrder))
                            {
                                // Insert the new item
                                items.Insert(items.IndexOf(items.FirstOrDefault(x => x.SortOrder == item.SortOrder)!), item);
                            }
                            else
                            {
                                // Put the item at the end
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
                        foreach (ResourceComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                        {
                            thisitem.SortOrder = position;
                            // DO NOT reassign Id - it should remain stable!
                            // thisitem.Id = position;
                            position += 1;
                        }

                        // Write items to file
                        await _repository.SaveAllAsync(items);
                        serviceResponse.Success = true;
                        // Get the item
                        var newitem = (await GetItemAsync((int)item.Id)).ResponseObject;
                        serviceResponse.ResponseObject = newitem;
                    }
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
        /// Deletes an item from the list of resource components.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <returns>A service response indicating the success of the operation.</returns>
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
                    var item = items.Find(x => x.Id == id && x.IsCustom == true);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        // Delete any resource type settings for the component using coordinator
                        await _coordinator.UpdateTypesOnComponentDeleteAsync(item.Name);

                        // Delete any custom components for this resource component using coordinator
                        await _coordinator.DeleteCustomComponentsByParentIdAsync(id);

                        // Remove the item from the collection
                        items.Remove(item);

                        // Update all the sort order values to reflect the removal
                        int position = 1;
                        foreach (ResourceComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                        {
                            thisitem.SortOrder = position;
                            // DO NOT reassign Id - it should remain stable!
                            // thisitem.Id = position;
                            position += 1;
                        }

                        // Write items to file
                        await _repository.SaveAllAsync(items);
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Resource Component not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Components not found!";
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
        /// Posts the configuration for a list of resource components.
        /// </summary>
        /// <param name="items">The list of resource components to configure.</param>
        /// <returns>A service response indicating the success of the operation.</returns>
        public async Task<ServiceResponse> PostConfigAsync(List<ResourceComponent> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                string[] componentnames = ["ResourceEnvironment", "ResourceInstance", "ResourceLocation", "ResourceOrg", "ResourceProjAppSvc", "ResourceType", "ResourceUnitDept", "ResourceFunction"];
                var newitems = new List<ResourceComponent>();

                // Examine the current items
                foreach (ResourceComponent item in items)
                {
                    // Check if the item is valid
                    if (!componentnames.Contains(item.Name))
                    {
                        item.IsCustom = true;
                    }
                    // Add the item to the update list
                    newitems.Add(item);
                }

                // Make sure all the component names are present
                foreach (string name in componentnames)
                {
                    if (!newitems.Exists(x => x.Name == name))
                    {
                        // Create a component object 
                        ResourceComponent newitem = new()
                        {
                            Name = name,
                            Enabled = false
                        };
                        newitems.Add(newitem);
                    }
                }

                // Determine new item ids (keep existing IDs, only assign to new items)
                long maxId = newitems.Any() ? newitems.Max(x => x.Id) : 0;

                foreach (ResourceComponent item in newitems)
                {
                    // Only assign new IDs to items that don't have one
                    if (item.Id == 0)
                    {
                        maxId++;
                        item.Id = maxId;
                    }
                    
                    // Keep the existing SortOrder - don't modify it!
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

        /// <summary>
        /// Updates the sort order of resource components without normalization.
        /// This method directly saves the provided items without re-sequencing IDs or sort orders.
        /// </summary>
        /// <param name="items">The list of resource components with updated sort orders.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public async Task<ServiceResponse> UpdateSortOrderAsync(List<ResourceComponent> items)
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