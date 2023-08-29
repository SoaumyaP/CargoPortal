using Groove.SP.Application.ViewSetting.ViewModels;
using System.Collections.Generic;

namespace Groove.SP.Application.ViewSetting.Interfaces
{
    public interface IHasViewSetting
    {
        public string ViewSettingModuleId { get; set; }

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }
    }
}