using AzureNamingTool.Models;
using AzureNamingTool.Services;
using AzureNamingTool.Shared;
using Blazored.Modal.Services;
using Blazored.Modal;
using AzureNamingTool.Pages;
using System.Reflection;

namespace AzureNamingTool.Helpers
{
    public class ModalHelper
    {
        public static async Task<bool> ShowConfirmationModal(IModalService modal, string title, string message, string headerstyle, ThemeInfo theme)
        {
            bool response = false;
            try
            {
                var parameters = new ModalParameters();
                parameters.Add(nameof(ConfirmationModal.title), title);
                parameters.Add(nameof(ConfirmationModal.message), message);
                parameters.Add(nameof(ConfirmationModal.headerstyle), headerstyle);
                parameters.Add("theme", theme);

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
                var parameters = new ModalParameters();
                parameters.Add(nameof(InformationModal.message), message);
                parameters.Add(nameof(InformationModal.title), title);
                parameters.Add(nameof(InformationModal.headerstyle), headerstyle);
                parameters.Add(nameof(InformationModal.component), component);
                parameters.Add(nameof(InformationModal.admin), admin);
                parameters.Add("theme", theme);

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

                var parameters = new ModalParameters();
                parameters.Add(nameof(AddModal.id), id);
                parameters.Add(nameof(AddModal.type), type);
                parameters.Add(nameof(AddModal.message), message);
                parameters.Add(nameof(AddModal.title), title);
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
                var parameters = new ModalParameters();
                parameters.Add(nameof(EditModal.id), id);
                parameters.Add(nameof(EditModal.type), type);
                parameters.Add(nameof(EditModal.message), message);
                parameters.Add(nameof(EditModal.title), title);
                parameters.Add(nameof(EditModal.protectedName), protectedname);
                parameters.Add(nameof(EditModal.parentcomponent), parentcomponent!);
                parameters.Add(nameof(EditModal.servicesData), servicesData);
                parameters.Add("theme", theme);

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
