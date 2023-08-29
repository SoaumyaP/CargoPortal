using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.ViewSetting.Services.Interfaces
{
    public interface IViewSettingService : IServiceBase<ViewSettingModel, ViewSettingViewModel>
    {
        /// <summary>
        /// Apply view settings to toggle properties in the data model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userRoleId"></param>
        /// <returns></returns>
        Task ApplyViewSettingsAsync<TViewModel>(TViewModel model, long userRoleId) where TViewModel : IHasViewSetting;

        /// <summary>
        /// Apply view settings to toggle properties in the list of data model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userRoleId"></param>
        /// <returns></returns>
        Task ApplyViewSettingsAsync<TViewModel>(IEnumerable<TViewModel> model, long userRoleId) where TViewModel : IHasViewSetting;

        /// <summary>
        /// Get and apply view settings to toggle properties in the data model.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="moduleId"></param>
        /// <param name="userRoleId"></param>
        /// <returns></returns>
        Task<List<ViewSettingDataSourceViewModel>> ApplyViewSettingsAsync<TViewModel>(TViewModel model, string moduleId, long userRoleId);

        /// <summary>
        /// To set value ModuleId to provided data
        /// </summary>
        /// <typeparam name="TViewModel">Data must inherit from IHasViewSetting</typeparam>
        /// <param name="moduleId">Value of ModuleId</param>
        /// <param name="model">Model data</param>
        void SetViewSettingModuleId<TViewModel>(string moduleId, params TViewModel[] model) where TViewModel : IHasViewSetting;

        /// <summary>
        /// To set value ModuleId to provided data
        /// </summary>
        /// <typeparam name="TViewModel">Data must inherit from IHasViewSetting</typeparam>
        /// <param name="moduleId">Value of ModuleId</param>
        /// <param name="model">Model data</param>
        void SetViewSettingModuleId<TViewModel>(string moduleId, IEnumerable<TViewModel> model) where TViewModel : IHasViewSetting;
    }
}