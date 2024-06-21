using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing the AdminUser configuration.
    /// </summary>
    public class AdminUserService
    {
        /// <summary>
        /// Retrieves a list of items.
        /// </summary>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> GetItems()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminUser>();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.OrderBy(x => x.Name).ToList();
                    serviceResponse.Success = true;
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
        /// Retrieves an item from the list based on the specified name.
        /// </summary>
        /// <param name="name">The name of the item to retrieve.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> GetItem(string name)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminUser>();
                if (GeneralHelper.IsNotNull(items))
                {
                    var item = items.Find(x => x.Name == name);
                    serviceResponse.ResponseObject = item;
                    serviceResponse.Success = true;
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
        /// Posts an item to the list of AdminUsers.
        /// </summary>
        /// <param name="item">The AdminUser item to be added.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> PostItem(AdminUser item)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminUser>();
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

                    items = [.. items.OrderBy(x => x.Name)];

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

                        // Check for the new sort order
                        if (items.Exists(x => x.Id == item.Id))
                        {
                            // Remove the updated item from the list
                            items.Insert(items.IndexOf(items.FirstOrDefault(x => x.Id == item.Id)!), item);
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
                        items.Add(item);
                    }

                    // Write items to file
                    await ConfigurationHelper.WriteList<AdminUser>(items);
                    serviceResponse.ResponseObject = "Item added!";
                    serviceResponse.Success = true;
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
        /// Deletes an item from the list based on the specified id.
        /// </summary>
        /// <param name="id">The id of the item to delete.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> DeleteItem(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<AdminUser>();
                if (GeneralHelper.IsNotNull(items))
                {
                    // Get the specified item
                    var item = items.Find(x => x.Id == id);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        // Remove the item from the collection
                        items.Remove(item);

                        // Write items to file
                        await ConfigurationHelper.WriteList<AdminUser>(items);
                        serviceResponse.Success = true;
                    }
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
        /// Posts the configuration items to the file.
        /// </summary>
        /// <param name="items">The list of configuration items.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> PostConfig(List<AdminUser> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var newitems = new List<AdminUser>();
                int i = 1;

                // Determine new item id
                foreach (AdminUser item in items)
                {
                    item.Id = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<AdminUser>(newitems);
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