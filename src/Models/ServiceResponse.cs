using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a service response.
    /// </summary>
    public class ServiceResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Gets or sets the response object.
        /// </summary>
        public dynamic? ResponseObject { get; set; } = null;

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string ResponseMessage { get; set; } = string.Empty;
    }
}
