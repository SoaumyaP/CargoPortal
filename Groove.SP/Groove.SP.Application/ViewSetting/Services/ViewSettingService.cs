using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.Services.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.ViewSetting.Services
{
    public class ViewSettingService : ServiceBase<ViewSettingModel, ViewSettingViewModel>, IViewSettingService
    {
        private readonly IViewSettingRepository _viewSettingRepository;

        public ViewSettingService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IViewSettingRepository viewSettingRepository) : base(unitOfWorkProvider)
        {
            _viewSettingRepository = viewSettingRepository;
        }

        public async Task ApplyViewSettingsAsync<TViewModel>(IEnumerable<TViewModel> model, long userRoleId) where TViewModel : IHasViewSetting
        {
            if (model != null && model.Any())
            {
                var viewSettingModuleId = model.First().ViewSettingModuleId;

                // query settings by user role
                var vwSettings = await _viewSettingRepository.QueryAsNoTrackingAsync(x =>
                    x.ModuleId.Contains(viewSettingModuleId) && x.ViewRoleSettings.Any(x => x.RoleId == userRoleId));

                if (vwSettings != null &&
                    vwSettings.Any())
                {
                    // group by module id
                    var groupedVwSetting = vwSettings
                        .GroupBy(x => x.ModuleId)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.Field).ToList());

                    foreach (var item in model)
                    {
                        // perform toggle properties display
                        PropertyToggler.ToggleDisplay(model, groupedVwSetting);
                        // respond back to the client for GUI rendering purposes.
                        item.ViewSettings = Mapper.Map<IEnumerable<ViewSettingDataSourceViewModel>>(vwSettings);
                    }
                }
            }
        }

        public async Task ApplyViewSettingsAsync<TViewModel>(TViewModel model, long userRoleId) where TViewModel : IHasViewSetting
        {
            if (model != null)
            {
                // query settings by user role
                var vwSettings = await _viewSettingRepository.QueryAsNoTrackingAsync(x =>
                    x.ModuleId.Contains(model.ViewSettingModuleId) && x.ViewRoleSettings.Any(x => x.RoleId == userRoleId));

                if (vwSettings != null &&
                    vwSettings.Any())
                {
                    // group by module id
                    var groupedVwSetting = vwSettings
                        .GroupBy(x => x.ModuleId)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.Field).ToList());

                    // perform toggle properties display
                    PropertyToggler.ToggleDisplay(model, groupedVwSetting);

                    // respond back to the client for GUI rendering purposes.
                    model.ViewSettings = Mapper.Map<IEnumerable<ViewSettingDataSourceViewModel>>(vwSettings);
                }
            }
        }

        public async Task<List<ViewSettingDataSourceViewModel>> ApplyViewSettingsAsync<TViewModel>(TViewModel model, string moduleId, long userRoleId)
        {
            List<ViewSettingDataSourceViewModel> result = new();

            if (!string.IsNullOrWhiteSpace(moduleId) &&
                model != null)
            {
                var vwSettings = await _viewSettingRepository.QueryAsNoTrackingAsync(
                    x => x.ModuleId.ToLower() == moduleId.ToLower() && x.ViewRoleSettings.Any(x => x.RoleId == userRoleId));

                if (vwSettings != null &&
                    vwSettings.Any())
                {
                    result = Mapper.Map<List<ViewSettingDataSourceViewModel>>(vwSettings);
                    PropertyToggler.ToggleDisplay(model, vwSettings.Select(x => x.Field).ToArray());
                }
            }

            return result;
        }

        public void SetViewSettingModuleId<TViewModel>(string moduleId, IEnumerable<TViewModel> model) where TViewModel : IHasViewSetting
        {
            if (model != null && model.Any())
            {
                SetViewSettingModuleId(moduleId, model.ToArray());
            }
        }

        public void SetViewSettingModuleId<TViewModel>(string moduleId, params TViewModel[] model) where TViewModel : IHasViewSetting
        {
            if (model != null && model.Any())
            {
                foreach (var item in model)
                {
                    item.ViewSettingModuleId = moduleId;
                }
            }
        }

       
    }
}