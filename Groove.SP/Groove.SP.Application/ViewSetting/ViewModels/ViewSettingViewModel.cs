using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.ViewSetting.ViewModels
{
    public class ViewSettingViewModel : ViewModelBase<ViewSettingModel>
    {
        public string Field { get; set; }
        public ViewSettingType ViewType { get; set; }
        public string ModuleId { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}