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
                    HideCloseButton = true,
                    UseCustomLayout = true
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
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
                    HideCloseButton = true,
                    UseCustomLayout = true
                };

                if (GeneralHelper.IsNotNull(modal))
                {
                    var displaymodal = modal.Show<InformationModal>("Instructions", parameters, options);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
                    HideCloseButton = true,
                    UseCustomLayout = true
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
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
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
        /// <param name="protectedname">A boolean indicating if the name is protected.</param>
        /// <param name="parentcomponent">The parent component of the modal.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating the user's response.</returns>
        public static async Task<bool> ShowEditModal(IModalService modal, ServicesData servicesData, ThemeInfo theme, string headerstyle, string title, string message, int id, string type, bool protectedname = true, string? parentcomponent = null)
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
                        { nameof(EditModal.protectedName), protectedname },
                        { nameof(EditModal.parentcomponent), parentcomponent! },
                        { nameof(EditModal.servicesData), servicesData },
                        { nameof(EditModal.headerstyle), headerstyle },
                        { "theme", theme }
                    };

                var options = new ModalOptions()
                {
                    HideCloseButton = true,
                    UseCustomLayout = true
                };
                if (GeneralHelper.IsNotNull(modal))
                {
                    var displaymodal = modal.Show<EditModal>(title, parameters, options);
                    var result = await displaymodal.Result;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return response;
        }
    }
}
