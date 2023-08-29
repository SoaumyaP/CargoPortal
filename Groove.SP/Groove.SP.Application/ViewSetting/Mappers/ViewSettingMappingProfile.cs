using Groove.SP.Application.Mappers;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.ViewSetting.Mappers
{
    public class ViewSettingMappingProfile : MappingProfileBase<ViewSettingModel, ViewSettingViewModel>
    {
        public ViewSettingMappingProfile()
        {
            CreateMap<ViewSettingModel, ViewSettingViewModel>();
            CreateMap<ViewSettingModel, ViewSettingDataSourceViewModel>();
        }
    }
}