using AzureNamingTool.Helpers;
using System.ComponentModel;

namespace AzureNamingTool.Models
{
    /// <summary>
    /// Represents a response message.
    /// </summary>
    public class ResponseMessage
    {
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        public MessageTypesEnum Type { get; set; } = MessageTypesEnum.INFORMATION;

        /// <summary>
        /// Gets or sets the header of the message.
        /// </summary>
        public string Header { get; set; } = "Message";

        /// <summary>
        /// Gets or sets the main content of the message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the additional details of the message.
        /// </summary>
        public string MessageDetails { get; set; } = string.Empty;
    }
}
