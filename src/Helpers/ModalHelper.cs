using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Components.Modals;
using Blazored.Modal;
using Blazored.Modal.Services;

namespace AzureNamingTool.Helpers
{
    /// <summary>
    /// Helper class for managing modals in the application.
    /// </summary>
    public class ModalHelper
    {
        /// <summary>
        /// Shows a confirmation modal.
        /// </summary>
        /// <param name="modal">The modal service.</param>
        /// <param name="title">The title of the modal.</param>
        /// <param name="message">The message of the modal.</param>
        /// <param name="headerstyle">The header style of the modal.</param>
        /// <param name="theme">The theme of the modal.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating the user's response.</returns>
        public static async Task<bool> ShowConfirmationModal(IModalService modal, string title, string message, string headerstyle, ThemeInfo theme)
        {
            bool response = false;
            try
            {
                var parameters = new ModalParameters
                    {
                        { nameof(ConfirmationModal.title), title },
                        { nameof(ConfirmationModal.message), message },
                        { nameof(ConfirmationModal.headerstyle), headerstyle },
                        { "theme", theme }
                    };

                var options = new ModalOptions()
                {
                    HideCloseButton = false,
                    UseCustomLayout = false
                };

                if (GeneralHelper.IsNotNull(modal))
                {
                    var displaymodal = modal.Show<ConfirmationModal>(title, parameters, options);
                    var result = await displaymodal.Result;
                    if (!result.Cancelled)
                    {
                        response = true;
                    }
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return response;
        }

        /// <summary>
        /// Shows a text confirmation modal that requires the user to type specific text to confirm.
        /// </summary>
        /// <param name="modal">The modal service.</param>
        /// <param name="title">The title of the modal.</param>
        /// <param name="message">The message of the modal.</param>
        /// <param name="requiredText">The text that the user must type to confirm.</param>
        /// <param name="headerstyle">The header style of the modal.</param>
        /// <param name="theme">The theme of the modal.</param>
        /// <param name="caseSensitive">Whether the text comparison is case-sensitive. Default is true.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating the user's response.</returns>
        public static async Task<bool> ShowTextConfirmationModal(IModalService modal, string title, string message, string requiredText, string headerstyle, ThemeInfo theme, bool caseSensitive = true)
        {
            bool response = false;
            try
            {
                var parameters = new ModalParameters
                    {
                        { nameof(TextConfirmationModal.title), title },
                        { nameof(TextConfirmationModal.message), message },
                        { nameof(TextConfirmationModal.requiredText), requiredText },
                        { nameof(TextConfirmationModal.headerstyle), headerstyle },
                        { nameof(TextConfirmationModal.caseSensitive), caseSensitive },
                        { "theme", theme }
                    };

                var options = new ModalOptions()
                {
                    HideCloseButton = false,
                    UseCustomLayout = false,
                    Size = ModalSize.Large
                };

                if (GeneralHelper.IsNotNull(modal))
                {
                    var displaymodal = modal.Show<TextConfirmationModal>(title, parameters, options);
                    var result = await displaymodal.Result;
                    if (!result.Cancelled)
                    {
                        response = true;
                    }
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return response;
        }

        /// <summary>
        /// Shows an information modal.
        /// </summary>
        /// <param name="modal">The modal service.</param>
        /// <param name="theme">The theme of the modal.</param>
        /// <param name="headerstyle">The header style of the modal.</param>
        /// <param name="title">The title of the modal.</param>
        /// <param name="message">The message of the modal.</param>
        /// <param name="component">The component to be displayed in the modal.</param>
        /// <param name="admin">A boolean indicating if the user is an admin.</param>
        /// <param name="externalurl">The external URL to be displayed in the modal.</param>
        public static void ShowInformationModal(IModalService modal, ThemeInfo theme, string headerstyle, string title, string message, object component, bool admin, string? externalurl = null)
        {
            try
            {
                var parameters = new ModalParameters
                    {
                        { nameof(InformationModal.message), message },
                        { nameof(InformationModal.title), title },
                        { nameof(InformationModal.headerstyle), headerstyle },
                        { nameof(InformationModal.component), component },
                        { nameof(InformationModal.admin), admin },
                        { nameof(InformationModal.externalurl), externalurl! },
                        { "theme", theme }
                    };

                var options = new ModalOptions()
                {
                    HideCloseButton = false,
                    UseCustomLayout = false,
                    Size = ModalSize.Large
                };

                if (GeneralHelper.IsNotNull(modal))
                {
                    var displaymodal = modal.Show<InformationModal>(title, parameters, options);
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        /// <summary>
        /// Shows an add modal.
        /// </summary>
        /// <param name="modal">The modal service.</param>
        /// <param name="servicesData">The services data.</param>
        /// <param name="theme">The theme of the modal.</param>
        /// <param name="headerstyle">The header style of the modal.</param>
        /// <param name="title">The title of the modal.</param>
        /// <param name="message">The message of the modal.</param>
        /// <param name="id">The ID of the modal.</param>
        /// <param name="type">The type of the modal.</param>
        /// <param name="admin">A boolean indicating if the user is an admin.</param>
        /// <param name="parentcomponent">The parent component of the modal.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating the user's response.</returns>
        public static async Task<bool> ShowAddModal(IModalService modal, ServicesData servicesData, ThemeInfo theme, string headerstyle, string title, string message, int id, string type, bool admin, string? parentcomponent)
        {
            bool response = false;
            try
            {

                var parameters = new ModalParameters
                    {
                        { nameof(AddModal.id), id },
                        { nameof(AddModal.type), type },
                        { nameof(AddModal.message), message },
                        { nameof(AddModal.title), title },
                        { nameof(AddModal.headerstyle), headerstyle },
                        { nameof(AddModal.admin), admin }
                    };
                if (GeneralHelper.IsNotNull(parentcomponent))
                {
                    parameters.Add(nameof(AddModal.parentcomponent), parentcomponent);
                }
                parameters.Add(nameof(AddModal.servicesData), servicesData);
                parameters.Add("theme", theme);

                var options = new ModalOptions()
                {
                    HideCloseButton = false,
                    UseCustomLayout = false,
                    Size = ModalSize.Large
                };

                if (GeneralHelper.IsNotNull(modal))
                {
                    var displaymodal = modal.Show<AddModal>(title, parameters, options);
                    var result = await displaymodal.Result;
                    if (!result.Cancelled)
                    {
                        response = true;
                    }
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return response;
        }

        /// <summary>
        /// Shows an edit modal.
        /// </summary>
        /// <param name="modal">The modal service.</param>
        /// <param name="servicesData">The services data.</param>
        /// <param name="theme">The theme of the modal.</param>
        /// <param name="headerstyle">The header style of the modal.</param>
        /// <param name="title">The title of the modal.</param>
        /// <param name="message">The message of the modal.</param>
        /// <param name="id">The ID of the modal.</param>
        /// <param name="type">The type of the modal.</param>
        /// <param name="isprotectedname">A boolean indicating if the name is protected.</param>
        /// <param name="parentcomponent">The parent component of the modal.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating the user's response.</returns>
        public static async Task<bool> ShowEditModal(IModalService modal, ServicesData servicesData, ThemeInfo theme, string headerstyle, string title, string message, int id, string type, bool isprotectedname = false, string? parentcomponent = null)
        {
            bool response = false;
            try
            {
                var parameters = new ModalParameters
                    {
                        { nameof(EditModal.id), id },
                        { nameof(EditModal.type), type },
                        { nameof(EditModal.message), message },
                        { nameof(EditModal.title), title },
                        { nameof(EditModal.isProtectedName), isprotectedname },
                        { nameof(EditModal.parentcomponent), parentcomponent! },
                        { nameof(EditModal.servicesData), servicesData },
                        { nameof(EditModal.headerstyle), headerstyle },
                        { "theme", theme }
                    };

                var options = new ModalOptions()
                {
                    HideCloseButton = false,
                    UseCustomLayout = false,
                    Size = ModalSize.Large
                };
                if (GeneralHelper.IsNotNull(modal))
                {
                    var displaymodal = modal.Show<EditModal>(title, parameters, options);
                    var result = await displaymodal.Result;
                    if (!result.Cancelled)
                    {
                        response = true;
                    }
                }
            }
            catch (Exception) {
                // TODO: Modernize helper - AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return response;
        }
    }
}
