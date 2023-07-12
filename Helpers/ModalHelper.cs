using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Shared.Modals;
using Blazored.Modal;
using Blazored.Modal.Services;

namespace AzureNamingTool.Helpers
{
    public class ModalHelper
    {
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

        public static void ShowInformationModal(IModalService modal, ThemeInfo theme, string headerstyle, string title, string message, object component, bool admin)
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
